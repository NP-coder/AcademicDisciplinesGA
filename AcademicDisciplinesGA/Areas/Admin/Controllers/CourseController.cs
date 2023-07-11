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
            IEnumerable<Course> courseList = _context.Courses.Include(x => x.Teacher).Include(y => y.Chair).ToList();
            return View(courseList);
        }

        public IActionResult Upsert(int? id)
        {
            CourseVM courseVM = new()
            {
                Course = new(),
                TeacherList = _context.Teachers.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }),
                ChairList = _context.Chairs.Select(x => new SelectListItem { Text = x.Title, Value = x.Id.ToString() })
            };

            if (id == 0 || id == null)
            {
                return View(courseVM);
            }
            else
            {
                courseVM.Course = _context.Courses.FirstOrDefault(x => x.Id == id);
                return View(courseVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CourseVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Course.Id == 0)
                {
                    //add
                    _context.Courses.Add(obj.Course);
                }
                else
                {
                    //update
                    _context.Courses.Update(obj.Course);
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

