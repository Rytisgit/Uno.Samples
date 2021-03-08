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
        private IList<SampleBase> samples;
        private SampleBase sample;
        public MainPage()
        {
            InitializeComponent();
            samples = SamplesManager.GetSamples().ToList();
            SamplesInitializer.Init();
            var o = new ToggleSwitch { FlowDirection = FlowDirection.RightToLeft };
            SetSample(samples.First());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            cancellations?.Cancel();
            cancellations = null;
        }

        private void OnSampleSelected(object sender, SelectionChangedEventArgs e)
        {
            var sample = e.AddedItems?.FirstOrDefault() as SampleBase;
            SetSample(sample);
        }
        private void OnPaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void OnPaintGL(object sender, SKPaintGLSurfaceEventArgs e)
        {
            OnPaintSurface(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void SetSample(SampleBase newSample)
        {
            // clean up the old sample
            if (sample != null)
            {
                sample.RefreshRequested -= OnRefreshRequested;
                sample.Destroy();
            }

            sample = newSample;

            var runtimeMode = string.Empty;
#if __WASM__
            runtimeMode = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_MONO_RUNTIME_MODE");
            if (runtimeMode.Equals("Interpreter", StringComparison.InvariantCultureIgnoreCase))
                runtimeMode = " (Interpreted)";
            else if (runtimeMode.Equals("FullAOT", StringComparison.InvariantCultureIgnoreCase))
                runtimeMode = " (AOT)";
            else if (runtimeMode.Equals("InterpreterAndAOT", StringComparison.InvariantCultureIgnoreCase))
                runtimeMode = " (Mixed)";
#endif

            // set the title
            //titleBar.Text = (sample?.Title ?? $"SkiaSharp for Uno Platform") + runtimeMode;

            // prepare the sample
            if (sample != null)
            {
                sample.RefreshRequested += OnRefreshRequested;
                sample.Init();
            }

            // refresh the view
            OnRefreshRequested(null, null);
        }
        private void OnRefreshRequested(object sender, EventArgs e)
        {
            

            //if (canvas.Visibility == Visibility.Visible)
            //    canvas.Invalidate();
            //if (glview.Visibility == Visibility.Visible)
            //    glview.Invalidate();
        }

        private void OnPaintSurface(SKCanvas canvas, int width, int height)
        {
            sample?.DrawSample(canvas, width, height);
        }

        private void OnSampleTapped(object sender, TappedRoutedEventArgs e)
        {
            sample?.Tap();
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



        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            await WriteAnotherImage();
        }

    }
}