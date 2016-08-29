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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IrtPhotos.Source
{
    public sealed partial class CloseAnimation : UserControl
    {
        public CloseAnimation()
        {
            this.InitializeComponent();
            close.Begin();
        }
       
        public double ActualWidth
        {
            get
            {
                return image.Width;
            }
        }

        public double ActualHeight
        {
            get
            {
                return image.Height;
            }
        }

        public Storyboard getCloseAnim()
        {
            return close;
        }
    }
}
