using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultForGlobalRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsGlobal",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePermissions_UserId",
                table: "UserRolePermissions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRolePermissions_Users_UserId",
                table: "UserRolePermissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRolePermissions_Users_UserId",
                table: "UserRolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserRolePermissions_UserId",
                table: "UserRolePermissions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsGlobal",
                table: "Roles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
