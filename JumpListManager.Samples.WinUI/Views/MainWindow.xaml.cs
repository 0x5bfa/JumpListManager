// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JumpListManager.Samples.WinUI;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		AppWindow.SetIcon("Assets/AppIcon.ico");
		ExtendsContentIntoTitleBar = true;

		var frame = new Frame();
		frame.Navigate(typeof(MainPage));

		Content = frame;
	}
}
