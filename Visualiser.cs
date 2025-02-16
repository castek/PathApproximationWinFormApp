using System.Drawing.Imaging;

namespace PathApproximation
{
    public class ImageDisplayForm : Form
    {
        private PictureBox pictureBox;

        public ImageDisplayForm(string imagePath)
        {
            this.Text = "Polyline Image";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = System.Drawing.Image.FromFile(imagePath),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(pictureBox);
        }

        public static void DrawCenteredPolyline(List<PointF> inputPath, List<PointF> outputPath, List<PointF> cornerpoints, int width, int height, string filePath)
        {
            if (inputPath == null || inputPath.Count < 2)
            {
                Console.WriteLine("Not enough points to draw a polyline.");
                return;
            }

            using Bitmap bitmap = new Bitmap(width, height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            float minX = inputPath.Min(p => p.X);
            float minY = inputPath.Min(p => p.Y);
            float maxX = inputPath.Max(p => p.X);
            float maxY = inputPath.Max(p => p.Y);

            float scaleX = (width * 0.8f) / (maxX - minX);
            float scaleY = (height * 0.8f) / (maxY - minY);
            float scale = Math.Min(scaleX, scaleY);

            float offsetX = width / 2f - ((minX + maxX) / 2f) * scale;
            float offsetY = height / 2f - ((minY + maxY) / 2f) * scale;

            PointF[] transformedPoints = inputPath
                .Select(p => new PointF(p.X * scale + offsetX, p.Y * scale + offsetY))
                .ToArray();

            using Pen blue = new Pen(Color.Blue, 3);
            g.DrawLines(blue, transformedPoints);

            transformedPoints = outputPath
                .Select(p => new PointF(p.X * scale + offsetX, p.Y * scale + offsetY))
                .ToArray();

            using Pen red = new Pen(Color.Red, 3);
            g.DrawLines(red, transformedPoints);

            transformedPoints = cornerpoints
                .Select(p => new PointF(p.X * scale + offsetX, p.Y * scale + offsetY))
                .ToArray();

            using Pen green = new Pen(Color.Green, 3);
            foreach (var point in transformedPoints)
            {
                g.DrawEllipse(green, point.X - 2, point.Y - 2, 4, 4);
            }

            bitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
