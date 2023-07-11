using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDisciplinesGA.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Faculty> facultyList = _context.Faculties.ToList();
            return View(facultyList);
        }

        public IActionResult Upsert(int? id)
        {
            Faculty faculty = new();

            if (id == 0 || id == null)
            {
                return View(faculty);
            }
            else
            {
                faculty = _context.Faculties.FirstOrDefault(x => x.Id == id);
                return View(faculty);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Faculty obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _context.Faculties.Add(obj);
                }
                else
                {
                    _context.Faculties.Update(obj);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var faculty = _context.Faculties.Find(id);

            if (faculty == null)
            {
                return NotFound();
            }

            _context.Faculties.Remove(faculty);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
