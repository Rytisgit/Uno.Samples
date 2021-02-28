using System;
using System.Collections.Generic;
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
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
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
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(assetFileName.Text));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream);
                await ReadExtraFileFolder("vault");
                output.Text = await FileIO.ReadTextAsync(file);
            }
            catch (Exception e)
            {
                output.Text = e.ToString();
            }
        }

        //public static Dictionary<string, SkiaSharp.SKBitmap> GetCharacterPNG(string gameLocation)
        //{

        //    var GetCharacterPNG = new Dictionary<string, SkiaSharp.SKBitmap>();

        //    List<string> allpngfiles = Directory.GetFiles(gameLocation + @"/rltiles/player/base", "*.png*", SearchOption.AllDirectories).ToList();
        //    allpngfiles.AddRange(Directory.GetFiles(gameLocation + @"/rltiles/player/felids", "*.png*", SearchOption.AllDirectories).ToList());
        //    foreach (var file in allpngfiles)
        //    {
        //        FileInfo info = new FileInfo(file);
        //        SkiaSharp.SKBitmap SKBitmap = SKBitmap.Decode(file);


        //        GetCharacterPNG[info.Name.Replace(".png", "")] = SKBitmap;

        //    }
        //    return GetCharacterPNG;
        //}

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
                    var outputFile = Path.Combine(localFolder.Path, entry.Name);

                    var outputDirectory = Path.GetDirectoryName(outputFile);
                    //Console.WriteLine(outputDirectory);
                    var correctFolder = await localFolder.CreateFolderAsync(outputDirectory, CreationCollisionOption.OpenIfExists);


                    if (entry.IsFile)
                    {

                        int size;
                        byte[] buffer = new byte[zipInStream.Length];

                        zipInStream.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(outputFile, buffer);
                        var bitmap = SKBitmap.Decode(buffer);
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
        private async Task ReadExtraFileFolder(string foldername)
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Console.WriteLine(localFolder.Path);
                var folders = await localFolder.GetFoldersAsync();
                foreach (var storageFolder in folders)
                {
                    foreach (var storageFile in await storageFolder.GetFilesAsync())
                    {
                        Console.WriteLine(storageFile.OpenAsync(FileAccessMode.Read));
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await LoadPackageFile();
        }
    }
}
