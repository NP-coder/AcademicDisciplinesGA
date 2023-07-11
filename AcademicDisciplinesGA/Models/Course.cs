using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AcademicDisciplinesGA.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ECTS { get; set; }
        public int TeacherId { get; set; }
        [ValidateNever]
        public Teacher Teacher { get; set; }
        public int ChairId { get; set; }
        [ValidateNever]
        public Chair Chair { get; set; }

    }
}
