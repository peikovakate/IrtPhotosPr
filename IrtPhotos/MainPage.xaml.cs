using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using IrtPhotos.Source;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.ViewManagement;
using Microsoft.AspNet.SignalR.Client;
using System.Threading.Tasks;
using Windows.UI.Core;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IrtPhotos
{


    public sealed partial class MainPage : Page
    {

       

        public MainPage()
        {
            //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();


        }


        


        private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var gradient = new CanvasRadialGradientBrush(sender, Colors.White, Color.FromArgb(255, 176, 177, 181) )
            {
                Center = new Vector2((float)BackgroundGrid.ActualWidth/2, (float)BackgroundGrid.ActualHeight/2),
                RadiusX = (float)BackgroundGrid.ActualWidth / 10 + (float)BackgroundGrid.ActualHeight / 2,
                RadiusY = (float)BackgroundGrid.ActualWidth / 10 + (float)BackgroundGrid.ActualHeight / 2,
            };
           
            args.DrawingSession.FillRectangle(0, 0, (float)BackgroundGrid.ActualWidth, (float)BackgroundGrid.ActualHeight, gradient);
        }

        private void startAnimation(object sender, PointerRoutedEventArgs e)
        {
            QrAnimation.Stop();
            QrAnimation.Begin();
            

        }

        private void goToSecondPage(object sender, PointerRoutedEventArgs e)
        {
            QrAnimationReversed.Begin();
            
            
        }

        private void QrAnimationReversed_OnCompleted(object sender, object e)
        {
            this.Frame.Navigate(typeof(PhotosPage));
        }
    }



}
