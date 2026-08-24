using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Interfaces;

/// <summary>
/// Entity mapper interface - Reflection-free entity mapping
/// AOT-Compatible: Compiled expressions kullanır
/// </summary>
public interface IEntityMapper
{
    /// <summary>
    /// SqlDataReader'dan entity'e map yapar (reflection-free)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="reader">SqlDataReader</param>
    /// <returns>Mapped entity</returns>
    T MapFromReader<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(SqlDataReader reader) where T : class;

    /// <summary>
    /// Entity'den DataTable'a map yapar (reflection-free)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="entities">Entity'ler</param>
    /// <returns>DataTable</returns>
    DataTable MapToDataTable<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IEnumerable<T> entities) where T : class;

    /// <summary>
    /// Entity'nin property değerlerini alır (reflection-free)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="entity">Entity</param>
    /// <returns>Property değerleri (name-value pairs)</returns>
    IReadOnlyDictionary<string, object?> GetPropertyValues<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity) where T : class;
}
