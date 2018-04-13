using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageWatermarker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string inputFile = ConfigurationManager.AppSettings["InputFileName"];
                string imageFolder = ConfigurationManager.AppSettings["ImagesFolderPath"];
                string outputFolder = ConfigurationManager.AppSettings["OutputFileDirectory"];

                string fontName = ConfigurationManager.AppSettings["Font"];
                int fontSize = Convert.ToInt32(ConfigurationManager.AppSettings["FontSize"]);
                int alpha = Convert.ToInt32(ConfigurationManager.AppSettings["alpha"]);
                int red = Convert.ToInt32(ConfigurationManager.AppSettings["red"]);
                int green = Convert.ToInt32(ConfigurationManager.AppSettings["green"]);
                int blue = Convert.ToInt32(ConfigurationManager.AppSettings["blue"]);
                int xPos = Convert.ToInt32(ConfigurationManager.AppSettings["HorizontalPosition"]);
                int yPos = Convert.ToInt32(ConfigurationManager.AppSettings["VerticalPosition"]);
                string alignment = ConfigurationManager.AppSettings["StringAlignment"];
                StringAlignment stringAlignment;

                switch (alignment.ToLowerInvariant())
                {
                    case "left":
                        stringAlignment = StringAlignment.Near;
                        break;
                    case "right":
                        stringAlignment = StringAlignment.Far;
                        break;
                    case "center":
                    default:
                        stringAlignment = StringAlignment.Center;
                        break;
                }

                if (!File.Exists(inputFile))
                {
                    Console.WriteLine("Input file does not exist.");
                }
                else if (!Directory.Exists(imageFolder))
                {
                    Console.WriteLine("Image folder does not exist.");
                }
                else if (!Directory.Exists(outputFolder))
                {
                    Console.WriteLine("Output folder does not exist.");
                }
                else
                {
                    List<string> imagesToGenerate = File.ReadAllLines(inputFile).ToList();

                    foreach (string imageToGenerate in imagesToGenerate)
                    {
                        string[] line = imageToGenerate.Split(',');

                        if (line.Count() != 2)
                        {
                            Console.WriteLine("Input line {0} invalid. Must have two parts: watermark text, image file name", imageToGenerate);
                            continue;
                        }

                        string watermarkText = line[0].Trim();
                        string fileName = line[1].Trim();

                        try
                        {
                            FileStream source = new FileStream(Path.Combine(imageFolder, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            Stream output = new MemoryStream();
                            Image img = Image.FromStream(source);

                            Font font = new Font(fontName, fontSize, GraphicsUnit.Pixel);

                            Color color = Color.FromArgb(alpha, red, green, blue);

                            Point pt = new Point(xPos, yPos);
                            SolidBrush brush = new SolidBrush(color);

                            Graphics graphics = Graphics.FromImage(img);
                            graphics.DrawString(watermarkText, font, brush, pt, new StringFormat { Alignment = stringAlignment });
                            graphics.Dispose();

                            img.Save(output, ImageFormat.Png);
                            Image imgFinal = Image.FromStream(output);

                            Bitmap bmp = new Bitmap(img.Width, img.Height, img.PixelFormat);
                            Graphics graphics2 = Graphics.FromImage(bmp);
                            graphics2.DrawImage(imgFinal, 0, 0, imgFinal.Width, imgFinal.Height);
                            bmp.Save(Path.Combine(outputFolder, watermarkText + ".png"), ImageFormat.Png);

                            imgFinal.Dispose();
                            img.Dispose();
                            graphics2.Dispose();

                            Console.WriteLine("Text: {0} - Image: {1} - Complete", watermarkText, fileName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error processing image. Text: {0} - Image: {1}", watermarkText, fileName);
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running program: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
            }

            Console.WriteLine("Processing Complete");
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}
