using AcademicDisciplinesGA.Areas.User.Models;
using AcademicDisciplinesGA.GA;
using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AcademicDisciplinesGA.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class GeneticAlgorithmController : Controller
    {
        DisciplinesPopulation population;
        private readonly ApplicationDbContext _context;
        DisciplinesChromosome disciplinesChromosomes;

        public GeneticAlgorithmController(ApplicationDbContext db)
        {
            _context = db;
        }

        public IActionResult Survey()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user  = _context.ApplicationUsers.FirstOrDefault(x => x.Id == claims.Value);

            SurveyVM surveyVM = new()
            {
                User = user,
                ChairList = _context.Chairs.Select(x => new SelectListItem { Text = x.Title, Value = x.Id.ToString() }),
                TeacherList = _context.Teachers.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };

            return View(surveyVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Survey(SurveyVM obj)
        {
            if (ModelState.IsValid)
            {
                List<Teacher> teachers = new List<Teacher>();
                List<Chair> chairs = new List<Chair>();
                var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == obj.User.Id);

                teachers.Add(_context.Teachers.FirstOrDefault(x => x.Id == obj.Teacher.Id));
                var chairRec = _context.Chairs.FirstOrDefault(x => x.Id == obj.Chair.Id);

                if (obj.ImproveExisting)
                {
                    var facultyId = _context.Chairs.Where(c => c.Id == user.ChairId).Select(c => c.FacultyId).FirstOrDefault();

                    var facultyChairs = _context.Chairs.Where(c => c.FacultyId == facultyId).ToList();
                    chairs.AddRange(facultyChairs);
                    
                    if (!chairs.Contains(chairRec))
                    {
                        chairs.Add(chairRec);
                    }                  
                }
                else
                {
                    chairs.Add(chairRec);
                }

                population = new DisciplinesPopulation(_context, teachers, chairs);

                disciplinesChromosomes = Run();

                ViewData["Courses"] = ConvertToCourses(disciplinesChromosomes.Sequence);

                //_context.Chairs.Add(obj.Chair);
                //_context.SaveChanges();

                return View("Result");
            }
            return View(obj);
        }

        private DisciplinesChromosome Run()
        {
            while (population.GenerationCount < GAConfig.MaxGenerations || population.NoImprovementCount < GAConfig.MaxNoImprovementCount)
            {
                population.DoGeneration();
            }

            return population.GetBestIndividual();
        }

        public List<Course> ConvertToCourses(List<CourseChromosome> courseChromosomes)
        {
            var courseTeacherIds = courseChromosomes.Select(c => c.TeacherId).ToList();
            var courseChairIds = courseChromosomes.Select(c => c.ChairId).ToList();

            var teachers = _context.Teachers.Where(t => courseTeacherIds.Contains(t.Id)).ToList();
            var chairs = _context.Chairs.Where(c => courseChairIds.Contains(c.Id)).ToList();

            var courses = courseChromosomes.Select(cc =>
            {
                var course = new Course
                {
                    Id = cc.Id,
                    Title = cc.Title,
                    ECTS = cc.ECTS,
                    TeacherId = cc.TeacherId,
                    ChairId = cc.ChairId
                };

                course.Teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                course.Chair = chairs.FirstOrDefault(c => c.Id == course.ChairId);

                return course;
            }).ToList();

            return courses;
        }
    }
}
