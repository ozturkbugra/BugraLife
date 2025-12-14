using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugraLife.Migrations
{
    /// <inheritdoc />
    public partial class tum_tablolar_olusturuldu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dailies",
                columns: table => new
                {
                    daily_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    daily_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daily_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    daily_status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dailies", x => x.daily_id);
                });

            migrationBuilder.CreateTable(
                name: "Debtors",
                columns: table => new
                {
                    debtor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    debtor_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debtors", x => x.debtor_id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseTypes",
                columns: table => new
                {
                    expensetype_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    expensetype_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expensetype_order = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_bank = table.Column<bool>(type: "bit", nullable: false),
                    is_home = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTypes", x => x.expensetype_id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTypes",
                columns: table => new
                {
                    incometype_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    incometype_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    incometype_order = table.Column<int>(type: "int", nullable: false),
                    is_bank = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTypes", x => x.incometype_id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    ingredient_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ingredient_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.ingredient_id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    location_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location_link = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "LoginUser",
                columns: table => new
                {
                    loginuser_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loginuser_username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loginuser_namesurname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    login_password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginUser", x => x.loginuser_id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    paymenttype_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    paymenttype_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paymenttype_order = table.Column<int>(type: "int", nullable: false),
                    paymenttype_balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_bank = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.paymenttype_id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    person_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    person_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    person_order = table.Column<int>(type: "int", nullable: false),
                    is_bank = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.person_id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedToDos",
                columns: table => new
                {
                    plannedtodo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    plannedtodo_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    plannedtodo_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    plannedtodo_done = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedToDos", x => x.plannedtodo_id);
                });

            migrationBuilder.CreateTable(
                name: "UnPlannedToDos",
                columns: table => new
                {
                    unplannedtodo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    unplannedtodo_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unplannedtodo_createdat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    unplannedtodo_done = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnPlannedToDos", x => x.unplannedtodo_id);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    asset_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ingredient_id = table.Column<int>(type: "int", nullable: false),
                    person_id = table.Column<int>(type: "int", nullable: false),
                    asset_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    asset_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    asset_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.asset_id);
                    table.ForeignKey(
                        name: "FK_Assets_Ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "Ingredients",
                        principalColumn: "ingredient_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Persons_person_id",
                        column: x => x.person_id,
                        principalTable: "Persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    expense_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    expensetype_id = table.Column<int>(type: "int", nullable: false),
                    paymenttype_id = table.Column<int>(type: "int", nullable: false),
                    person_id = table.Column<int>(type: "int", nullable: false),
                    expense_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expense_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expense_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_bankmovement = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.expense_id);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseTypes_expensetype_id",
                        column: x => x.expensetype_id,
                        principalTable: "ExpenseTypes",
                        principalColumn: "expensetype_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_PaymentTypes_paymenttype_id",
                        column: x => x.paymenttype_id,
                        principalTable: "PaymentTypes",
                        principalColumn: "paymenttype_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_Persons_person_id",
                        column: x => x.person_id,
                        principalTable: "Persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                columns: table => new
                {
                    income_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    incometype_id = table.Column<int>(type: "int", nullable: false),
                    paymenttype_id = table.Column<int>(type: "int", nullable: false),
                    person_id = table.Column<int>(type: "int", nullable: false),
                    income_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    income_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    income_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_bankmovement = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.income_id);
                    table.ForeignKey(
                        name: "FK_Incomes_IncomeTypes_incometype_id",
                        column: x => x.incometype_id,
                        principalTable: "IncomeTypes",
                        principalColumn: "incometype_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incomes_PaymentTypes_paymenttype_id",
                        column: x => x.paymenttype_id,
                        principalTable: "PaymentTypes",
                        principalColumn: "paymenttype_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incomes_Persons_person_id",
                        column: x => x.person_id,
                        principalTable: "Persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movements",
                columns: table => new
                {
                    movement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    debtor_id = table.Column<int>(type: "int", nullable: false),
                    ingredient_id = table.Column<int>(type: "int", nullable: false),
                    person_id = table.Column<int>(type: "int", nullable: false),
                    movement_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    movement_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    movement_description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movements", x => x.movement_id);
                    table.ForeignKey(
                        name: "FK_Movements_Debtors_debtor_id",
                        column: x => x.debtor_id,
                        principalTable: "Debtors",
                        principalColumn: "debtor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Movements_Ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "Ingredients",
                        principalColumn: "ingredient_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Movements_Persons_person_id",
                        column: x => x.person_id,
                        principalTable: "Persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ingredient_id",
                table: "Assets",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_person_id",
                table: "Assets",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_expensetype_id",
                table: "Expenses",
                column: "expensetype_id");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_paymenttype_id",
                table: "Expenses",
                column: "paymenttype_id");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_person_id",
                table: "Expenses",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_incometype_id",
                table: "Incomes",
                column: "incometype_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_paymenttype_id",
                table: "Incomes",
                column: "paymenttype_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_person_id",
                table: "Incomes",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_debtor_id",
                table: "Movements",
                column: "debtor_id");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_ingredient_id",
                table: "Movements",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_person_id",
                table: "Movements",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Dailies");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Incomes");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "LoginUser");

            migrationBuilder.DropTable(
                name: "Movements");

            migrationBuilder.DropTable(
                name: "PlannedToDos");

            migrationBuilder.DropTable(
                name: "UnPlannedToDos");

            migrationBuilder.DropTable(
                name: "ExpenseTypes");

            migrationBuilder.DropTable(
                name: "IncomeTypes");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "Debtors");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
