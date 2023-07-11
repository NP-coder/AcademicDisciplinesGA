namespace AcademicDisciplinesGA.Models
{
    public struct CourseChromosome
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ECTS { get; set; }
        public int ChairId { get; set; }
        public int TeacherId { get; set; }
    }
}
