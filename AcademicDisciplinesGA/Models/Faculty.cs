using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AcademicDisciplinesGA.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [ValidateNever]
        public List<Chair> Chairs { get; set; }
    }
}
