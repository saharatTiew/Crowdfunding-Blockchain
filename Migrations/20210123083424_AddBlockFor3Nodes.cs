using Microsoft.EntityFrameworkCore.Migrations;

namespace blockchain_project.Migrations
{
    public partial class AddBlockFor3Nodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashedBy",
                table: "BlockchainsValidator1",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlockchainsValidator2",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Height = table.Column<int>(nullable: false),
                    UnixTimeStamp = table.Column<int>(nullable: false),
                    BlockSize = table.Column<int>(nullable: false),
                    PreviousHash = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    HashedBy = table.Column<string>(nullable: true),
                    TransactionJsons = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainsValidator2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainsValidator3",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Height = table.Column<int>(nullable: false),
                    UnixTimeStamp = table.Column<int>(nullable: false),
                    BlockSize = table.Column<int>(nullable: false),
                    PreviousHash = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    HashedBy = table.Column<string>(nullable: true),
                    TransactionJsons = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainsValidator3", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainsValidator2");

            migrationBuilder.DropTable(
                name: "BlockchainsValidator3");

            migrationBuilder.DropColumn(
                name: "HashedBy",
                table: "BlockchainsValidator1");
        }
    }
}
