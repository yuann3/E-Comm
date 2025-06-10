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
                name: "Book_genre",
                columns: table => new
                {
                    subGenreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book_genre", x => x.subGenreID);
                });

            migrationBuilder.CreateTable(
                name: "Game_genre",
                columns: table => new
                {
                    subGenreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game_genre", x => x.subGenreID);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    genreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.genreID);
                });

            migrationBuilder.CreateTable(
                name: "Movie_genre",
                columns: table => new
                {
                    subGenreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie_genre", x => x.subGenreID);
                });

            migrationBuilder.CreateTable(
                name: "Patrons",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Salt = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    HashPW = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patrons", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    isAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    HashPW = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "Source",
                columns: table => new
                {
                    sourceid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genre = table.Column<int>(type: "int", nullable: true)
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
                name: "TO",
                columns: table => new
                {
                    customerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatronId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PostCode = table.Column<int>(type: "int", nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardOwner = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Expiry = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CVV = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TO", x => x.customerID);
                    table.ForeignKey(
                        name: "FK_TO_Patrons_PatronId",
                        column: x => x.PatronId,
                        principalTable: "Patrons",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genre = table.Column<int>(type: "int", nullable: true),
                    SubGenre = table.Column<int>(type: "int", nullable: true),
                    Published = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Product_Genre_Genre",
                        column: x => x.Genre,
                        principalTable: "Genre",
                        principalColumn: "genreID");
                    table.ForeignKey(
                        name: "FK_Product_User_LastUpdatedBy",
                        column: x => x.LastUpdatedBy,
                        principalTable: "User",
                        principalColumn: "UserName");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer = table.Column<int>(type: "int", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PostCode = table.Column<int>(type: "int", nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    produktId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_Product_LastUpdatedBy",
                table: "Product",
                column: "LastUpdatedBy");

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

            migrationBuilder.CreateIndex(
                name: "IX_TO_PatronId",
                table: "TO",
                column: "PatronId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Book_genre");

            migrationBuilder.DropTable(
                name: "Game_genre");

            migrationBuilder.DropTable(
                name: "Movie_genre");

            migrationBuilder.DropTable(
                name: "ProductsInOrders");

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
                name: "Patrons");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Genre");
        }
    }
}
