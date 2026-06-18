using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrderSupplierNullableCustomerRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut PO'lar için: SupplierId'ye göre eşleşen Customer yoksa, soft-delete yap
            migrationBuilder.Sql(@"
                DELETE FROM PurchaseOrderLines WHERE PurchaseOrderId IN (SELECT Id FROM PurchaseOrders WHERE CustomerId IS NULL);
                DELETE FROM PurchaseOrders WHERE CustomerId IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "PurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "PurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
