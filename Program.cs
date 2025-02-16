using PathApproximation;
using Point = PathApproximation.Point;

namespace PathApproximationWinFormApp
{
    public static class Program
    {
        private static List<Point> Snake = new List<Point> {
            new(0, 0),
            new(1, 0),
            new(2, 1),
            new(2, 2),
            new(1, 4),
            new(0, 5),
            new(1, 6),
            new(2, 6),
            new(3, 6),
            new(4, 7),
            new(4, 8),
            new(3, 8),
            new(2, 8),
            new(1, 8),
        };

        [STAThread]
        static void Main()
        {
            int imageWidth = 400;
            int imageHeight = 400;
            string outputPath = "polyline.png";

            var approximizer = new Approximizer();
            var cornerpoints = approximizer.FilterCornerpoints(Snake);
            var approximatedPath = approximizer.Approximate(Snake, 1d);

            ImageDisplayForm.DrawCenteredPolyline(
                Snake.Select(p => new PointF((float)p.X, (float)p.Y)).ToList(), 
                approximatedPath.Select(p => new PointF((float)p.X, (float)p.Y)).ToList(), 
                cornerpoints.Select(p => new PointF((float)p.X, (float)p.Y)).ToList(), 
                imageWidth, imageHeight, outputPath);

            Application.EnableVisualStyles();
            Application.Run(new ImageDisplayForm(outputPath));
        }
    }
}