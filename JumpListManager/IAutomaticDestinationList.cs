// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Windows.Win32.System.Com
{
	// I have reverse engineered this interface via IDA Pro and WinDbg. Name of parameters is determined by how they are used.
	[GeneratedComInterface, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("E9C5EF8D-FD41-4F72-BA87-EB03BAD5817C")]
	public unsafe partial interface IAutomaticDestinationList
	{
		/// <summary>
		/// Initializes the instance.
		/// </summary>
		/// <param name="appId">Sets AUMID.</param>
		/// <param name="appFullPath">Sets the full path to the application executable. Used only for unpackaged apps; otherwise, this is NULL.</param>
		/// <param name="customAutoDestFullFilePath">Sets the full path to the custom auto dest file, if needed. Usually, this is NULL.</param>
		/// <returns>If succeeded, returns S_OK; otherwise, an HRESULT error code.</returns>
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT Initialize(PCWSTR appId, [Optional] PCWSTR appFullPath, [Optional] PCWSTR customAutoDestFullFilePath);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT HasList([MarshalAs(UnmanagedType.Bool)] out bool hasList);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT GetList(DESTLISTTYPE type, int maxCount, GETDESTLISTFLAGS flags, Guid* riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT AddUsagePoint([MarshalAs(UnmanagedType.Interface)] object punk);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT PinItem([MarshalAs(UnmanagedType.Interface)] object punk, int index);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT GetPinIndex([MarshalAs(UnmanagedType.Interface)] object punk, out int piIndex);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT RemoveDestination([MarshalAs(UnmanagedType.Interface)] object psi);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT SetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, in float pAccessCount, in long aLastAccessedUtc);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT GetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, out float pAccessCount, out long aLastAccessedUtc);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT ResolveDestination(HWND hWnd, int a2, [MarshalAs(UnmanagedType.Interface)] IShellItem psi, Guid* riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT ClearList([MarshalAs(UnmanagedType.Bool)] bool removePinsAsWell);
	}

	public enum DESTLISTTYPE : uint
	{
		PINNED,
		RECENT,
		FREQUENT,
	}

	public enum GETDESTLISTFLAGS : uint
	{
		NONE,
		EXCLUDE_UNNAMED_DESTINATIONS,
	}
}
