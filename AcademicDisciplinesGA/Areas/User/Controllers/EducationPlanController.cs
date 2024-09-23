using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using AcademicDisciplinesGA.Areas.User.Models;
using Microsoft.AspNetCore.Authorization;
using AcademicDisciplinesGA.GA;
using GeneticSharp;

namespace AcademicDisciplinesGA.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class EducationPlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        DisciplinesPopulation population;
        DisciplinesChromosome disciplinesChromosomes;

        public EducationPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Показ форми для вибору компетентностей
        public IActionResult Index()
        {
            var model = new CompetenceSelectionVM
            {
                Competences = _context.Competences.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SelectCompetences(CompetenceSelectionVM model)
        {
            if (model.SelectedCompetenceIds != null && model.SelectedCompetenceIds.Count > 0)
            {
                var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == model.User.Id);

                return RedirectToAction("GenerateEducationPlan", new { compIds = model.SelectedCompetenceIds });
            }

            return View(model);
        }

        // Генерація навчального плану з обраними компетентностями
        public IActionResult GenerateEducationPlan(List<int> compIds)
        {
            var competences = _context.Competences.Where(c => compIds.Contains(c.Id)).ToList();
            population = new DisciplinesPopulation(_context, competences);

            disciplinesChromosomes = Run();

            ViewData["Courses"] = ConvertToCourses(disciplinesChromosomes.Sequence);

            return View();
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
