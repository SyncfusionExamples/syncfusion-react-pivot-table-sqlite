using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Syncfusion.EJ2.Base;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PivotTable_SQLite.Server.Controllers
{
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor that injects the configuration to retrieve the connection string.
        /// </summary>
        public SalesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SalesDb")!;
        }

        /// <summary>
        /// Handles GET requests to retrieve all sales data for the Pivot Table.
        /// This endpoint is called when the Pivot Table first loads or refreshes data.
        /// </summary>
        /// <returns>Returns a list of all sales records from the database.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<List<SalesData>> GetSalesData()
        {
            const string Query = @"SELECT * FROM salesdata ORDER BY orderid;";

            var dbPath = @"C:\Users\Karthickraja\Downloads\SQLite\salesdb.db";
            Console.WriteLine(System.IO.File.Exists(dbPath));
            Console.WriteLine(dbPath);


            using var Connection = new SqliteConnection(_connectionString);
            await Connection.OpenAsync();

            using var Command = new SqliteCommand(Query, Connection);
            using var Reader = await Command.ExecuteReaderAsync();

            var DataTable = new DataTable();
            DataTable.Load(Reader);

            // Convert database rows to SalesData objects
            var DataSource = (from DataRow Data in DataTable.Rows
                select new SalesData{
                    OrderID = Data["orderid"] == DBNull.Value ? (int?)null : Convert.ToInt32(Data["orderid"]),
                    CustomerName = Data["customername"] == DBNull.Value ? null : Data["customername"].ToString(),
                    Region = Data["region"] == DBNull.Value ? null : Data["region"].ToString(),
                    Country = Data["country"] == DBNull.Value ? null : Data["country"].ToString(),
                    ProductCategory = Data["productcategory"] == DBNull.Value ? null : Data["productcategory"].ToString(),
                    ProductName = Data["productname"] == DBNull.Value ? null : Data["productname"].ToString(),
                    OrderDate = Data["orderdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(Data["orderdate"]),
                    Quantity = Data["quantity"] == DBNull.Value ? (int?)null : Convert.ToInt32(Data["quantity"]),
                    UnitPrice = Data["unitprice"] == DBNull.Value ? 0m : Convert.ToDecimal(Data["unitprice"]),
                    TotalAmount = Data["totalamount"] == DBNull.Value ? 0m : Convert.ToDecimal(Data["totalamount"]),
                    SalesPerson = Data["salesperson"] == DBNull.Value ? null : Data["salesperson"].ToString()
                }
            ).ToList();
            return DataSource;
        }

        /// <summary>
        /// Handles POST requests from the Pivot Table DataManager.
        /// Processes the data request and returns formatted data for the component.
        /// </summary>
        /// <param name="DataManagerRequest">
        /// Contains the details of the data operation requested.
        /// </param>
        /// <returns>
        /// Returns the data records along with the total count.
        /// </returns>
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<object> Post([FromBody] DataManagerRequest DataManagerRequest)
        {
            // Retrieve all sales data from the database
            List<SalesData> salesData = await GetSalesData();
            IQueryable<SalesData> DataSource = salesData.AsQueryable();

            // Get the total number of records
            int totalRecordsCount = DataSource.Count();

            // Return data and count to the client
            return new { result = DataSource, count = totalRecordsCount };
        }

        /// <summary>
        /// Data model that represents the structure of a sales record.
        /// This class maps to the columns in the 'salesdata' table in SQLite.
        /// </summary>
        public class SalesData
        {
            /// <summary>
            /// Unique identifier for each order (Primary Key).
            /// The [Key] attribute marks this as the primary key for CRUD operations.
            /// </summary>
            [Key]
            public int? OrderID { get; set; }

            /// <summary>
            /// Name of the customer who placed the order.
            /// </summary>
            public string? CustomerName { get; set; }

            /// <summary>
            /// Geographic region where the customer is located.
            /// Useful for regional analysis in the pivot table.
            /// </summary>
            public string? Region { get; set; }

            /// <summary>
            /// Country where the order was placed.
            /// </summary>
            public string? Country { get; set; }

            /// <summary>
            /// Category of the product (e.g., Electronics, Furniture).
            /// Used as a dimension in the pivot table analysis.
            /// </summary>
            public string? ProductCategory { get; set; }

            /// <summary>
            /// Name of the specific product ordered.
            /// </summary>
            public string? ProductName { get; set; }

            /// <summary>
            /// Date when the order was placed.
            /// </summary>
            public DateTime? OrderDate { get; set; }

            /// <summary>
            /// Number of units ordered.
            /// </summary>
            public int? Quantity { get; set; }

            /// <summary>
            /// Price per unit of the product.
            /// </summary>
            public decimal UnitPrice { get; set; }

            /// <summary>
            /// Total cost of the order (typically quantity × unitprice).
            /// Used as a measure/aggregate value in pivot table analysis.
            /// </summary>
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// Name of the sales representative who handled the order.
            /// </summary>
            public string? SalesPerson { get; set; }
        }

        /// <summary>
        /// Inserts a new sales record into the database.
        /// This method is called when a new row is added in the Pivot Table.
        /// </summary>
        /// <param name="value">Contains the new sales data to insert.</param>
        /// <returns>Returns the inserted record with its new OrderID.</returns>
        [HttpPost]
        [Route("api/[controller]/Insert")]
        public async Task<IActionResult> Insert([FromBody] CRUDModel<SalesData> value)
        {
            try
            {
                const string sql = @"
            INSERT INTO salesdata
            (customername, region, country, productcategory, productname, orderdate, quantity, unitprice, totalamount, salesperson)
            VALUES (@CustomerName, @Region, @Country, @ProductCategory, @ProductName, @OrderDate, @Quantity, @UnitPrice, @TotalAmount, @SalesPerson);
            SELECT last_insert_rowid();
        ";

                using var conn = new SqliteConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqliteCommand(sql, conn);

                // Add parameters to prevent SQL injection
                cmd.Parameters.AddWithValue("@CustomerName", (object?)value.value?.CustomerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", (object?)value.value?.Region ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Country", (object?)value.value?.Country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductCategory", (object?)value.value?.ProductCategory ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductName", (object?)value.value?.ProductName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OrderDate", (object?)value.value?.OrderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", (object?)value.value?.Quantity ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnitPrice", (object?)value.value?.UnitPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalAmount", (object?)value.value?.TotalAmount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SalesPerson", (object?)value.value?.SalesPerson ?? DBNull.Value);

                // Execute the query and get the newly created OrderID
                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // Update the value object with the new ID
                if (value.value != null) value.value.OrderID = newId;

                return Ok(new { key = newId, value = value.value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Insert failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing sales record in the database.
        /// This method is called when a row is edited in the Pivot Table.
        /// </summary>
        /// <param name="value">Contains the updated sales data.</param>
        /// <returns>Returns the number of rows updated.</returns>
        [HttpPost]
        [Route("api/[controller]/Update")]
        public async Task<IActionResult> Update([FromBody] CRUDModel<SalesData> value)
        {
            if (value?.value == null || value.value.OrderID == null)
                return BadRequest("OrderID and payload are required.");

            try
            {
                const string sql = @"
            UPDATE salesdata
            SET customername    = @CustomerName,
                region          = @Region,
                country         = @Country,
                productcategory = @ProductCategory,
                productname     = @ProductName,
                orderdate       = @OrderDate,
                quantity        = @Quantity,
                unitprice       = @UnitPrice,
                totalamount     = @TotalAmount,
                salesperson     = @SalesPerson
            WHERE orderid = @OrderID;
        ";

                using var conn = new SqliteConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqliteCommand(sql, conn);

                // Add parameters to prevent SQL injection
                cmd.Parameters.AddWithValue("@CustomerName", (object?)value.value?.CustomerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", (object?)value.value?.Region ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Country", (object?)value.value?.Country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductCategory", (object?)value.value?.ProductCategory ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductName", (object?)value.value?.ProductName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OrderDate", (object?)value.value?.OrderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", (object?)value.value?.Quantity ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnitPrice", (object?)value.value?.UnitPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalAmount", (object?)value.value?.TotalAmount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SalesPerson", (object?)value.value?.SalesPerson ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OrderID", value.value.OrderID);

                // Execute the update
                var rows = await cmd.ExecuteNonQueryAsync();
                return Ok(new { updated = rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Update failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a sales record from the database.
        /// This method is called when a row is deleted in the Pivot Table.
        /// </summary>
        /// <param name="value">Contains the OrderID of the record to delete.</param>
        /// <returns>Returns the number of rows deleted.</returns>
        [HttpPost]
        [Route("api/[controller]/Remove")]
        public async Task<IActionResult> Remove([FromBody] CRUDModel<SalesData> value)
        {
            if (value?.key == null)
                return BadRequest("Missing key.");

            if (!int.TryParse(value.key.ToString(), out var id))
                return BadRequest("Invalid OrderID.");

            try
            {
                const string sql = @"DELETE FROM salesdata WHERE orderid = @OrderID;";

                using var conn = new SqliteConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@OrderID", id);

                // Execute the delete
                var rows = await cmd.ExecuteNonQueryAsync();
                return Ok(new { deleted = rows, key = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Delete failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Generic model for handling CRUD operations from the Pivot Table.
        /// The Pivot Table uses this structure to send data to Insert, Update, and Remove endpoints.
        /// </summary>
        /// <typeparam name="T">The data type (e.g., SalesData)</typeparam>
        public class CRUDModel<T> where T : class
        {
            /// <summary>
            /// Action being performed (e.g., 'Add', 'Edit', 'Delete').
            /// Indicates the type of operation requested by the client.
            /// </summary>
            public string? action { get; set; }

            /// <summary>
            /// Primary key column name (e.g., 'orderid').
            /// Identifies which column is used as the unique identifier.
            /// </summary>
            public string? keyColumn { get; set; }

            /// <summary>
            /// The primary key value (e.g., the OrderID).
            /// Used to identify the specific record being operated on.
            /// </summary>
            public object? key { get; set; }

            /// <summary>
            /// The single record being operated on (for Insert, Update operations).
            /// Contains all field values for the record.
            /// </summary>
            public T? value { get; set; }

            /// <summary>
            /// Additional parameters sent by the client.
            /// Can contain extra metadata or configuration options.
            /// </summary>
            public IDictionary<string, object>? @params { get; set; }
        }
    }
}