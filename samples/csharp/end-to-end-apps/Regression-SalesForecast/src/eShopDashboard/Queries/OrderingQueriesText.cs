using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Queries
{
    public static class OrderingQueriesText
    {
        public static string ProductHistory(string productId)
        {
            var sqlCommandText = $@"
select p.productId, p.[year], p.[month], p.units, p.[avg], p.[count], p.[max], p.[min],
    LAG (units, 1) OVER (PARTITION BY p.productId ORDER BY p.productId, p.date) as prev,
    LEAD (units, 1) OVER (PARTITION BY p.productId ORDER BY p.productId, p.date) as [next]
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
    ) as p";

            return sqlCommandText;
        }

        public static string CountryHistory(string country)
        {
            var sqlCommandText = $@"
select 
    LEAD (log10(sum(R.sale)), 1) OVER (PARTITION BY R.country ORDER BY R.[year], R.[month]) as [next],
	R.country, R.year, R.month, sum(R.sale) as sales, count(R.sale) as count, 
    max(R.p_max) as [max], min(R.p_med) as [med], min(R.p_min) as [min], stdevp(R.sale) as std,
    LAG (sum(R.sale), 1) OVER (PARTITION BY R.country ORDER BY R.[year], R.[month]) as prev
from (
    select S.country, S.[month], S.[year], S.sale,
        PERCENTILE_CONT(0.20) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_min,
        PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_med,
        PERCENTILE_CONT(0.80) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_max
        from 
        (select min(T.country) as country, min(T.year) as [year], min(T.month) as [month], sum(T.sale) as sale
            from (
            select oo.Address_Country as country, oo.Id as id, YEAR(oo.OrderDate) as [year], MONTH(oo.OrderDate) as [month], oi.UnitPrice * oi.Units as sale
            from [ordering].[orderItems] oi
            inner join [ordering].[orders] oo on oi.OrderId = oo.Id {(string.IsNullOrEmpty(country) ? string.Empty : "and oo.Address_Country = (@country)")}
        ) as T
            group by T.id
        ) as S
    ) as R
group by R.country, R.year, R.month
order by R.country, R.year, R.month";

            return sqlCommandText;
        }
    }
}
