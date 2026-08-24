using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace SqlService.Core.Interfaces;

/// <summary>
/// SQL command builder interface - SQL komut inşa
/// AOT-Compatible: Reflection-free SQL generation
/// </summary>
public interface ISqlCommandBuilder
{
    /// <summary>
    /// SELECT BY ID komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="id">Primary key değeri</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildSelectByIdCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(object id) where T : class;

    /// <summary>
    /// SELECT ALL komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildSelectAllCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T : class;

    /// <summary>
    /// SELECT WHERE komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="whereClause">WHERE koşulu</param>
    /// <param name="parameters">Parametreler</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildSelectWhereCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string whereClause, IReadOnlyDictionary<string, object?> parameters) where T : class;

    /// <summary>
    /// SELECT PAGED komutu oluşturur (OFFSET-FETCH)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="pageNumber">Sayfa numarası (1-based)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildSelectPagedCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(int pageNumber, int pageSize) where T : class;

    /// <summary>
    /// SELECT PAGED WITH COUNT komutu oluşturur (Single query with COUNT(*) OVER())
    /// Performance optimization: 40-50% faster than separate COUNT + SELECT queries
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="pageNumber">Sayfa numarası (1-based)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <returns>SqlCommand with TotalCount column</returns>
    SqlCommand BuildSelectPagedWithCountCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(int pageNumber, int pageSize) where T : class;

    /// <summary>
    /// COUNT komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="whereClause">WHERE koşulu (opsiyonel)</param>
    /// <param name="parameters">Parametreler (opsiyonel)</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildCountCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string? whereClause = null, IReadOnlyDictionary<string, object?>? parameters = null) where T : class;

    /// <summary>
    /// INSERT komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="entity">Eklenecek entity</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildInsertCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity) where T : class;

    /// <summary>
    /// INSERT komutu oluşturur - OUTPUT INSERTED ile Identity değerini döndürür
    /// Entity'de Identity tanımlı değilse normal INSERT komutu döner
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="hasIdentity">Entity'de Identity var mı (out parameter)</param>
    /// <returns>SqlCommand (OUTPUT INSERTED.Id içerir eğer Identity varsa)</returns>
    SqlCommand BuildInsertWithOutputCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity, out bool hasIdentity) where T : class;

    /// <summary>
    /// UPDATE komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="entity">Güncellenecek entity</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildUpdateCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T entity) where T : class;

    /// <summary>
    /// DELETE komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="id">Primary key değeri</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildDeleteCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(object id) where T : class;

    /// <summary>
    /// DELETE WHERE komutu oluşturur
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="whereClause">WHERE koşulu</param>
    /// <param name="parameters">Parametreler</param>
    /// <returns>SqlCommand</returns>
    SqlCommand BuildDeleteWhereCommand<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string whereClause, IReadOnlyDictionary<string, object?> parameters) where T : class;
}
