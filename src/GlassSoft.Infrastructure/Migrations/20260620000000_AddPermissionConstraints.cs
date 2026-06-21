using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using GlassSoft.Infrastructure.Data;

#nullable disable

namespace GlassSoft.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620000000_AddPermissionConstraints")]
public partial class AddPermissionConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(name: "Module", table: "Permissions", type: "nvarchar(50)", maxLength: 50, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>(name: "Action", table: "Permissions", type: "nvarchar(30)", maxLength: 30, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>(name: "Description", table: "Permissions", type: "nvarchar(200)", maxLength: 200, nullable: true, oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true);
        migrationBuilder.CreateIndex(name: "IX_Permissions_Module_Action", table: "Permissions", columns: new[] { "Module", "Action" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Permissions_Module_Action", table: "Permissions");
        migrationBuilder.AlterColumn<string>(name: "Module", table: "Permissions", type: "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(50)", oldMaxLength: 50);
        migrationBuilder.AlterColumn<string>(name: "Action", table: "Permissions", type: "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(30)", oldMaxLength: 30);
        migrationBuilder.AlterColumn<string>(name: "Description", table: "Permissions", type: "nvarchar(max)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(200)", oldMaxLength: 200, oldNullable: true);
    }
}
