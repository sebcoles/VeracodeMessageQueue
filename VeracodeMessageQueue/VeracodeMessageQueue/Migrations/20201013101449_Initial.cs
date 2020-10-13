using Microsoft.EntityFrameworkCore.Migrations;

namespace VeracodeMessageQueue.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apps",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Builds",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AppId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Builds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flaws",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AppId = table.Column<string>(nullable: true),
                    RemediationStatus = table.Column<string>(nullable: true),
                    MitigationStatus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flaws", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apps");

            migrationBuilder.DropTable(
                name: "Builds");

            migrationBuilder.DropTable(
                name: "Flaws");
        }
    }
}
