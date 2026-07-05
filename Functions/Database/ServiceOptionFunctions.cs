using LogMate.Domain.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Company.Function
{
    public partial class SqlFunctions
    {
        public static async Task<ServiceHierarchy> GetHierarchy()
        {
            string? connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            using var conn = new SqlConnection(connString);
            await conn.OpenAsync();

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

            return new ServiceHierarchy
            {
                MotorbikeOptions = BuildHierarchy(motorbikeFlat),
                OwnershipOptions = BuildHierarchy(ownershipFlat),
            };
        }

        public static async Task<bool> AddServiceTypeById(string optionId, string serviceTypeId)
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

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

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@OptionId", SqlDbType.Int).Value = optionId;
            cmd.Parameters.Add("@ServiceTypeId", SqlDbType.Int).Value = serviceTypeId;

            await conn.OpenAsync();

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public static async Task<bool> AddParentOptionById(int optionId, int parentId)
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

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

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId;
            cmd.Parameters.Add("@ChildId", SqlDbType.Int).Value = optionId;

            await conn.OpenAsync();

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public static async Task<bool> CreateNewServiceOption(
            string name,
            string description,
            int categoryId
        )
        {
            string connString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            const string sql =
                @"
                DECLARE @ServiceOptionId INT;

                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ServiceOption WHERE Name = @Name
                )
                BEGIN
                    INSERT INTO dbo.ServiceOption (Name, Description, CategoryId)
                    VALUES (@Name, @Description, @CategoryId);

                    SET @ServiceOptionId = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @ServiceOptionId = Id
                    FROM dbo.ServiceOption
                    WHERE Name = @Name;
                END

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
            ";

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(
                description
            )
                ? DBNull.Value
                : description;
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value =
                categoryId > 0 ? categoryId : DBNull.Value;

            await conn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            return true;
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

        private static List<ServiceOption> BuildHierarchy(List<FlatServiceOption> flatList)
        {
            var dict = flatList
                .GroupBy(x => x.Id)
                .Select(g => new ServiceOption
                {
                    Id = g.Key,
                    Name = g.First().Name,
                    Description = g.First().Description,
                    ServiceTypes = g.SelectMany(x => x.ServiceTypes).Distinct().ToList(),
                })
                .ToDictionary(x => x.Id);

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && dict.ContainsKey(item.ParentId.Value))
                {
                    dict[item.ParentId.Value].Children.Add(dict[item.Id]);
                }
            }

            return dict
                .Values.Where(x => !flatList.Any(f => f.Id == x.Id && f.ParentId.HasValue))
                .ToList();
        }
    }
}
