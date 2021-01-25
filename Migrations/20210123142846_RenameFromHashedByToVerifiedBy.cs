using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class RenameFromHashedByToVerifiedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashedBy",
                table: "BlockchainsValidator3");

            migrationBuilder.DropColumn(
                name: "HashedBy",
                table: "BlockchainsValidator2");

            migrationBuilder.DropColumn(
                name: "HashedBy",
                table: "BlockchainsValidator1");

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "BlockchainsValidator3",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "BlockchainsValidator2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "BlockchainsValidator1",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "BlockchainsValidator3");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "BlockchainsValidator2");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "BlockchainsValidator1");

            migrationBuilder.AddColumn<string>(
                name: "HashedBy",
                table: "BlockchainsValidator3",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashedBy",
                table: "BlockchainsValidator2",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashedBy",
                table: "BlockchainsValidator1",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
