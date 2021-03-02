using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FrameGenerator;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using PackageResourcesSample.Shared;
using SkiaSharp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PackageResourcesSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var o = new ToggleSwitch { FlowDirection = FlowDirection.RightToLeft };
        }

        public async Task LoadPackageFile()
        {
            try
            {
                //var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(assetFileName.Text));
                //var bytes = await FileIO.ReadBufferAsync(file);
                //var stream = bytes.AsStream();
                //await ExtractExtraFileFolder(stream);
                //output.Text = await FileIO.ReadTextAsync(file);

                var generator = new MainGenerator(new UnoFileReader(), 69);
                await generator.InitialiseGenerator();
                output.Text = $"{generator.GenerateImage(null).Height}";
            }
            catch (Exception e)
            {
                output.Text = e.ToString();
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
            await LoadPackageFile();
        }
    }
}
