using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace IrtPhotos.Source
{
    class Downloader : IDisposable
    {
        private int width = 0;
        private int height = 0;
        private WriteableBitmap bitmap;
        private Stream bitmapStream;
        private byte[] prevPixels, pixels;
        private int loadedPixels = 0;
        private int loadedBytes = 0;
        private int prevLoadedPixels = 0;
        private int size = 0;

        public enum MediaType { IMAGE, VIDEO };

        private int[] colorSum = new int[4];

        bool disposed = false;

        public Downloader()
        {
        }

        public async Task loadImage(WriteableBitmap outputBitmap, string url)
        {

            var client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            size = Int32.Parse(response.Headers.GetValues("size").FirstOrDefault());

            height = outputBitmap.PixelHeight;
            width = outputBitmap.PixelWidth;

            bitmap = new WriteableBitmap(width, height);
            bitmapStream = bitmap.PixelBuffer.AsStream();
            Stream outputStream = outputBitmap.PixelBuffer.AsStream();

            prevPixels = new byte[width * height * 4];
            pixels = new byte[width * height * 4];

            loadedBytes = 0;
            byte[] buffer = new byte[16384];
            int receivedBytes = 0;

            try
            {
                using (MemoryStream imageStream = new MemoryStream())
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    while (loadedBytes < size)
                    {
                        receivedBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                        imageStream.Position = loadedBytes;
                        await imageStream.WriteAsync(buffer, 0, receivedBytes);
                        loadedBytes += receivedBytes;

                        await editImage(outputBitmap, outputStream, imageStream);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

         
            outputStream.Dispose();
            message.Dispose();
            client.Dispose();
            response.Dispose();
            Dispose();
        }

        private async Task editImage(WriteableBitmap outputBitmap, Stream outputStream, MemoryStream input)
        {
            try
            {
                input.Position = 0;
                await bitmap.SetSourceAsync(input.AsRandomAccessStream());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }

            bitmapStream = bitmap.PixelBuffer.AsStream();
            bitmapStream.Position = loadedPixels;
            await bitmapStream.ReadAsync(pixels, loadedPixels, pixels.Length - loadedPixels);

            if (prevPixels != null && loadedBytes != size)
            {
                prevLoadedPixels = loadedPixels;

                for (int i = loadedPixels; i < prevPixels.Length; i += 4)
                {
                    if (prevPixels[i] - pixels[i] != 0 ||
                        prevPixels[i + 1] - pixels[i + 1] != 0 ||
                        prevPixels[i + 2] - pixels[i + 2] != 0 ||
                        prevPixels[i + 3] - pixels[i + 3] != 0)
                    {
                        loadedPixels = i;

                        break;
                    }
                    else
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            colorSum[j] += pixels[i + j];
                        }
                    }
                }

                Buffer.BlockCopy(pixels, loadedPixels, prevPixels, loadedPixels, prevPixels.Length - loadedPixels);

                if (loadedPixels != 0)
                {
                    for (int i = loadedPixels; i < pixels.Length; i += 4)
                    {
                        pixels[i] = (byte)(colorSum[0] / loadedPixels * 4);
                        pixels[i + 1] = (byte)(colorSum[1] / loadedPixels * 4);
                        pixels[i + 2] = (byte)(colorSum[2] / loadedPixels * 4);
                        pixels[i + 3] = (byte)(colorSum[3] / loadedPixels * 4);
                    }

                    outputStream.Position = prevLoadedPixels;
                    await outputStream.WriteAsync(pixels, prevLoadedPixels, pixels.Length - prevLoadedPixels);
                    outputBitmap.Invalidate();
                }
                else
                {
                   
                }
            }
            if (loadedBytes == size)
            {
                outputStream.Position = loadedPixels;
                await outputStream.WriteAsync(pixels, loadedPixels, pixels.Length - loadedPixels);
                outputBitmap.Invalidate();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                bitmapStream.Dispose();
            }
            pixels = null;
            prevPixels = null;


            disposed = true;
        }
    }
}
