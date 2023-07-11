using AcademicDisciplinesGA.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicDisciplinesGA.Areas.User.Models
{
    public class SurveyVM
    {
        [ValidateNever]
        public ApplicationUser User { get; set; }

        public bool ImproveExisting {get; set;}

        [ValidateNever]
        public IEnumerable<SelectListItem> TeacherList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ChairList { get; set; }
        [ValidateNever]
        public Teacher Teacher { get; set; }
        [ValidateNever]
        public Chair Chair { get; set; }
    }
}
