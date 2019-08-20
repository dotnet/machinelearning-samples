using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace eShopDashboard.Infrastructure.Migrations.Catalog
{
    public partial class InitialMigration_CatalogContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catalog");

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    AvailableStock = table.Column<int>(nullable: false),
                    CatalogBrandId = table.Column<int>(nullable: false),
                    CatalogTypeId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    MaxStockThreshold = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    OnReorder = table.Column<bool>(nullable: false),
                    PictureFileName = table.Column<string>(nullable: true),
                    PictureUri = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    RestockThreshold = table.Column<int>(nullable: false),
                    TagsJson = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems",
                schema: "Catalog");
        }
    }
}
