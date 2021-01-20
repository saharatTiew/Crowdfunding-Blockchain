using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddOrganizationAndTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Foundations",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(nullable: true),
                    Deadline = table.Column<DateTime>(nullable: true),
                    DonateGoal = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foundations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAddress = table.Column<string>(nullable: true),
                    ToAddress = table.Column<string>(nullable: true),
                    IsCommitted = table.Column<bool>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foundations");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
