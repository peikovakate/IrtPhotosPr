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
    public sealed partial class AppearingDisapearingAnimations : UserControl
    {
        public AppearingDisapearingAnimations()
        {
            this.InitializeComponent();
        }

        public Storyboard getAppearing()
        {
            return appearence;
        }  
        public Storyboard getImageAppearing()
        {
            return imageAppearence;
        }
        public Storyboard getImageDeleting()
        {
            return imageDeleting;
        }

        public Storyboard getImageScale()
        {
            return imageScale;
        }
    }
}
