using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FParsec;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TechTreeCreator.DTO;
using Buffer = System.Buffer;

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
                        entity.IconFound = true;
                        break;
                    }
                    else {
                        // sometimes the icon lies!
                        filePath = Path.Combine(inputDirRoot, entity.Id + ".dds");
                        // if we find it break out immideately
                        if (File.Exists(filePath)) {
                            entity.IconFound = true;
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
                    Log.Logger.Warning("No file {filePath} found for id {id} with icon {icon}", filePath, entity.Id, entity.Icon);
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
                    
                    
                    /*
                     * Huge thanks to Nick Babcock of Pfim for helping me determine what was wrong with weird images coming out from ImageMagic.
                     */
                    
                    byte[] newData;

                    // Since image sharp can't handle data with line padding in a stride
                    // we create an stripped down array if any padding is detected
                    var tightStride = (ddsImage.Width * ddsImage.BitsPerPixel + 7) / 8;
                    if (ddsImage.Stride != tightStride)
                    {
                        newData = new byte[ddsImage.Height * tightStride];
                        for (int i = 0; i < ddsImage.Height; i++)
                        {
                            Buffer.BlockCopy(ddsImage.Data, i * ddsImage.Stride, newData, i * tightStride, tightStride);
                        }
                    }
                    else
                    {
                        newData = ddsImage.Data;
                    }


                    switch (ddsImage.Format) {
                        case Pfim.ImageFormat.Rgba32:
                            Image.LoadPixelData<Bgra32>(newData, ddsImage.Width, ddsImage.Height)
                                .Save(outputPath);
                            return true;
                        case Pfim.ImageFormat.Rgb24:
                            Image.LoadPixelData<Bgr24>(newData, ddsImage.Width, ddsImage.Height).Save(outputPath);
                            return true;
                        case Pfim.ImageFormat.R5g5b5:
                            Image.LoadPixelData<Bgra5551>(newData, ddsImage.Width, ddsImage.Height).Save(outputPath);
                            return true;
                        default:
                            Log.Logger.Warning("Image {inputPath} had unknown format {format}", inputPath, ddsImage.Format);
                            return false;
                    }
                }
                catch (Exception e) {
                    Log.Logger.Warning( "Error processing file: {inputPath} to: {outputPath}.  {message}", inputPath, outputPath, e.Message);
                    return false;
                }
            }
            else {
                Log.Logger.Warning("No file {filePath} found", inputPath);
                return false;
            }
        }
    }
}
