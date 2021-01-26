using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddBlockHeightAndTransactionIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockHeight",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator3",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator1",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockHeight",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator3");

            migrationBuilder.DropColumn(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator2");

            migrationBuilder.DropColumn(
                name: "HashedTransactionIds",
                table: "BlockchainsValidator1");
        }
    }
}
