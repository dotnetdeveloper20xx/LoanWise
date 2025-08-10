using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanWise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLenderRepayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepaymentAmountCurrency",
                table: "Repayments");

            migrationBuilder.RenameColumn(
                name: "RepaymentAmountValue",
                table: "Repayments",
                newName: "Amount");

            migrationBuilder.CreateTable(
                name: "LenderRepayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LenderRepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LenderRepayments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LenderRepayments_Repayments_RepaymentId",
                        column: x => x.RepaymentId,
                        principalTable: "Repayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LenderRepayments_Users_LenderId",
                        column: x => x.LenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LenderRepayments_LenderId_LoanId",
                table: "LenderRepayments",
                columns: new[] { "LenderId", "LoanId" });

            migrationBuilder.CreateIndex(
                name: "IX_LenderRepayments_LoanId",
                table: "LenderRepayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LenderRepayments_RepaymentId",
                table: "LenderRepayments",
                column: "RepaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LenderRepayments");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Repayments",
                newName: "RepaymentAmountValue");

            migrationBuilder.AddColumn<string>(
                name: "RepaymentAmountCurrency",
                table: "Repayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "GBP");
        }
    }
}
