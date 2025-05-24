using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Comm.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    genreID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.genreID);
                });

            migrationBuilder.CreateTable(
                name: "TO",
                columns: table => new
                {
                    customerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatronId = table.Column<int>(type: "INTEGER", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PostCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Suburb = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CardOwner = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Expiry = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    CVV = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TO", x => x.customerID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    isAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    HashPW = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Genre = table.Column<int>(type: "INTEGER", nullable: true),
                    SubGenre = table.Column<int>(type: "INTEGER", nullable: true),
                    Published = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Product_Genre_Genre",
                        column: x => x.Genre,
                        principalTable: "Genre",
                        principalColumn: "genreID");
                });

            migrationBuilder.CreateTable(
                name: "Source",
                columns: table => new
                {
                    sourceid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source_name = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalLink = table.Column<string>(type: "TEXT", nullable: true),
                    Genre = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Source", x => x.sourceid);
                    table.ForeignKey(
                        name: "FK_Source_Genre_Genre",
                        column: x => x.Genre,
                        principalTable: "Genre",
                        principalColumn: "genreID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    customer = table.Column<int>(type: "INTEGER", nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PostCode = table.Column<int>(type: "INTEGER", nullable: true),
                    Suburb = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_TO_customer",
                        column: x => x.customer,
                        principalTable: "TO",
                        principalColumn: "customerID");
                });

            migrationBuilder.CreateTable(
                name: "Stocktake",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true),
                    Price = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocktake", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Stocktake_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Stocktake_Source_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Source",
                        principalColumn: "sourceid");
                });

            migrationBuilder.CreateTable(
                name: "ProductsInOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    produktId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsInOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsInOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_ProductsInOrders_Stocktake_produktId",
                        column: x => x.produktId,
                        principalTable: "Stocktake",
                        principalColumn: "ItemId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_customer",
                table: "Orders",
                column: "customer");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Genre",
                table: "Product",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsInOrders_OrderId",
                table: "ProductsInOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsInOrders_produktId",
                table: "ProductsInOrders",
                column: "produktId");

            migrationBuilder.CreateIndex(
                name: "IX_Source_Genre",
                table: "Source",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_Stocktake_ProductId",
                table: "Stocktake",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocktake_SourceId",
                table: "Stocktake",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductsInOrders");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Stocktake");

            migrationBuilder.DropTable(
                name: "TO");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Source");

            migrationBuilder.DropTable(
                name: "Genre");
        }
    }
}
