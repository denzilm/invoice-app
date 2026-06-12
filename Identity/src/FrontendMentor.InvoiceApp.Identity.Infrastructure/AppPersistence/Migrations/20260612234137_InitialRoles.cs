using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: ["Id", "Name", "Description", "StatusId"],
                new object[,]
                {
                    {"EFA2B0E7-D457-4A93-BDA6-655864906C49", "SuperUser", "Gives you access to all system functionality", RoleStatusEnum.Active.Value},
                    {"E8017381-F684-402E-8FA3-101F92ED2D8D", "CompanyAdmin", "Gives you the ability to manage the company and members of the company", RoleStatusEnum.Active.Value},
                    {"E200A373-2F81-4A76-B615-BC6A20B15F9C", "InvoiceCapturer", "Gives you the ability to capture invoices for allocated companies", RoleStatusEnum.Active.Value},
                    {"4313C39A-5B8E-41C3-859F-95FB1F535FDB", "ElevatedInvoiceCapturer", "Gives you all InvoiceCapturer permissions with special permissions", RoleStatusEnum.Active.Value},
                    {"3ACCF9C1-7779-431B-8EB3-733CFE449FD2", "RegularUser", "Gives you limited permissions to the allocated company", RoleStatusEnum.Active.Value}
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[Roles]");
        }
    }
}
