using eShopDashboard.EntityModels.Ordering;
using eShopDashboard.Infrastructure.Data.Ordering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlBatchInsert;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;

namespace eShopDashboard.Infrastructure.Setup
{
    public class OrderingContextSetup
    {
        private readonly string _connectionString;
        private readonly OrderingContext _dbContext;
        private readonly ILogger<OrderingContextSetup> _logger;
        private readonly string _setupPath;
        private Order[] _orderDataArray;
        private OrderItem[] _orderItemDataArray;
        private SeedingStatus _status;

        public OrderingContextSetup(
            OrderingContext dbContext,
            IHostingEnvironment env,
            ILogger<OrderingContextSetup> logger,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _setupPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "DataFiles");
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<SeedingStatus> GetSeedingStatusAsync()
        {
            if (_status != null) return _status;

            if (await _dbContext.Orders.AnyAsync()) return _status = new SeedingStatus(false);

            GetOrdersDataToLoad();
            GetOrderItemsDataToLoad();
            int dataLinesCount = GenerateHistoricalData();

            return _status = new SeedingStatus(dataLinesCount);
        }

        public async Task SeedAsync(IProgress<int> orderingProgressHandler)
        {
            var seedingStatus = await GetSeedingStatusAsync();

            if (!seedingStatus.NeedsSeeding) return;

            _logger.LogInformation($@"----- Seeding OrderingContext from ""{_setupPath}""");

            var ordersLoaded = 0;
            var orderItemsLoaded = 0;

            var ordersProgressHandler = new Progress<int>(value =>
            {
                ordersLoaded = value;
                orderingProgressHandler.Report(ordersLoaded + orderItemsLoaded);
            });

            var orderItemsProgressHandler = new Progress<int>(value =>
            {
                orderItemsLoaded = value;
                orderingProgressHandler.Report(ordersLoaded + orderItemsLoaded);
            });

            await SeedOrdersAsync(ordersProgressHandler);
            await SeedOrderItemsAsync(orderItemsProgressHandler);
        }

        private int GenerateHistoricalData()
        {
            //Generates an additional 24 months of historical data.

            var _lastOrderId = _orderDataArray.OrderBy(o => o.Id).Select(o => o.Id).Last();
            var newOrderId = _lastOrderId + 1;

            var _lastOrderItemId = _orderItemDataArray.OrderBy(oi => oi.Id).Select(oi => oi.Id).Last();
            var newOrderItemId = _lastOrderItemId + 1;

            var generatedOrders = new List<Order>();
            var generatedOrderItems = new List<OrderItem>();

            int progressCounter = 0;
            int totalCounter = _orderDataArray.Length;

            foreach (var order in _orderDataArray)
            {
                _logger.LogInformation($@"----- Generating data { (int)decimal.Round(progressCounter /(decimal)totalCounter * 100)}%");
                progressCounter++;

                // Generate the same order for one year prior
                var newOrder1 = new Order
                {
                    Id = newOrderId++,
                    Description = $"Generated from order: {order.Id}",
                    Address_Country = order.Address_Country,
                    OrderDate = order.OrderDate.AddYears(-1)
                };
                generatedOrders.Add(newOrder1);

                // Generate the same order for two years prior
                var newOrder2 = new Order
                {
                    Id = newOrderId++,
                    Description = $"Generated from order: {order.Id}",
                    Address_Country = order.Address_Country,
                    OrderDate = order.OrderDate.AddYears(-2)
                };
                generatedOrders.Add(newOrder2);

                var orderItems = _orderItemDataArray.Where(oi => oi.OrderId == order.Id);
                foreach (var orderItem in orderItems)
                {
                    generatedOrderItems.Add(new OrderItem
                    {
                        Id = newOrderItemId++,
                        OrderId = newOrder1.Id,
                        ProductName = orderItem.ProductName,
                        ProductId = orderItem.ProductId,
                        UnitPrice = orderItem.UnitPrice,
                        Units = (int)(orderItem.Units * .5)
                    });

                    generatedOrderItems.Add(new OrderItem
                    {
                        Id = newOrderItemId++,
                        OrderId = newOrder2.Id,
                        ProductName = orderItem.ProductName,
                        ProductId = orderItem.ProductId,
                        UnitPrice = orderItem.UnitPrice,
                        Units = (int)(orderItem.Units * .25)
                    });
                }
            }

            _orderDataArray = _orderDataArray.Concat(generatedOrders).ToArray();
            _orderItemDataArray = _orderItemDataArray.Concat(generatedOrderItems).ToArray();

            return _orderDataArray.Length + _orderItemDataArray.Length;
        }

        private int GetOrderItemsDataToLoad()
        {
            CsvParser<OrderItem> parser = CsvOrderItemParserFactory.CreateParser();
            var dataFile = Path.Combine(_setupPath, "OrderItems.csv");

            var loadResult = parser.ReadFromFile(dataFile, Encoding.UTF8).ToList();

            if (loadResult.Any(r => !r.IsValid))
            {
                _logger.LogError("----- DATA PARSING ERRORS: {DataFile}\n{Details}", dataFile,
                    string.Join("\n", loadResult.Where(r => !r.IsValid).Select(r => r.Error)));

                throw new InvalidOperationException($"Data parsing error loading \"{dataFile}\"");
            }

            _orderItemDataArray = loadResult.Select(r => r.Result).ToArray();

            return _orderItemDataArray.Length;
        }

        private int GetOrdersDataToLoad()
        {
            CsvParser<Order> parser = CsvOrderParserFactory.CreateParser();
            var dataFile = Path.Combine(_setupPath, "Orders.csv");

            var loadResult = parser.ReadFromFile(dataFile, Encoding.UTF8).ToList();

            if (loadResult.Any(r => !r.IsValid))
            {
                _logger.LogError("----- DATA PARSING ERRORS: {DataFile}\n{Details}", dataFile,
                    string.Join("\n", loadResult.Where(r => !r.IsValid).Select(r => r.Error)));

                throw new InvalidOperationException($"Data parsing error loading \"{dataFile}\"");
            }

            _orderDataArray = loadResult.Select(r => r.Result).ToArray();

            return _orderDataArray.Length;
        }

        private async Task SeedOrderItemsAsync(IProgress<int> recordsProgressHandler)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Seeding OrderItems");

            var batcher = new SqlBatcher<OrderItem>(_orderItemDataArray, "Ordering.OrderItems", CsvOrderItemParserFactory.HeaderColumns);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlInsert;

                while ((sqlInsert = batcher.GetInsertCommand()) != string.Empty)
                {
                    var sqlCommand = new SqlCommand(sqlInsert, connection);
                    await sqlCommand.ExecuteNonQueryAsync();

                    recordsProgressHandler.Report(batcher.RowPointer);
                }
            }

            _logger.LogInformation("----- {TotalRows} {TableName} Inserted ({TotalSeconds:n3}s)", batcher.RowPointer, "OrderItems", sw.Elapsed.TotalSeconds);
        }

        private async Task SeedOrdersAsync(IProgress<int> recordsProgressHandler)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Seeding Orders");

            var batcher = new SqlBatcher<Order>(_orderDataArray, "Ordering.Orders", CsvOrderParserFactory.HeaderColumns);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlInsert;

                while ((sqlInsert = batcher.GetInsertCommand()) != string.Empty)
                {
                    var sqlCommand = new SqlCommand(sqlInsert, connection);
                    await sqlCommand.ExecuteNonQueryAsync();

                    recordsProgressHandler.Report(batcher.RowPointer);
                }
            }

            _logger.LogInformation("----- {TotalRows} {TableName} Inserted ({TotalSeconds:n3}s)", batcher.RowPointer, "Orders", sw.Elapsed.TotalSeconds);
        }
    }
}
 