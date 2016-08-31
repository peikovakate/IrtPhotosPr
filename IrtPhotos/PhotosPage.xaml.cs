using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IrtPhotos.Source;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using static IrtPhotos.Source.Downloader;
using System.Diagnostics;
using System.Net.Http;
using Windows.Media.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Microsoft.AspNet.SignalR.Client;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IrtPhotos
{

    public sealed partial class PhotosPage : Page
    {
        private List<IrtImage> _images;
        private readonly string _url;
        private static string[] _link = { "ms-appx:///nature.jpeg", "ms-appx:///colors.jpg", "ms-appx:///sailboat.jpg" };

        private int row = 0;
        private int col = 0;

        private string GUID = "test";
        private static string urlAzure = "http://sharephotoirt.azurewebsites.net";
        private static string urlLocal = "http://192.168.1.110:53233";
        private string URL = urlAzure;

        public PhotosPage()
        {
            this.InitializeComponent();
            _images = new List<IrtImage>();
            Task.Run(() =>
            {
                ((App)Application.Current).CreateConnection(URL + "/Notifier");
                ((App)Application.Current).HubProxy.On<long, string, int, int>("NotifyExtended", onNotificationExtended);
                ((App)Application.Current).HubProxy.Invoke("Register", GUID);
            });



            addImButton._backgroundGrid = BackgroundGrid;

            Canvas.SetZIndex(addImButton, 1000);

            
            var transform = (CompositeTransform)(addImButton.RenderTransform);
            transform.TranslateX = -1000;

        }


        

        private async void onNotificationExtended(long id, string type, int width, int height)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
           {
               if (type.Contains("image"))
               {

                   int angle = 0;
                   bool isRotated = true;
                   for (int i = 0; i < _images.Count; i++)
                   {
                       Debug.WriteLine(i);
                       if (_images[i].IsMoved)
                       {
                           Debug.WriteLine("rotated");
                           isRotated = false;
                           break;
                       }
                   }
                   if (isRotated)
                   {
                       angle = _images.Count * 15;
                   }
                   IrtImage im = new IrtImage(PhotosGrid, angle, width, height);
                   _images.Add(im);

                   Downloader downloader = new Downloader();
                   WriteableBitmap bitmap = new WriteableBitmap(width, height);
                   Image image = new Image();
                   image.Source = bitmap;
                   im.LoadImage(image);


                   downloader.loadImage(bitmap, URL + "/api/media?guid=" + GUID + "&id=" + id.ToString());

               }
               else
               {

                   var video = new IrtVideo(PhotosGrid, 0);
                   video.LoadVideo(URL + "/api/media?guid=" + GUID + "&id=" + id.ToString());
               }
           });
        }


    }
}
