using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettingsAndOrderFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineFeatures_ProductFeatureOptions_ProductFeatureOptionId",
                table: "OrderLineFeatures");

            migrationBuilder.DropIndex(
                name: "IX_OrderLineFeatures_ProductFeatureOptionId",
                table: "OrderLineFeatures");

            migrationBuilder.RenameColumn(
                name: "ProductFeatureOptionId",
                table: "OrderLineFeatures",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "PriceEffect",
                table: "OrderLineFeatures",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<string>(
                name: "PozNo",
                table: "OrderLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeatureDefinitionId",
                table: "OrderLineFeatures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderFeatureDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFeatureDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineFeatures_FeatureDefinitionId",
                table: "OrderLineFeatures",
                column: "FeatureDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineFeatures_OrderFeatureDefinitions_FeatureDefinitionId",
                table: "OrderLineFeatures",
                column: "FeatureDefinitionId",
                principalTable: "OrderFeatureDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineFeatures_OrderFeatureDefinitions_FeatureDefinitionId",
                table: "OrderLineFeatures");

            migrationBuilder.DropTable(
                name: "OrderFeatureDefinitions");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_OrderLineFeatures_FeatureDefinitionId",
                table: "OrderLineFeatures");

            migrationBuilder.DropColumn(
                name: "PozNo",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "FeatureDefinitionId",
                table: "OrderLineFeatures");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderLineFeatures",
                newName: "PriceEffect");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderLineFeatures",
                newName: "ProductFeatureOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineFeatures_ProductFeatureOptionId",
                table: "OrderLineFeatures",
                column: "ProductFeatureOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineFeatures_ProductFeatureOptions_ProductFeatureOptionId",
                table: "OrderLineFeatures",
                column: "ProductFeatureOptionId",
                principalTable: "ProductFeatureOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
