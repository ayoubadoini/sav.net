using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsAndEnforceCleanArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add audit fields to Complaints table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Complaints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Complaints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Complaints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Complaints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Complaints",
                type: "datetime2",
                nullable: true);

            // Rename CreatedAt to CreatedDate in Complaints
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Complaints",
                newName: "CreatedDate");

            // Add audit fields to TechnicalInterventions table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "TechnicalInterventions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "TechnicalInterventions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "TechnicalInterventions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TechnicalInterventions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "TechnicalInterventions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TechnicalInterventions",
                newName: "CreatedDate");

            // Add audit fields to Invoices table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            // Add audit fields to Clients table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Clients",
                newName: "CreatedDate");

            // Add audit fields to Technicians table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Technicians",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Technicians",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Technicians",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Technicians",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Technicians",
                type: "datetime2",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Technicians",
                newName: "CreatedDate");

            // Add audit fields to Categories table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            // Add audit fields to Articles table
            migrationBuilder.AddColumn<int?>(
                name: "CreatedBy",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int?>(
                name: "UpdatedBy",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Articles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Articles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Articles",
                newName: "CreatedDate");

            // Create indexes for audit fields (for soft-delete queries)
            migrationBuilder.CreateIndex(
                name: "IX_Complaints_IsDeleted",
                table: "Complaints",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CreatedBy",
                table: "Complaints",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalInterventions_IsDeleted",
                table: "TechnicalInterventions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IsDeleted",
                table: "Invoices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsDeleted",
                table: "Clients",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_IsDeleted",
                table: "Technicians",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsDeleted",
                table: "Articles",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Complaints_IsDeleted",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CreatedBy",
                table: "Complaints");

            // Revert changes (reverse order)
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Complaints");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Complaints",
                newName: "CreatedAt");

            // ... (similar for other tables)
        }
    }
}
