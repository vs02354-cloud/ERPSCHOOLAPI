using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentPromotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FromClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromotionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PromotedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPromotions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotions_StudentId",
                table: "StudentPromotions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentPromotions");
        }
    }
}
