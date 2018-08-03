using System;

namespace eShopDashboard.EntityModels.Ordering
{
    public class Order
    {
        public string Address_Country { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public DateTime OrderDate { get; set; }
    }
}