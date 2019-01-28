using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TechTree.DTO;

namespace TechTree.Output
{
    class ImageOutput
    {

        /// <summary>
        /// Transforms the stellaris DDS images into pngs.
        /// </summary>
        /// <param name="ddsDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="entities"></param>
        public static void transformAndOutputImages(string ddsDir, string outputDir, IEnumerable<Entity> entities)
        {
            // make sure we can output
            Directory.CreateDirectory(outputDir);

            foreach(var entity in entities)
            {
                var filePath = Path.Combine(ddsDir, entity.Icon + ".dds");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(ddsDir, entity.Id + ".dds");
                }

                var outputFilePath = Path.Combine(outputDir, entity.Id + ".png");
                if (File.Exists(filePath))
                {                    
                    var ddsImage = Pfim.Pfim.FromFile(filePath);
                    if (ddsImage.Compressed)
                    {
                        ddsImage.Decompress();
                    }
                    if (ddsImage.Format == Pfim.ImageFormat.Rgba32)
                    {
                        Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height).Save(outputFilePath);
                    }
                    else if (ddsImage.Format == Pfim.ImageFormat.Rgb24)
                    {
                        Image.LoadPixelData<Bgr24>(ddsImage.Data, ddsImage.Width, ddsImage.Height).Save(outputFilePath);
                    }
                    else
                    {
                        Debug.WriteLine("Image " + filePath + " had unknown format " + ddsImage.Format);
                        Console.WriteLine("Image " + filePath + " had unknown format " + ddsImage.Format);
                    }
                }
                else
                {
                    Debug.WriteLine("No file " + filePath + " found");
                    Console.WriteLine("No file " + filePath + " found");
                }
            }
        }
    }
}
