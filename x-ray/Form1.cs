using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Emit;
using Emgu.CV.Util; // Add this line for CvInvoke

//using System.Drawing.Imaging;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using ColorMapType = Emgu.CV.CvEnum.ColorMapType;
using static System.Net.Mime.MediaTypeNames;
namespace x_ray
{
    public partial class Form1 : Form
    {
        private Rectangle selectedArea;
        private bool isSelecting = false;
        private Point startPoint;
        private Pen selectionPen = new Pen(Color.Red, 2);
        private Bitmap originalImage;
        int windowWidth;
        int windowHeight;
        double widthRatio;
        double heightRatio;
        //private ColorMap[] colorMap;
        //private Image<Bgr, byte> originalImage;
        ColorMapType selectedMap;

        public Form1()
        {
            InitializeComponent();
            //comboBox1.Items.AddRange(new string[] { "Grayscale", "Red-to-Blue", "Hot", "Inverted Grayscale", "Rainbow" });
            List<ColorMapType> excludedColorMaps = new List<ColorMapType>
            {
             ColorMapType.Autumn,
             ColorMapType.Bone,
             ColorMapType.Jet,
             ColorMapType.Hot,
             ColorMapType.Cool,
             ColorMapType.Spring,
             ColorMapType.Summer,
             ColorMapType.Winter,
             ColorMapType.Rainbow,
             ColorMapType.Ocean,
             ColorMapType.Hsv,
             ColorMapType.Pink,
             ColorMapType.Magma,
             ColorMapType.Inferno,
             ColorMapType.Plasma
            };

            foreach (ColorMapType mapType in Enum.GetValues(typeof(ColorMapType)))
            {
                if (excludedColorMaps.Contains(mapType))
                {
                    comboBox1.Items.Add(mapType);
                }
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.png)|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.BorderStyle = BorderStyle.Fixed3D;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                pictureBox.Image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                originalImage = new Bitmap(pictureBox.Image);
                windowWidth = pictureBox1.Width;
                windowHeight = pictureBox1.Height;

                widthRatio = (double)originalImage.Width / windowWidth;
                heightRatio = (double)originalImage.Height / windowHeight;


            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox.Image != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    originalImage = new Bitmap(pictureBox.Image);

                    isSelecting = true;
                    selectedArea = new Rectangle(e.Location, new Size(0, 0));
                    startPoint = e.Location;
                }
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {

            if (isSelecting && e.Button == MouseButtons.Left)
            {

                int x = (int)(Math.Min(e.X, startPoint.X) * widthRatio);
                int y = (int)(Math.Min(e.Y, startPoint.Y) * heightRatio);
                int width = (int)(Math.Abs(startPoint.X - e.X) * widthRatio);
                int height = (int)(Math.Abs(startPoint.Y - e.Y) * heightRatio);
                selectedArea.Location = new Point(x, y);
                selectedArea.Size = new Size(width, height);
                label1.Text = $"mousemove\n x:{x},Y:{y},\nimage information:\nsize:{originalImage.Width},heg:{originalImage.Height}";

                pictureBox.Invalidate();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = false;
                pictureBox1.Refresh();
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
           
            if (pictureBox.Image != null)
            {
                Rectangle r = new Rectangle();
                r.Width = (int)(selectedArea.Size.Width / widthRatio);
                r.Height = (int)(selectedArea.Size.Height / heightRatio);
                r.X = (int)(selectedArea.Location.X / widthRatio);
                r.Y = (int)(selectedArea.Location.Y / heightRatio);


                if (isSelecting)
                {
                    e.Graphics.DrawRectangle(selectionPen, r);
                }
                else if (selectedArea.Width > 0 && selectedArea.Height > 0)
                {
                    e.Graphics.DrawRectangle(selectionPen, r);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (originalImage != null)
            {
                selectedMap = (Emgu.CV.CvEnum.ColorMapType)comboBox1.SelectedItem;

            }
        }





        private void button2_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {


                Bitmap bitmap = new Bitmap(originalImage);

                //// Draw the original image onto the new bitmap
                //using (Graphics g = Graphics.FromImage(bitmap))
                //{
                //    g.DrawImage(originalImage, new Point(0, 0));
                //}
                // Replace with your chosen ColorMapType
                ColorMapType selectedColorMapType = selectedMap;
                //Image<Bgr, byte> originalImage = pictureBox.Image;

                for (int x = selectedArea.Location.X; x < selectedArea.Location.X + selectedArea.Width; x++)
                {
                    for (int y = selectedArea.Location.Y; y < selectedArea.Location.Y + selectedArea.Height; y++)
                    {
                        Console.WriteLine(x.ToString(), y);
                        if (selectedArea.Contains(x, y)) // Check if within image bounds
                        {

                            Color originalColor = originalImage.GetPixel(x, y);
                            Color newColor = ApplyColorMap(originalColor, selectedColorMapType);
                            bitmap.SetPixel(x, y, newColor);

                        }
                    }
                }
                pictureBox1.Visible = true;
                button3.Visible = true;
                pictureBox1.Image = bitmap;

                pictureBox1.Refresh();
                pictureBox1.Invalidate();
            }
            else MessageBox.Show("please enter image");
        }

        private Color ApplyColorMap(Color originalColor, ColorMapType colorMapType)
        {

            switch (colorMapType)
            {
                case ColorMapType.Autumn:
                    return Color.FromArgb(originalColor.R, originalColor.G, 255 - originalColor.B);
                case ColorMapType.Bone:
                    int grayScale = (int)(0.2989 * originalColor.R + 0.5870 * originalColor.G + 0.1140 * originalColor.B);
                    return Color.FromArgb(grayScale, grayScale, grayScale);
                case ColorMapType.Jet:
                    double hue = 240.0 * (1.0 - (0.2989 * originalColor.R + 0.5870 * originalColor.G + 0.1140 * originalColor.B) / 255.0);
                    return Color.FromArgb(
                        (int)(255 * (1 + Math.Cos((hue - 120) * Math.PI / 180)) / 2),
                        (int)(255 * (1 + Math.Cos(hue * Math.PI / 180)) / 2),
                        (int)(255 * (1 + Math.Cos((hue + 120) * Math.PI / 180)) / 2));
                case ColorMapType.Hot:
                    return Color.FromArgb(
                        Math.Min(255, (int)(originalColor.R * 1.6)),
                        Math.Min(255, (int)(originalColor.G * 0.75)),
                        Math.Min(255, (int)(originalColor.B * 0.4)));
                case ColorMapType.Cool:
                    return Color.FromArgb(
                        255 - originalColor.B,
                        originalColor.G,
                        255 - originalColor.R);
                case ColorMapType.Spring:
                    return Color.FromArgb(
                        originalColor.R,
                        255 - originalColor.B,
                        originalColor.G);
                case ColorMapType.Summer:
                    return Color.FromArgb(
                        (int)(originalColor.R * 0.5 + originalColor.G * 0.4),
                        (int)(originalColor.G * 0.5 + originalColor.B * 0.4),
                        (int)(originalColor.B * 0.4));
                case ColorMapType.Winter:
                    return Color.FromArgb(
                        originalColor.B,
                        0,
                        originalColor.R);
                case ColorMapType.Rainbow:
                    double hue2 = 360.0 * (0.2989 * originalColor.R + 0.5870 * originalColor.G + 0.1140 * originalColor.B) / 765.0;
                    return Color.FromArgb(
                        (int)(255 * (1 + Math.Cos((hue2 - 120) * Math.PI / 180)) / 2),
                        (int)(255 * (1 + Math.Cos(hue2 * Math.PI / 180)) / 2),
                        (int)(255 * (1 + Math.Cos((hue2 + 120) * Math.PI / 180)) / 2));
                case ColorMapType.Ocean:
                    return Color.FromArgb(
                        (int)(originalColor.B * 0.5 + originalColor.G * 0.5),
                        (int)(originalColor.G * 0.5),
                        originalColor.B);
                case ColorMapType.Hsv:
                    double h = 240.0 * (0.2989 * originalColor.R + 0.5870 * originalColor.G + 0.1140 * originalColor.B) / 765.0;
                    double s = Math.Max(0.0, 1.0 - 3.0 * Math.Min(originalColor.R, Math.Min(originalColor.G, originalColor.B)) / (originalColor.R + originalColor.G + originalColor.B));
                    double v = (originalColor.R + originalColor.G + originalColor.B) / 765.0;
                    return Color.FromArgb(
                        (int)(255 * (1 + Math.Cos((h - 120) * Math.PI / 180)) / 2),
                        (int)(255 * s),
                        (int)(255 * v));
                case ColorMapType.Pink:
                    return Color.FromArgb(
                        originalColor.R,
                        (int)(originalColor.G * 0.6 + originalColor.B * 0.4),
                        (int)(originalColor.B * 0.6 + originalColor.R * 0.4));
                case ColorMapType.Magma:
                    return Color.FromArgb(
                        Math.Min(255, (int)(originalColor.R * 2.2)),
                        Math.Min(255, (int)(originalColor.G * 1.1)),
                        Math.Min(255, (int)(originalColor.B * 0.5)));
                case ColorMapType.Inferno:
                    return Color.FromArgb(
                        Math.Min(255, (int)(originalColor.R * 2.2)),
                        Math.Min(255, (int)(originalColor.G * 1.1)),
                        Math.Min(255, (int)(originalColor.B * 0.5)));
                case ColorMapType.Plasma:
                    return Color.FromArgb(
                        Math.Min(255, (int)(originalColor.R * 2.2)),
                        Math.Min(255, (int)(originalColor.G * 1.1)),
                        Math.Min(255, (int)(originalColor.B * 0.5)));
                case ColorMapType.Viridis:
                    return Color.FromArgb(
                        Math.Min(255, (int)(originalColor.R * 1.5)),
                        Math.Min(255, (int)(originalColor.G * 1.2)),
                        Math.Min(255, (int)(originalColor.B * 0.8)));
                default:
                    return originalColor;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "jpg file|*.jpg|png file|*.png";
            sfd.Title = "save image";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
