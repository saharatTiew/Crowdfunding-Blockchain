using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddNewColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "RemainingMoney",
                table: "Users",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "HashedTransactionId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnixTimeStamp",
                table: "Transactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "CurrentAmount",
                table: "Foundations",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingMoney",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HashedTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UnixTimeStamp",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CurrentAmount",
                table: "Foundations");
        }
    }
}
