using eShopDashboard.EntityModels.Ordering;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RFC4180;

namespace eShopDashboard.Infrastructure.Setup
{
	public class CsvOrderItemParserFactory
	{
		public static string[] HeaderColumns => new[]
		{
			"Id",
			"OrderId",
			"ProductId",
			"UnitPrice",
			"Units",
			"ProductName"
		};

		public static CsvParser<OrderItem> CreateParser()
		{
			var tokenizerOptions = new Options('"', '\\', ',');
			var tokenizer = new RFC4180Tokenizer(tokenizerOptions);
			var parserOptions = new CsvParserOptions(true, tokenizer);
			var mapper = new CsvOrderItemMapper();
			var parser = new CsvParser<OrderItem>(parserOptions, mapper);

			return parser;
		}

		private class CsvOrderItemMapper : CsvMapping<OrderItem>
		{
			public CsvOrderItemMapper()
			{
				MapProperty(0, m => m.Id);
				MapProperty(1, m => m.OrderId);
				MapProperty(2, m => m.ProductId);
				MapProperty(3, m => m.UnitPrice);
				MapProperty(4, m => m.Units);
				MapProperty(5, m => m.ProductName);
			}
		}
	}
}