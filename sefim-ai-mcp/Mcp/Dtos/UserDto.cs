namespace SefimMcp.Mcp.Dtos;

/// <summary>Safe MCP projection. Authentication material is intentionally absent.</summary>
public sealed record UserDto(int Id, string? UserName, string? ProductTemplate, string? Branch, string? Department, decimal? DiscRate, decimal? DiscAmount, bool? Deleted, bool Aktarildi, bool? IsSynced, bool? IsUpdated, string? DrawerPort, string? Role)
{
    public static UserDto From(User user) => new(user.Id, user.UserName, user.ProductTemplate, user.Branch, user.Department, user.DiscRate, user.DiscAmount, user.Deleted, user.Aktarildi, user.IsSynced, user.IsUpdated, user.DrawerPort, user.Role);
}
