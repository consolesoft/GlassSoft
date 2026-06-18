using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductItemFromGlassPlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlassPlateDefinitions_ProductItems_ProductItemId",
                table: "GlassPlateDefinitions");

            migrationBuilder.AlterColumn<int>(
                name: "ProductItemId",
                table: "GlassPlateDefinitions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GlassPlateDefinitions_ProductItems_ProductItemId",
                table: "GlassPlateDefinitions",
                column: "ProductItemId",
                principalTable: "ProductItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlassPlateDefinitions_ProductItems_ProductItemId",
                table: "GlassPlateDefinitions");

            migrationBuilder.AlterColumn<int>(
                name: "ProductItemId",
                table: "GlassPlateDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GlassPlateDefinitions_ProductItems_ProductItemId",
                table: "GlassPlateDefinitions",
                column: "ProductItemId",
                principalTable: "ProductItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
