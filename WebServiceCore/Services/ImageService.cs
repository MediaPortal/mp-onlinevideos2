using SkiaSharp;

namespace WebServiceCore.Services
{
    public class ImageService : IImageService
    {
        static readonly SemaphoreSlim Writes = new(1, 1);
        static async Task SaveBounded(byte[] data, string path)
        {
            await Writes.WaitAsync();
            var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var directory = Path.GetDirectoryName(path)!;
                Directory.CreateDirectory(directory);
                if (new DirectoryInfo(directory).EnumerateFiles().Sum(f => f.Length) + data.Length > 128L * 1024 * 1024)
                    throw new IOException("Image storage quota reached.");
                await File.WriteAllBytesAsync(temp, data);
                File.Move(temp, path, true);
            }
            finally { File.Delete(temp); Writes.Release(); }
        }
        public async Task<string> TrySaveIcon(byte[] icon, string savePath)
        {
            try
            {
                if (icon == null || icon.Length > InputRules.MaxImageBytes) return "Icon invalid!";
                using (var data = SKData.CreateCopy(icon))
                using (var imageInfo = SKCodec.Create(data))
                {
                    if (imageInfo == null)
                        return "Icon invalid!";
                    if (imageInfo.Info.Width > 4096 || imageInfo.Info.Height > 4096 || imageInfo.Info.Width <= 0 || imageInfo.Info.Height <= 0)
                        return "Image dimensions exceed limits!";
                    if (imageInfo.EncodedFormat != SKEncodedImageFormat.Png)
                        return "Icon not saved! Must be PNG!";
                    if (imageInfo.Info.Width != imageInfo.Info.Height)
                        return "Icon not saved! Height must be equal to width!";
                    await SaveBounded(icon, savePath);
                    return "Icon saved!";
                }
            }
            catch
            {
                return "Icon invalid!";
            }
        }

        public async Task<string> TrySaveBanner(byte[] banner, string savePath)
        {
            try
            {
                if (banner == null || banner.Length > InputRules.MaxImageBytes) return "Banner invalid!";
                using (var data = SKData.CreateCopy(banner))
                using (var imageInfo = SKCodec.Create(data))
                {
                    if (imageInfo == null)
                        return "Banner invalid!";
                    if (imageInfo.Info.Width > 4096 || imageInfo.Info.Height > 4096 || imageInfo.Info.Width <= 0 || imageInfo.Info.Height <= 0)
                        return "Image dimensions exceed limits!";
                    if (imageInfo.EncodedFormat != SKEncodedImageFormat.Png)
                        return "Banner not saved! Must be PNG!";
                    if (imageInfo.Info.Width != 3 * imageInfo.Info.Height)
                        return "Banner not saved! Width must be 3 times height!";
                    await SaveBounded(banner, savePath);
                    return "Banner saved!";
                }
            }
            catch
            {
                return "Banner invalid!";
            }
        }
    }
}
