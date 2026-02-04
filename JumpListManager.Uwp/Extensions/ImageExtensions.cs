// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace JumpListManager.WinUI.Extensions
{
	public sealed class ImageExtensions : DependencyObject
	{
		public static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.RegisterAttached(
				"ImageSource",
				typeof(byte[]),
				typeof(ImageExtensions),
				new PropertyMetadata(null, OnImageSourceChanged));

		public static byte[] GetImageSource(DependencyObject obj)
		{
			return (byte[])obj.GetValue(ImageSourceProperty);
		}

		public static void SetImageSource(DependencyObject obj, byte[] value)
		{
			obj.SetValue(ImageSourceProperty, value);
		}

		private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Image image && e.NewValue is byte[] data)
				image.Source = ConvertToBitmapImage(data);
		}

		private static BitmapImage? ConvertToBitmapImage(byte[]? @this, int decodeSize = -1)
		{
			if (@this is null)
				return null;

			try
			{
				using var ms = new MemoryStream(@this);
				var image = new BitmapImage();

				if (decodeSize > 0)
				{
					image.DecodePixelWidth = decodeSize;
					image.DecodePixelHeight = decodeSize;
				}

				image.DecodePixelType = DecodePixelType.Logical;
				image.SetSource(ms.AsRandomAccessStream());

				return image;
			}
			catch
			{
				return null;
			}
		}
	}
}
