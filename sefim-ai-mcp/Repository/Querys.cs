namespace SefimMcp.Repository;

public static class Querys
{
    public static string ProductListQuery = 
        @$" SELECT Id, ProductName, ProductGroup, ProductCode, [Order], Price, VatRate, Plu, IsSynced FROM dbo.Product
            WHERE ProductName LIKE @search OR ProductGroup LIKE @search OR 
            ProductCode LIKE @search OR Price LIKE @search OR VatRate LIKE @search 
            ";

    public static string CustomerListQuery =
        $@"
        SELECT Id, CustomerName, FullName, CustomerCode, Adi, Soyadi, Eposta, TaxOffice, TaxNumber, Address1, Address2, Category, PhoneNumber, DiscountRate, Passive, CreditAllowance, CardNo, Aktarildi, IsSynced, IsUpdated FROM dbo.Customer
        WHERE CustomerName LIKE @Search OR FullName LIKE @Search OR CustomerCode LIKE @Search OR Adi LIKE @Search OR Soyadi LIKE @Search OR Eposta LIKE @Search OR TaxOffice LIKE @Search OR TaxNumber LIKE @Search OR
        Address1 LIKE @Search OR Address2 LIKE @Search OR Category LIKE @Search OR PhoneNumber LIKE @Search OR CardNo LIKE @Search OR
          ";
    
    public static string SoftDeleteCustomerQuery =
        $@"
        UPDATE dbo.Customer
        SET Passive = 1
        WHERE Id = @P1
          ";
    
    public static string ListUserQuery =
        $@"
        SELECT Id, UserName, [Password], [Role] FROM dbo.[User] WHERE UserName LIKE @Search OR [Role] LIKE @Search
          ";
    
    public static string ListUserProductQuery =
        $@"
        SELECT Id, UserName, ProductId FROM dbo.[UserProducts] WHERE UserName LIKE @Search
          ";
    
    public static string ListPermissionQuery =
        $@"
        SELECT Id, UserName, PermissionName, PermissionValue, Aktarildi, IsSynced, IsUpdated FROM [dbo].[Permission]
        WHERE UserName LIKE @Search OR PermissionName LIKE @Search
          ";
    
    public static string ListMenuQuery =
        $@"
        SELECT Id, MenuName, ChoiceCount, Price, Active, Aktarildi, IsSynced, IsUpdated FROM dbo.Menu
        WHERE MenuName LIKE @Search
          ";
    
    public static string ListMenuProductQuery =
        $@"
        SELECT Id, MenuId, ProductName, Price, Aktarildi, IsSynced, IsUpdated, ProductId FROM dbo.MenuProduct
        WHERE ProductName LIKE @Search
          ";
}