// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using JumpListManager.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace JumpListManager.Views
{
	public sealed partial class CrcCalculatorDialog : ContentDialog
	{
		private CrcCalculatorViewModel ViewModel { get; } = new();

		public CrcCalculatorDialog()
		{
			InitializeComponent();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewModel.CrcHash = string.Empty;
		}

		private void TextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
		{
			if (sender is TextBox textBox && e.Key is Windows.System.VirtualKey.Enter)
			{
				ViewModel.CalculateCrcHash(textBox.Text);
			}
		}
	}
}
