using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicDisciplinesGA.Migrations
{
    public partial class AddMoreCoursesAndCompetences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competences_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoursePrerequisite",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePrerequisite", x => new { x.CourseId, x.PrerequisiteId });
                    table.ForeignKey(
                        name: "FK_CoursePrerequisite_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePrerequisite_Courses_PrerequisiteId",
                        column: x => x.PrerequisiteId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseCompetence",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CompetenceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCompetence", x => new { x.CourseId, x.CompetenceId });
                    table.ForeignKey(
                        name: "FK_CourseCompetence_Competences_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "Competences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseCompetence_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Competences",
                columns: new[] { "Id", "ApplicationUserId", "IsMandatory", "Name" },
                values: new object[,]
                {
                    { 1, null, true, "Programming Basics" },
                    { 2, null, true, "Object-Oriented Design" },
                    { 3, null, true, "Data Structures and Algorithms" },
                    { 4, null, true, "Software Engineering Principles" },
                    { 5, null, true, "Database Systems" },
                    { 6, null, true, "Web Development" },
                    { 7, null, false, "Frontend Development" },
                    { 8, null, true, "Backend Development" },
                    { 9, null, false, "Computer Vision" },
                    { 10, null, true, "Biostatistics" },
                    { 11, null, false, "Cryptology" },
                    { 12, null, true, "Quantum Computing" },
                    { 13, null, false, "Networking" },
                    { 14, null, true, "Ethical Hacking" },
                    { 15, null, false, "Cloud Computing" },
                    { 16, null, true, "Game Development" },
                    { 17, null, true, "Artificial Intelligence" },
                    { 18, null, false, "Mobile App Development" }
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Faculty of Computer Science" },
                    { 2, "Faculty of Mathematics" },
                    { 3, "Faculty of Physics" },
                    { 4, "Faculty of Chemistry" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Dr. Emily Johnson" },
                    { 2, "Prof. Mark Brown" },
                    { 3, "Dr. Susan Clark" },
                    { 4, "Prof. Joseph Davis" },
                    { 5, "Dr. Karen Wilson" },
                    { 6, "Prof. Christopher Taylor" },
                    { 7, "Dr. Daniel Moore" },
                    { 8, "Prof. Laura Thompson" },
                    { 9, "Dr. Patricia White" },
                    { 10, "Prof. Sarah Harris" }
                });

            migrationBuilder.InsertData(
                table: "Chairs",
                columns: new[] { "Id", "FacultyId", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Department of Computer Technologies" },
                    { 2, 2, "Department of Theoretical Mathematics" },
                    { 3, 3, "Department of Quantum Mechanics" },
                    { 4, 4, "Department of Organic Chemistry" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "ChairId", "ECTS", "TeacherId", "Title" },
                values: new object[,]
                {
                    { 1, 1, 5, 1, "Intro to Computer Science" },
                    { 2, 1, 6, 2, "Advanced Programming" },
                    { 3, 1, 5, 3, "Algorithms and Data Structures" },
                    { 4, 1, 5, 4, "Software Engineering" },
                    { 5, 1, 5, 5, "Databases 101" },
                    { 6, 1, 5, 6, "Web Development Fundamentals" },
                    { 7, 1, 5, 7, "Frontend Web Development" },
                    { 8, 1, 6, 8, "Backend Web Development" },
                    { 9, 1, 5, 9, "Computer Vision Basics" },
                    { 10, 1, 5, 10, "Biostatistics 101" },
                    { 11, 1, 5, 1, "Introduction to Cryptology" },
                    { 12, 1, 6, 2, "Quantum Computing Fundamentals" },
                    { 13, 1, 4, 3, "Networking Fundamentals" },
                    { 14, 1, 5, 4, "Introduction to Ethical Hacking" },
                    { 15, 1, 5, 5, "Cloud Computing Basics" },
                    { 16, 1, 6, 6, "Game Development Basics" },
                    { 17, 1, 5, 7, "Introduction to AI" },
                    { 18, 1, 5, 8, "Mobile App Development 101" }
                });

            migrationBuilder.InsertData(
                table: "CourseCompetence",
                columns: new[] { "CompetenceId", "CourseId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 },
                    { 5, 5 },
                    { 6, 6 },
                    { 7, 7 },
                    { 8, 8 },
                    { 9, 9 },
                    { 10, 10 },
                    { 11, 11 },
                    { 12, 12 },
                    { 13, 13 },
                    { 14, 14 },
                    { 15, 15 },
                    { 16, 16 },
                    { 17, 17 },
                    { 18, 18 }
                });

            migrationBuilder.InsertData(
                table: "CoursePrerequisite",
                columns: new[] { "CourseId", "PrerequisiteId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 3, 2 },
                    { 4, 3 },
                    { 5, 1 },
                    { 6, 1 },
                    { 8, 7 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 5 },
                    { 12, 3 },
                    { 13, 1 },
                    { 14, 6 },
                    { 15, 3 },
                    { 16, 17 },
                    { 17, 8 },
                    { 18, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Competences_ApplicationUserId",
                table: "Competences",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompetence_CompetenceId",
                table: "CourseCompetence",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisite_PrerequisiteId",
                table: "CoursePrerequisite",
                column: "PrerequisiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseCompetence");

            migrationBuilder.DropTable(
                name: "CoursePrerequisite");

            migrationBuilder.DropTable(
                name: "Competences");

            migrationBuilder.DeleteData(
                table: "Chairs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Chairs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Chairs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Chairs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
