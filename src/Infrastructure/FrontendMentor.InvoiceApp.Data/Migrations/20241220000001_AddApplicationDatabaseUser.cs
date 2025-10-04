using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrontendMentor.InvoiceApp.Data.Migrations;

/// <inheritdoc />
public partial class AddApplicationDatabaseUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create the application database user
        migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'invoice-app-user')
            BEGIN
                CREATE USER [invoice-app-user] FOR LOGIN [invoice-app-user] WITH DEFAULT_SCHEMA=[dbo];
                
                -- Grant CRUD permissions to the user
                ALTER ROLE [db_datareader] ADD MEMBER [invoice-app-user];
                ALTER ROLE [db_datawriter] ADD MEMBER [invoice-app-user];
                
                -- Grant execute permission on stored procedures if needed
                GRANT EXECUTE TO [invoice-app-user];
            END
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove the application database user
        migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'invoice-app-user')
            BEGIN
                -- Remove user from roles first
                ALTER ROLE db_datareader DROP MEMBER [invoice-app-user];
                ALTER ROLE db_datawriter DROP MEMBER [invoice-app-user];
                
                -- Drop the user
                DROP USER [invoice-app-user];
            END
        ");
    }
}
