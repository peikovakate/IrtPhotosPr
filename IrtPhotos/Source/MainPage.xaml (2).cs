using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static SharePhotoClient.Downloader;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SharePhotoClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {


        private int row = 0;
        private int col = 0;

        private string GUID = "test";
        private static string urlAzure = "http://sharephotoirt.azurewebsites.net";
        private static string urlLocal = "http://192.168.1.110:53233";
        private string URL = urlLocal; 

        public MainPage()
        {
            this.InitializeComponent();

            ((App)Application.Current).CreateConnection(URL + "/Notifier");
            ((App)Application.Current).HubProxy.On("Notify", onNotification);
            ((App)Application.Current).HubProxy.On<long, string>("NotifyExtended", onNotificationExtended);
            ((App)Application.Current).HubProxy.Invoke("Register", GUID);
        }

        private async void onNotification()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Debug.WriteLine("notification received");
                
                Downloader downloader = new Downloader();
                downloader.createElement += createElement;
                downloader.imageReady += showImage;
                await downloader.loadImage( URL + "/api/media?guid=" + GUID + "&id=0");
                //await loadImage(image);
            });
        }

        private async void onNotificationExtended(long id, string type)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (type.Contains("image"))
                {
                        Debug.WriteLine("notification received");

                        Downloader downloader = new Downloader();
                        downloader.createElement += createElement;
                        downloader.imageReady += showImage;
                        await downloader.loadImage(URL + "/api/media?guid=" + GUID + "&id=" + id.ToString());
                        //await loadImage(image);
                
                }
                else
                {
                    MediaElement media = (MediaElement)createElement(MediaType.VIDEO);
                    media.Source = new Uri(URL + "/api/media?guid=" + GUID + "&id=" + id.ToString());
                }
            });
        }

        private async Task showImage(Image image, WriteableBitmap bitmap)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                image.Source = bitmap;
            });
        }

        private object createElement(MediaType type)
        {
            if (row == 3)
            {
                row = 0;
                col++;
            }
            if (type == MediaType.IMAGE)
            {
                Image image = new Image();
                Grid.SetRow(image, row);
                Grid.SetColumn(image, col);
                row++;
                grid.Children.Add(image);
                return image;
            } 
            else
            {
                MediaElement mediaElement = new MediaElement();
                Grid.SetRow(mediaElement, row);
                Grid.SetColumn(mediaElement, col);
                row++;
                grid.Children.Add(mediaElement);
                return mediaElement;
            }
        }

        private bool isSamePixel(byte red1, byte green1, byte blue1, byte red2, byte green2, byte blue2)
        {
            return red1 - red2 == 0 && green1 - green2 == 0 && blue1 - blue2 == 0;
        }

        //private void DecodeImagePart(byte[] part)
        //{
        //    JpegBitmapDecoder decoder = new JpegBitmapDecoder();
        //}

        //private byte[] CreateBackgroung(int height, int width, Color color)
        //{
        //    //int width = 128;
        //    //int height = width;
        //    int stride = width / 8;
        //    byte[] pixels = new byte[height * stride];

        //    List<Color> colors = new List<Color>();
        //    colors.Add(color);
        //    BitmapPalette myPalette = new BitmapPalette(colors);

        //    BitmapSource image = BitmapSource.Create(
        //        width,
        //        height,
        //        96,
        //        96,
        //        PixelFormats.Indexed1,
        //        myPalette,
        //        pixels,
        //        stride);

        //    var encoder = new JpegBitmapEncoder();

        //    encoder.Frames.Add(BitmapFrame.Create(image));
        //    var stream = new MemoryStream();
        //    encoder.Save(stream);
        //    stream.Position = 0;

        //    byte[] data = new byte[stream.Length];
        //    stream.Read(data, 0, data.Length);
        //    return data;
        //}
    }
}
