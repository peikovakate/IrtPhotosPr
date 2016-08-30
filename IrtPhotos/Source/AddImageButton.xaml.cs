using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IrtPhotos.Source
{
    public sealed partial class AddImageButton : UserControl
    {
        private OppositeDirection _direction;
        private CompositeTransform _transform;
        public Grid _backgroundGrid;
        private double K = 0.5;

        public AddImageButton()
        {
            this.InitializeComponent();
            _direction = new OppositeDirection();
            _transform = new CompositeTransform();
            this.ManipulationMode = ManipulationModes.All;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = _transform;
            this.ManipulationDelta += AddImageButton_ManipulationDelta;
            this.ManipulationCompleted += AddImageButton_ManipulationCompleted;
            ellipse.Loaded += Ellipse_Loaded;
            turnToButton.SpeedRatio = 2;
            turnToQr.SpeedRatio = 2;
            turnToButton.Completed += TurnToButton_Completed;
            turnToQr.Completed += turnToQr_Completed;
        }

        private void TurnToButton_Completed(object sender, object e)
        {
            //attention.Stop();
            //_transform = (CompositeTransform)this.RenderTransform;

            attention.Begin();
        }

        private void Ellipse_Loaded(object sender, RoutedEventArgs e)
        {
            addShadow();
            attention.Begin();
        }

        private void addShadow()
        {
            var compositor = ElementCompositionPreview.GetElementVisual(ShadowGrid).Compositor;
            var spriteVisual = compositor.CreateSpriteVisual();
            spriteVisual.Size = new Vector2((float)ellipse.RenderSize.Width, (float)ellipse.RenderSize.Height);
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.Offset = new Vector3(0, 0, 0);
            dropShadow.Color = Color.FromArgb(127, 0, 0, 0);
            dropShadow.BlurRadius = 30;
            dropShadow.Mask = ellipse.GetAlphaMask();
            spriteVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(ShadowGrid, spriteVisual);
        }

        private void AddImageButton_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Container == null) return;
            _direction.X = false;
            _direction.Y = false;
        }

        bool isQr = false;

        private void AddImageButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Container == null) return;
            if (Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - grid.ActualWidth * scale/ 2 < 0.0)
            {
                _direction.X = !_direction.X;
                if (_transform.TranslateX < 0)
                {
                    _transform.TranslateX -= Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - grid.ActualWidth * scale/ 2;
                }
                else
                {
                    _transform.TranslateX += Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - grid.ActualWidth * scale/ 2;
                }
            }

            if (Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - grid.ActualHeight * scale/ 2 < 0)
            {
                _direction.Y = !_direction.Y;
                if (_transform.TranslateY < 0)
                {
                    _transform.TranslateY -= Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - grid.ActualHeight * scale/ 2;
                }
                else
                {
                    _transform.TranslateY += Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - grid.ActualHeight * scale/ 2;
                }

            }
            var tX = e.Delta.Translation.X;
            var tY = e.Delta.Translation.Y;
            if (e.IsInertial)
            {
                tX *= K;
                tY *= K;
            }

            if (!_direction.X)
            {
                _transform.TranslateX += tX;
            }
            else
            {
                _transform.TranslateX -= tX;
            }

            if (!_direction.Y)
            {
                _transform.TranslateY += tY;
            }
            else
            {
                _transform.TranslateY -= tY;
            }
        }

        int scale = 1;

        private void addPressed(object sender, TappedRoutedEventArgs e)
        {
            // scanAnimation.Stop();
            turnToButton.Stop();
            turnToQr.Stop();
            if (isQr)
            {
                attention.Stop();
                turnToButton.Begin();
                isQr = false;
                scale = 1;
               
            }
            else
            {
                attention.Stop();
                turnToQr.Begin();
                isQr = true;
                scale = 2;
            }

        }

        private async void turnToQr_Completed(object sender, object e)
        {
            //await Task.Delay(10000);
            //turnToButton.Begin();
            // scanAnimation.Begin();
        }



    }
}
