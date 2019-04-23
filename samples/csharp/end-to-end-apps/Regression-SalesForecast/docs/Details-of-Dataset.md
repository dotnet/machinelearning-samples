Here's a detailed info about the dataset:

# Base Dataset 
eShopDashboardML dataset is based on a public Online Retail Dataset from UCI: 
http://archive.ics.uci.edu/ml/datasets/online+retail 

> Daqing Chen, Sai Liang Sain, and Kun Guo, Data mining for the online retail industry: A case study of RFM model-based customer segmentation using data mining, Journal of Database Marketing and Customer Strategy Management, Vol. 19, No. 3, pp. 197 - 208, 2012 (Published online before print: 27 August 2012. doi: 10.1057/dbm.2012.17). 

This dataset has several attributes: 

- nvoiceNo: Invoice number. Nominal, a 6-digit integral number uniquely assigned to each transaction. If this code starts with letter 'c', it indicates a cancellation. 
- StockCode: Product (item) code. Nominal, a 5-digit integral number uniquely assigned to each distinct product. 
- Description: Product (item) name. Nominal. 
- Quantity: The quantities of each product (item) per transaction. Numeric. 
- InvoiceDate: Invice Date and time. Numeric, the day and time when each transaction was generated.  
- UnitPrice: Unit price. Numeric, Product price per unit in sterling.  
- CustomerID: Customer number. Nominal, a 5-digit integral number uniquely assigned to each customer.  
- Country: Country name. Nominal, the name of the country where each customer resides. 


# eShopDashboardML datasets: 
Based on previous dataset we generated two new transformed and simpler datasets. 

## countries.stats 

- next : next month units sold 
- prev : previous month units sold 
- max : max items sold in a day of month 
- min : min items sold in a day of month 
- med : average of items sold per day  
- … 

## products.stats 

- next : next month units sold 
- prev : previous month units sold 
- max : max items sold in a day of month 
- min : min items sold in a day of month 
- avg : average of items sold per day 
- …
