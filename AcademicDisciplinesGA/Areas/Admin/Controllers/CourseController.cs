using AcademicDisciplinesGA.Areas.Admin.Models;
using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Course> courseList = _context.Courses
                .Include(x => x.Teacher)
                .Include(y => y.Chair)
                .Include(c => c.Prerequisites)
                    .ThenInclude(pr => pr.Prerequisite) // Завантаження властивостей пререквізитів
                .Include(c => c.Competences)
                    .ThenInclude(co => co.Competence)
                .ToList();
            return View(courseList);
        }

        public IActionResult Upsert(int? id)
        {
            CourseVM courseVM = new()
            {
                Course = new(),
                TeacherList = _context.Teachers.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }),
                ChairList = _context.Chairs.Select(x => new SelectListItem { Text = x.Title, Value = x.Id.ToString() }),
                PrerequisiteList = _context.Courses.Select(c => new SelectListItem { Text = c.Title, Value = c.Id.ToString() }),
                CompetenceList = _context.Competences.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }),
                SelectedCompetenceIds = new List<int>()
            };

            if (id == 0 || id == null)
            {
                return View(courseVM);
            }
            else
            {
                courseVM.Course = _context.Courses.Include(x => x.Prerequisites).Include(x => x.Competences).FirstOrDefault(x => x.Id == id);

                if (courseVM.Course != null)
                {
                    courseVM.SelectedPrerequisiteId = courseVM.Course.Prerequisites?.FirstOrDefault()?.PrerequisiteId; // Assuming only one prerequisite
                    courseVM.SelectedCompetenceIds = courseVM.Course.Competences.Select(c => c.CompetenceId).ToList();
                }

                return View(courseVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CourseVM obj)
        {
            if (ModelState.IsValid)
            {
                var courseInDb = _context.Courses.Include(x => x.Prerequisites).Include(x => x.Competences).FirstOrDefault(x => x.Id == obj.Course.Id);

                if (obj.Course.Id == 0)
                {
                    _context.Courses.Add(obj.Course);
                }
                else
                {
                    if (courseInDb != null)
                    {
                        _context.Entry(courseInDb).CurrentValues.SetValues(obj.Course);
                        // Update prerequisite
                        courseInDb.Prerequisites.Clear();
                        if (obj.SelectedPrerequisiteId.HasValue)
                        {
                            courseInDb.Prerequisites.Add(new CoursePrerequisite { PrerequisiteId = obj.SelectedPrerequisiteId.Value });
                        }

                        // Update competences
                        courseInDb.Competences.Clear();
                        foreach (int competenceId in obj.SelectedCompetenceIds)
                        {
                            courseInDb.Competences.Add(new CourseCompetence { CompetenceId = competenceId });
                        }
                    }
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var course = _context.Courses.FirstOrDefault(x => x.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

