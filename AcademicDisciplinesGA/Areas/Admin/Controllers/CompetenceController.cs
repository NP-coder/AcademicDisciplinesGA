using AcademicDisciplinesGA.Areas.Admin.Models;
using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicDisciplinesGA.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CompetenceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompetenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Competence
        public IActionResult Index()
        {
            var competences = _context.Competences.ToList();
            return View(competences);
        }

        // GET: Competence/Upsert/5
        public IActionResult Upsert(int? id)
        {
            CompetenceVM competenceVM = new CompetenceVM()
            {
                Competence = new Competence()
            };
            if (id == null)
            {
                // create new competence
                return View(competenceVM);
            }
            else
            {
                // edit existing competence
                competenceVM.Competence = _context.Competences.FirstOrDefault(x => x.Id == id);
                if (competenceVM.Competence == null)
                {
                    return NotFound();
                }
                return View(competenceVM);
            }
        }

        // POST: Competence/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CompetenceVM competenceVM)
        {
            if (ModelState.IsValid)
            {
                if (competenceVM.Competence.Id == 0)
                {
                    _context.Competences.Add(competenceVM.Competence);
                }
                else
                {
                    _context.Competences.Update(competenceVM.Competence);
                }
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(competenceVM);
        }

        // GET: Competence/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var competence = _context.Competences.Find(id);
            if (competence == null)
            {
                return NotFound();
            }
            _context.Competences.Remove(competence);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
