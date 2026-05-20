using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArticleProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Clients_ClientId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Articles_ArticleId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ClientId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "Articles");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Articles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Articles_ArticleId",
                table: "Complaints",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Articles_ArticleId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Articles");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ClientId",
                table: "Articles",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Clients_ClientId",
                table: "Articles",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Articles_ArticleId",
                table: "Complaints",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
