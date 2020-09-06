using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GCloud.Migrations
{
    public partial class RsaKeyPair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RsaKeyPairs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: true),
                    PrivateKey = table.Column<byte[]>(maxLength: 2048, nullable: true),
                    PublicKey = table.Column<byte[]>(maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RsaKeyPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RsaKeyPairs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RsaKeyPairs_UserId",
                table: "RsaKeyPairs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RsaKeyPairs");
        }
    }
}
