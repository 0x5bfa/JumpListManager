// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.GdiPlus;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace JumpListManager
{
	public unsafe static class ThumbnailHelper
	{
		private static (Guid Format, Guid Encorder)[]? GdiEncoders;

		public static byte[]? GetThumbnail(IShellItem item, int size = 32)
		{
			HRESULT hr = default;
			HBITMAP hBitmap = default;
			byte* flippedBits = null;

			try
			{
				var shellItemImageFactory = (IShellItemImageFactory)item;
				if (shellItemImageFactory is null) return null;

				// Get HBITMAP
				hr = shellItemImageFactory.GetImage(new(size, size), SIIGBF.SIIGBF_ICONONLY, &hBitmap);
				if (FAILED(hr) || hBitmap.IsNull) return null;

				// Retrieve BITMAP data
				BITMAP bmp = default;
				if (PInvoke.GetObject(hBitmap, sizeof(BITMAP), &bmp) is 0)
					return null;

				// Allocate buffer for flipped pixel data
				flippedBits = (byte*)NativeMemory.AllocZeroed((nuint)(bmp.bmWidthBytes * bmp.bmHeight));

				// Flip the image manually row by row
				for (int y = 0; y < bmp.bmHeight; y++)
				{
					Buffer.MemoryCopy(
						(byte*)bmp.bmBits + y * bmp.bmWidthBytes,
						flippedBits + (bmp.bmHeight - y - 1) * bmp.bmWidthBytes,
						bmp.bmWidthBytes,
						bmp.bmWidthBytes);
				}

				// Create GpBitmap from the flipped pixel data
				GpBitmap* gpBitmap = default;
				var status = PInvoke.GdipCreateBitmapFromScan0(bmp.bmWidth, bmp.bmHeight, bmp.bmWidthBytes, 2498570, flippedBits, &gpBitmap);
				if (status is not Status.Ok) return null;

				if (!ConvertGpBitmapToByteArray(gpBitmap, out var thumbnailData)) return null;

				return thumbnailData;
			}
			finally
			{
				if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);
				if (flippedBits is not null) NativeMemory.Free(flippedBits);
			}
		}

		public static byte[]? GetThumbnail(IShellLinkW link, int size = 32)
		{
			HRESULT hr = default;
			char* pwszIconLocation = null;
			HICON hIconLarge = default;
			HICON hIconSmall = default;
			GpBitmap* pGpBitmap = null;

			try
			{
				pwszIconLocation = (char*)NativeMemory.Alloc(256);

				hr = link.GetIconLocation(pwszIconLocation, 256, out var nIconIndex);
				if (FAILED(hr)) return null;

				var extractIconW = (IExtractIconW)link;
				if (FAILED(hr)) return null;

				hr = extractIconW.Extract(pwszIconLocation, (uint)nIconIndex, &hIconLarge, &hIconSmall, (uint)size);
				if (FAILED(hr) || hIconLarge.IsNull || hIconSmall.IsNull) return null;

				// Use GDI+ to convert the icon to a bitmap with alpha mask
				using var icon = System.Drawing.Icon.FromHandle(hIconLarge);
				using var bitmap = new System.Drawing.Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using var g = System.Drawing.Graphics.FromImage(bitmap);
				g.Clear(System.Drawing.Color.Transparent);
				g.DrawIcon(icon, 0, 0);
				using var ms = new MemoryStream();
				bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

				return ms.ToArray();
			}
			finally
			{
				if (pwszIconLocation is not null) NativeMemory.Free(pwszIconLocation);
				if (!hIconLarge.IsNull) PInvoke.DestroyIcon(hIconLarge);
				if (!hIconSmall.IsNull) PInvoke.DestroyIcon(hIconSmall);
				if (pGpBitmap is not null) PInvoke.GdipDisposeImage((GpImage*)pGpBitmap);
			}
		}

		public static bool ConvertGpBitmapToByteArray(GpBitmap* gpBitmap, out byte[]? imageData)
		{
			imageData = null;
			byte* pRawThumbnailData = null;

			try
			{
				// Get an encoder for PNG
				Guid format = Guid.Empty;
				if (PInvoke.GdipGetImageRawFormat((GpImage*)gpBitmap, &format) is not Status.Ok)
					return false;

				Guid encoder = GetEncoderClsid(format);
				if (format == PInvoke.ImageFormatJPEG || encoder == Guid.Empty)
				{
					format = PInvoke.ImageFormatPNG;
					encoder = GetEncoderClsid(format);
				}

				HRESULT hr = PInvoke.CreateStreamOnHGlobal(HGLOBAL.Null, true, out var stream);
				if (hr.ThrowOnFailure().Failed)
					return false;

				if (PInvoke.GdipSaveImageToStream((GpImage*)gpBitmap, stream, &encoder, (EncoderParameters*)null) is not Status.Ok)
					return false;

				STATSTG stat = default;
				hr = stream.Stat(&stat, STATFLAG.STATFLAG_NONAME);
				if (hr.ThrowOnFailure().Failed)
					return false;

				ulong statSize = stat.cbSize & 0xFFFFFFFF;
				pRawThumbnailData = (byte*)NativeMemory.Alloc((nuint)statSize);
				stream.Seek(0L, (SeekOrigin)STREAM_SEEK.STREAM_SEEK_SET, null);
				hr = stream.Read(pRawThumbnailData, (uint)statSize);
				if (hr.ThrowOnFailure().Failed)
					return false;

				imageData = new ReadOnlySpan<byte>(pRawThumbnailData, (int)statSize / sizeof(byte)).ToArray();

				return true;
			}
			finally
			{
				if (pRawThumbnailData is not null) NativeMemory.Free(pRawThumbnailData);
			}

			Guid GetEncoderClsid(Guid format)
			{
				foreach ((Guid Format, Guid Encoder) in GetGdiEncoders())
					if (Format == format)
						return Encoder;

				return Guid.Empty;
			}

			(Guid Format, Guid Encorder)[] GetGdiEncoders()
			{
				if (GdiEncoders is not null)
					return GdiEncoders;

				if (PInvoke.GdipGetImageEncodersSize(out var numEncoders, out var size) is not Status.Ok)
					return [];

				ImageCodecInfo* pImageCodecInfo = (ImageCodecInfo*)NativeMemory.Alloc(size);

				if (PInvoke.GdipGetImageEncoders(numEncoders, size, pImageCodecInfo) is not Status.Ok)
					return [];

				ReadOnlySpan<ImageCodecInfo> codecs = new(pImageCodecInfo, (int)numEncoders);
				GdiEncoders = new (Guid Format, Guid Encoder)[codecs.Length];
				for (int index = 0; index < codecs.Length; index++)
					GdiEncoders[index] = (codecs[index].FormatID, codecs[index].Clsid);

				return GdiEncoders;
			}
		}
	}
}
