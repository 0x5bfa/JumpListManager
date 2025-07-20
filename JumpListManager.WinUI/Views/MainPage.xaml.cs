// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using JumpListManager.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace JumpListManager.Views
{
	public sealed partial class MainPage : Page
	{
		private MainPageViewModel ViewModel { get; } = new();

		public MainPage()
		{
			InitializeComponent();
		}

		private void ApplicationItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.CommandItems.Clear();
			ViewModel.EnumerateJumpListItems();
		}

		private void JumpListListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.EnumerateContextMenuItems();
		}
	}
}
