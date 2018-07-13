using eShopDashboard.EntityModels.Catalog;
using eShopDashboard.Infrastructure.Data.Catalog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlBatchInsert;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TinyCsvParser;

namespace eShopDashboard.Infrastructure.Setup
{
    public class CatalogContextSetup
    {
        private readonly CatalogContext _dbContext;
        private readonly ILogger<CatalogContextSetup> _logger;
        private readonly string _setupPath;
        private readonly string _connectionString;

        private CatalogItem[] _dataArray;
        private SeedingStatus _status;

        public CatalogContextSetup(
            CatalogContext dbContext,
            IHostingEnvironment env,
            ILogger<CatalogContextSetup> logger,
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

            if (await _dbContext.CatalogItems.AnyAsync()) return _status = new SeedingStatus(false);

            int dataLinesCount = GetDataToLoad();

            return _status = new SeedingStatus(dataLinesCount);
        }

        public async Task SeedAsync(IProgress<int> catalogProgressHandler)
        {
            var seedingStatus = await GetSeedingStatusAsync();

            if (!seedingStatus.NeedsSeeding) return;

            _logger.LogInformation($@"----- Seeding CatalogContext from ""{_setupPath}""");

            await SeedCatalogItemsAsync(catalogProgressHandler);
        }

        private int GetDataToLoad()
        {
            CsvParser<CatalogItem> parser = CsvCatalogItemParserFactory.CreateParser();
            var dataFile = Path.Combine(_setupPath, "CatalogItems.csv");

            var loadResult = parser.ReadFromFile(dataFile, Encoding.UTF8).ToList();

            if (loadResult.Any(r => !r.IsValid))
            {
                _logger.LogError("----- DATA PARSING ERRORS: {DataFile}\n{Details}", dataFile, 
                    string.Join("\n", loadResult.Where(r => !r.IsValid).Select(r => r.Error)));

                throw new InvalidOperationException($"Data parsing error loading \"{dataFile}\"");
            }

            _dataArray = loadResult.Select(r => r.Result).ToArray();

            //---------------------------------------------
            // Times 2 to account for item tags processing
            //---------------------------------------------

            return _dataArray.Length; // * 2 ; // Include times 2 if processing catalog tags
        }

        private async Task SeedCatalogItemsAsync(IProgress<int> recordsProgressHandler)
        {
            var sw = new Stopwatch();
            sw.Start();

            var itemCount = 0;
            var tagCount = 0;

            void Aggregator ()
            {
                recordsProgressHandler.Report(itemCount + tagCount);
            };

            var itemsProgressHandler = new Progress<int>(value =>
            {
                itemCount = value;
                Aggregator();
            });

            var tagsProgressHandler = new Progress<int>(value =>
            {
                tagCount = value;
                Aggregator();
            });

            _logger.LogInformation("----- Seeding CatalogItems");

            var batcher = new SqlBatcher<CatalogItem>(_dataArray, "Catalog.CatalogItems", CsvCatalogItemParserFactory.HeaderColumns);

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

            _logger.LogInformation("----- {TotalRows} {TableName} Inserted ({TotalSeconds:n3}s)", batcher.RowPointer, "CatalogItems", sw.Elapsed.TotalSeconds);


            //----------------------------------------------------------------------
            // Not needed now because CatalogItems.csv already includes CatalogTags
            // Could be needed later on in case the items or tags get updated
            //----------------------------------------------------------------------

            //await SeedCatalogTagsAsync(tagsProgressHandler);
        }

        private async Task SeedCatalogTagsAsync(IProgress<int> recordsProgressHandler)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Adding CatalogTags");
            var tagsText = await File.ReadAllTextAsync(Path.Combine(_setupPath, "CatalogTags.txt"));

            var tags = JsonConvert.DeserializeObject<List<CatalogFullTag>>(tagsText);

            _logger.LogInformation("----- Adding tags to CatalogItems");

            int i = 0;

            foreach (var tag in tags)
            {
                var entity = await _dbContext.CatalogItems.FirstOrDefaultAsync(ci => ci.Id == tag.ProductId);

                if (entity == null) continue;

                entity.TagsJson = JsonConvert.SerializeObject(tag);

                _dbContext.Update(entity);

                recordsProgressHandler.Report(++i);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("----- {TotalRows} CatalogTags Added ({TotalSeconds:n3}s)", i, sw.Elapsed.TotalSeconds);
        }
    }
}