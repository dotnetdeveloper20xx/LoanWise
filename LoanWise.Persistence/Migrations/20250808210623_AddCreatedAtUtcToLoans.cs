using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanWise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtUtcToLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Loans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DisbursedAtUtc",
                table: "Loans",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "DisbursedAtUtc",
                table: "Loans");
        }
    }
}
