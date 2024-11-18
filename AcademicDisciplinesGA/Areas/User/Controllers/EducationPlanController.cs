using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using AcademicDisciplinesGA.Areas.User.Models;
using Microsoft.AspNetCore.Authorization;
using AcademicDisciplinesGA.GA;
using GeneticSharp;
using Microsoft.EntityFrameworkCore;
using AcademicDisciplinesGA.Areas.Admin.Models;
using CourseVM = AcademicDisciplinesGA.Areas.User.Models.CourseVM;

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

        public IActionResult Index()
        {
            var model = new CompetenceSelectionVM
            {
                Competences = _context.Competences
                    .Where(c => !c.IsMandatory)
                    .Select(c => new SelectListItem
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
            var mandatoryCompIds = _context.Competences
                .Where(c => c.IsMandatory)
                .Select(c => c.Id)
                .ToList();

            compIds.AddRange(mandatoryCompIds);

            var competences = _context.Competences.Where(c => compIds.Contains(c.Id)).ToList();
            population = new DisciplinesPopulation(_context, competences);

            disciplinesChromosomes = Run();

            ViewData["Courses"] = ConvertToCourses(disciplinesChromosomes.Sequence);

            return View("Result");
        }

        private DisciplinesChromosome Run()
        {
            while (population.GenerationCount < GAConfig.MaxGenerations || population.NoImprovementCount < GAConfig.MaxNoImprovementCount)
            {
                population.DoGeneration();
            }

            return population.GetBestIndividual();
        }

        public List<CourseVM> ConvertToCourses(List<CourseChromosome> courseChromosomes)
        {
            var courseIds = courseChromosomes.Select(c => c.Id).ToList();
            var coursesDetails = _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Include(c => c.Teacher)
                .Include(c => c.Chair)
                .Include(c => c.Competences).ThenInclude(cc => cc.Competence)
                .Include(c => c.Prerequisites).ThenInclude(pr => pr.Prerequisite)
                .ToList();

            var courses = courseChromosomes.Select(cc =>
            {
                var courseData = coursesDetails.FirstOrDefault(c => c.Id == cc.Id);
                if (courseData == null)
                    return null;

                return new CourseVM
                {
                    Id = cc.Id,
                    Title = cc.Title,
                    ECTS = cc.ECTS,
                    Teacher = courseData.Teacher,
                    Chair = courseData.Chair,
                    Competences = courseData.Competences,
                    Prerequisites = courseData.Prerequisites,
                    Year = cc.Year,
                    Semester = cc.Semester
                };
            }).Where(c => c != null).ToList();

            courses.Sort((x, y) =>
            {
                int yearComparison = x.Year.CompareTo(y.Year);
                if (yearComparison == 0)
                {
                    return x.Semester.CompareTo(y.Semester);
                }
                return yearComparison;
            });

            return courses;
        }
    }
}
