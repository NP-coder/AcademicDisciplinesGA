namespace AcademicDisciplinesGA.Helpers
{
    public struct Slice
    {
        public Slice(float xLower, float yLower, float xUpper, float yUpper)
        {
            XLower = xLower;
            YLower = yLower;
            XUpper = xUpper;
            YUpper = yUpper;
        }

        public float XLower { get; }
        public float YLower { get; }
        public float XUpper { get; }
        public float YUpper { get; }
        public float Area => (XUpper - XLower) * (YUpper - YLower);

    }
}
