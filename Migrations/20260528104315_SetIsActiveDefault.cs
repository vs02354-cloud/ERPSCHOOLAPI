using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class SetIsActiveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Students SET IsActive = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
