using PathApproximation;

namespace PathApproximationWinFormApp
{
    public static class Program
    {

        public static List<PointLog> Snake = new List<PointLog> {
            new(new(0, 0), TimeSpan.FromMinutes(0)),//
            new(new(1, 0), TimeSpan.FromMinutes(4)),//
            new(new(2, 1), TimeSpan.FromMinutes(5)),//
            new(new(2, 2), TimeSpan.FromMinutes(6)),//
            new(new(1, 4), TimeSpan.FromMinutes(7)),//
            new(new(0, 5), TimeSpan.FromMinutes(8)),//
            new(new(1, 6), TimeSpan.FromMinutes(9)),//
            new(new(2, 6), TimeSpan.FromMinutes(10)),
            new(new(3, 6), TimeSpan.FromMinutes(11)),//
            new(new(4, 7), TimeSpan.FromMinutes(12)),//
            new(new(4, 8), TimeSpan.FromMinutes(13)),//
            new(new(3, 8), TimeSpan.FromMinutes(14)),
            new(new(2, 8), TimeSpan.FromMinutes(15)),
            new(new(1, 8), TimeSpan.FromMinutes(16)),//
        };

        [STAThread]
        static void Main()
        {
            int imageWidth = 400;
            int imageHeight = 400;
            string outputPath = "polyline.png";

            var approximizer = new Approximizer();
            var cornerpoints = approximizer.FilterCornerpoints(Snake);
            var approximatedPath = approximizer.Approximate(Snake, 0.1d);

            ImageDisplayForm.DrawCenteredPolyline(Snake, approximatedPath, cornerpoints, imageWidth, imageHeight, outputPath);

            Application.EnableVisualStyles();
            Application.Run(new ImageDisplayForm(outputPath));
        }
    }
}