using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Comm.Migrations
{
    /// <inheritdoc />
    public partial class FinalInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropUniqueConstraint(
            //     name: "AK_User_UserName",
            //     table: "User");

            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_User",
            //     table: "User");

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_User",
            //     table: "User",
            //     column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_User_UserName",
                table: "User",
                column: "UserName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "UserID");
        }
    }
}
