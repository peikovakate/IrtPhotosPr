using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using System.Numerics;

using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Threading;
using static IrtPhotos.Source.Downloader;
using Windows.UI.Core;
using Windows.Media.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Microsoft.AspNet.SignalR.Client;

namespace IrtPhotos.Source
{
    class IrtImage
    {
        private Grid _grid;
        private double d;
        private Grid _shadowGrid;
        private Rectangle _shadowOnTopRect;
        private Grid _bluredGrid;
        private Storyboard _deletingAnim;
        private Storyboard _imageAppearence;
        private Storyboard _appearence;
        private readonly OppositeDirection _direction;
        private  CompositeTransform _transform;
        private bool sizeChanged = false;
        private CompositeEffect _effect;
        private const float blurAmount = 40;
        private readonly Grid _backgroundGrid;
        private const double K = 0.5;
        private string _link;
        private Ink ink;
        Image image;
        Rectangle r;
        private CloseAnimation closeAnim;

        private float borderWidth = 80;
        private float MinScale = 0.2f;

        AppearingDisapearingAnimations animation;
        const float blurConst = 15;
        Pointer remPointer;
        Pointer pointer;
      
        SpriteVisual blurVisual;

        private int realWidth;
        private int realHeight;

        Storyboard _scaleAnim;
        Storyboard _closeAnim;

        public int Angle = 0;
        public bool IsMoved = false;

        //---------------------
        public IrtImage(Grid back, int angle, int width, int height)
        {
            _backgroundGrid = back;
            Angle = angle;
            _grid = new Grid();
            _shadowGrid = new Grid();
            _shadowOnTopRect = new Rectangle();
            _bluredGrid = new Grid();

            _grid.Children.Add(_bluredGrid);
            _bluredGrid.Children.Add(_shadowGrid);

            //animation
            animation = new AppearingDisapearingAnimations();
            _imageAppearence = animation.getImageAppearing();
            _deletingAnim = animation.getImageDeleting();
            _appearence = animation.getAppearing();


            _grid.HorizontalAlignment = HorizontalAlignment.Center;
            _grid.VerticalAlignment = VerticalAlignment.Center;
            _backgroundGrid.Children.Add(_grid);

            var scaleAnim = new ScaleAnim();
            _scaleAnim = scaleAnim.getImageScale();


            var rotate = (DoubleAnimationUsingKeyFrames)_imageAppearence.Children[0];
            rotate.KeyFrames[1].SetValue(DiscreteDoubleKeyFrame.ValueProperty, angle);

            rotate = (DoubleAnimationUsingKeyFrames)_appearence.Children[0];
            rotate.KeyFrames[1].SetValue(DiscreteDoubleKeyFrame.ValueProperty, angle);


            _imageAppearence.SpeedRatio = 2;
            _appearence.SpeedRatio = 2;
            _deletingAnim.SpeedRatio = 2;
            _backgroundGrid.Children.Add(animation);

            closeAnim = new CloseAnimation();
            closeAnim.Tapped += CloseAnim_Tapped;
            _closeAnim = closeAnim.getCloseAnim();
            _closeAnim.Completed += _closeAnim_Completed;

            //manipulaions
             _direction = new OppositeDirection();
            _transform = new CompositeTransform();
            _grid.ManipulationStarting += Canvas_ManipulationStarting;
            _grid.ManipulationCompleted += Canvas_ManipulationCompleted;
            _grid.ManipulationDelta += Canvas_ManipulationDelta;
            _grid.RenderTransform = _transform;
            _grid.ManipulationMode = ManipulationModes.All;
            _grid.RenderTransformOrigin = new Point(0.5, 0.5);
            _grid.PointerPressed += _grid_PointerPressed;
            _grid.PointerReleased += _grid_PointerReleased;

           // image = new Image();
            r = new Rectangle(); //rectangle for image frame
            r.Fill = new SolidColorBrush(Colors.White);
            //r.RadiusX = borderWidth / 2;
            //r.RadiusY = borderWidth / 2;
            r.RenderTransformOrigin = new Point(0.5, 0.5);
            r.RenderTransform = new CompositeTransform();
            //_shadowOnTopRect.RadiusX = borderWidth / 2;
            //_shadowOnTopRect.RadiusY = borderWidth / 2;
            _bluredGrid.Children.Add(r);
            //_bluredGrid.Children.Add(image);
            _bluredGrid.Children.Add(_shadowOnTopRect);

            realHeight = height;
            realWidth = width;



        }

        private void _closeAnim_Completed(object sender, object e)
        {
            removeClose();
   
            if (_transform.ScaleX <= MinScale+0.01)
            {

                _scaleAnim.Stop();
                Storyboard.SetTarget(_scaleAnim.Children[0], _grid);
                Storyboard.SetTarget(_scaleAnim.Children[1], _grid);
                _scaleAnim.Begin();
                //scaleComposition();
            }
        }

        private void scaleComposition()
        {
            var view = ElementCompositionPreview.GetElementVisual(_grid);
            view.CenterPoint = new Vector3((float)_grid.ActualWidth/2.0f, (float)_grid.ActualHeight/2.0f, 0.0f);
            var compositor = view.Compositor;
            
            var anim = compositor.CreateVector3KeyFrameAnimation();
            anim.InsertKeyFrame(0.0f, new Vector3(1, 1, 0));
            anim.InsertKeyFrame(1.0f, new Vector3(1.5f, 1.5f, 0));

            anim.Duration = TimeSpan.FromSeconds(0.5);
           
            view.StartAnimation("Scale", anim);
            //_transform = (CompositeTransform)_grid.RenderTransform;


        }

        private void _grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            remPointer = pointer;
        }

        private void CloseAnim_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _deletingAnim.Stop();
            Storyboard.SetTarget(_deletingAnim.Children[0], _grid);
            Storyboard.SetTarget(_deletingAnim.Children[1], _grid);
            Storyboard.SetTarget(_deletingAnim.Children[2], _grid);

            _deletingAnim.Completed += _deletingAnim_Completed;
            var t = new CompositeTransform();
            t.TranslateX = _transform.TranslateX;
            t.TranslateY = _transform.TranslateY;
            animation.RenderTransform = t;
            _appearence.Stop();
            _appearence.Begin();
            _deletingAnim.Begin();

        }

        private void _grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointer = e.Pointer;
        }

        private void _deletingAnim_Completed(object sender, object e)
        {
            _backgroundGrid.Children.Remove(_grid);
            _backgroundGrid.Children.Remove(animation);
        }

        public async Task LoadImage(string link)
        {
            _link = link;
            BitmapImage bitmap = new BitmapImage(new Uri(link));
            image.Source = bitmap;

            bitmap.ImageOpened += (sender, e) =>
            {
                realWidth = bitmap.PixelWidth;
                realHeight = bitmap.PixelHeight;

                r.Width = realWidth + borderWidth * 2;
                r.Height = realHeight + borderWidth * 2;
                _shadowOnTopRect.Width = r.Width;
                _shadowOnTopRect.Height = r.Height;

                _grid.Height = r.Height + 20;
                _grid.Width = r.Width + 20;
                image.Width = realWidth;
            };

            _imageAppearence.Completed += ImageAppearence_Completed;
            _imageAppearence.Stop();

            Storyboard.SetTarget(_imageAppearence.Children[0], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[1], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[2], _grid);
            _appearence.Begin();
            _imageAppearence.Begin();
            _scaleAnim.Completed += _scaleAnim_Completed;

        }

        public void LoadImage(Image image)
        {
            this.image = image;
            setSize();

            // image.Width = realWidth;
            _bluredGrid.Children.Add(this.image);
            _imageAppearence.Completed += ImageAppearence_Completed;
            _imageAppearence.Stop();

            Storyboard.SetTarget(_imageAppearence.Children[0], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[1], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[2], _grid);
            _appearence.Begin();
            _imageAppearence.Begin();

            _scaleAnim.Completed += _scaleAnim_Completed;

        }

        public void setSize()
        {
            //_grid.UseLayoutRounding = false;

           
            //int deltaX = 0, deltaY = 0;
            //if (width > _backgroundGrid.ActualWidth)
            //{
            //    deltaX = (int)((width - _backgroundGrid.ActualWidth)/2*0.3);
            //}
            //if (width > _backgroundGrid.ActualWidth)
            //{
            //    deltaY = (int)((height - _backgroundGrid.ActualHeight)/2*0.3);
            //}

            image.Width = realWidth;
            image.Height = realHeight;


            borderWidth = (realWidth * 80) / 3000;

            r.Width = realWidth + borderWidth * 2;
            r.Height = realHeight + borderWidth * 2;
            _shadowOnTopRect.Width = r.Width;
            _shadowOnTopRect.Height = r.Height;

            _grid.Height = r.Height;
            _grid.Width = r.Width;
        }

        private void _scaleAnim_Completed(object sender, object e)
        {
            _transform = (CompositeTransform)_grid.RenderTransform;
        }

        private void ImageAppearence_Completed(object sender, object e)
        {
            _transform = (CompositeTransform)(_grid.RenderTransform);
            AddShadow();
        }
        
        private double calcProjectionX(double rotation)
        {
            double angle = 0;
            if (Math.Abs(rotation) % 180 < 90)
            {
                angle = ((Math.Abs(rotation) % 90) / 360 * 2 * Math.PI);
            }
            else
            {
                angle = ((90 - Math.Abs(rotation) % 90) / 360 * 2 * Math.PI);
            }
            return (_grid.Width * Math.Cos(angle) + _grid.Height * Math.Sin(angle)) * _transform.ScaleX;
        }

        private double calcProjectionY(double rotation)
        {
            double angle = 0;
            if (Math.Abs(rotation) % 180 < 90)
            {
                angle = ((Math.Abs(rotation) % 90) / 360 * 2 * Math.PI);
            }
            else
            {
                angle = ((90 - Math.Abs(rotation) % 90) / 360 * 2 * Math.PI);
            }
            return (_grid.Width * Math.Sin(angle) + _grid.Height * Math.Cos(angle)) * _transform.ScaleY;
        }

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
         //   if (e.Container == null) return;

            var potentialRotation = _transform.Rotation + e.Delta.Rotation;
            if (calcProjectionX(potentialRotation)<_backgroundGrid.ActualWidth && 
                calcProjectionY(potentialRotation)< _backgroundGrid.ActualHeight)
            {
                _transform.Rotation += e.Delta.Rotation;
            }

            var tX = e.Delta.Translation.X;
            var tY = e.Delta.Translation.Y;
            if (e.IsInertial)
            {
                tX *= K;
                tY *= K;
            }

            double projectionX = calcProjectionX(_transform.Rotation);
            double distX = Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX)
                - projectionX / 2;

            if (distX < 0.0)
            {
                if (!(pointer.IsInContact && remPointer != pointer))
                {
                    _direction.X = !_direction.X;
                }
                
                if (_transform.TranslateX < 0)
                {
                    _transform.TranslateX -= distX;
                }
                else
                {
                    _transform.TranslateX += distX;
                }
            }

                if (!_direction.X)
                {
                    _transform.TranslateX += tX;
                }
                else
                {
                    _transform.TranslateX -= tX;
                }

            double projectionY = calcProjectionY(_transform.Rotation);
            double distY = Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY)
                - projectionY / 2;

            if (distY < 0)
            {
                if (!(pointer.IsInContact && remPointer != pointer))
                {
                    _direction.Y = !_direction.Y;
                }
         
                if (_transform.TranslateY < 0)
                {
                    _transform.TranslateY -= distY;
                }
                else
                {
                    _transform.TranslateY += distY;
                }
            }

            if (!_direction.Y)
            {
                _transform.TranslateY += tY;
            }
            else
            {
                _transform.TranslateY -= tY;
            }

            if (_transform.ScaleX * e.Delta.Scale >= MinScale && 
                _transform.ScaleX * e.Delta.Scale <= 1 && 
                projectionX * e.Delta.Scale < (_backgroundGrid.ActualWidth-10) &&
                projectionY * e.Delta.Scale < (_backgroundGrid.ActualHeight-10))
            {
                

                _transform.ScaleX *= e.Delta.Scale;
                _transform.ScaleY *= e.Delta.Scale;

   
                //var rectTransform = (CompositeTransform)(r.RenderTransform);
                //rectTransform.ScaleX *= 1 +(e.Delta.Scale*(1-borderWidth/_grid.Width));
                //rectTransform.ScaleY *= 1 +(e.Delta.Scale*(1 - borderWidth / _grid.Width));

                if (_transform.ScaleX <= MinScale+0.01)
                {

                    addClose();
                }
                else
                {
                    removeClose();
                }
            }
        }

        async void addClose()
        {

            if (!_grid.Children.Contains(closeAnim))
            {
                _scaleAnim.Stop();
                _grid.Children.Add(closeAnim);
                _shadowOnTopRect.Fill = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
                addBlurAnim();
                _closeAnim.Begin();
            }
        }

        void removeClose()
        {
            if (_grid.Children.Contains(closeAnim))
            {
                _grid.Children.Remove(closeAnim);
                _closeAnim.Stop();
                //blurVisual.Dispose();
                removeBlurAnim();
            }
        }

        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //if (e.Container == null) return;
            _direction.X = false;
            _direction.Y = false;
            _transform = (CompositeTransform)_grid.RenderTransform;
        }

        private void Canvas_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            //if (e.Container == null) return;
            IsMoved = true;
            foreach (var item in _backgroundGrid.Children)
            {
                Canvas.SetZIndex(item, 0);
            }
            Canvas.SetZIndex(e.Container, 1);
        }

        void AddShadow()
        {
            var compositor = ElementCompositionPreview.GetElementVisual(_shadowGrid).Compositor;
            var spriteVisual = compositor.CreateSpriteVisual();
            spriteVisual.Size = new Vector2((float)r.RenderSize.Width, (float)r.RenderSize.Height);
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.Offset = new Vector3(20, 20, 0);
            dropShadow.Color = Color.FromArgb(205, 0, 0, 0);
            dropShadow.BlurRadius = 300;
            spriteVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(_shadowGrid, spriteVisual);
        }

        void addBlurAnim()
        {
            var closeVisual = ElementCompositionPreview.GetElementVisual(closeAnim);
            var shadowVisual = ElementCompositionPreview.GetElementVisual(_shadowOnTopRect);

            closeVisual.CenterPoint =
              new Vector3(
                (float)closeAnim.ActualWidth / 2.0f,
                (float)closeAnim.ActualHeight / 2.0f,
                0.0f);

            var compositor = closeVisual.Compositor;

            var rotatingAnim = compositor.CreateScalarKeyFrameAnimation();
            rotatingAnim.InsertKeyFrame(0.0f, 0.0f);
            rotatingAnim.InsertKeyFrame(0.5f, 360.0f);
            rotatingAnim.Duration = TimeSpan.FromSeconds(3);

            var shadowAnim = compositor.CreateScalarKeyFrameAnimation();
            shadowAnim.InsertKeyFrame(0.0f, 0.0f);
            shadowAnim.InsertKeyFrame(0.5f, 1.0f);
            shadowAnim.Duration = TimeSpan.FromSeconds(3);

            closeVisual.StartAnimation("RotationAngleInDegrees", rotatingAnim);

            shadowVisual.StartAnimation("Opacity", shadowAnim);

            var blur = new GaussianBlurEffect()
            {
                Name = "Blur", 
                BorderMode = EffectBorderMode.Hard,
                BlurAmount = 0.0f,
                Source = new CompositionEffectSourceParameter("source")
            };
            var effectFactory = compositor.CreateEffectFactory(blur,
              new string[] { "Blur.BlurAmount" });

            var effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            blurVisual = compositor.CreateSpriteVisual();
            blurVisual.Size = new Vector2(
             (float) _bluredGrid.ActualWidth, (float)_bluredGrid.ActualHeight);

            blurVisual.Brush = effectBrush;

            ElementCompositionPreview.SetElementChildVisual(_bluredGrid, blurVisual);
            var exprAnimation = compositor.CreateExpressionAnimation(
              "rectangle.RotationAngleInDegrees / 36");

            exprAnimation.SetReferenceParameter("rectangle", closeVisual);
            effectBrush.StartAnimation("Blur.BlurAmount", exprAnimation);

        }

        void removeBlurAnim()
        {

            var shadowVisual = ElementCompositionPreview.GetElementVisual(_shadowOnTopRect);

            var compositor = shadowVisual.Compositor;

            var shadowAnim = compositor.CreateScalarKeyFrameAnimation();
            shadowAnim.InsertKeyFrame(0.0f, 1.0f);
            shadowAnim.InsertKeyFrame(0.5f, 0.0f);
            shadowAnim.Duration = TimeSpan.FromSeconds(3);
            
            shadowVisual.StartAnimation("Opacity", shadowAnim);

            var blur = new GaussianBlurEffect()
            {
                Name = "Blur", // Name needed here so we can animate it.
                BorderMode = EffectBorderMode.Hard,
                BlurAmount = 100.0f,
                Source = new CompositionEffectSourceParameter("source")
            };
            var effectFactory = compositor.CreateEffectFactory(blur,
              new string[] { "Blur.BlurAmount" });

            var effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            blurVisual = compositor.CreateSpriteVisual();
            blurVisual.Size = new Vector2(
             (float)_bluredGrid.ActualWidth, (float)_bluredGrid.ActualHeight);

            blurVisual.Brush = effectBrush;

            ElementCompositionPreview.SetElementChildVisual(_bluredGrid, blurVisual);
            var exprAnimation = compositor.CreateExpressionAnimation(
              "rectangle.Opacity");

            exprAnimation.SetReferenceParameter("rectangle", shadowVisual);
            effectBrush.StartAnimation("Blur.BlurAmount", exprAnimation);
        }

    }
}