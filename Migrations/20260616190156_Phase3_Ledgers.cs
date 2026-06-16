using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexCms.InvestPro.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_Ledgers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "investpro_capital_contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investpro_capital_contributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_investpro_capital_contributions_investpro_investments_Inves~",
                        column: x => x.InvestmentId,
                        principalTable: "investpro_investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investpro_capital_contributions_investpro_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "investpro_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "investpro_expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidTo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReceiptNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investpro_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_investpro_expenses_investpro_expense_categories_ExpenseCate~",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "investpro_expense_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_investpro_expenses_investpro_investments_InvestmentId",
                        column: x => x.InvestmentId,
                        principalTable: "investpro_investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investpro_expenses_investpro_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "investpro_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "investpro_labor_contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HoursOrDays = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    RatePerUnit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investpro_labor_contributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_investpro_labor_contributions_investpro_investments_Investm~",
                        column: x => x.InvestmentId,
                        principalTable: "investpro_investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investpro_labor_contributions_investpro_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "investpro_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "investpro_ledger_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LedgerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LedgerEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    AttachmentLabel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investpro_ledger_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "investpro_revenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Customer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SalesChannel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investpro_revenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_investpro_revenues_investpro_investments_InvestmentId",
                        column: x => x.InvestmentId,
                        principalTable: "investpro_investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investpro_revenues_investpro_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "investpro_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_investpro_capital_contributions_InvestmentId",
                table: "investpro_capital_contributions",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_capital_contributions_PartnerId",
                table: "investpro_capital_contributions",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_capital_contributions_TransactionDate",
                table: "investpro_capital_contributions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_expenses_ExpenseCategoryId",
                table: "investpro_expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_expenses_InvestmentId",
                table: "investpro_expenses",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_expenses_PartnerId",
                table: "investpro_expenses",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_expenses_TransactionDate",
                table: "investpro_expenses",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_labor_contributions_InvestmentId",
                table: "investpro_labor_contributions",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_labor_contributions_PartnerId",
                table: "investpro_labor_contributions",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_labor_contributions_TransactionDate",
                table: "investpro_labor_contributions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_ledger_attachments_LedgerType_LedgerEntryId",
                table: "investpro_ledger_attachments",
                columns: new[] { "LedgerType", "LedgerEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_investpro_revenues_InvestmentId",
                table: "investpro_revenues",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_revenues_PartnerId",
                table: "investpro_revenues",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_investpro_revenues_TransactionDate",
                table: "investpro_revenues",
                column: "TransactionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "investpro_capital_contributions");

            migrationBuilder.DropTable(
                name: "investpro_expenses");

            migrationBuilder.DropTable(
                name: "investpro_labor_contributions");

            migrationBuilder.DropTable(
                name: "investpro_ledger_attachments");

            migrationBuilder.DropTable(
                name: "investpro_revenues");
        }
    }
}
