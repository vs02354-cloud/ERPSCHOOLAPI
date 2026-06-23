using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPrincipalMessageToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrincipalCredentials",
                table: "HomePageSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrincipalImageUrl",
                table: "HomePageSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrincipalMessage",
                table: "HomePageSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrincipalName",
                table: "HomePageSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrincipalTitle",
                table: "HomePageSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrincipalCredentials",
                table: "HomePageSettings");

            migrationBuilder.DropColumn(
                name: "PrincipalImageUrl",
                table: "HomePageSettings");

            migrationBuilder.DropColumn(
                name: "PrincipalMessage",
                table: "HomePageSettings");

            migrationBuilder.DropColumn(
                name: "PrincipalName",
                table: "HomePageSettings");

            migrationBuilder.DropColumn(
                name: "PrincipalTitle",
                table: "HomePageSettings");
        }
    }
}
