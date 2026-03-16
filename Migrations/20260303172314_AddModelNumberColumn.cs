using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectricalPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddModelNumberColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add ModelNumber to Products - other columns already exist in the database
            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Note: Sales table columns (CurrentBalance, CustomerName, CustomerPhone, InvoiceNumber, Notes, PreviousBalance)
            // and Invoices/InvoiceItems tables already exist in the database
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only remove ModelNumber from Products
            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "Products");
        }
    }
}
