using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasKey(cp => new { cp.CourseId, cp.PrerequisiteId });

            modelBuilder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.Prerequisites)  // Assuming this navigation property in Course
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Prerequisite)
                .WithMany()
                .HasForeignKey(cp => cp.PrerequisiteId)
                .OnDelete(DeleteBehavior.Restrict);  // Modified to prevent CASCADE DELETE issues

            modelBuilder.Entity<CourseCompetence>()
                .HasKey(cp => new { cp.CourseId, cp.CompetenceId });

            modelBuilder.Entity<CourseCompetence>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.Competences)
                .HasForeignKey(cp => cp.CourseId);

            modelBuilder.Entity<CourseCompetence>()
                .HasOne(cp => cp.Competence)
                .WithMany()
                .HasForeignKey(cp => cp.CompetenceId);

            Seed(modelBuilder);
        }

        public DbSet<Chair> Chairs { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Competence> Competences { get; set; }



        private void Seed(ModelBuilder modelBuilder)
        {
            var additionalCompetenceNames = new List<string>
            {
                "Advanced Database Management", "Frontend Development", "Backend Development",
                "Computer Vision", "Biostatistics", "Cryptology", "Quantum Computing",
                "Robotics", "Computer Graphics", "Human-Computer Interaction", "Network Security",
                "Game Development", "Big Data Analysis", "IoT Systems", "Ethical Hacking",
                "Programming Basics", "Object-Oriented Design", "Data Structures and Algorithms",
                "Software Engineering Principles", "Database Systems", "Web Development",
                "Machine Learning", "Cybersecurity Fundamentals", "Cloud Computing Basics",
                "Artificial Intelligence", "Mobile Application Development"
            };

            var additionalCourseTitles = new List<string>
            {
                "Intro to Computer Science", "Advanced Programming", "Algorithms and Data Structures",
                "Database Design", "Web Development Advanced", "Machine Learning Introduction",
                "Cybersecurity Concepts", "Cloud Services Overview", "Artificial Intelligence Essentials",
                "Mobile Dev Fundamentals",
                "Advanced Databases", "Frontend Web Programming", "Backend Web Programming",
                "Image Processing", "Introduction to Biostatistics", "Foundations of Cryptology",
                "Principles of Quantum Computing", "Introduction to Robotics", "Computer Graphics",
                "Interacting with Computers", "Network Security", "Game Design", "Big Data Fundamentals",
                "Building IoT Systems", "Ethical Hacking Techniques"
            };

            // Додаємо факультети
            modelBuilder.Entity<Faculty>().HasData(
                new Faculty { Id = 1, Title = "Faculty of Computer Science" },
                new Faculty { Id = 2, Title = "Faculty of Mathematics" },
                new Faculty { Id = 3, Title = "Faculty of Physics" },      
                new Faculty { Id = 4, Title = "Faculty of Chemistry" }       
            );

            // Додаємо кафедри
            modelBuilder.Entity<Chair>().HasData(
                new Chair { Id = 1, Title = "Department of Computer Technologies", FacultyId = 1 },
                new Chair { Id = 2, Title = "Department of Theoretical Mathematics", FacultyId = 2 },
                new Chair { Id = 3, Title = "Department of Quantum Mechanics", FacultyId = 3 },   
                new Chair { Id = 4, Title = "Department of Organic Chemistry", FacultyId = 4 }   
            );

            var teachers = new List<Teacher>
            {
                new Teacher { Id = 1, Name = "Dr. Emily Johnson" },
                new Teacher { Id = 2, Name = "Prof. Mark Brown" },
                new Teacher { Id = 3, Name = "Dr. Susan Clark" },
                new Teacher { Id = 4, Name = "Prof. Joseph Davis" },
                new Teacher { Id = 5, Name = "Dr. Karen Wilson" },
                new Teacher { Id = 6, Name = "Prof. Christopher Taylor" },
                new Teacher { Id = 7, Name = "Dr. Daniel Moore" },
                new Teacher { Id = 8, Name = "Prof. Laura Thompson" },
                new Teacher { Id = 9, Name = "Dr. Patricia White" },
                new Teacher { Id = 10, Name = "Prof. Sarah Harris" },
            };
            modelBuilder.Entity<Teacher>().HasData(teachers);

            var competences = new List<Competence>
            {
                new Competence { Id = 1, Name = "Programming Basics", IsMandatory = true },
                new Competence { Id = 2, Name = "Object-Oriented Design", IsMandatory = true },
                new Competence { Id = 3, Name = "Data Structures and Algorithms", IsMandatory = true },
                new Competence { Id = 4, Name = "Software Engineering Principles", IsMandatory = true },
                new Competence { Id = 5, Name = "Database Systems", IsMandatory = true },
                new Competence { Id = 6, Name = "Web Development", IsMandatory = true },
                new Competence { Id = 7, Name = "Frontend Development", IsMandatory = false },
                new Competence { Id = 8, Name = "Backend Development", IsMandatory = true },
                new Competence { Id = 9, Name = "Computer Vision", IsMandatory = false },
                new Competence { Id = 10, Name = "Biostatistics", IsMandatory = true },
                new Competence { Id = 11, Name = "Cryptology", IsMandatory = false },
                new Competence { Id = 12, Name = "Quantum Computing", IsMandatory = true },
                new Competence { Id = 13, Name = "Networking", IsMandatory = false },
                new Competence { Id = 14, Name = "Ethical Hacking", IsMandatory = true },
                new Competence { Id = 15, Name = "Cloud Computing", IsMandatory = false },
                new Competence { Id = 16, Name = "Game Development", IsMandatory = true },
                new Competence { Id = 17, Name = "Artificial Intelligence", IsMandatory = true },
                new Competence { Id = 18, Name = "Mobile App Development", IsMandatory = false }
            };

            var courses = new List<Course>
            {
                new Course { Id = 1, Title = "Intro to Computer Science", ECTS = 5, TeacherId = 1, ChairId = 1 },
                new Course { Id = 2, Title = "Advanced Programming", ECTS = 6, TeacherId = 2, ChairId = 1 },
                new Course { Id = 3, Title = "Algorithms and Data Structures", ECTS = 5, TeacherId = 3, ChairId = 1 },
                new Course { Id = 4, Title = "Software Engineering", ECTS = 5, TeacherId = 4, ChairId = 1 },
                new Course { Id = 5, Title = "Databases 101", ECTS = 5, TeacherId = 5, ChairId = 1 },
                new Course { Id = 6, Title = "Web Development Fundamentals", ECTS = 5, TeacherId = 6, ChairId = 1 },
                new Course { Id = 7, Title = "Frontend Web Development", ECTS = 5, TeacherId = 7, ChairId = 1 },
                new Course { Id = 8, Title = "Backend Web Development", ECTS = 6, TeacherId = 8, ChairId = 1 },
                new Course { Id = 9, Title = "Computer Vision Basics", ECTS = 5, TeacherId = 9, ChairId = 1 },
                new Course { Id = 10, Title = "Biostatistics 101", ECTS = 5, TeacherId = 10, ChairId = 1 },
                new Course { Id = 11, Title = "Introduction to Cryptology", ECTS = 5, TeacherId = 1, ChairId = 1 },
                new Course { Id = 12, Title = "Quantum Computing Fundamentals", ECTS = 6, TeacherId = 2, ChairId = 1 },
                new Course { Id = 13, Title = "Networking Fundamentals", ECTS = 4, TeacherId = 3, ChairId = 1 },
                new Course { Id = 14, Title = "Introduction to Ethical Hacking", ECTS = 5, TeacherId = 4, ChairId = 1 },
                new Course { Id = 15, Title = "Cloud Computing Basics", ECTS = 5, TeacherId = 5, ChairId = 1 },
                new Course { Id = 16, Title = "Game Development Basics", ECTS = 6, TeacherId = 6, ChairId = 1 },
                new Course { Id = 17, Title = "Introduction to AI", ECTS = 5, TeacherId = 7, ChairId = 1 },
                new Course { Id = 18, Title = "Mobile App Development 101", ECTS = 5, TeacherId = 8, ChairId = 1 }
            };

            modelBuilder.Entity<Competence>().HasData(competences);
            modelBuilder.Entity<Course>().HasData(courses);

            var courseCompetences = new List<CourseCompetence>
            {
                new CourseCompetence { CourseId = 1, CompetenceId = 1 }, // Програмування баз для Intro to CS
                new CourseCompetence { CourseId = 2, CompetenceId = 2 }, // ООП для Advanced Programming
                new CourseCompetence { CourseId = 3, CompetenceId = 3 }, // Алгоритми для Algorithms and Data Structures
                new CourseCompetence { CourseId = 4, CompetenceId = 4 }, // Принципи ПЗ для Software Engineering
                new CourseCompetence { CourseId = 5, CompetenceId = 5 }, // Системи БД для Databases 101
                new CourseCompetence { CourseId = 6, CompetenceId = 6 }, // Веб-розробка для Web Development Fundamentals
                new CourseCompetence { CourseId = 7, CompetenceId = 7 },
                new CourseCompetence { CourseId = 8, CompetenceId = 8 },
                new CourseCompetence { CourseId = 9, CompetenceId = 9 },
                new CourseCompetence { CourseId = 10, CompetenceId = 10 },
                new CourseCompetence { CourseId = 11, CompetenceId = 11 },
                new CourseCompetence { CourseId = 12, CompetenceId = 12 },
                new CourseCompetence { CourseId = 13, CompetenceId = 13 },
                new CourseCompetence { CourseId = 14, CompetenceId = 14 },
                new CourseCompetence { CourseId = 15, CompetenceId = 15 },
                new CourseCompetence { CourseId = 16, CompetenceId = 16 },
                new CourseCompetence { CourseId = 17, CompetenceId = 17 },
                new CourseCompetence { CourseId = 18, CompetenceId = 18 }
            };

            modelBuilder.Entity<CourseCompetence>().HasData(courseCompetences);

            var coursePrerequisites = new List<CoursePrerequisite>
            {
                new CoursePrerequisite { CourseId = 2, PrerequisiteId = 1 }, // Advanced Programming після Intro to CS
                new CoursePrerequisite { CourseId = 3, PrerequisiteId = 2 }, // Алгоритми після Advanced Programming
                new CoursePrerequisite { CourseId = 4, PrerequisiteId = 3 }, // Software Engineering після Algorithms and Data Structures
                new CoursePrerequisite { CourseId = 5, PrerequisiteId = 1 }, // Databases 101 після Intro to CS
                new CoursePrerequisite { CourseId = 6, PrerequisiteId = 1 }, // Web Dev після Intro to CS
                new CoursePrerequisite { CourseId = 8, PrerequisiteId = 7 },  // Backend після Frontend
                new CoursePrerequisite { CourseId = 9, PrerequisiteId = 1 },  // Computer Vision після Intro to CS
                new CoursePrerequisite { CourseId = 10, PrerequisiteId = 1 }, // Biostatistics після Intro to CS
                new CoursePrerequisite { CourseId = 11, PrerequisiteId = 5 }, // Cryptology після Databases 101
                new CoursePrerequisite { CourseId = 12, PrerequisiteId = 3 }, // Quantum Computing після Algorithms
                new CoursePrerequisite { CourseId = 13, PrerequisiteId = 1 }, // Networking після Intro to CS
                new CoursePrerequisite { CourseId = 14, PrerequisiteId = 6 }, // Ethical Hacking після Web Development Fundamentals
                new CoursePrerequisite { CourseId = 15, PrerequisiteId = 3 }, // Cloud Computing після Algorithms and Data Structures
                new CoursePrerequisite { CourseId = 16, PrerequisiteId = 17 }, // Game Development після Introduction to AI
                new CoursePrerequisite { CourseId = 17, PrerequisiteId = 8 },  // Introduction to AI після Backend Web Development
                new CoursePrerequisite { CourseId = 18, PrerequisiteId = 7 }   // Mobile App Development після Frontend Web Development
            };

            modelBuilder.Entity<CoursePrerequisite>().HasData(coursePrerequisites);
        }
    }
}
