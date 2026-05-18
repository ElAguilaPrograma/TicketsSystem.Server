using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketsSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeMCPTicketIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MCPRequest_Ticket",
                table: "MCPRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "MCPRequests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MCPRequest_Ticket",
                table: "MCPRequests",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "TicketId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MCPRequest_Ticket",
                table: "MCPRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "MCPRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MCPRequest_Ticket",
                table: "MCPRequests",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "TicketId");
        }
    }
}
