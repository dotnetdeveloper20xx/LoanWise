using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanWise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BorrowerRiskSnapshots",
                columns: table => new
                {
                    BorrowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    RiskTier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    KycStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FlagsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LastVerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastScoreAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowerRiskSnapshots", x => x.BorrowerId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: true),
                    Tier = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    CalculatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_CreditProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    RejectedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisbursedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsVisibleToLenders = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Users_BorrowerId",
                        column: x => x.BorrowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VerificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BlobReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationDocuments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscrowTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fundings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FundedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fundings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fundings_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fundings_Users_LenderId",
                        column: x => x.LenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fundings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Repayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OverdueNotifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repayments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_BorrowerRiskSnapshots_KycStatus",
                table: "BorrowerRiskSnapshots",
                column: "KycStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowerRiskSnapshots_LastScoreAtUtc",
                table: "BorrowerRiskSnapshots",
                column: "LastScoreAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowerRiskSnapshots_LastVerifiedAtUtc",
                table: "BorrowerRiskSnapshots",
                column: "LastVerifiedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_LoanId",
                table: "EscrowTransactions",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Fundings_LenderId",
                table: "Fundings",
                column: "LenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Fundings_LoanId",
                table: "Fundings",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Fundings_UserId",
                table: "Fundings",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BorrowerId",
                table: "Loans",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_TokenHash",
                table: "RefreshTokens",
                columns: new[] { "UserId", "TokenHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repayments_LoanId",
                table: "Repayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemEvents_UserId",
                table: "SystemEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationDocuments_UserId",
                table: "VerificationDocuments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowerRiskSnapshots");

            migrationBuilder.DropTable(
                name: "CreditProfiles");

            migrationBuilder.DropTable(
                name: "EscrowTransactions");

            migrationBuilder.DropTable(
                name: "Fundings");

            migrationBuilder.DropTable(
                name: "LenderRepayments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SystemEvents");

            migrationBuilder.DropTable(
                name: "VerificationDocuments");

            migrationBuilder.DropTable(
                name: "Repayments");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
