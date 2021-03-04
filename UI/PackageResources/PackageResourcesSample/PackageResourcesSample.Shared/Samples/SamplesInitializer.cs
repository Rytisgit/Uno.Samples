using System;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
#if __WASM__
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Uno.Foundation;
#endif
#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
#if !__WASM__
using Launcher = Xamarin.Essentials.Launcher;
#endif
#endif

namespace Xamarin.Essentials
{
	// dummy placeholder
}

namespace PackageResourcesSample
{
	public static class SamplesInitializer
	{
		public static void Init()
		{

#if WINDOWS_UWP || __IOS__ || __TVOS__ || __ANDROID__ || __TIZEN__
			var localStorage = FileSystem.AppDataDirectory;
#elif __WASM__
			var localStorage = ApplicationData.Current.LocalFolder.Path;
#endif

			SamplesManager.OpenFile += OnOpenSampleFile;
			SamplesManager.TempDataPath = Path.Combine(localStorage, "SkiaSharpSample", "TemporaryData");
			if (!Directory.Exists(SamplesManager.TempDataPath))
			{
				Directory.CreateDirectory(SamplesManager.TempDataPath);
			}
		}

		private static async void OnOpenSampleFile(string path)
		{
#if WINDOWS_UWP || __TVOS__ || __IOS__ || __ANDROID__ || __TIZEN__
			var title = "Open " + Path.GetExtension(path).ToUpperInvariant();
			await Launcher.OpenAsync(new OpenFileRequest(title, new ReadOnlyFile(path)));
#elif __MACOS__
			if (!NSWorkspace.SharedWorkspace.OpenFile(path))
			{
				var alert = new NSAlert();
				alert.AddButton("OK");
				alert.MessageText = "SkiaSharp";
				alert.InformativeText = "Unable to open file.";
				alert.RunSheetModal(NSApplication.SharedApplication.MainWindow);
			}
#elif __DESKTOP__
			Process.Start(path);
#elif __WASM__
			var data = File.ReadAllBytes(path);
			var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
			var pinnedData = gch.AddrOfPinnedObject();
			try
			{
				WebAssemblyRuntime.InvokeJS($"fileSaveAs('{Path.GetFileName(path)}', {pinnedData}, {data.Length})");
			}
			finally
			{
				gch.Free();
			}
#endif
		}
	}
}
