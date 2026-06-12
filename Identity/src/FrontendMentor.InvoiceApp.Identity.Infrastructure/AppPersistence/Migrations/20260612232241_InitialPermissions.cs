using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Id", "Name", "Description", "StatusId"],
                values: new object[,]
                {
                    { "DF4FA786-2DE7-488D-B75D-42011EE086BF", "company:create", "Can create new companies", PermissionStatusEnum.Active.Value },
                    { "3BED2BE8-45AF-4D43-8436-B1722E33C452", "company:read", "Can view company information", PermissionStatusEnum.Active.Value },
                    { "ADCD6F20-DA98-4C0C-B8FA-0FB1C274A545", "company:update", "Can update company information", PermissionStatusEnum.Active.Value },
                    { "5EC5FC3A-E3F0-44D2-81FE-4F5CA1167CEA", "company:delete", "Can delete a company", PermissionStatusEnum.Active.Value },
                    { "6099BDCE-5411-48C6-A93B-70FCF08B42FC", "user:create", "Can create new users", PermissionStatusEnum.Active.Value },
                    { "8FC58092-5A66-469C-B8C4-8EEFB8B8F6D4", "user:read", "Can view information about a user", PermissionStatusEnum.Active.Value },
                    { "BD82F43D-25AD-4987-8BE9-9C91113E463E", "user:update", "Can update a user's information", PermissionStatusEnum.Active.Value },
                    { "5CDE8B0D-1B6F-4E88-A3AD-120B18732950", "user:delete", "Can delete a user", PermissionStatusEnum.Active.Value },
                    { "8DA8A9A6-E3D8-4A4A-B445-99C70172CFCD", "user:transfer", "Can transfer a user to another company", PermissionStatusEnum.Active.Value },
                    { "FF29C36B-89E4-49AC-885A-E5147C447F78", "user:assign_company", "Can assign additional companies to the user", PermissionStatusEnum.Active.Value },
                    { "162C0CB3-85D8-4D52-BCCB-1D87B0B9A863", "invoice:create", "Can create invoices", PermissionStatusEnum.Active.Value },
                    { "A2CD3B2A-C486-44FE-B07B-2183E26A068D", "invoice:read", "Can view invoices", PermissionStatusEnum.Active.Value },
                    { "A63DCBD7-BD17-495A-B6D0-BCC69EB21539", "invoice:update:own", "Can update invoices created by self", PermissionStatusEnum.Active.Value },
                    { "8E00C42C-50BF-4147-BDB6-026A3552B8C6", "invoice:update:all", "Can update any invoice", PermissionStatusEnum.Active.Value },
                    { "30C65AAD-E4F8-47A6-A7D1-938C4BA5902C", "invoice:delete:own", "Can delete invoice created by self", PermissionStatusEnum.Active.Value },
                    { "31F0673F-D0E9-4471-AB7A-51EDF3B91FBD", "invoice:delete:all", "Can delete any invoice", PermissionStatusEnum.Active.Value },
                    { "70E4401B-482F-409E-98F8-58959CC48FA3", "invoice:download", "Can download invoices", PermissionStatusEnum.Active.Value },
                    { "82A6E341-D63B-48AD-A6D1-944407279564", "company:manage_users", "Can manage users within the company", PermissionStatusEnum.Active.Value },
                    { "9E4B2676-191B-4DF1-8ED7-09DB942202FD", "company:update_info", "Can update the company's information", PermissionStatusEnum.Active.Value }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[Permissions]");
        }
    }
}
