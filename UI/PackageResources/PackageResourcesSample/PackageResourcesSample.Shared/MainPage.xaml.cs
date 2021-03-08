using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ICSharpCode.SharpZipLib.Zip;
using SkiaSharp;
using SkiaSharp.Views.UWP;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PackageResourcesSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CancellationTokenSource cancellations;
        public MainPage()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            cancellations?.Cancel();
            cancellations = null;
        }



        public async Task WriteAnotherImage()
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColor.Parse(color.Text));
            }
            
            output.Text = $"loaded, uint byte maxvalue = {(uint)byte.MaxValue}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;

        }

        private async void Button_Click_rgba(object sender, RoutedEventArgs e)
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(new SKColor(byte.Parse(colorr.Text), byte.Parse(colorg.Text), byte.Parse(colorb.Text),128));
            }

            output.Text = $"loaded, uint byte maxvalue = {colorr.Text} {colorg.Text}, {colorb.Text}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;
        }
        private async void Button_Click_rgb(object sender, RoutedEventArgs e)
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(new SKColor(byte.Parse(colorr.Text), byte.Parse(colorg.Text), byte.Parse(colorb.Text)));
            }

            output.Text = $"loaded, uint byte maxvalue = {colorr.Text} {colorg.Text}, {colorb.Text}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;
        }
        private async void Button_Click_red(object sender, RoutedEventArgs e)
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Red);
            }

            output.Text = $"loaded, skcolors.Red {SKColors.Red.Red}, {SKColors.Red.Green}, {SKColors.Red.Blue}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;
        }
        private async void Button_Click_green(object sender, RoutedEventArgs e)
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Green);
            }

            output.Text = $"loaded, skcolors.green {SKColors.Green.Red}, {SKColors.Green.Green}, {SKColors.Green.Blue}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;
        }

        private async void Button_Click_blue(object sender, RoutedEventArgs e)
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            output.Text = $"loaded, skcolors.blue {SKColors.Blue.Red}, {SKColors.Blue.Green}, {SKColors.Blue.Blue}";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;
        }



        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            await WriteAnotherImage();
        }

    }
}