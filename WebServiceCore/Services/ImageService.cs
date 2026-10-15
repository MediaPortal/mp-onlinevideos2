using SkiaSharp;

namespace WebServiceCore.Services
{
    public class ImageService : IImageService
    {
        public async Task<string> TrySaveIcon(byte[] icon, string savePath)
        {
            try
            {
                using (var data = SKData.CreateCopy(icon))
                using (var imageInfo = SKCodec.Create(data))
                {
                    if (imageInfo == null)
                        return "Icon invalid!";
                    if (imageInfo.EncodedFormat != SKEncodedImageFormat.Png)
                        return "Icon not saved! Must be PNG!";
                    if (imageInfo.Info.Width != imageInfo.Info.Height)
                        return "Icon not saved! Height must be equal to width!";
                    await File.WriteAllBytesAsync(savePath, icon);
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
                using (var data = SKData.CreateCopy(banner))
                using (var imageInfo = SKCodec.Create(data))
                {
                    if (imageInfo == null)
                        return "Banner invalid!";
                    if (imageInfo.EncodedFormat != SKEncodedImageFormat.Png)
                        return "Banner not saved! Must be PNG!";
                    if (imageInfo.Info.Width != 3 * imageInfo.Info.Height)
                        return "Banner not saved! Width must be 3 times height!";
                    await File.WriteAllBytesAsync(savePath, banner);
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
