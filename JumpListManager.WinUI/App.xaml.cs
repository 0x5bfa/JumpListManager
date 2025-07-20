// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using JumpListManager.Views;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.GdiPlus;

namespace JumpListManager
{
	public unsafe partial class App : Application
	{
		public static Window? MainWindow { get; private set; }

		public static float Dpi { get; private set; }

		private nuint _dwGdiPlusToken;

		public App()
		{
			InitializeComponent();

			GdiplusStartupInput gpsi = default;
			gpsi.GdiplusVersion = 1;
			nuint dwGdiPlusToken;
			var status = PInvoke.GdiplusStartup(&dwGdiPlusToken, &gpsi, null);
			_dwGdiPlusToken = dwGdiPlusToken;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			MainWindow = new MainWindow();
			MainWindow.Activate();

			Dpi = PInvoke.GetDpiForWindow((HWND)WinRT.Interop.WindowNative.GetWindowHandle(MainWindow)) / 96f;
		}

		~App()
		{
			PInvoke.GdiplusShutdown(_dwGdiPlusToken);
		}
	}
}
