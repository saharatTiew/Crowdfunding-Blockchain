using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddTotalUnDonate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAmount",
                table: "Foundations");

            migrationBuilder.DropColumn(
                name: "RemainingRequiredAmount",
                table: "Foundations");

            migrationBuilder.AddColumn<float>(
                name: "TotalDonate",
                table: "Foundations",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalUnDonate",
                table: "Foundations",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDonate",
                table: "Foundations");

            migrationBuilder.DropColumn(
                name: "TotalUnDonate",
                table: "Foundations");

            migrationBuilder.AddColumn<float>(
                name: "CurrentAmount",
                table: "Foundations",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "RemainingRequiredAmount",
                table: "Foundations",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
