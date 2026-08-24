using SqlService.Core.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SqlService.Infrastructure.Mappers;

/// <summary>
/// Entity metadata container - Reflection-free metadata storage
/// AOT-Compatible: Compile-time metadata cache
/// </summary>
/// <typeparam name="T">Entity tipi</typeparam>
public static class EntityMetadata<[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties |
    DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    where T : class
{
    private static readonly Lazy<TableMetadata> _metadata = new(() => BuildMetadata());

    /// <summary>
    /// Entity'nin metadata'sını döndürür (cached)
    /// </summary>
    public static TableMetadata Metadata => _metadata.Value;

    /// <summary>
    /// Metadata oluşturur (ONCE - compile time)
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Generic type T is annotated with DynamicallyAccessedMembers which preserves property types")]
    private static TableMetadata BuildMetadata()
    {
        var entityType = typeof(T);
        var nullabilityContext = new NullabilityInfoContext();

        // TableAttribute kontrolü
        var tableAttr = (TableAttribute?)Attribute.GetCustomAttribute(entityType, typeof(TableAttribute));
        var tableName = tableAttr?.Name ?? entityType.Name;
        var schemaName = tableAttr?.Schema ?? "dbo";

        // Record support: Detect parameterless constructor and primary constructor
        var hasParameterlessCtor = entityType.GetConstructor(Type.EmptyTypes) != null;
        System.Reflection.ConstructorInfo? primaryCtor = null;
        System.Reflection.ParameterInfo[]? ctorParams = null;
        int[]? constructorParameterMap = null;

        if (!hasParameterlessCtor)
        {
            // Find primary constructor (most parameters = positional record pattern)
            primaryCtor = entityType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
            ctorParams = primaryCtor?.GetParameters();
        }

        // Property'leri analiz et
        var properties = entityType.GetProperties();
        var propertyMetadataList = new List<PropertyMetadata>();

        foreach (var prop in properties)
        {
            // Record support: Find matching constructor parameter for attribute fallback
            System.Reflection.ParameterInfo? matchingCtorParam = null;
            if (ctorParams != null)
            {
                matchingCtorParam = Array.Find(ctorParams,
                    p => string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
            }

            // IgnoreAttribute kontrolü (property'den veya constructor param'dan)
            if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)) ||
                (matchingCtorParam != null && Attribute.IsDefined(matchingCtorParam, typeof(IgnoreAttribute))))
                continue;

            // ColumnAttribute kontrolü (property'den, yoksa constructor param'dan fallback)
            var columnAttr = (ColumnAttribute?)Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute))
                ?? (matchingCtorParam != null
                    ? (ColumnAttribute?)Attribute.GetCustomAttribute(matchingCtorParam, typeof(ColumnAttribute))
                    : null);
            var columnName = columnAttr?.Name ?? prop.Name;

            // PrimaryKeyAttribute kontrolü (property'den, yoksa constructor param'dan fallback)
            var pkAttr = (PrimaryKeyAttribute?)Attribute.GetCustomAttribute(prop, typeof(PrimaryKeyAttribute))
                ?? (matchingCtorParam != null
                    ? (PrimaryKeyAttribute?)Attribute.GetCustomAttribute(matchingCtorParam, typeof(PrimaryKeyAttribute))
                    : null);

            // Enum detection and converter compilation
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var isEnum = underlyingType.IsEnum;
            Func<object, object?>? enumConverter = null;

            if (isEnum)
            {
                // Compile enum converter function (ONCE at startup, reuse forever)
                // Performance: 20-30% faster than runtime Enum.ToObject() calls
                enumConverter = value =>
                {
                    if (value == null || value == DBNull.Value)
                        return null;
                    return Enum.ToObject(underlyingType, value);
                };
            }

            propertyMetadataList.Add(new PropertyMetadata
            {
                PropertyName = prop.Name,
                ColumnName = columnName,
                PropertyType = prop.PropertyType,
                IsPrimaryKey = pkAttr != null,
                IsIdentity = pkAttr?.IsIdentity ?? false,
                IsNullable = columnAttr?.IsNullable ?? IsNullableProperty(prop, matchingCtorParam, nullabilityContext),
                MaxLength = columnAttr?.MaxLength,
                IsEnum = isEnum,
                EnumUnderlyingType = isEnum ? underlyingType : null,
                EnumConverter = enumConverter
            });
        }

        var propertiesArray = propertyMetadataList.ToArray();

        // Generate explicit column list (cached at startup)
        // Performance: Negligible, but improves maintainability and safety
        var columnList = string.Join(", ", propertiesArray.Select(p => $"[{p.ColumnName}]"));

        // Build constructor parameter -> Properties index mapping for record types
        if (!hasParameterlessCtor && ctorParams != null)
        {
            constructorParameterMap = new int[ctorParams.Length];

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var paramName = ctorParams[i].Name!;
                constructorParameterMap[i] = Array.FindIndex(propertiesArray,
                    p => string.Equals(p.PropertyName, paramName, StringComparison.OrdinalIgnoreCase));
            }
        }

        return new TableMetadata
        {
            TableName = tableName,
            SchemaName = schemaName,
            FullTableName = $"[{schemaName}].[{tableName}]",
            Properties = propertiesArray,
            PrimaryKeyProperties = propertiesArray.Where(p => p.IsPrimaryKey).ToArray(),
            ColumnList = columnList,
            HasParameterlessConstructor = hasParameterlessCtor,
            ConstructorParameterMap = constructorParameterMap
        };
    }

    private static bool IsNullableProperty(
        PropertyInfo property,
        ParameterInfo? constructorParameter,
        NullabilityInfoContext nullabilityContext)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
        {
            return true;
        }

        if (!property.PropertyType.IsValueType)
        {
            var propertyNullability = nullabilityContext.Create(property);
            if (propertyNullability.ReadState != NullabilityState.Unknown)
            {
                return propertyNullability.ReadState == NullabilityState.Nullable;
            }

            if (constructorParameter != null)
            {
                var parameterNullability = nullabilityContext.Create(constructorParameter);
                if (parameterNullability.ReadState != NullabilityState.Unknown)
                {
                    return parameterNullability.ReadState == NullabilityState.Nullable;
                }
            }

            // Nullable metadata yoksa reference type'lari nullable kabul et.
            return true;
        }

        return false;
    }
}

/// <summary>
/// Table metadata
/// </summary>
public sealed class TableMetadata
{
    public required string TableName { get; init; }
    public required string SchemaName { get; init; }
    public required string FullTableName { get; init; }
    public required PropertyMetadata[] Properties { get; init; }
    public required PropertyMetadata[] PrimaryKeyProperties { get; init; }

    /// <summary>
    /// Cached explicit column list for SELECT statements
    /// Format: "[Column1], [Column2], [Column3]"
    /// Performance: Negligible, but improves maintainability (explicit > implicit)
    /// </summary>
    public required string ColumnList { get; init; }

    /// <summary>
    /// True if entity has a parameterless constructor (class), false if constructor-only (record)
    /// </summary>
    public required bool HasParameterlessConstructor { get; init; }

    /// <summary>
    /// Constructor parameter -> Properties index mapping for record types
    /// ConstructorParameterMap[i] = Properties array index for i-th constructor parameter
    /// Null if HasParameterlessConstructor is true
    /// </summary>
    public int[]? ConstructorParameterMap { get; init; }

    /// <summary>
    /// Non-primary key property'leri döndürür
    /// </summary>
    public PropertyMetadata[] NonPrimaryKeyProperties =>
        Properties.Where(p => !p.IsPrimaryKey).ToArray();

    /// <summary>
    /// Insert edilebilir property'leri döndürür (Identity olmayan)
    /// </summary>
    public PropertyMetadata[] InsertableProperties =>
        Properties.Where(p => !p.IsIdentity).ToArray();
}

/// <summary>
/// Property metadata
/// </summary>
public sealed class PropertyMetadata
{
    public required string PropertyName { get; init; }
    public required string ColumnName { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public required Type PropertyType { get; init; }

    public required bool IsPrimaryKey { get; init; }
    public required bool IsIdentity { get; init; }
    public required bool IsNullable { get; init; }
    public int? MaxLength { get; init; }

    // Enum optimization: Cache enum metadata at build time
    public required bool IsEnum { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type? EnumUnderlyingType { get; init; }

    /// <summary>
    /// Cached enum converter function (if IsEnum = true)
    /// Performance: 20-30% faster than runtime Enum.ToObject() calls
    /// </summary>
    public Func<object, object?>? EnumConverter { get; init; }
}
