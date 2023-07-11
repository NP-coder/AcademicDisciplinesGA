namespace AcademicDisciplinesGA.GA
{
    public static class GAConfig
    {
        public static int MaxGenerations => 1000;
        public static double MutationChance => 0.05;
        public static int PopulationCount => 20;
        public static int MaxNoImprovementCount => 20;
    }
}
