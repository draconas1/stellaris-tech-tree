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

            foreach (var entity in entities)
            {
                var outputFilePath = Path.Combine(outputDir, entity.Id + ".png");
                if (File.Exists(outputFilePath))
                {
                    entity.IconFound = true;
                    continue;
                }

                var filePath = Path.Combine(ddsDir, entity.Icon + ".dds");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(ddsDir, entity.Id + ".dds");
                }

                if (File.Exists(filePath))
                {
                    var ddsImage = Pfim.Pfim.FromFile(filePath);
                    if (ddsImage.Compressed)
                    {
                        ddsImage.Decompress();
                    }

                    entity.IconFound = transformAndOutputImage(filePath, outputFilePath);
                }
                else
                {
                    Debug.WriteLine("No file " + filePath + " found");
                    Console.WriteLine("No file " + filePath + " found");
                }
            }
        }

        public static bool transformAndOutputImage(string inputPath, string outputPath)
        {
            // make sure we can output
            if (File.Exists(inputPath))
            {
                var ddsImage = Pfim.Pfim.FromFile(inputPath);
                if (ddsImage.Compressed)
                {
                    ddsImage.Decompress();
                }
                if (ddsImage.Format == Pfim.ImageFormat.Rgba32)
                {
                    Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height).Save(outputPath);
                    return true;
                }
                else if (ddsImage.Format == Pfim.ImageFormat.Rgb24)
                {
                    Image.LoadPixelData<Bgr24>(ddsImage.Data, ddsImage.Width, ddsImage.Height).Save(outputPath);
                    return true;
                }
                else
                {
                    Debug.WriteLine("Image " + inputPath + " had unknown format " + ddsImage.Format);
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("No file " + inputPath + " found");
                return false;
            }
        }
    }
}
