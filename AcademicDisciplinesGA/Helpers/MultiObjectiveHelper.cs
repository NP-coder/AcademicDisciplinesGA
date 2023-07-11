using AcademicDisciplinesGA.GA;

namespace AcademicDisciplinesGA.Helpers
{
    public static class MultiObjectiveHelper
    {
        public static void UpdatePopulationFitness(List<DisciplinesChromosome> population)
        {
            foreach (var individual in population)
            {
                individual.Rank = -1;
            }

            CalculateRank(population);
        }

        private static void CalculateRank(List<DisciplinesChromosome> population)
        {
            var currentFront = new List<DisciplinesChromosome>();
            var individualsDominated = new Dictionary<DisciplinesChromosome, List<DisciplinesChromosome>>();
            var individualDominationCount = new Dictionary<DisciplinesChromosome, int>();

            foreach (var individualA in population)
            {
                individualsDominated.Add(individualA, new List<DisciplinesChromosome>());
                individualDominationCount.Add(individualA, 0);

                foreach (var individualB in population)
                {
                    if (individualA == individualB)
                    {
                        continue;
                    }

                    if (Dominates(individualB, individualA))
                    {
                        individualDominationCount[individualA]++;
                    }
                    else if (Dominates(individualA, individualB))
                    {
                        individualsDominated[individualA].Add(individualB);
                    }
                }

                if (individualDominationCount[individualA] == 0)
                {
                    individualA.Rank = 0;
                    currentFront.Add(individualA);
                }
            }

            var i = 0;
            while (currentFront.Any())
            {
                var nextFront = new List<DisciplinesChromosome>();
                foreach (var individualA in currentFront)
                {
                    foreach (var individualB in individualsDominated[individualA])
                    {
                        individualDominationCount[individualB]--;
                        if (individualDominationCount[individualB] == 0)
                        {
                            individualB.Rank = i + 1;
                            nextFront.Add(individualB);
                        }
                    }
                }
                i++;

                currentFront = nextFront;
            }
        }

        public static bool Dominates(DisciplinesChromosome a, DisciplinesChromosome b)
        {
            if (a.ECTSCount == 60 &&
                a.TeacherFitness > b.TeacherFitness &&
                a.ChairFitness > b.ChairFitness)
            {
                return true;
            }

            return false;
        }

        internal static float CalculateArea(IOrderedEnumerable<DisciplinesChromosome> firstRank)
        {
            var slices = GetSlices(firstRank);

            return slices.Sum(s => s.Area);
        }

        private static IEnumerable<Slice> GetSlices(IOrderedEnumerable<DisciplinesChromosome> firstRank)
        {
            var previousSlice = new Slice(0, 0, 0, 0);

            foreach(var individual in firstRank)
            {
                previousSlice = new Slice(previousSlice.XUpper, 0, individual.TeacherFitness, individual.ChairFitness);
                yield return previousSlice;
            }
        }
    }
}
