namespace SefimMcp.Interfaces;

public interface IUser
{
    // Users
    public ValueTask<int> AddUser(User user, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertUser(List<User> user, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateUser(User user, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateUser(List<User> user, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteUser(int userid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteUser(List<int> userIds, CancellationToken cancellationToken = default);
    
    public ValueTask<User> GetUser(int userid, CancellationToken cancellationToken = default);
    
    public ValueTask<List<User>> GetUsers(CancellationToken cancellationToken = default);
    
    public ValueTask<List<User>> GetFilteredUsers(string search, CancellationToken cancellationToken = default);
    
    // User Product
    public ValueTask<int> AddUserProduct(UserProduct user, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertUserProduct(List<UserProduct> userProducts, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdateUserProduct(UserProduct userProduct, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdateUserProduct(List<UserProduct> userProducts, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeleteUserProduct(int userproductid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeleteUserProduct(List<int> userProductids, CancellationToken cancellationToken = default);
    
    public ValueTask<UserProduct> GetUserProduct(int userproductid, CancellationToken cancellationToken = default);
    
    public ValueTask<List<UserProduct>> GetUserProducts(CancellationToken cancellationToken = default);
    
    public ValueTask<List<UserProduct>> GetFilteredUserProducts(string search, CancellationToken cancellationToken = default);
    
    
    // Permissions
    public ValueTask<int> AddPermission(Permission user, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchInsertPermission(List<Permission> permissions, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> UpdatePermission(Permission permission, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchUpdatePermission(List<Permission> permissions, CancellationToken cancellationToken = default);
    
    public ValueTask<bool> DeletePermission(int permissionid, CancellationToken cancellationToken = default);
    
    public ValueTask<int> BatchDeletePermission(List<int> permissionids, CancellationToken cancellationToken = default);
    
    public ValueTask<Permission> GetPermission(int permissionid, CancellationToken cancellationToken = default);
    
    public ValueTask<List<Permission>> GetPermissions(CancellationToken cancellationToken = default);
    
    public ValueTask<List<Permission>> GetFilteredPermissions(string search, CancellationToken cancellationToken = default);
}