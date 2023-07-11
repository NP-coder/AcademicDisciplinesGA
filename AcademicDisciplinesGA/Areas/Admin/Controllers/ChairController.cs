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
    public class ChairController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChairController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Chair> chairList = _context.Chairs.Include(x => x.Faculty).ToList();
            return View(chairList);
        }

        public IActionResult Upsert(int? id)
        {
            ChairVM chairVM = new()
            {
                Chair = new(),
                FacultyList = _context.Faculties.Select(x => new SelectListItem { Text = x.Title, Value = x.Id.ToString() })
            };
            
            if (id == 0 || id == null)
            {
                return View(chairVM);
            }
            else
            {
                chairVM.Chair = _context.Chairs.FirstOrDefault(x => x.Id == id);
                return View(chairVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ChairVM obj)
        {
            if (ModelState.IsValid)
            {
                if(obj.Chair.Id == 0)
                {
                    _context.Chairs.Add(obj.Chair);
                }
                else
                {
                    _context.Chairs.Update(obj.Chair);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var chair = _context.Chairs.FirstOrDefault(x => x.Id == id);

            if (chair == null)
            {
                return NotFound();
            }

            _context.Chairs.Remove(chair);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
