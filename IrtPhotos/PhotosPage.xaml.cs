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


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IrtPhotos
{

    public sealed partial class PhotosPage : Page
    {
        private List<IrtImage> _images;
        private readonly string _url;
        private static string[] _link = { "ms-appx:///nature.jpeg", "ms-appx:///colors.jpg", "ms-appx:///sailboat.jpg" };



        public PhotosPage()
        {
            this.InitializeComponent();
           
            addImButton._backgroundGrid = BackgroundGrid;
            Canvas.SetZIndex(addImButton, 1000);
            addImButton.DoubleTapped += AddImButton_DoubleTapped;
            
            var transform = (CompositeTransform)(addImButton.RenderTransform);
            transform.TranslateX = -1000;

            //AddImage(_link[0]);
            //AddVideo();
            AddSeveral(4);
        }

        private async void AddImButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Random r = new Random();
            
            await AddImage(_link[r.Next(0, 2)]);
        }

        private async Task AddImage(string link)
        {    
            var image = new IrtImage(PhotosGrid, 0);
            await image.LoadImage(link);

        }

        private void AddVideo()
        {
            var video = new IrtVideo(PhotosGrid, 0);
            video.LoadVideo("");
        }

        private async void AddSeveral(int amount)
        {
            Random r = new Random();
            for (int i = 0; i < amount - 1; i++)
            {
                var image = new IrtImage(PhotosGrid, i * 15);
                image.LoadImage(_link[r.Next(0, 2)]);
                await Task.Delay(500);
            }
            var video = new IrtVideo(PhotosGrid, (amount - 1) * 15);
            video.LoadVideo("");
        }


    }
}
