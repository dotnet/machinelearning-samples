using eShopDashboard.EntityModels.Ordering;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RFC4180;

namespace eShopDashboard.Infrastructure.Setup
{
	public class CsvOrderParserFactory
	{
		public static string[] HeaderColumns => new[]
		{
			"Id",
			"Address_Country",
			"OrderDate",
			"Description"
		};

		public static CsvParser<Order> CreateParser()
		{
			var tokenizerOptions = new Options('"', '\\', ',');
			var tokenizer = new RFC4180Tokenizer(tokenizerOptions);
			var parserOptions = new CsvParserOptions(true, tokenizer);
			var mapper = new CsvOrderMapper();
			var parser = new CsvParser<Order>(parserOptions, mapper);

			return parser;
		}

		private class CsvOrderMapper : CsvMapping<Order>
		{
			public CsvOrderMapper()
			{
				MapProperty(0, m => m.Id);
				MapProperty(1, m => m.Address_Country);
				MapProperty(2, m => m.OrderDate);
				MapProperty(3, m => m.Description);
			}
		}
	}
}