using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace eShopDashboard.Infrastructure.Migrations.Ordering
{
    public partial class AddDescriptionsMigration_OrderingContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address_Country",
                schema: "Ordering",
                table: "Orders",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Ordering",
                table: "Orders",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                schema: "Ordering",
                table: "OrderItems",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                "update oi set ProductName = ci.Name from Ordering.OrderItems oi join Catalog.CatalogItems ci on ci.Id = oi.ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Ordering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductName",
                schema: "Ordering",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Address_Country",
                schema: "Ordering",
                table: "Orders",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
