using System.Data;
using Microsoft.Data.SqlClient;

namespace Company.Function
{
    public partial class SqlFunctions
    {
        public static async Task InsertTags()
        {
            string connectionString =
                Environment.GetEnvironmentVariable("SqlConnectionString");

            string insertQuery =
                @"
                INSERT INTO [dbo].[tags]
                (
                    [Make],
                    [Model],
                    [Year],
                    [Vehicle],
                    [Style],
                    [Engine],
                    [Fuel],
                    [Transmission],
                    [Color],
                    [VinNumber],
                    [LicencePlate]
                )
                VALUES
                (@Make1, @Model1, @Year1, @Vehicle1, @Style1, @Engine1, @Fuel1, @Transmission1, @Color1, @VinNumber1, @LicencePlate1),
                (@Make2, @Model2, @Year2, @Vehicle2, @Style2, @Engine2, @Fuel2, @Transmission2, @Color2, @VinNumber2, @LicencePlate2),
                (@Make3, @Model3, @Year3, @Vehicle3, @Style3, @Engine3, @Fuel3, @Transmission3, @Color3, @VinNumber3, @LicencePlate3);";

            var tags = new[]
            {
                new
                {
                    Make = "Harley-Davidson",
                    Model = "Sportster S",
                    Year = 2023,
                    Vehicle = "Motorcycle",
                    Style = "Cruiser",
                    Engine = 1252,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Black",
                    VinNumber = "1HD1ZES1XNB016123",
                    LicencePlate = "MOTO123",
                },
                new
                {
                    Make = "Yamaha",
                    Model = "MT-07",
                    Year = 2022,
                    Vehicle = "Motorcycle",
                    Style = "Naked",
                    Engine = 689,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Blue",
                    VinNumber = "JYARN39E1NA012345",
                    LicencePlate = "BIKE456",
                },
                new
                {
                    Make = "Kawasaki",
                    Model = "Ninja 650",
                    Year = 2021,
                    Vehicle = "Motorcycle",
                    Style = "Sport",
                    Engine = 649,
                    Fuel = "Petrol",
                    Transmission = "Manual",
                    Color = "Green",
                    VinNumber = "JKAEXKD1NMA123456",
                    LicencePlate = "RIDE789",
                },
            };

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Make1", tags[0].Make);
                        command.Parameters.AddWithValue("@Model1", tags[0].Model);
                        command.Parameters.AddWithValue("@Year1", tags[0].Year);
                        command.Parameters.AddWithValue("@Vehicle1", tags[0].Vehicle);
                        command.Parameters.AddWithValue("@Style1", tags[0].Style);
                        command.Parameters.AddWithValue("@Engine1", tags[0].Engine);
                        command.Parameters.AddWithValue("@Fuel1", tags[0].Fuel);
                        command.Parameters.AddWithValue("@Transmission1", tags[0].Transmission);
                        command.Parameters.AddWithValue("@Color1", tags[0].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber1",
                            tags[0].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate1",
                            tags[0].LicencePlate ?? (object)DBNull.Value
                        );

                        command.Parameters.AddWithValue("@Make2", tags[1].Make);
                        command.Parameters.AddWithValue("@Model2", tags[1].Model);
                        command.Parameters.AddWithValue("@Year2", tags[1].Year);
                        command.Parameters.AddWithValue("@Vehicle2", tags[1].Vehicle);
                        command.Parameters.AddWithValue("@Style2", tags[1].Style);
                        command.Parameters.AddWithValue("@Engine2", tags[1].Engine);
                        command.Parameters.AddWithValue("@Fuel2", tags[1].Fuel);
                        command.Parameters.AddWithValue("@Transmission2", tags[1].Transmission);
                        command.Parameters.AddWithValue("@Color2", tags[1].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber2",
                            tags[1].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate2",
                            tags[1].LicencePlate ?? (object)DBNull.Value
                        );

                        command.Parameters.AddWithValue("@Make3", tags[2].Make);
                        command.Parameters.AddWithValue("@Model3", tags[2].Model);
                        command.Parameters.AddWithValue("@Year3", tags[2].Year);
                        command.Parameters.AddWithValue("@Vehicle3", tags[2].Vehicle);
                        command.Parameters.AddWithValue("@Style3", tags[2].Style);
                        command.Parameters.AddWithValue("@Engine3", tags[2].Engine);
                        command.Parameters.AddWithValue("@Fuel3", tags[2].Fuel);
                        command.Parameters.AddWithValue("@Transmission3", tags[2].Transmission);
                        command.Parameters.AddWithValue("@Color3", tags[2].Color);
                        command.Parameters.AddWithValue(
                            "@VinNumber3",
                            tags[2].VinNumber ?? (object)DBNull.Value
                        );
                        command.Parameters.AddWithValue(
                            "@LicencePlate3",
                            tags[2].LicencePlate ?? (object)DBNull.Value
                        );

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"{rowsAffected} row(s) inserted.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
