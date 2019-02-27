using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FParsec;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TechTreeCreator.DTO;

namespace TechTreeCreator.Output
{
    class ImageOutput
    {

        /// <summary>
        /// Transforms the stellaris DDS images into pngs.
        /// </summary>
        /// <param name="ddsDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="entities"></param>
        public static void TransformAndOutputImages(string ddsDir, string outputDir, IEnumerable<Entity> entities) {
            TransformAndOutputImages(new[] {ddsDir}, outputDir, entities);
        }
        
        /// <summary>
        /// Transforms the stellaris DDS images into pngs.
        /// </summary>
        /// <param name="ddsDirs">A list of posible locations for the image, that will be checked in order</param>
        /// <param name="outputDir"></param>
        /// <param name="entities"></param>
        public static void TransformAndOutputImages(IEnumerable<string> ddsDirs, string outputDir, IEnumerable<Entity> entities)
        {
            // make sure we can output
            Directory.CreateDirectory(outputDir);
            var inputDirRoots = ddsDirs.ToList();
            foreach (var entity in entities)
            {
                var outputFilePath = Path.Combine(outputDir, entity.Id + ".png");
                if (File.Exists(outputFilePath))
                {
                    entity.IconFound = true;
                    continue;
                }

                string filePath = null;
                // loop voer the possible directories for the file
                foreach (var inputDirRoot in inputDirRoots) {
                    filePath = Path.Combine(inputDirRoot, entity.Icon + ".dds");
                    // happy days, found it based on icon
                    if (File.Exists(filePath)) {
                        break;
                    }
                    else {
                        // sometimes the icon lies!
                        filePath = Path.Combine(inputDirRoot, entity.Id + ".dds");
                        // if we find it break out immideately
                        if (File.Exists(filePath)) {
                            break;
                        }
                    }
                }
               

                if (filePath != null && File.Exists(filePath))
                {
                    entity.IconFound = TransformAndOutputImage(filePath, outputFilePath);
                }
                else
                {
                    Debug.WriteLine("No file " + filePath + " found");
                }
            }
        }

        public static bool TransformAndOutputImage(string inputPath, string outputPath)
        {
            // make sure we can output
            if (File.Exists(inputPath))
            {
                try {
                    var ddsImage = Pfim.Pfim.FromFile(inputPath);
                    if (ddsImage.Compressed) {
                        ddsImage.Decompress();
                    }

                    switch (ddsImage.Format) {
                        case Pfim.ImageFormat.Rgba32:
                            Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height)
                                .Save(outputPath);
                            return true;
                        case Pfim.ImageFormat.Rgb24:
                            Image.LoadPixelData<Bgr24>(ddsImage.Data, ddsImage.Width, ddsImage.Height).Save(outputPath);
                            return true;
                        default:
                            Debug.WriteLine("Image " + inputPath + " had unknown format " + ddsImage.Format);
                            return false;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("Error processing file: " + inputPath + " to: " + outputPath + ": " + e.Message);
                }
            }

            Debug.WriteLine("No file " + inputPath + " found");
            return false;
        }
    }
}
