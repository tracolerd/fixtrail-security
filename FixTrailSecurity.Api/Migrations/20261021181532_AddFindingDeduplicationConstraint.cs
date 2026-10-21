using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixTrailSecurity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFindingDeduplicationConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Findings_AssetId",
                table: "Findings");

            migrationBuilder.CreateIndex(
                name: "IX_Findings_AssetId_Title_Source",
                table: "Findings",
                columns: new[] { "AssetId", "Title", "Source" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Findings_AssetId_Title_Source",
                table: "Findings");

            migrationBuilder.CreateIndex(
                name: "IX_Findings_AssetId",
                table: "Findings",
                column: "AssetId");
        }
    }
}
