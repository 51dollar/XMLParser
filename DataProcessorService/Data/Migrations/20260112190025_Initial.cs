using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataProcessorService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleCategoryId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ModuleState = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ModuleCategoryId",
                table: "Modules",
                column: "ModuleCategoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Modules");
        }
    }
}
