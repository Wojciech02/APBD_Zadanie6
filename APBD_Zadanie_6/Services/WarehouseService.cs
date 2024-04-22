using APBD_Task_6.Models;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;

        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async void AddProduct(ProductWarehouse productWarehouse)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;

            await connection.OpenAsync();

            cmd.CommandText = "SELECT TOP 1 [Order].IdOrder FROM [Order] " +
                "LEFT JOIN Product_Warehouse ON [Order].IdOrder = Product_Warehouse.IdOrder " +
                "WHERE [Order].IdProduct = @IdProduct " +
                "AND [Order].Amount = @Amount " +
                "AND Product.Warehouse.IdProductWarehouse IS NULL " +
                "AND [Order.CreatedAt] < @CreatedAt";

            cmd.Paeameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
            cmd.Paeameters.AddWithValue("Amount", productWarehouse.Amount);
            cmd.Paeameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

            var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new Exception();

            await reader.ReadAsync();
            int OrderId = int Parse(reader["IdOrder"].ToString());

            await reader.CloseAsync();
            cmd.Parameter.Clear();

            cmd.CommandText = "SELECT price FROM product WHERE IdProduct =@IdProduct";
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);

            reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new Exception();

            double Amount = double Parse(reader["Amount"].ToString());

            await reader.CloseAsync();
            cmd.Parameter.Clear();

            cmd.CommandText = "SELECT IdWarehouse FROM warehouse WHERE IdWarehouse =@IdWarehouse";
            cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);

            reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new Exception();

            await reader.CloseAsync();
            cmd.Parameter.Clear();

            var transaction =(SqlTransaction)await connection.BeginTransactionAsync();
            cmd.Trnsaction = transaction;

            try
            {
                cmd.CommandText = "UPDATE [Order] SET FullfilledAt = @CreatedAt WHERE IdOrder =@IdOrder";
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
                cmd.Parameters.AddWithValue("IdOrder", OrderId);

                int rowUpdated = await cmd.ExecuteNonQueryAsync();
                if (rowUpdated < 1) throw new Exception();

                cmd.Parameters.Clear();


                cmd.CommandText = "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, Amount, Price, CreatedAt) " +
                    $"VALUES(@IdWarehouse, @IdProduct, @Amount, @Amount*{price}, @CreatedAt)";
                cmd.Parameters.AddWithValue();
                cmd.Parameters.AddWithValue();
                cmd.Parameters.AddWithValue();
                cmd.Parameters.AddWithValue();
                cmd.Parameters.AddWithValue();

                int rowsInserted = await cmd.ExecuteNonQueryAsync();

                if(rowsInserted < 1)throw new Exception();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await connection.RollbackAsync();
                throw new Exception;
            }

            cmd.Parameters.Clear();

            cmd.CommandText = "SELECT TOP 1 IdProductWarehouse from Product_Warehouse ORDER BY DESC"

            reader = cmd.ExecuteReaderAsync();
            await reader.ReadeAsync();


            return 1;
           
        }
    }
}
