using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bio",
                table: "user_profile");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "user_profile");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "user_profile");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "user_profile",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "user_profile",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "user_profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_birth",
                table: "user_profile",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(
                    1,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "user_profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}