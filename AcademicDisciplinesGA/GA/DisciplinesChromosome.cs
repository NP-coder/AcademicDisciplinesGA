using AcademicDisciplinesGA.Models;
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

        public static int ECTSKap = 50;

        static readonly Random Random = new Random();

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

        //public void AllocateCoursesWithPrerequisites()
        //{
        //    var graph = new Dictionary<int, List<int>>();
        //    var inDegree = new Dictionary<int, int>();
        //    var courseIndexMap = new Dictionary<int, int>();

        //    // Initialize graph structure and inDegree map
        //    for (int index = 0; index < Sequence.Count; index++)
        //    {
        //        var course = Sequence[index];
        //        courseIndexMap[course.Id] = index;

        //        if (!graph.ContainsKey(course.Id))
        //        {
        //            graph[course.Id] = new List<int>();
        //            inDegree[course.Id] = 0;
        //        }

        //        foreach (var prereq in course.Prerequisites)
        //        {
        //            if (!graph.ContainsKey(prereq))
        //            {
        //                graph[prereq] = new List<int>();
        //                inDegree[prereq] = 0;
        //            }
        //            graph[prereq].Add(course.Id);
        //            inDegree[course.Id]++;
        //        }
        //    }

        //    var queue = new Queue<int>();
        //    foreach (var key in inDegree)
        //    {
        //        if (key.Value == 0)
        //            queue.Enqueue(key.Key);
        //    }

        //    int semester = 1, year = 1;
        //    while (queue.Count > 0)
        //    {
        //        int size = queue.Count;
        //        for (int i = 0; i < size; i++)
        //        {
        //            var courseId = queue.Dequeue();
        //            var courseIndex = courseIndexMap[courseId];
        //            var course = Sequence[courseIndex];

        //            course.Year = year;
        //            course.Semester = semester;
        //            Sequence[courseIndex] = course;

        //            foreach (var neighbor in graph[courseId])
        //            {
        //                inDegree[neighbor]--;
        //                if (inDegree[neighbor] == 0)
        //                    queue.Enqueue(neighbor);
        //            }
        //        }

        //        semester++;
        //        if (semester > 2)
        //        {
        //            semester = 1;
        //            year++;
        //        }
        //    }
        //}

        public void AllocateCoursesWithPrerequisites()
        {
            var graph = new Dictionary<int, List<int>>();
            var inDegree = new Dictionary<int, int>();
            var courseIndexMap = new Dictionary<int, int>();
            var semesterCredits = new Dictionary<int, int>();

            // Визначаємо ідеальну кількість ECTS на семестр
            int totalECTS = Sequence.Sum(course => course.ECTS);
            int idealECTSPerSemester = totalECTS / 8;

            // Ініціалізація структур для графа та семестрів
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

            for (int i = 1; i <= 8; i++) // Приготування словаря для відстеження ECTS по семестру
            {
                semesterCredits[i] = 0;
            }

            var queue = new Queue<int>();
            foreach (var item in inDegree)
            {
                if (item.Value == 0)
                    queue.Enqueue(item.Key);
            }

            int semester = 1;
            while (queue.Count > 0)
            {
                int size = queue.Count;
                for (int i = 0; i < size; i++)
                {
                    var courseId = queue.Dequeue();
                    var course = Sequence[courseIndexMap[courseId]];

                    // Знайти підходящий семестр для розміщення курсу
                    while (semesterCredits[semester] + course.ECTS > idealECTSPerSemester + 5 && semester <= 8)
                    {
                        semester++;
                    }

                    if (semester <= 8)
                    {
                        // Встановлення року та семестру виходячи з номеру семестру
                        course.Year = (semester - 1) / 2 + 1;
                        course.Semester = (semester % 2 == 0) ? 2 : 1;
                        semesterCredits[semester] += course.ECTS;
                        Sequence[courseIndexMap[courseId]] = course;
                    }

                    // Зменшення ступенів вхідності наступників
                    foreach (var neighbor in graph[courseId])
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }

                // Реалокація весів залишків на наступний семестр, якщо nuстигли до кінця
                if (semester > 8)
                {
                    break; // Ми закінчили всі можливі семестри
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
        public int AddCourseWithPrerequisites(Course course, HashSet<int> selectedCoursesIds, List<CourseChromosome> result)
        {
            if (selectedCoursesIds.Contains(course.Id))
                return 0;

            int totalECTS = 0;

            foreach (var prerequisite in course.Prerequisites)
            {
                Course prereqCourse = _dataContext.Courses.Find(prerequisite.PrerequisiteId);
                if (prereqCourse != null)
                    totalECTS += AddCourseWithPrerequisites(prereqCourse, selectedCoursesIds, result);
            }

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

            return totalECTS + course.ECTS;
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
            int currentECTS = 0;
            HashSet<int> triedIndices = new HashSet<int>();

            // Обробка обов'язкових курсів
            //var mandatoryCourses = allCourses.Where(c => c.Competences.Any(cc => cc.Competence.IsMandatory)).ToList();
            //foreach (var mandatoryCourse in mandatoryCourses)
            //{
            //    if (!selectedCoursesIds.Contains(mandatoryCourse.Id) && (currentECTS + mandatoryCourse.ECTS <= ECTSKap))
            //    {
            //        AddCourseWithPrerequisites(mandatoryCourse, selectedCoursesIds, result);
            //        currentECTS += mandatoryCourse.ECTS;
            //    }
            //}

            while (currentECTS < ECTSKap && triedIndices.Count < allCourses.Count)
            {
                int index = Random.Next(0, allCourses.Count);
                if (!triedIndices.Add(index))
                {
                    continue;
                }

                Course selectedCourse = allCourses[index];
                if (!selectedCoursesIds.Contains(selectedCourse.Id))
                {
                    int possibleECTS = AddCourseWithPrerequisites(selectedCourse, selectedCoursesIds, result);
                    if (currentECTS + possibleECTS <= ECTSKap)
                    {
                        currentECTS += possibleECTS;
                    }
                    else
                    {
                        result.RemoveAll(c => c.Id == selectedCourse.Id);
                        selectedCoursesIds.Remove(selectedCourse.Id);
                        break;
                    }
                }
            }

            Sequence = result;
        }
    }
}
