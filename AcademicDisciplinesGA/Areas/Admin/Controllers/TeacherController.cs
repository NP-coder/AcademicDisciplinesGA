using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDisciplinesGA.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Teacher> teacherList = _context.Teachers.ToList();
            return View(teacherList);
        }

        public IActionResult Upsert(int? id)
        {
            Teacher teacher = new();

            if (id == 0 || id == null)
            {
                return View(teacher);
            }
            else
            {
                teacher = _context.Teachers.FirstOrDefault(x => x.Id == id);
                return View(teacher);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Teacher obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _context.Teachers.Add(obj);
                }
                else
                {
                    _context.Teachers.Update(obj);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var teacher = _context.Teachers.FirstOrDefault(x => x.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            _context.Teachers.Remove(teacher);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
