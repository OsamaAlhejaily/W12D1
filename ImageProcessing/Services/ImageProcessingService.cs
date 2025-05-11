using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageProcessingApi.Services
{
    public interface IImageProcessingService
    {
        byte[] ApplyFilter(byte[] imageData, string filter);
    }

    public class ImageProcessingService : IImageProcessingService
    {
        // Defines a method that accepts raw image bytes and a filter name, and returns the processed image as bytes.
        public byte[] ApplyFilter(byte[] imageData, string filter)
        {
            // Loads the image from the input byte array into an Image object.
            using var image = Image.Load(imageData);

            // If the requested filter is "grayscale" (case-insensitive), apply grayscale transformation.
            if (string.Equals(filter, "grayscale", System.StringComparison.OrdinalIgnoreCase))
            {
                image.Mutate(x => x.Grayscale());
            }
            // If the requested filter is "sepia" (case-insensitive), apply sepia transformation.
            else if (string.Equals(filter, "sepia", System.StringComparison.OrdinalIgnoreCase))
            {
                image.Mutate(x => x.Sepia());
            }

            // Create a memory stream to store the output (processed image).
            using var outputStream = new MemoryStream();

            // Save the processed image into the memory stream in PNG format.
            image.Save(outputStream, new PngEncoder());

            // Return the processed image as a byte array.
            return outputStream.ToArray();
        }
    }
}