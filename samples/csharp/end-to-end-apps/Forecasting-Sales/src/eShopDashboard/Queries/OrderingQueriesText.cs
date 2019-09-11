namespace eShopDashboard.Queries
{
    public static class OrderingQueriesText
    {
        //Returns product data for those products that have at least 12 months of data.
        public static string ProductHistory(string productId)
        {
            var sqlCommandText = $@"
select p1.productId, p1.[year], p1.[month], p1.units, p1.[avg], p1.[count], p1.[max], p1.[min],
    LAG (units, 1) OVER (PARTITION BY p1.productId ORDER BY p1.productId, p1.date) as prev,
    LEAD (units, 1) OVER (PARTITION BY p1.productId ORDER BY p1.productId, p1.date) as [next]
from (
    select oi.ProductId as productId, 
        YEAR(CAST(oi.OrderDate as datetime)) as [year], 
        MONTH(CAST(oi.OrderDate as datetime)) as [month], 
        MIN(CAST(oi.OrderDate as datetime)) as date,
        sum(oi.Units) as units,
        avg(oi.Units) as [avg],
        count(oi.Units) as [count],
        max(oi.Units) as [max],
        min(oi.Units) as [min]
    from (
        select CONVERT(date, oo.OrderDate) as OrderDate, oi.ProductId, sum(oi.Units) as units
        from [ordering].[orderItems] oi
        inner join [ordering].[orders] oo on oi.OrderId = oo.Id
		{(string.IsNullOrEmpty(productId) ? string.Empty : "where oi.ProductId = @productId")} 
        group by CONVERT(date, oo.OrderDate), oi.ProductId) as oi 
        group by oi.ProductId, YEAR(CAST(oi.OrderDate as datetime)), MONTH(CAST(oi.OrderDate as datetime))
    ) p1

inner join 

(
select p2.productId
	from (
		select oi.ProductId as productId, 
			YEAR(CAST(oi.OrderDate as datetime)) as [year], 
			MONTH(CAST(oi.OrderDate as datetime)) as [month], 
			MIN(CAST(oi.OrderDate as datetime)) as date,
			sum(oi.Units) as units,
			avg(oi.Units) as [avg],
			count(oi.Units) as [count],
			max(oi.Units) as [max],
			min(oi.Units) as [min]
		from (
			select CONVERT(date, oo.OrderDate) as OrderDate, oi.ProductId, sum(oi.Units) as units
			from [ordering].[orderItems] oi
			inner join [ordering].[orders] oo on oi.OrderId = oo.Id
		    {(string.IsNullOrEmpty(productId) ? string.Empty : "where oi.ProductId = @productId")} 
			group by CONVERT(date, oo.OrderDate), oi.ProductId) as oi 
			group by oi.ProductId, YEAR(CAST(oi.OrderDate as datetime)), MONTH(CAST(oi.OrderDate as datetime))
		) as p2
		Group By p2.productId
		Having count(*) >= 12
) as p2

on p1.productId = p2.productId";

            return sqlCommandText;
        }
    }
}
