# SqlService

**AOT-Compatible, Zero-Reflection SQL Server Data Access Library**

.NET 10 Native AOT ve Full Trimming ile tam uyumlu, yuksek performansli veritabani erisim kutuphanesi.

---

## Ozellikler

- **Zero Reflection**: Compiled expression trees ile AOT tam uyumluluk
- **Full Trimming Support**: `PublishTrimmed=true` ile sorunsuz calisir
- **Native AOT Ready**: `PublishAot=true` destegi
- **High Performance**: Connection pooling, command caching, ordinal caching
- **Stored Procedure Support**: Multiple result set destegi (Dapper'dan kolay)
- **Batch Operations**: SqlBulkCopy tabanli toplu islemler
- **Type-Safe**: Generic constraint'ler ile derleme zamani tip guvenligi

---

## Kurulum

```csharp
// Program.cs veya Startup.cs
services.AddSqlService(configuration);

// veya manuel
services.AddSqlService("Server=.;Database=MyDb;...");
```

**appsettings.json:**
```json
{
  "SqlService": {
    "ConnectionString": "Server=.;Database=TVSLS;Trusted_Connection=True;TrustServerCertificate=True;",
    "CommandTimeout": 30,
    "EnableRetry": true,
    "MaxRetryCount": 3
  }
}
```

---

## Kullanim

### 1. Entity Tanimlama

```csharp
using SqlService.Core.Attributes;

// Record destegi (immutable)
[Table("Users", Schema = "Identity")]
public sealed record UserEntity(
    [property: PrimaryKey(IsIdentity = false)]
    [property: Column("UserId")]
    Guid Id,

    [property: Column("Email", MaxLength = 256)]
    string Email,

    [property: Column("FullName", MaxLength = 200)]
    string FullName,

    [property: Column("PasswordHash")]
    string PasswordHash,

    [property: Column("IsActive")]
    bool IsActive,

    [property: Column("CreatedAt")]
    DateTime CreatedAt,

    [property: Column("RoleId")]
    Guid RoleId
);

// Class destegi (mutable)
[Table("Roles", Schema = "Identity")]
public sealed class RoleEntity
{
    [PrimaryKey(IsIdentity = false)]
    [Column("RoleId")]
    public Guid Id { get; set; }

    [Column("RoleName", MaxLength = 50)]
    public required string Name { get; set; }

    [Column("RoleCode", MaxLength = 20)]
    public required string Code { get; set; }
}
```

---

### 2. Temel CRUD Islemleri

```csharp
public class UserRepository
{
    private readonly ISqlService<UserEntity> _sqlService;

    public UserRepository(ISqlService<UserEntity> sqlService)
    {
        _sqlService = sqlService;
    }

    // Tek kayit getir
    public async Task<UserEntity?> GetByIdAsync(Guid id)
    {
        return await _sqlService.GetByIdAsync(id);
    }

    // Tum kayitlar
    public async Task<IEnumerable<UserEntity>> GetAllAsync()
    {
        return await _sqlService.GetAllAsync();
    }

    // Kosullu sorgu
    public async Task<IEnumerable<UserEntity>> GetActiveUsersAsync()
    {
        return await _sqlService.GetWhereAsync(
            "IsActive = @IsActive",
            new Dictionary<string, object?> { ["IsActive"] = true }
        );
    }

    // Sayfalama
    public async Task<QueryResult<UserEntity>> GetPagedAsync(int page, int size)
    {
        return await _sqlService.GetPagedAsync(page, size);
    }

    // Ekleme (etkilenen kayit sayisi doner)
    public async Task<int> CreateAsync(UserEntity user)
    {
        return await _sqlService.InsertAsync(user);
    }

    // Ekleme ve Identity ID alma (OUTPUT INSERTED)
    public async Task<int?> CreateAndGetIdAsync(OrderEntity order)
    {
        // Identity (auto-increment) olan tablolarda yeni ID'yi dondurur
        return await _sqlService.InsertAndGetIdAsync<int>(order);
    }

    // Guncelleme
    public async Task<int> UpdateAsync(UserEntity user)
    {
        return await _sqlService.UpdateAsync(user);
    }

    // Silme
    public async Task<int> DeleteAsync(Guid id)
    {
        return await _sqlService.DeleteAsync(id);
    }
}
```

---

### 3. Batch (Toplu) Islemler

```csharp
// Toplu ekleme (SqlBulkCopy - cok hizli)
var users = new List<UserEntity> { /* 10000 kayit */ };
await _sqlService.BatchInsertAsync(users, batchSize: 5000);

// Toplu guncelleme (MERGE statement)
await _sqlService.BatchUpdateAsync(users, batchSize: 1000);

// Toplu silme
var idsToDelete = new List<object> { id1, id2, id3 };
await _sqlService.BatchDeleteAsync(idsToDelete);
```

---

### 4. Stored Procedure Kullanimi

#### 4.1 Tek Result Set

```csharp
private readonly ISprocService _sprocService;

// Liste donduren SP
var users = await _sprocService.QueryListAsync<UserDto>(
    "Identity.sp_GetActiveUsers",
    new SprocParams().Add("@TenantId", tenantId)
);

// Tek kayit donduren SP
var user = await _sprocService.QuerySingleAsync<UserDto>(
    "Identity.sp_GetUserById",
    new SprocParams().Add("@UserId", userId)
);

// Scalar deger (COUNT, SUM, vb.)
var count = await _sprocService.QueryScalarAsync<int>(
    "Identity.sp_GetUserCount",
    new SprocParams().Add("@IsActive", true)
);
```

#### 4.2 Iki Result Set (Sayfalama Pattern)

sp_UserList gibi procedure'ler icin (ilk result: count, ikinci result: data):

```csharp
// Kolay yontem - Extension method
var result = await _sprocService.QueryPagedAsync<UserDto>(
    "Identity.sp_UserList",
    new SprocParams()
        .Add("@PageNumber", 1)
        .Add("@PageSize", 20)
        .AddNullable("@Search", searchText)
        .AddNullable("@RoleCode", roleCode)
        .AddNullable("@DealerId", dealerId)
        .AddNullable("@IsActive", isActive)
        .Add("@SortBy", "CreatedAt")
        .Add("@SortDir", "DESC")
);

// result.TotalCount → Toplam kayit sayisi
// result.Data → List<UserDto>

// veya manuel
var (totalCount, users) = await _sprocService.QueryScalarAndListAsync<int?, UserDto>(
    "Identity.sp_UserList",
    new SprocParams()
        .Add("@PageNumber", 1)
        .Add("@PageSize", 20)
);
```

#### 4.3 Uc Result Set

```csharp
var (users, roles, permissions) = await _sprocService.QueryAsync<
    List<UserDto>,
    List<RoleDto>,
    List<PermissionDto>
>("dbo.sp_GetUserWithRolesAndPermissions",
    new SprocParams().Add("@UserId", userId)
);
```

#### 4.4 Dort+ Result Set (Manuel Reader)

```csharp
await using var reader = await _sprocService.QueryMultipleAsync(
    "dbo.sp_GetDashboardData",
    new SprocParams().Add("@TenantId", tenantId)
);

var userCount = await reader.ReadScalarAsync<int>();        // 1. result set
var users = await reader.ReadListAsync<UserDto>();          // 2. result set
var roles = await reader.ReadListAsync<RoleDto>();          // 3. result set
var permissions = await reader.ReadListAsync<PermissionDto>(); // 4. result set
var settings = await reader.ReadSingleAsync<SettingsDto>(); // 5. result set

// Output parametreleri
var outputs = reader.OutputParameters;
var returnValue = reader.ReturnValue;
```

#### 4.5 Non-Query (INSERT/UPDATE/DELETE SP)

```csharp
// Basit
var affected = await _sprocService.ExecuteAsync(
    "Identity.sp_UpdateUserStatus",
    new SprocParams()
        .Add("@UserId", userId)
        .Add("@IsActive", false)
);

// Output parametreli
var (affected, outputs) = await _sprocService.ExecuteWithOutputAsync(
    "Identity.sp_CreateUser",
    new SprocParams()
        .Add("@Email", email)
        .Add("@PasswordHash", hash)
        .AddOutput("@NewUserId", SqlDbType.UniqueIdentifier)
        .AddReturnValue()
);

var newUserId = (Guid?)outputs["@NewUserId"];
var returnCode = outputs["@ReturnValue"];
```

---

### 5. SprocParams Fluent API

```csharp
var parameters = new SprocParams()
    // Temel parametreler
    .Add("@Name", "John")
    .Add("@Age", 25)
    .Add("@BirthDate", DateTime.Now)
    .Add("@Id", Guid.NewGuid())

    // Nullable parametreler (null ise DBNull.Value)
    .AddNullable("@MiddleName", middleName)      // string?
    .AddNullable("@Score", score)                // int?
    .AddNullable("@DealerId", dealerId)          // Guid?

    // Tip belirterek
    .Add("@Description", desc, SqlDbType.NVarChar, 4000)
    .Add("@Amount", amount, SqlDbType.Decimal)

    // Output parametreleri
    .AddOutput("@TotalCount", SqlDbType.Int)
    .AddOutput("@NewId", SqlDbType.UniqueIdentifier)
    .AddInputOutput("@Counter", 0, SqlDbType.Int)

    // Return value
    .AddReturnValue();
```

---

## Mimari

```
SqlService/
├── Core/
│   ├── Attributes/
│   │   ├── TableAttribute.cs      # [Table("Name", Schema="dbo")]
│   │   ├── ColumnAttribute.cs     # [Column("Name", MaxLength=100)]
│   │   ├── PrimaryKeyAttribute.cs # [PrimaryKey(IsIdentity=true)]
│   │   └── IgnoreAttribute.cs     # [Ignore]
│   │
│   ├── Interfaces/
│   │   ├── ISqlService.cs         # Ana CRUD interface
│   │   ├── IConnectionFactory.cs  # Connection olusturma
│   │   ├── ISqlCommandBuilder.cs  # SQL command builder
│   │   ├── IBatchOperation.cs     # Toplu islemler
│   │   └── IEntityMapper.cs       # Entity mapping
│   │
│   ├── Models/
│   │   ├── QueryResult.cs         # Sayfalama sonucu
│   │   └── SqlConnectionSettings.cs
│   │
│   └── Sprocs/
│       ├── ISprocService.cs       # Stored Procedure interface
│       ├── ISprocReader.cs        # Multiple result set reader
│       ├── SprocParams.cs         # Fluent parameter builder
│       └── SprocExtensions.cs     # Extension methods
│
└── Infrastructure/
    ├── Services/
    │   └── SqlService.cs          # ISqlService implementasyonu
    │
    ├── Sprocs/
    │   ├── SprocService.cs        # ISprocService implementasyonu
    │   └── SprocReader.cs         # ISprocReader implementasyonu
    │
    ├── Mappers/
    │   ├── EntityMapper.cs        # AOT-compatible mapping
    │   └── EntityMetadata.cs      # Compile-time metadata
    │
    ├── Builders/
    │   ├── SqlCommandBuilder.cs   # SQL command generation
    │   └── BatchOperationBuilder.cs
    │
    ├── Factories/
    │   ├── SqlConnectionFactory.cs
    │   └── SqlCommandFactory.cs
    │
    ├── Extensions/
    │   ├── ServiceCollectionExtensions.cs  # DI registration
    │   └── SqlDataReaderExtensions.cs
    │
    └── Utilities/
        ├── DataTablePool.cs       # DataTable object pooling
        └── SqlCommandCache.cs     # Command caching
```

---

## Dapper Karsilastirmasi

| Ozellik | Dapper | SqlService |
|---------|--------|------------|
| AOT Uyumluluk | Hayir (Reflection) | Tam Uyumlu |
| Trimming | Sorunlu | Tam Uyumlu |
| Multiple Result Set | `QueryMultiple` + `Read` | Tuple return (tek satir) |
| Parametre | Anonymous object | `SprocParams` fluent |
| Batch Insert | Yok (loop) | SqlBulkCopy |
| Connection Pool | Manuel | Otomatik |
| Generic Constraint | Yok | `DynamicallyAccessedMembers` |

**Dapper:**
```csharp
using var multi = await conn.QueryMultipleAsync("sp_UserList",
    new { PageNumber = 1, PageSize = 20 }, commandType: CommandType.StoredProcedure);
var count = await multi.ReadFirstAsync<int>();
var users = (await multi.ReadAsync<UserDto>()).ToList();
```

**SqlService:**
```csharp
var (count, users) = await _sprocService.QueryScalarAndListAsync<int?, UserDto>(
    "sp_UserList",
    new SprocParams().Add("@PageNumber", 1).Add("@PageSize", 20)
);
```

---

## Performans Optimizasyonlari

1. **Ordinal Caching**: Column ordinal'lari ilk read'de cache'lenir
2. **Expression Caching**: Property getter/setter'lar compiled expression olarak saklanir
3. **DataTable Pooling**: SqlBulkCopy icin DataTable object pooling
4. **Connection Pooling**: SqlClient'in built-in pool'u kullanilir
5. **Command Caching**: Sik kullanilan SQL command'lari cache'lenir

---

## AOT/Trimming Uyumlulugu

```xml
<!-- .csproj -->
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
</PropertyGroup>
```

Entity'ler icin `DynamicallyAccessedMembers` attribute otomatik uygulanir:

```csharp
public interface ISqlService<[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties |
    DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    where T : class
```

---

## Lisans

Bu kutuphane TVSLS (Trade Vision Secure Licensing System) projesinin bir parcasidir.
