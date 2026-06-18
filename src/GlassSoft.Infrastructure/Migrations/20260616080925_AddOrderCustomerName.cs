using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderCustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCustomerName",
                table: "Orders");
        }
    }
}
