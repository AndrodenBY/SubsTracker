using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubsTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UserRelatedModels_Modification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "Subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserGroupId",
                table: "Subscriptions",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_UserGroups_UserGroupId",
                table: "Subscriptions",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_UserGroups_UserGroupId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserGroupId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
