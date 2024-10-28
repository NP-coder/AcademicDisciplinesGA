using AcademicDisciplinesGA.Areas.Admin.Models;
using AcademicDisciplinesGA.Models;
using GeneticSharp;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.GA
{
    public class DisciplinesChromosome
    {
        private readonly ApplicationDbContext _dataContext;
        public List<CourseChromosome> Sequence { get; set; }
        public int Rank { get; set; }
        public int ECTSCount { get; set; }
        public int TeacherFitness { get; set; }
        public int ChairFitness { get; set; }
        public int CompetenceFitness { get; set; }
        public int LoadFitness { get; set; }
        public List<Competence> StudentCompetences { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Chair> Chairs { get; set; }

        public static int _length = 10;

        static readonly Random Random = new Random();

        public DisciplinesChromosome(ApplicationDbContext dataContext, List<Teacher> teachers, List<Chair> chairs)
        {
            _dataContext = dataContext;
            Generate();
            Teachers = teachers;
            Chairs = chairs;
            ECTSCount = GetTotalECTS();
            TeacherFitness = IsTeacherSelected(Teachers);
            ChairFitness = IsChairSelected(Chairs);
        }

        public DisciplinesChromosome(List<CourseChromosome> courses, List<Teacher> teachers, List<Chair> chairs)
        {
            Sequence = courses.ToList();
            Teachers = teachers;
            Chairs = chairs;
            ECTSCount = GetTotalECTS();
            TeacherFitness = IsTeacherSelected(Teachers);
            ChairFitness = IsChairSelected(Chairs);
        }

        // є можливість що в популяцію непопаде дисципліна що є пррериквізитом
        public DisciplinesChromosome(ApplicationDbContext dataContext, List<Competence> studentInterests)
        {
            _dataContext = dataContext;
            StudentCompetences = studentInterests;
            Generate();
            // Розподіл дисциплін із пререквізитами
            AllocateCoursesWithPrerequisites();
            // Розподіл навантаження по семестрах
            LoadFitness = CalculateLoadDistribution();
            // Підсумування ECTS після встановлення усіх курсів 
            ECTSCount = GetTotalECTS();
            // Оцінювання за критеріями
            CompetenceFitness = CalculateCompetenceFitness();
        }

        public DisciplinesChromosome(List<CourseChromosome> courses, List<Competence> studentInterests)
        {
            Sequence = courses.ToList();
            StudentCompetences = studentInterests;
            AllocateCoursesWithPrerequisites();
            LoadFitness = CalculateLoadDistribution();
            ECTSCount = GetTotalECTS();
            CompetenceFitness = CalculateCompetenceFitness();
        }

        public void RecalculateCompetenceFitness()
        {
            var allCourses = _dataContext.Courses
                .Include(course => course.Competences)
                .ThenInclude(cc => cc.Competence)
                .ToList();

            for (int i = 0; i < Sequence.Count; i++)
            {
                var updatedCourse = allCourses.Find(c => c.Id == Sequence[i].Id);
                if (updatedCourse != null)
                {
                    var newCourse = new CourseChromosome()
                    {
                        Id = updatedCourse.Id,
                        Title = updatedCourse.Title,
                        ECTS = updatedCourse.ECTS,
                        ChairId = updatedCourse.ChairId,
                        TeacherId = updatedCourse.TeacherId,
                        RequiredCompetences = updatedCourse.Competences.Select(cc => cc.Competence).ToList(),
                        Prerequisites = updatedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()
                    };

                    Sequence[i] = newCourse;
                }
            }

            CompetenceFitness = CalculateCompetenceFitness();
        }

        private int CalculateCompetenceFitness()
        {
            int totalFitness = 0;

            foreach (var course in Sequence)
            {
                var matchedCompetences = course.RequiredCompetences.Intersect(StudentCompetences).Count();
                totalFitness += matchedCompetences * 5; // Припущення: кожний співпадаючий курс добавляє 5 до фітнесу
            }

            return totalFitness;
        }

        public int CalculateLoadDistribution()
        {
            // Крок 2.2: Розрахунок штрафу за розходження між фактичним і ідеальним навантаженням
            var semesterECTS = new Dictionary<string, int>();
            foreach (var course in Sequence)
            {
                string key = $"{course.Year}-{course.Semester}";
                if (!semesterECTS.ContainsKey(key))
                    semesterECTS[key] = 0;

                semesterECTS[key] += course.ECTS;
            }

            // Визначення ідеального розподілу навантаження
            int idealECTS = semesterECTS.Values.Sum() / semesterECTS.Count;
            int loadVariancePenalty = semesterECTS.Values.Select(x => Math.Abs(x - idealECTS)).Sum();

            return -loadVariancePenalty; // Мінус означає штраф за велике відхилення від ідеального розподілу
        }

        public void AllocateCoursesWithPrerequisites()
        {
            var graph = new Dictionary<int, List<int>>();
            var inDegree = new Dictionary<int, int>();
            var courseIndexMap = new Dictionary<int, int>();

            // Initialize graph structure and inDegree map
            for (int index = 0; index < Sequence.Count; index++)
            {
                var course = Sequence[index];
                courseIndexMap[course.Id] = index;

                if (!graph.ContainsKey(course.Id))
                {
                    graph[course.Id] = new List<int>();
                    inDegree[course.Id] = 0;
                }

                foreach (var prereq in course.Prerequisites)
                {
                    if (!graph.ContainsKey(prereq))
                    {
                        graph[prereq] = new List<int>();
                        inDegree[prereq] = 0;
                    }
                    graph[prereq].Add(course.Id);
                    inDegree[course.Id]++;
                }
            }

            var queue = new Queue<int>();
            foreach (var key in inDegree)
            {
                if (key.Value == 0)
                    queue.Enqueue(key.Key);
            }

            int semester = 1, year = 1;
            while (queue.Count > 0)
            {
                int size = queue.Count;
                for (int i = 0; i < size; i++)
                {
                    var courseId = queue.Dequeue();
                    var courseIndex = courseIndexMap[courseId];
                    var course = Sequence[courseIndex];

                    course.Year = year;
                    course.Semester = semester;
                    Sequence[courseIndex] = course;

                    foreach (var neighbor in graph[courseId])
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }

                semester++;
                if (semester > 2)
                {
                    semester = 1;
                    year++;
                }
            }
        }

        public int GetTotalECTS()
        {
            var total = 0;

            for (int i = 0; i < Sequence.Count(); i++)
            {
                var fromCourse = Sequence[i];

                total += fromCourse.ECTS;
            }

            return total;
        }

        public int IsTeacherSelected(List<Teacher> teachers)
        {
            var totalSelected = 0;
            for (int i = 0; i < Sequence.Count(); i++)
            {
                for (int j = 0; j < teachers.Count; j++)
                {
                    if (Sequence[i].TeacherId.Equals(teachers[j].Id))
                    {
                        totalSelected++;
                    }
                }
            }
            return totalSelected;
        }

        public int IsChairSelected(List<Chair> chairs)
        {
            var totalSelected = 0;
            for (int i = 0; i < Sequence.Count(); i++)
            {
                for (int j = 0; j < chairs.Count; j++)
                {
                    if (Sequence[i].ChairId.Equals(chairs[j].Id))
                    {
                        totalSelected++;
                    }

                }
            }
            return totalSelected;
        }

        // додати в сек обовязкові курси
        // перевірка на ектс???
        public void AddCourseWithPrerequisites(Course course, HashSet<int> selectedCoursesIds, List<CourseChromosome> result)
        {
            if (selectedCoursesIds.Contains(course.Id))
                return;

            // Спочатку додаємо пререквізити
            foreach (var prerequisite in course.Prerequisites)
            {
                Course prereqCourse = _dataContext.Courses.Find(prerequisite.PrerequisiteId);
                if (prereqCourse != null)
                    AddCourseWithPrerequisites(prereqCourse, selectedCoursesIds, result);
            }

            // Додаємо себе
            selectedCoursesIds.Add(course.Id);
            result.Add(new CourseChromosome
            {
                Id = course.Id,
                Title = course.Title,
                ECTS = course.ECTS,
                ChairId = course.ChairId,
                TeacherId = course.TeacherId,
                RequiredCompetences = course.Competences.Select(cc => cc.Competence).ToList(),
                Prerequisites = course.Prerequisites.Select(p => p.PrerequisiteId).ToList()
            });
        }

        public void Generate()
        {
            var allCourses = _dataContext.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Chair)
                .Include(course => course.Competences).ThenInclude(courseCompetence => courseCompetence.Competence)
                .Include(course => course.Prerequisites)
                .ToList();

            var selectedCoursesIds = new HashSet<int>();
            var result = new List<CourseChromosome>();

            while (result.Count < _length)
            {
                int index = Random.Next(0, allCourses.Count);
                Course selectedCourse = allCourses[index];

                AddCourseWithPrerequisites(selectedCourse, selectedCoursesIds, result);

                // Обмеження за кількістю курсів
                if (result.Count >= _length)
                    break;
            }

            Sequence = result;
        }
    }
}
