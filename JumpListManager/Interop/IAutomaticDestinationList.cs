// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;

namespace Windows.Win32.System.Com;

[GeneratedComInterface]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("E9C5EF8D-FD41-4F72-BA87-EB03BAD5817C")]
public partial interface IAutomaticDestinationList
{
    /// <summary>
    /// Initializes the instance.
    /// </summary>
    /// <param name="appId">Sets AUMID.</param>
    /// <param name="appFullPath">Sets the full path to the application executable. Used only for unpackaged apps; otherwise, this is NULL.</param>
    /// <param name="customAutoDestFullFilePath">Sets the full path to the custom auto dest file, if needed. Usually, this is NULL.</param>
    /// <returns>If succeeded, returns S_OK; otherwise, an HRESULT error code.</returns>
    [PreserveSig]
    HRESULT Initialize(PCWSTR appId, [Optional] PCWSTR appFullPath, [Optional] PCWSTR customAutoDestFullFilePath);

    [PreserveSig]
    HRESULT HasList([MarshalAs(UnmanagedType.Bool)] out bool hasList);

    [PreserveSig]
    HRESULT GetList(DESTLISTTYPE type, int maxCount, GETDESTLISTFLAGS flags, in Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

    [PreserveSig]
    HRESULT AddUsagePoint([MarshalAs(UnmanagedType.Interface)] object punk);

    [PreserveSig]
    HRESULT PinItem([MarshalAs(UnmanagedType.Interface)] object punk, int index);

    [PreserveSig]
    HRESULT GetPinIndex([MarshalAs(UnmanagedType.Interface)] object punk, out int piIndex);

    [PreserveSig]
    HRESULT RemoveDestination([MarshalAs(UnmanagedType.Interface)] object psi);

    [PreserveSig]
    HRESULT SetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, in float pAccessCount, in long aLastAccessedUtc);

    [PreserveSig]
    HRESULT GetUsageData([MarshalAs(UnmanagedType.Interface)] object punk, out float pAccessCount, out long aLastAccessedUtc);

    [PreserveSig]
    HRESULT ResolveDestination(HWND hWnd, int a2, [MarshalAs(UnmanagedType.Interface)] IShellItem psi, in Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

    [PreserveSig]
    HRESULT ClearList([MarshalAs(UnmanagedType.Bool)] bool removePinsAsWell);
}

[GeneratedComInterface]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("8DC24A1A-6314-4769-9D68-179786F4CED6")]
public partial interface IAutomaticDestinationList2 : IAutomaticDestinationList
{
    [PreserveSig]
    HRESULT AddUsagePointsEx([MarshalAs(UnmanagedType.Interface)] object pUnk, int createDestinationItem, int action);

    [PreserveSig]
    HRESULT BlockItem([MarshalAs(UnmanagedType.Interface)] object pUnk);

    [PreserveSig]
    HRESULT ClearBlocked();

    [PreserveSig]
    HRESULT TransferPoints([MarshalAs(UnmanagedType.Interface)] object pFrom, [MarshalAs(UnmanagedType.Interface)] object pTo);

    [PreserveSig]
    HRESULT HasListEx([MarshalAs(UnmanagedType.Bool)] out bool pfHasList, out int pUnknown);

    [PreserveSig]
    HRESULT SetDataInternal([MarshalAs(UnmanagedType.Interface)] object pItem, out float usagePoints, [MarshalUsing(typeof(FILETIMEDateTimeMarshaller))] out DateTime pLastAccessTime, int unknown);

    [PreserveSig]
    HRESULT GetDataInternal([MarshalAs(UnmanagedType.Interface)] object pItem, int matchTarget, out float usagePoints, [MarshalUsing(typeof(FILETIMEDateTimeMarshaller))] out DateTime pLastAccessTime, out uint pUnknownOut1, out int pUnknownOut2);

    [PreserveSig]
    HRESULT UpdateRenamedItems(IObjectCollection pOldItems, IObjectCollection pNewItems, out int pUpdatedCount);

    [PreserveSig]
    HRESULT RemoveDeletedItems(IObjectCollection pDeletedItems, out int pRemovedCount);

    [PreserveSig]
    HRESULT AddUsagePointsForFolders(IObjectCollection pFolders, int action);

    [PreserveSig]
    HRESULT UpdateCachedItems(IObjectCollection pItems, out int pUpdatedCount);

    [PreserveSig]
    HRESULT TryAddUsagePointsIfExists([MarshalAs(UnmanagedType.Interface)] object pUnk, out int pUpdated);

    [PreserveSig]
    HRESULT AddFileUsagePoints([MarshalAs(UnmanagedType.Interface)] object pUnk, int createDestinationItem, uint actionOrFlags);
}

[GeneratedComInterface]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("136A95C3-D245-4808-BE33-38CA17670BFA")]
public partial interface IAutomaticDestinationList3 : IAutomaticDestinationList2
{
}

[GeneratedComInterface]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("5D96756C-6AB4-4730-BC86-5DE97D5F6DEA")]
public partial interface IAutomaticDestinationList4 : IAutomaticDestinationList3
{
}
