// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Windows.Win32.System.Com
{
	[GeneratedComInterface, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("E9C5EF8D-FD41-4F72-BA87-EB03BAD5817C")]
	public unsafe partial interface IAutomaticDestinationList
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT Initialize(PCWSTR szAppId, PCWSTR a2, PCWSTR a3);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT HasList([MarshalAs(UnmanagedType.Bool)] out bool pfHasList);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT GetList(DESTLISTTYPE type, int maxCount, GETDESTLISTFLAGS flags, Guid* riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

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
		HRESULT SetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, out float a2, out long pFileTime);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT GetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, out float a2, out long pFileTime);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT ResolveDestination(HWND hWnd, int a2, [MarshalAs(UnmanagedType.Interface)] IShellItem psi, Guid* riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		HRESULT ClearList([MarshalAs(UnmanagedType.Bool)] bool removePinsToo);
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
