using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddRequiredAmount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "Transactions",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsDonated",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "RemainingRequiredAmount",
                table: "Foundations",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDonated",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RemainingRequiredAmount",
                table: "Foundations");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(float));
        }
    }
}
