using AcademicDisciplinesGA.GA;
using AcademicDisciplinesGA.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicDisciplinesGA.Helpers
{
    public class PopulationHelper
    {
        private static readonly Random random = new Random();

        public static List<DisciplinesChromosome> SpawnPopulation(
            ApplicationDbContext dataContext, List<Competence> competences)
        {
            var population = new HashSet<DisciplinesChromosome>();

            int remainingCount = GAConfig.PopulationCount;
            while (remainingCount > 0)
            {
                var individuals = Enumerable.Range(0, remainingCount)
                                            .Select(i => new DisciplinesChromosome(dataContext, competences))
                                            .ToList();

                foreach (var individual in individuals)
                {
                    population.Add(individual);
                }

                remainingCount = GAConfig.PopulationCount - population.Count;
            }
            return population.ToList();
        }

        public static (DisciplinesChromosome, DisciplinesChromosome) GetCandidateParents(List<DisciplinesChromosome> population)
        {
            var candidateA = population[random.Next(GAConfig.PopulationCount)];
            var candidateB = population[random.Next(GAConfig.PopulationCount)];

            while (candidateA == candidateB)
            {
                candidateB = population[random.Next(GAConfig.PopulationCount)];
            }

            return (candidateA, candidateB);
        }

        public static DisciplinesChromosome TournamentSelection(DisciplinesChromosome candidateA, DisciplinesChromosome candidateB)
        {
            if (candidateA.Rank <= candidateB.Rank)
            {
                return candidateA;
            }
            else
            {
                return candidateB;
            }

        }

        //public static DisciplinesChromosome DoCrossover(DisciplinesChromosome individualA, DisciplinesChromosome individualB, List<Competence> competences)
        //{
        //    var offspringSequence = new List<CourseChromosome>();
        //    var appeared = new HashSet<int>();
        //    int currentECTS = 0;
        //    int maxECTS = 50;

        //    // Сначала добавляем обов'язкові курси
        //    //AddMandatoryCourses(offspringSequence, individualA, individualB, appeared, ref currentECTS, maxECTS);

        //    // Потім добавляємо курси за допомогу уніформного схрещування
        //    for (int i = 0; i < individualA.Sequence.Count; i++)
        //    {
        //        if (currentECTS >= maxECTS)
        //            break;

        //        CourseChromosome course = random.NextDouble() > 0.5 ? individualA.Sequence[i] : individualB.Sequence[i];

        //        if (!appeared.Contains(course.Id) && currentECTS + course.ECTS <= maxECTS)
        //        {
        //            if (ArePrerequisitesMet(course, offspringSequence))
        //            {
        //                offspringSequence.Add(course);
        //                appeared.Add(course.Id);
        //                currentECTS += course.ECTS;
        //            }
        //        }
        //    }

        //    return new DisciplinesChromosome(offspringSequence, competences);
        //}

        public static DisciplinesChromosome DoCrossover(DisciplinesChromosome individualA, DisciplinesChromosome individualB, List<Competence> competences)
        {
            var offspringSequence = new List<CourseChromosome>();
            var appeared = new HashSet<int>();
            int currentECTS = 0;
            int maxECTS = 50;

            int minSequenceLength = Math.Min(individualA.Sequence.Count, individualB.Sequence.Count);

            // Перемішування індексів, щоб уніформне схрещування було більш випадковим
            List<int> indices = Enumerable.Range(0, minSequenceLength).OrderBy(x => random.Next()).ToList();

            foreach (int index in indices)
            {
                if (currentECTS >= maxECTS)
                    break;

                CourseChromosome chosenCourse = random.NextDouble() > 0.5 ? individualA.Sequence[index] : individualB.Sequence[index];

                if (!appeared.Contains(chosenCourse.Id) && currentECTS + chosenCourse.ECTS <= maxECTS)
                {
                    if (ArePrerequisitesMet(chosenCourse, offspringSequence))
                    {
                        offspringSequence.Add(chosenCourse);
                        appeared.Add(chosenCourse.Id);
                        currentECTS += chosenCourse.ECTS;
                    }
                }
            }

            // Якщо потрібно, можна додати залишок курсів з батьківських хромосом, якщо вони відповідають критеріям
            return new DisciplinesChromosome(offspringSequence, competences);
        }


        private static void AddMandatoryCourses(List<CourseChromosome> offspringSequence, DisciplinesChromosome individualA, DisciplinesChromosome individualB, HashSet<int> appeared, ref int currentECTS, int maxECTS)
        {
            foreach (var individual in new[] { individualA, individualB })
            {
                foreach (var course in individual.Sequence)
                {
                    if (course.RequiredCompetences.Any(c => c.IsMandatory) && !appeared.Contains(course.Id) && currentECTS + course.ECTS <= maxECTS)
                    {
                        if (ArePrerequisitesMet(course, offspringSequence))
                        {
                            offspringSequence.Add(course);
                            appeared.Add(course.Id);
                            currentECTS += course.ECTS;
                        }
                    }
                }
            }
        }

        private static bool ArePrerequisitesMet(CourseChromosome course, List<CourseChromosome> offspringSequence)
        {
            foreach (var prereqId in course.Prerequisites)
            {
                if (!offspringSequence.Any(c => c.Id == prereqId))
                {
                    return false;
                }
            }
            return true;
        }

        public static DisciplinesChromosome DoMutate(DisciplinesChromosome individual, ApplicationDbContext dataContext, List<Competence> competences)
        {
            var courses = dataContext.Courses
                .Include(course => course.Teacher)
                .Include(course => course.Chair)
                .Include(course => course.Competences).ThenInclude(courseCompetence => courseCompetence.Competence)
                .Include(course => course.Prerequisites)
                .ToList();

            var sequence = individual.Sequence;
            int randomIndex = random.Next(0, sequence.Count);
            int newCourseId;
            CourseChromosome newCourse;

            do
            {
                newCourseId = random.Next(courses.First().Id, courses.Last().Id + 1);
                var selectedCourse = courses.Find(c => c.Id.Equals(newCourseId));
                newCourse = new CourseChromosome()
                {
                    Id = selectedCourse.Id,
                    Title = selectedCourse.Title,
                    ECTS = selectedCourse.ECTS,
                    ChairId = selectedCourse.ChairId,
                    TeacherId = selectedCourse.TeacherId,
                    RequiredCompetences = selectedCourse.Competences.Select(cc => cc.Competence).ToList(),
                    Prerequisites = selectedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()
                };
            }
            while (sequence.Contains(newCourse));

            sequence[randomIndex] = newCourse;

            return new DisciplinesChromosome(sequence, competences);
        }

        //public static DisciplinesChromosome DoMutate(DisciplinesChromosome individual, ApplicationDbContext dataContext, List<Competence> competences)
        //{
        //    var courses = dataContext.Courses.Include(c => c.Prerequisites).ToList();
        //    var sequence = new List<CourseChromosome>(individual.Sequence);

        //    int randomIndex = random.Next(sequence.Count);
        //    CourseChromosome randomCourse = sequence[randomIndex];
        //    int newCourseId;

        //    do
        //    {
        //        newCourseId = courses[random.Next(courses.Count)].Id;
        //    }
        //    while (sequence.Any(c => c.Id == newCourseId) || courses.FirstOrDefault(c => c.Id == newCourseId).Prerequisites.Any(p => !sequence.Any(sc => sc.Id == p.PrerequisiteId)));

        //    var selectedCourse = courses.Find(c => c.Id == newCourseId);
        //    sequence[randomIndex] = new CourseChromosome()
        //    {
        //        Id = selectedCourse.Id,
        //        Title = selectedCourse.Title,
        //        ECTS = selectedCourse.ECTS,
        //        ChairId = selectedCourse.ChairId,
        //        TeacherId = selectedCourse.TeacherId,
        //        RequiredCompetences = selectedCourse.Competences.Select(cc => cc.Competence).ToList(),
        //        Prerequisites = selectedCourse.Prerequisites.Select(p => p.PrerequisiteId).ToList()
        //    };

        //    return new DisciplinesChromosome(sequence, competences);
        //}

        public static (DisciplinesChromosome, DisciplinesChromosome) Mutate(DisciplinesChromosome individualA, DisciplinesChromosome individualB, ApplicationDbContext dataContext, List<Competence> competences)
        {
            var newIndividualA = new DisciplinesChromosome(individualA.Sequence, competences);
            var newindividualB = new DisciplinesChromosome(individualB.Sequence, competences);

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                // newIndividualA = DoMutate(individualA, dataContext, competences);
            }

            if (random.NextDouble() < GAConfig.MutationChance)
            {
                // newindividualB = DoMutate(individualB, dataContext, competences);
            }

            return (newIndividualA, newindividualB);
        }

    }
}
