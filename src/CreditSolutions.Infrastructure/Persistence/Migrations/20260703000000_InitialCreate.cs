using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditSolutions.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                MobileNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Customers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AuditLog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AuditLog", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey("FK_Payments_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Promises",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CapturedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                PromiseDate = table.Column<DateOnly>(type: "date", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                KeptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                BrokenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Promises", x => x.Id);
                table.ForeignKey("FK_Promises_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ReminderQueue",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PromiseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MobileNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Outcome = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ReminderQueue", x => x.Id));

        migrationBuilder.InsertData("Users", new[] { "Id", "DisplayName", "Email", "CreatedAt" }, new object[] { Guid.Parse("10000000-0000-0000-0000-000000000001"), "Demo Collector", "collector@css.local", DateTimeOffset.UtcNow });
        migrationBuilder.InsertData("Customers", new[] { "Id", "AccountNumber", "FullName", "MobileNumber", "Balance", "CreatedAt" }, new object[] { Guid.Parse("20000000-0000-0000-0000-000000000001"), "ACC-1001", "Nandi Dlamini", "+27821234567", 1500m, DateTimeOffset.UtcNow });
        migrationBuilder.InsertData("Customers", new[] { "Id", "AccountNumber", "FullName", "MobileNumber", "Balance", "CreatedAt" }, new object[] { Guid.Parse("20000000-0000-0000-0000-000000000002"), "ACC-1002", "Johan Botha", "+27829876543", 950m, DateTimeOffset.UtcNow });

        migrationBuilder.CreateIndex("IX_Customers_AccountNumber", "Customers", "AccountNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_AuditLog_EntityName_EntityId", "AuditLog", new[] { "EntityName", "EntityId" });
        migrationBuilder.CreateIndex("IX_Payments_CustomerId", "Payments", "CustomerId");
        migrationBuilder.CreateIndex("IX_Payments_Reference", "Payments", "Reference");
        migrationBuilder.CreateIndex("IX_Promises_CustomerId", "Promises", "CustomerId");
        migrationBuilder.CreateIndex("IX_Promises_Status_PromiseDate", "Promises", new[] { "Status", "PromiseDate" });
        migrationBuilder.CreateIndex("IX_ReminderQueue_Status_CreatedAt", "ReminderQueue", new[] { "Status", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AuditLog");
        migrationBuilder.DropTable("Payments");
        migrationBuilder.DropTable("Promises");
        migrationBuilder.DropTable("ReminderQueue");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("Customers");
    }
}
