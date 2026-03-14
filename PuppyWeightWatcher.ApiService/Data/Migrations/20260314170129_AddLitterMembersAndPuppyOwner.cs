using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuppyWeightWatcher.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLitterMembersAndPuppyOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Puppies",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LitterMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LitterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LitterMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LitterMembers_Litters_LitterId",
                        column: x => x.LitterId,
                        principalTable: "Litters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puppies_OwnerId",
                table: "Puppies",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LitterMembers_LitterId_UserId",
                table: "LitterMembers",
                columns: new[] { "LitterId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LitterMembers_UserId",
                table: "LitterMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LitterMembers");

            migrationBuilder.DropIndex(
                name: "IX_Puppies_OwnerId",
                table: "Puppies");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Puppies");
        }
    }
}
