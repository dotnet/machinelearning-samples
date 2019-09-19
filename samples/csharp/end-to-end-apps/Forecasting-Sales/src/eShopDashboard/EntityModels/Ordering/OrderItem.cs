using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.ComponentModel.DataAnnotations;

namespace eShopDashboard.EntityModels.Ordering
{
    public class OrderItem
    {
     //   [Key]
        public int Id { get; set; }

        public Order Order { get; set; }

      //  [Key]
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public int Units { get; set; }
    }
}