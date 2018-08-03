using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaserSpriteSheetUnpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            //local variables
            dynamic sprites;
            string jsonText;
            Bitmap bitmap;
            //
            Console.WriteLine("Phaser Sprite Sheet Unpacker v1.0 - (C)2016 Marcelo Lv Cabral");
            Console.WriteLine("Extracts each frame from the sprite sheet as a separare png");
            Console.WriteLine("");

            if (args.Length >= 1)
            {
                try
                {
                    jsonText = System.IO.File.ReadAllText(args[0]);
                    sprites = JsonConvert.DeserializeObject(jsonText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading/deserializing the json file: {0}", ex.Message);
                    return;
                }

                string fileName = string.Empty;
                if (args.Length >= 2)
                {
                    fileName = args[1];
                }
                else
                {
                    try
                    {
                        fileName = sprites.meta.image.Value;
                        if (!File.Exists(fileName))
                        {
                            fileName = args[0].Substring(0, args[0].LastIndexOf('/') + 1) + fileName;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                try
                {
                    bitmap = Image.FromFile(fileName) as Bitmap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading the png file: {0}", ex.Message);
                    Console.ReadKey();
                    return;
                }

                var folderPath = GetFolderPath(args);
                Directory.CreateDirectory(folderPath);

                try
                {
                    foreach (var item in sprites.frames)
                    {
                        var frame = item.Value.frame;
                        Rectangle cropRect = new Rectangle((int)frame.x.Value, (int)frame.y.Value, (int)frame.w.Value, (int)frame.h.Value);

                        Bitmap sprite = CropImage(bitmap, cropRect);

                        var trimmed = (bool)item.Value.trimmed.Value;
                        if (trimmed)
                        {
                            var spriteSourceSize = item.Value.spriteSourceSize;
                            var position = new Point((int)spriteSourceSize.x.Value, (int)spriteSourceSize.y.Value);

                            var sourceSize = item.Value.sourceSize;
                            var size = new Point((int)sourceSize.w.Value, (int)sourceSize.h.Value);

                            sprite = PlaceImage(sprite, position, size);
                        }

                        string filePath = GetFilePath(folderPath, (string)item.Name);
                        Console.WriteLine(filePath);

                        sprite.Save(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error unpacking the frames: {0}", ex.Message);
                    return;
                }
            }
            else
            {
                Help();
            }

            #if DEBUG
                Console.ReadKey();
            #endif
        }

        private static string GetFolderPath(string[] args)
        {
            string tempPath;
            if (args.Length >= 3)
            {
                tempPath = args[2];
            }
            else
            {
                tempPath = args[0].Substring(0, args[0].LastIndexOf('.'));
            }

            int index = 0;
            string folderPath = tempPath;
            while (File.Exists(folderPath) || Directory.Exists(folderPath))
            {
                index++;
                folderPath = tempPath + " (" + index + ")";
            }

            return folderPath;
        }

        private static Bitmap CropImage(Bitmap src, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using(Graphics g = Graphics.FromImage(target))
            {
               g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), 
                                cropRect,                        
                                GraphicsUnit.Pixel);
            }
            return target;
        }

        private static Bitmap PlaceImage(Bitmap src, Point position, Point size)
        {
            Bitmap target = new Bitmap(size.X, size.Y);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, size.X, size.Y),
                                 new Rectangle(-position.X, -position.Y, size.X, size.Y),
                                 GraphicsUnit.Pixel);
            }
            return target;
        }

        private static string GetFilePath(string folderPath, string fileName)
        {
            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            string filePath;

            if (fileName.Length > 3 && fileName.Substring(fileName.Length - 4) == ".png")
            {
                filePath = Path.Combine(folderPath, fileName);
            }
            else
            {
                filePath = Path.Combine(folderPath, fileName + ".png");
            }

            return filePath.Replace('\\', '/');
        }

        private static void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("pssu <json path> <png path> <output folder>");
            Console.WriteLine("");
        }
    }
}
