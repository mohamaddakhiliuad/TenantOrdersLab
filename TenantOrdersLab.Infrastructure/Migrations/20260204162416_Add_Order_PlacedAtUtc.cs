using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantOrdersLab.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Order_PlacedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlacedAtUtc",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlacedAtUtc",
                table: "Orders");
        }
    }
}
