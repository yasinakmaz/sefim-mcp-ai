namespace SefimMcp.Repository;

[McpServerToolType]
public class Users (
        ISqlService<User> userService,
        ISqlService<UserProduct> userProductService,
        ISqlService<Permission> permissionService
        ) : IUser
{
    #region User

    [McpServerTool]
    [Description("Adds users (staff) to the ‘Şefim’ app.")]
    public async ValueTask<int> AddUser(User user, CancellationToken cancellationToken = default)
    {
        var result = await userService.InsertAndGetIdAsync<int>(user, cancellationToken);

        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("Adds users (staff) to the ‘Şefim’ app in bulk.")]
    public async ValueTask<int> BatchInsertUser(List<User> user, CancellationToken cancellationToken = default)
    {
        var result = await userService.BatchInsertAsync(user, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("It sends updates to users (staff) on the “Şefim” app.")]
    public async ValueTask<bool> UpdateUser(User user, CancellationToken cancellationToken = default)
    {
        var result = await userService.UpdateAsync(user, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("It sends a bulk update to users (staff) on the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateUser(List<User> user, CancellationToken cancellationToken = default)
    {
        var result = await userService.BatchUpdateAsync(user, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Deletes the specified user from the “Şefim” app. Please note that this action cannot be undone; please inform the user and obtain their consent.")]
    public async ValueTask<bool> DeleteUser(int userid, CancellationToken cancellationToken = default)
    {
        var result = await userService.DeleteAsync(userid, cancellationToken);

        return result > 0;
    }

    [McpServerTool]
    [Description("Deletes the relevant users from the “Şefim” app in bulk. Please note that this action cannot be undone; be sure to notify the user and obtain their approval.")]
    public async ValueTask<int> BatchDeleteUser(List<int> userids, CancellationToken cancellationToken = default)
    {
        var result = await userService.BatchDeleteAsync(userids.Cast<object>(), 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Retrieves the specific user's information from the “Şefim” app.")]
    public async ValueTask<User> GetUser(int userid, CancellationToken cancellationToken = default)
    {
        var result = await userService.GetByIdAsync(userid, cancellationToken);
        
        return result ?? new User();
    }

    [McpServerTool]
    [Description("Returns a list of all users on “Şefim” in a single query.")]
    public async ValueTask<List<User>> GetUsers(CancellationToken cancellationToken = default)
    {
        var result = await userService.GetAllAsync(cancellationToken);

        return result.ToList() ?? new List<User>();
    }

    [McpServerTool]
    [Description("It returns users from “Şefim” by filtering them (using the search parameter).")]
    public async ValueTask<List<User>> GetFilteredUsers(string search, CancellationToken cancellationToken = default)
    {
        string normalizedSearch = $"%{search}%";

        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalizedSearch.Trim()
        };

        var result = await userService.ExecuteRawQueryAsync(Querys.ListUserQuery, parameters, cancellationToken);
        
        return result.ToList();
    }

    #endregion

    #region UserProduct

    [McpServerTool]
    [Description("It adds items individually to user products in the “Şefim” app.")]
    public async ValueTask<int> AddUserProduct(UserProduct user, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.InsertAndGetIdAsync<int>(user, cancellationToken);
        
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("It adds user products in bulk to the ‘Şefim’ app.")]
    public async ValueTask<int> BatchInsertUserProduct(List<UserProduct> userProducts, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.BatchInsertAsync(userProducts, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("The “Şefim” app applies updates to the relevant user's product on an individual basis.")]
    public async ValueTask<bool> UpdateUserProduct(UserProduct userProduct, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.UpdateAsync(userProduct, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("It applies a bulk update to the relevant user product in the “Şefim” app.")]
    public async ValueTask<int> BatchUpdateUserProduct(List<UserProduct> userProducts, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.BatchUpdateAsync(userProducts, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("The “Şefim” app permanently deletes the relevant user's product, and this action cannot be undone. Please obtain additional permission from the user.")]
    public async ValueTask<bool> DeleteUserProduct(int userproductid, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.DeleteAsync(userproductid, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("The “Şefim” app will permanently delete the relevant user products via a hard-batch delete, and this action cannot be undone. Please obtain additional permission from the user.")]
    public async ValueTask<int> BatchDeleteUserProduct(List<int> userProductids, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.BatchDeleteAsync(userProductids.Cast<object>(), 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("The “Şefim” app provides detailed information about each user's product.")]
    public async ValueTask<UserProduct> GetUserProduct(int userproductid, CancellationToken cancellationToken = default)
    {
        var result = await userProductService.GetByIdAsync(userproductid, cancellationToken);
        
        return result ?? new UserProduct();
    }

    [McpServerTool]
    [Description("The “Şefim” app provides all user products at once.")]
    public async ValueTask<List<UserProduct>> GetUserProducts(CancellationToken cancellationToken = default)
    {
        var result = await userProductService.GetAllAsync(cancellationToken);
        
        return result.ToList() ?? new List<UserProduct>();
    }

    [McpServerTool]
    [Description("The “Şefim” app displays user products by filtering them.")]
    public async ValueTask<List<UserProduct>> GetFilteredUserProducts(string search, CancellationToken cancellationToken = default)
    {
        string normalizedSearch = $"%{search}%";

        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalizedSearch.Trim()
        };
        
        var result = await userProductService.ExecuteRawQueryAsync(Querys.ListUserProductQuery, parameters, cancellationToken);
        
        return result.ToList() ?? new List<UserProduct>();
    }

    #endregion

    #region Permission

    [McpServerTool]
    [Description("It assigns permissions to the user on the “Şefim” app.")]
    public async ValueTask<int> AddPermission(Permission user, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.InsertAndGetIdAsync<int>(user, cancellationToken);
        
        return result.HasValue ? result.Value : 0;
    }

    [McpServerTool]
    [Description("It assigns permissions in bulk to users on the “Şefim” app.")]
    public async ValueTask<int> BatchInsertPermission(List<Permission> permission, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.BatchInsertAsync(permission, 1000, cancellationToken);

        return result;
    }

    [McpServerTool]
    [Description("Updates the permissions for the relevant user in the “Şefim” app")]
    public async ValueTask<bool> UpdatePermission(Permission permission, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.UpdateAsync(permission, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk-updates user permissions on the ‘Şefim’ app")]
    public async ValueTask<int> BatchUpdatePermission(List<Permission> permission, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.BatchUpdateAsync(permission, 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Removes the relevant user permission on the ‘Şefim’ app")]
    public async ValueTask<bool> DeletePermission(int permissionid, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.DeleteAsync(permissionid, cancellationToken);
        
        return result > 0;
    }

    [McpServerTool]
    [Description("Bulk-deletes user permissions on the ‘Şefim’ app")]
    public async ValueTask<int> BatchDeletePermission(List<int> permissionids, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.BatchDeleteAsync(permissionids.Cast<object>(), 1000, cancellationToken);
        
        return result;
    }

    [McpServerTool]
    [Description("Retrieves user permissions on the ‘Şefim’ app individually")]
    public async ValueTask<Permission> GetPermission(int permissionid, CancellationToken cancellationToken = default)
    {
        var result = await permissionService.GetByIdAsync(permissionid, cancellationToken);
        
        return result ?? new Permission();
    }

    [McpServerTool]
    [Description("Grants user permissions on the ‘Şefim’ app all at once")]
    public async ValueTask<List<Permission>> GetPermissions(CancellationToken cancellationToken = default)
    {
        var result = await permissionService.GetAllAsync(cancellationToken);
        
        return result.ToList() ?? new List<Permission>();
    }

    [McpServerTool]
    [Description("The “Şefim” app grants user permissions by filtering them")]
    public async ValueTask<List<Permission>> GetFilteredPermissions(string search, CancellationToken cancellationToken = default)
    {
        string normalizedSearch = $"%{search}%";

        var parameters = new Dictionary<string, object?>()
        {
            ["Search"] = normalizedSearch.Trim()
        };
        
        var result = await permissionService.ExecuteRawQueryAsync(Querys.ListPermissionQuery, parameters, cancellationToken);
        
        return result.ToList() ?? new List<Permission>();
    }

    #endregion
}