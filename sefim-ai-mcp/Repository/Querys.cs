namespace SefimMcp.Repository;

public static class Querys
{
    public static string ProductListQuery = 
        @$" SELECT Id, ProductName, ProductGroup, ProductCode, [Order], Price, VatRate, Plu, IsSynced FROM dbo.Product
            WHERE ProductName LIKE @search OR ProductGroup LIKE @search OR 
            ProductCode LIKE @search OR Price LIKE @search OR VatRate LIKE @search 
            ";
}