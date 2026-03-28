using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Клуб_6.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Criteria",
                columns: table => new
                {
                    CriterionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriterionName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CompositionID = table.Column<int>(type: "int", nullable: true),
                    IsImportant = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Criteria__647C3BD1AA41C4FB", x => x.CriterionID);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    DisciplineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisciplineName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    WorkingScore = table.Column<int>(type: "int", nullable: true),
                    CompositionID = table.Column<int>(type: "int", nullable: true),
                    Coefficient = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Discipli__29093750E59CB0B5", x => x.DisciplineID);
                });

            migrationBuilder.CreateTable(
                name: "EventComposition",
                columns: table => new
                {
                    CompositionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EventCom__B8E2333F11031CD1", x => x.CompositionID);
                });

            migrationBuilder.CreateTable(
                name: "EventStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EventSta__C8EE2063DB185660", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Kennels",
                columns: table => new
                {
                    KennelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KennelName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    FoundationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "Kazakhstan"),
                    City = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Kennels__921B7EC337B69530", x => x.KennelID);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    OwnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Owners__8193859888119E94", x => x.OwnerID);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    OptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    OptionValue = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    TextInfo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CompositionID = table.Column<int>(type: "int", nullable: true),
                    CriterionID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Options__92C7A1DF0928F6DE", x => x.OptionID);
                    table.ForeignKey(
                        name: "FK_Options_Criteria",
                        column: x => x.CriterionID,
                        principalTable: "Criteria",
                        principalColumn: "CriterionID");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EventVenue = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CompositionID = table.Column<int>(type: "int", nullable: true),
                    Judge1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Judge2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Host = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CommitteeChairman = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Organization = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    TestOrganizer = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Events__7944C8708520FBCD", x => x.EventID);
                    table.ForeignKey(
                        name: "FK__Events__Composit__6E01572D",
                        column: x => x.CompositionID,
                        principalTable: "EventComposition",
                        principalColumn: "CompositionID");
                    table.ForeignKey(
                        name: "FK__Events__StatusId__6D0D32F4",
                        column: x => x.StatusId,
                        principalTable: "EventStatuses",
                        principalColumn: "StatusId");
                });

            migrationBuilder.CreateTable(
                name: "Dogs",
                columns: table => new
                {
                    ChipNumber = table.Column<int>(type: "int", nullable: false),
                    DogName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Breed = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    MotherName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    FatherName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    HeightCM = table.Column<int>(type: "int", nullable: true),
                    WeightKG = table.Column<int>(type: "int", nullable: true),
                    IsAlive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    KennelID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Dogs__7460557C7A3AB5C3", x => x.ChipNumber);
                    table.ForeignKey(
                        name: "FK__Dogs__KennelID__4F7CD00D",
                        column: x => x.KennelID,
                        principalTable: "Kennels",
                        principalColumn: "KennelID");
                });

            migrationBuilder.CreateTable(
                name: "DogList",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    DogID = table.Column<int>(type: "int", nullable: false),
                    DogName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Owner = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    TrialPassed = table.Column<int>(type: "int", nullable: true),
                    TrialFailed = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DogList__FBDF78C92E05AB2D", x => x.RecordID);
                    table.ForeignKey(
                        name: "FK__DogList__DogID__6FE99F9F",
                        column: x => x.DogID,
                        principalTable: "Dogs",
                        principalColumn: "ChipNumber");
                    table.ForeignKey(
                        name: "FK__DogList__EventID__6EF57B66",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "EventID");
                });

            migrationBuilder.CreateTable(
                name: "DogOwners",
                columns: table => new
                {
                    DogOwnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerID = table.Column<int>(type: "int", nullable: false),
                    ChipNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DogOwner__EA0FED7342E7CAF5", x => x.DogOwnerID);
                    table.ForeignKey(
                        name: "FK__DogOwners__ChipN__5629CD9C",
                        column: x => x.ChipNumber,
                        principalTable: "Dogs",
                        principalColumn: "ChipNumber");
                    table.ForeignKey(
                        name: "FK__DogOwners__Owner__5535A963",
                        column: x => x.OwnerID,
                        principalTable: "Owners",
                        principalColumn: "OwnerID");
                });

            migrationBuilder.CreateTable(
                name: "DogCriteriaResults_DogList",
                columns: table => new
                {
                    ResultID_RecordID_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    CriterionID = table.Column<int>(type: "int", nullable: false),
                    OptionID = table.Column<int>(type: "int", nullable: false),
                    UserInput = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DogCrite__AA41C4FB12345678", x => x.ResultID_RecordID_ID);
                    table.ForeignKey(
                        name: "FK__DogCritResDogList__Criterion",
                        column: x => x.CriterionID,
                        principalTable: "Criteria",
                        principalColumn: "CriterionID");
                    table.ForeignKey(
                        name: "FK__DogList__DogCritResDogList",
                        column: x => x.RecordID,
                        principalTable: "DogList",
                        principalColumn: "RecordID");
                });

            migrationBuilder.CreateTable(
                name: "DogDisciplines",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    DisciplineID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DogDisci__3214EC27DC7E3462", x => x.ID);
                    table.ForeignKey(
                        name: "FK__DogDiscip__Disci__7C4F7684",
                        column: x => x.DisciplineID,
                        principalTable: "Disciplines",
                        principalColumn: "DisciplineID");
                    table.ForeignKey(
                        name: "FK__DogDiscip__Recor__7B5B524B",
                        column: x => x.RecordID,
                        principalTable: "DogList",
                        principalColumn: "RecordID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DogCriteriaResults_DogList_CriterionID",
                table: "DogCriteriaResults_DogList",
                column: "CriterionID");

            migrationBuilder.CreateIndex(
                name: "IX_DogCriteriaResults_DogList_RecordID",
                table: "DogCriteriaResults_DogList",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_DogDisciplines_DisciplineID",
                table: "DogDisciplines",
                column: "DisciplineID");

            migrationBuilder.CreateIndex(
                name: "IX_DogDisciplines_RecordID",
                table: "DogDisciplines",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_DogList_DogID",
                table: "DogList",
                column: "DogID");

            migrationBuilder.CreateIndex(
                name: "IX_DogList_EventID",
                table: "DogList",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_DogOwners_ChipNumber",
                table: "DogOwners",
                column: "ChipNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DogOwners_OwnerID",
                table: "DogOwners",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_KennelID",
                table: "Dogs",
                column: "KennelID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CompositionID",
                table: "Events",
                column: "CompositionID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StatusId",
                table: "Events",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Options_CriterionID",
                table: "Options",
                column: "CriterionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DogCriteriaResults_DogList");

            migrationBuilder.DropTable(
                name: "DogDisciplines");

            migrationBuilder.DropTable(
                name: "DogOwners");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "DogList");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropTable(
                name: "Criteria");

            migrationBuilder.DropTable(
                name: "Dogs");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Kennels");

            migrationBuilder.DropTable(
                name: "EventComposition");

            migrationBuilder.DropTable(
                name: "EventStatuses");
        }
    }
}
