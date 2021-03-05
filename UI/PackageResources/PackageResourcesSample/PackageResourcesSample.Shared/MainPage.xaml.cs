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
using FrameGenerator;
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
        private MainGenerator generator;
        public MainPage()
        {
            InitializeComponent();
            samples = SamplesManager.GetSamples().ToList();
            SamplesInitializer.Init();
            var o = new ToggleSwitch { FlowDirection = FlowDirection.RightToLeft };

            generator = new MainGenerator(new UnoFileReader(), 69);
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

        public async void LoadPackageFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(assetFileName.Text));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream);
                output.Text = await FileIO.ReadTextAsync(file);

            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
        }

        public async Task LoadDCSSImage()
        {

                await generator.InitialiseGenerator();
                var skBitmap = generator.GenerateImage(null);
                output.Text = $"{skBitmap.Height}, {skBitmap.Width}";

                var bitmapSource = new WriteableBitmap(1602, 768);

                using (Stream stream = bitmapSource.PixelBuffer.AsStream())
                {
                    //write to bitmap
                    await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
                }
                myImage.Source = bitmapSource;

        }



        public async Task WriteAnotherImage()
        {
            var skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Red);
            }

            output.Text = $"switch";

            var bitmapSource = new WriteableBitmap(1602, 768);
            using (Stream stream = bitmapSource.PixelBuffer.AsStream())
            {
                //write to bitmap
                await stream.WriteAsync(skBitmap.Bytes, 0, skBitmap.Bytes.Length).ConfigureAwait(false);
            }
            myImage.Source = bitmapSource;

        }


        public async Task StartImageLoop()
        {
            var side = false;
            while (int.Parse(speed.Text) >= 0)
            {
                
                if (side)
                {
                    await LoadDCSSImage();
                }
                else
                {
                    await WriteAnotherImage();
                }

                side = !side;
                await Task.Delay(int.Parse(speed.Text));
            }
            
        }


        private async Task ExtractExtraFileFolder(Stream stream)
        {
            try
            {
                var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), @"Extra.zip");
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Console.WriteLine(localFolder.Path);
                var folder = await localFolder.CreateFolderAsync("Extra", CreationCollisionOption.OpenIfExists);

                var zipInStream = new ZipInputStream(stream);
                var entry = zipInStream.GetNextEntry();
                while (entry != null && entry.CanDecompress)
                {
                    var outputFile = Path.Combine(folder.Path, entry.Name);

                    var outputDirectory = Path.GetDirectoryName(outputFile);
                    //Console.WriteLine(outputDirectory);
                    var correctFolder = await folder.CreateFolderAsync(outputDirectory, CreationCollisionOption.OpenIfExists);


                    if (entry.IsFile)
                    {

                        int size;
                        byte[] buffer = new byte[zipInStream.Length];
                        //Debug.WriteLine(buffer.Length);
                        zipInStream.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(outputFile, buffer);
                        //var bitmap = SKBitmap.Decode(buffer);
                        ////Console.WriteLine(bitmap.ByteCount);
                        //Console.WriteLine(bitmap.Height);
                        //SKBitmap bitmap = SKBitmap.Decode(buffer);
                        //Console.WriteLine(bitmap.ByteCount);
                    }

                    entry = zipInStream.GetNextEntry();
                }
                zipInStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private async Task<List<StorageFile>> GetFilesFromFolderAndSubfolders(string foldername)
        {
            var files = new List<StorageFile>();
            try
            {
                
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var folder = await localFolder.GetFolderAsync(foldername);
                Debug.WriteLine(folder.Path);
                
                files.AddRange((await folder.GetFilesAsync()).Where(file => file.Name.EndsWith("png",true,CultureInfo.InvariantCulture)));

                var subFolders = await folder.GetFoldersAsync();
                foreach (var subFolder in subFolders)
                {
                    files.AddRange((await subFolder.GetFilesAsync()).Where(file => file.Name.EndsWith("png", true, CultureInfo.InvariantCulture)));
                }
                Debug.WriteLine($"loaded {files.Count} files from {foldername}");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return files;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await LoadDCSSImage();
        }
        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            await WriteAnotherImage();
        }
        private async void Button_Click3(object sender, RoutedEventArgs e)
        {
            await StartImageLoop();
        }
    }
}