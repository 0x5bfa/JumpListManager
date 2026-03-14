// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

[GeneratedComInterface]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("507101CD-F6AD-46C8-8E20-EEB9E6BAC47F")]
public unsafe partial interface IInternalCustomDestinationList
{
	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT SetMinItems(uint dwMinItems);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT SetApplicationID(PCWSTR pszAppID);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT GetSlotCount(out uint pdwSlotCount);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT GetCategoryCount(out uint pdwCategoryCount);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT GetCategory(uint index, GETCATFLAG flags, out APPDESTCATEGORY pCategory);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT DeleteCategory(uint a1, int a2);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT EnumerateCategoryDestinations(uint index, in Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT RemoveDestination([MarshalAs(UnmanagedType.Interface)] object pUnk);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT HasListEx(out int a1, out int a2);

	[PreserveSig]
	[return: MarshalAs(UnmanagedType.Error)]
	HRESULT ClearRemovedDestinations();
}

[StructLayout(LayoutKind.Sequential)]
public struct APPDESTCATEGORY
{
	[StructLayout(LayoutKind.Explicit)]
	public struct _Anonymous_e__Union
	{
		[FieldOffset(0)]
		public PWSTR Name;

		[FieldOffset(0)]
		public KNOWNDESTCATEGORY Known;
	}

	public KNOWNDESTCATEGORY Type;

	public _Anonymous_e__Union Anonymous;

	public uint ItemCount;

	public uint SeparatorCount;
}

public enum GETCATFLAG : uint
{
    GCF_DEFAULT = 0,
	GCF_COPY_NAME = 1,
}

public enum KNOWNDESTCATEGORY : uint
{
	CUSTOM = 0,
	KNOWN = 1,
	TASKS = 2,
}
