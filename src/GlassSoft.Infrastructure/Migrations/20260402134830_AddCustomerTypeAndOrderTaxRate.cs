using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerTypeAndOrderTaxRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxRate",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GlassPlateDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "GlassPlateDefinitions");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Customers");
        }
    }
}
