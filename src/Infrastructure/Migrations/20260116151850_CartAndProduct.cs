using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CartAndProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carts",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    transaction_id = table.Column<string>(type: "text", nullable: true),
                    payment_type = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    manufacture_part = table.Column<string>(type: "text", nullable: false),
                    short_desc = table.Column<string>(type: "text", nullable: false),
                    barcode = table.Column<string>(type: "text", nullable: false),
                    dlc = table.Column<bool>(type: "boolean", nullable: false),
                    ul = table.Column<bool>(type: "boolean", nullable: false),
                    etl = table.Column<bool>(type: "boolean", nullable: false),
                    es = table.Column<bool>(type: "boolean", nullable: false),
                    cul = table.Column<bool>(type: "boolean", nullable: false),
                    cetl = table.Column<bool>(type: "boolean", nullable: false),
                    ip = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    spc = table.Column<string>(type: "text", nullable: false),
                    manufactorer_part_number = table.Column<string>(type: "text", nullable: false),
                    income_account_id = table.Column<int>(type: "integer", nullable: true),
                    image = table.Column<string>(type: "text", nullable: false),
                    reorder_point = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    taxable = table.Column<bool>(type: "boolean", nullable: false),
                    salable = table.Column<bool>(type: "boolean", nullable: false),
                    enable = table.Column<bool>(type: "boolean", nullable: false),
                    batch_traceable = table.Column<bool>(type: "boolean", nullable: false),
                    deleted = table.Column<bool>(type: "boolean", nullable: true),
                    eccomerce_viewable = table.Column<bool>(type: "boolean", nullable: false),
                    eccomerce_salable = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    weight_kg = table.Column<float>(type: "real", nullable: true),
                    length_cm = table.Column<float>(type: "real", nullable: true),
                    width_cm = table.Column<float>(type: "real", nullable: true),
                    height_cm = table.Column<float>(type: "real", nullable: true),
                    alternate_name = table.Column<string>(type: "text", nullable: false),
                    preferred_supplier_id = table.Column<int>(type: "integer", nullable: true),
                    maximum_stock_level = table.Column<float>(type: "real", nullable: false),
                    minimum_stock_level = table.Column<float>(type: "real", nullable: false),
                    purchase_price = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    original_price = table.Column<decimal>(type: "numeric", nullable: false),
                    discounted_price = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    tax_price = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_id = table.Column<string>(type: "text", nullable: true),
                    coupon_code = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalSchema: "dbo",
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id",
                schema: "dbo",
                table: "cart_items",
                column: "cart_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "products",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "carts",
                schema: "dbo");
        }
    }
}
