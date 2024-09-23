using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDisciplinesGA.Areas.User.Models
{
    public class CompetenceSelectionVM
    {
        public ApplicationUser User { get; set; }
        public List<SelectListItem> Competences { get; set; } = new List<SelectListItem>();
        public List<int> SelectedCompetenceIds { get; set; } = new List<int>();
    }
}
