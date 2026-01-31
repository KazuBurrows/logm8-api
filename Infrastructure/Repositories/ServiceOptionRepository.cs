using System.Data;
using Microsoft.Data.SqlClient;

public class ServiceOptionRepository : IServiceOptionRepository
{
    private readonly SqlConnectionFactory _factory;

    public ServiceOptionRepository(SqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<FlatServiceOption>> GetMotorbikeServiceOptions()
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();

        // =======================
        // 🚗 Motorbike hierarchy
        // =======================
        var motorbikeSql =
            @"
                    WITH MotorbikeOptions AS (
                        SELECT so.Id, so.Name, so.Description, CAST(NULL AS INT) AS ParentId
                        FROM ServiceOption so
                        JOIN VehicleTypeServiceOption vtso ON vtso.ServiceOptionId = so.Id
                        JOIN VehicleType vt ON vt.Id = vtso.VehicleTypeId
                        WHERE vt.Name = 'Motorbike'

                        UNION ALL

                        SELECT child.Id, child.Name, child.Description, rel.ParentId
                        FROM ServiceOptionRelation rel
                        JOIN ServiceOption child ON child.Id = rel.ChildId
                        JOIN MotorbikeOptions m ON m.Id = rel.ParentId
                    )
                    SELECT mo.Id,
                        mo.Name,
                        mo.Description,
                        mo.ParentId,
                        STRING_AGG(st.Name, ',') AS ServiceTypes
                    FROM MotorbikeOptions mo
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = mo.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    GROUP BY mo.Id, mo.Name, mo.Description, mo.ParentId;
                    ";
        var motorbikeFlat = await QueryFlatOptions(conn, motorbikeSql);
        return motorbikeFlat;
    }

    public async Task<List<FlatServiceOption>> GetOwnershipServiceOptions()
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();

        // =======================
        // 🧾 Ownership
        // =======================
        var ownershipSql =
            @"
                    SELECT so.Id,
                        so.Name,
                        so.Description,
                        CAST(NULL AS INT) AS ParentId,
                        STRING_AGG(st.Name, ',') AS ServiceTypes
                    FROM ServiceOption so
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = so.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    WHERE so.Name = 'Ownership'
                    GROUP BY so.Id, so.Name, so.Description;
                    ";

        var ownershipFlat = await QueryFlatOptions(conn, ownershipSql);
        return ownershipFlat;
    }

    public async Task<int> AddServiceOptionAsync(string name, string? description, int? categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        using var conn = _factory.Create();
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();

        const string sql =
            @"
                DECLARE @ServiceOptionId INT;

                -- Insert ServiceOption if it does not exist
                IF NOT EXISTS (SELECT 1 FROM dbo.ServiceOption WHERE Name = @Name)
                BEGIN
                    INSERT INTO dbo.ServiceOption (Name, Description, CategoryId)
                    VALUES (@Name, @Description, @CategoryId);
                    SET @ServiceOptionId = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @ServiceOptionId = Id FROM dbo.ServiceOption WHERE Name = @Name;
                END

                -- Link to Motorbike (VehicleTypeId = 3)
                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.VehicleTypeServiceOption
                    WHERE VehicleTypeId = 3
                    AND ServiceOptionId = @ServiceOptionId
                )
                BEGIN
                    INSERT INTO dbo.VehicleTypeServiceOption (VehicleTypeId, ServiceOptionId)
                    VALUES (3, @ServiceOptionId);
                END

                SELECT @ServiceOptionId;
            ";

        using var cmd = new SqlCommand(sql, conn, transaction);
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).Value = name.Trim();
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value =
            string.IsNullOrWhiteSpace(description) ? DBNull.Value : description;
        cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value =
            categoryId.HasValue && categoryId.Value > 0 ? categoryId.Value : DBNull.Value;

        var serviceOptionId = (int)await cmd.ExecuteScalarAsync();

        transaction.Commit();

        return serviceOptionId; // returns the inserted or existing ServiceOption ID
    }

    private static async Task<List<FlatServiceOption>> QueryFlatOptions(
        SqlConnection conn,
        string sql
    )
    {
        var results = new List<FlatServiceOption>();

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(
                new FlatServiceOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    ServiceTypes = reader.IsDBNull(4)
                        ? new List<string>()
                        : reader.GetString(4).Split(',').ToList(),
                }
            );
        }

        return results;
    }

    public async Task<int> AddParentServiceOptionAsync(int optionId, int parentId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();

        const string sql =
            @"
                INSERT INTO dbo.ServiceOptionRelation (ParentId, ChildId)
                SELECT @ParentId, @ChildId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.ServiceOptionRelation
                    WHERE ParentId = @ParentId
                    AND ChildId = @ChildId
                );
            ";

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId;
        cmd.Parameters.Add("@ChildId", SqlDbType.Int).Value = optionId;

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected;
    }

    public async Task<int> AddServiceOptionServiceTypeAsync(string optionId, string serviceTypeId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();

        const string sql =
            @"
                INSERT INTO dbo.ServiceOptionServiceType (ServiceOptionId, ServiceTypeId)
                SELECT @OptionId, @ServiceTypeId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.ServiceOptionServiceType
                    WHERE ServiceOptionId = @OptionId
                    AND ServiceTypeId = @ServiceTypeId
                );
            ";

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@OptionId", SqlDbType.Int).Value = optionId;
        cmd.Parameters.Add("@ServiceTypeId", SqlDbType.Int).Value = serviceTypeId;

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        // true = inserted, false = already existed
        return rowsAffected;
    }
}
