// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace JumpListManager;

/// <summary>
/// Represents a native wrapper for accessing and managing Windows Jump Lists using unmanaged COM interfaces.
/// </summary>
public unsafe partial class JumpList
{
    private IAutomaticDestinationList _autoDestList = null!;
    private ICustomDestinationList _customDestList = null!;
    private IInternalCustomDestinationList _customDestList2 = null!;

    private IObjectCollection pinnedItemsObjectCollection = null!;

    /// <summary>
    /// Creates an instance of <see cref="JumpList"/> for the specified Application User Model ID (AppUserModelID).
    /// </summary>
    /// <param name="szAppId">The AMUID to initialize an instance with.</param>
    /// <returns>If succeeded, returns a valid instance of <see cref="JumpList"/>; otherwise, returns null.</returns>
    public static JumpList? Create(string? szAppId)
    {
        if (string.IsNullOrEmpty(szAppId)) return null;

        HRESULT hr = default;

        hr = PInvoke.CoCreateInstance(PInvoke.CLSID_AutomaticDestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, out IAutomaticDestinationList autoDestListPtr).ThrowOnFailure();
        if (FAILED(hr)) return null;

        hr = PInvoke.CoCreateInstance(PInvoke.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, out ICustomDestinationList customDestListPtr).ThrowOnFailure();
        if (FAILED(hr)) return null;

        hr = PInvoke.CoCreateInstance(PInvoke.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, out IInternalCustomDestinationList customDestList2Ptr).ThrowOnFailure();
        if (FAILED(hr)) return null;

        hr = customDestListPtr.SetAppID(szAppId).ThrowOnFailure();
        if (FAILED(hr)) return null;

        // These internally convert the passed AMUID string to the corresponding CRC hash and initialize the path to the destination lists with FOLDERID_Recent.
        fixed (char* pwszAppId = szAppId)
        {
            hr = autoDestListPtr.Initialize(pwszAppId, default, default).ThrowOnFailure();
            if (FAILED(hr)) return null;

            hr = customDestList2Ptr.SetApplicationID(pwszAppId).ThrowOnFailure();
            if (FAILED(hr)) return null;
        }

        return new() { _autoDestList = autoDestListPtr, _customDestList = customDestListPtr, _customDestList2 = customDestList2Ptr };
    }

    public bool HasAutomaticDestinationsOf(DESTLISTTYPE type)
    {
        HRESULT hr = _autoDestList.HasList(out var hasList);
        if (FAILED(hr) || !hasList) return false;

        hr = _autoDestList.GetList(type, 1, GETDESTLISTFLAGS.NONE, typeof(IObjectCollection).GUID, out var objectCollectionObj);
        if (FAILED(hr)) return false;

        var objectCollection = (IObjectCollection)objectCollectionObj;

        hr = objectCollection.GetCount(out uint pcObjects);
        if (FAILED(hr)) return false;

        return pcObjects is not 0U;
    }

    public IEnumerable<JumpListItem>? EnumeratePinnedItems(int count = 20)
    {
        HRESULT hr = _autoDestList.GetList(DESTLISTTYPE.PINNED, count, GETDESTLISTFLAGS.NONE, typeof(IObjectCollection).GUID, out var objectCollectionObj);
        if (FAILED(hr)) return null;

        var objectCollection = (IObjectCollection)objectCollectionObj;

        // Cache the pinned items collection for later use
        pinnedItemsObjectCollection = objectCollection;

        return CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, objectCollection);
    }

    public IEnumerable<JumpListItem>? EnumerateRecentItems(int count = 20)
    {
        HRESULT hr = default;

        hr = _autoDestList.GetList(DESTLISTTYPE.RECENT, count, GETDESTLISTFLAGS.NONE, typeof(IObjectCollection).GUID, out var objectCollectionObj);
        if (FAILED(hr)) return null;

        var objectCollection = (IObjectCollection)objectCollectionObj;

        return CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, objectCollection);
    }

    public IEnumerable<JumpListItem>? EnumerateFrequentItems(int count = 20)
    {
        HRESULT hr = _autoDestList.GetList(DESTLISTTYPE.FREQUENT, count, GETDESTLISTFLAGS.NONE, typeof(IObjectCollection).GUID, out var objectCollectionObj);
        if (FAILED(hr)) return null;

        var objectCollection = (IObjectCollection)objectCollectionObj;

        return CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, objectCollection);
    }

    public int GetCustomDestinationsCount()
    {
        HRESULT hr = _customDestList2.GetCategoryCount(out var count);
        if (FAILED(hr)) return -1;

        return (int)count;
    }

    public IEnumerable<JumpListItem>? EnumerateCustomDestinationsAt(uint dwIndex, out string categoryName, int count = 20)
    {
        HRESULT hr = default;

        categoryName = string.Empty;

        APPDESTCATEGORY category = default;
        char* pszCategoryName = null;

        try
        {
            // Get the category data (e.g., the type, the name, and the count of the destinations)
            hr = _customDestList2.GetCategory(dwIndex, GETCATFLAG.GCF_COPY_NAME, out category);
            if (FAILED(hr) || category.Type is not Windows.Win32.System.Com.KNOWNDESTCATEGORY.CUSTOM)
                return null;

            // Get the category name
            pszCategoryName = (char*)NativeMemory.AllocZeroed(256);
            hr = PInvoke.SHLoadIndirectString(category.Anonymous.Name, pszCategoryName, 256);
            if (FAILED(hr))
                return null;

            categoryName = new string(pszCategoryName);

            // Enumerate the destinations in the category
            hr = _customDestList2.EnumerateCategoryDestinations(dwIndex, typeof(IObjectCollection).GUID, out var objectCollectionObj);
            if (FAILED(hr))
                return null;

            var objectCollection = (IObjectCollection)objectCollectionObj;

            return CreateCollectionFromIObjectCollection(JumpListItemType.Custom, objectCollection);
        }
        finally
        {
            if (pszCategoryName is not null) NativeMemory.Free(pszCategoryName);
            if (category.Anonymous.Name.Value is not null && category.Type is Windows.Win32.System.Com.KNOWNDESTCATEGORY.CUSTOM) PInvoke.CoTaskMemFree(category.Anonymous.Name);
        }
    }

    public IEnumerable<JumpListItem>? EnumerateTasks(int count = 20)
    {
        HRESULT hr = default;

        int dwCategoryCount = GetCustomDestinationsCount();
        if (dwCategoryCount is -1)
            return null;

        for (uint dwCategoryIndex = 0U; dwCategoryIndex < (uint)dwCategoryCount; dwCategoryIndex++)
        {
            APPDESTCATEGORY category = default;
            char* pszCategoryName = null;
            try
            {
                // Get the category data (e.g., the type, the name, and the count of the destinations)
                hr = _customDestList2.GetCategory(dwCategoryIndex, GETCATFLAG.GCF_COPY_NAME, out category);
                if (FAILED(hr) || category.Type is not Windows.Win32.System.Com.KNOWNDESTCATEGORY.TASKS)
                    continue;

                // Enumerate and add the destinations in the category to the list
                hr = _customDestList2.EnumerateCategoryDestinations(dwCategoryIndex, typeof(IObjectCollection).GUID, out var objectCollectionObj);
                if (FAILED(hr)) return null;

                var objectCollection = (IObjectCollection)objectCollectionObj;

                return CreateCollectionFromIObjectCollection(JumpListItemType.Task, objectCollection);
            }
            finally
            {
                if (pszCategoryName is not null) NativeMemory.Free(pszCategoryName);
                if (category.Anonymous.Name.Value is not null && category.Type is Windows.Win32.System.Com.KNOWNDESTCATEGORY.CUSTOM) PInvoke.CoTaskMemFree(category.Anonymous.Name);
            }
        }

        return null;
    }

    public bool ClearAutomaticDestinations()
    {
        HRESULT hr = default;

        hr = _autoDestList.HasList(out var hasList);
        if (FAILED(hr) || !hasList) return false;

        hr = _autoDestList.ClearList(BOOL.TRUE);
        if (FAILED(hr)) return false;

        return true;
    }

    public bool PinItem(JumpListItem item)
    {
        if (item is null || item.IsPinned || item.Type is JumpListItemType.Task)
            return false;

        HRESULT hr = default;

        hr = _autoDestList.PinItem(item.ComObject, -1 /*Pin at the last*/);
        if (FAILED(hr)) return false;

        item.IsPinned = true;
        return true;
    }

    public bool UnpinItem(JumpListItem item)
    {
        if (item is null || !item.IsPinned || item.Type is JumpListItemType.Task)
            return false;

        HRESULT hr = default;

        hr = _autoDestList.PinItem(item.ComObject, -2 /*Unpin*/);
        if (FAILED(hr)) return false;

        item.IsPinned = false;
        return true;
    }

    public bool RemoveItem(JumpListItem item)
    {
        if (item is null || item.Type is JumpListItemType.Task)
            return false;

        HRESULT hr = default;

        if (item.Type is JumpListItemType.Automatic)
        {
            hr = _autoDestList.RemoveDestination(item.ComObject);
            if (FAILED(hr)) return false;
        }
        else if (item.Type is JumpListItemType.Custom)
        {
            hr = _customDestList2.RemoveDestination(item.ComObject);
            if (FAILED(hr)) return false;
        }

        return true;
    }

    private IEnumerable<JumpListItem> CreateCollectionFromIObjectCollection(JumpListItemType type, IObjectCollection objectCollection)
    {
        HRESULT hr = objectCollection.GetCount(out uint pcObjects);
        if (FAILED(hr)) return [];

        List<JumpListItem> items = [];
        for (uint dwIndex = 0U; dwIndex < pcObjects; dwIndex++)
        {
            hr = objectCollection.GetAt(dwIndex, IUnknown.IID_Guid, out var itemObj);
            if (FAILED(hr)) continue;

            if (CreateItemFromIUnknown(type, itemObj) is { } item)
                items.Add(item);
        }

        return items;
    }

    private JumpListItem? CreateItemFromIUnknown(JumpListItemType type, object obj)
    {
        HRESULT hr = default;

        if (obj is IShellItem shellItem)
        {
            // Get the display name
            using ComHeapPtr<char> pwszName = default;
            hr = shellItem.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, (PWSTR*)pwszName.GetAddressOf());
            if (FAILED(hr)) return null;

            // Get the thumbnail
            var bitmapImageData = ThumbnailHelper.GetThumbnail(shellItem);

            _autoDestList.GetUsageData(obj, out var accessCount, out var LastAccessedUtc);

            return new(type, bitmapImageData, new string(pwszName.Get()), IsPinned(shellItem), JumpListDataType.Item, (uint)Math.Round(accessCount), DateTime.FromFileTimeUtc(LastAccessedUtc).ToLocalTime(), shellItem);
        }
        else
        {
            if (obj is IShellLinkW shellLink)
            {
                // Get the display name
                var propertyStore = (IPropertyStore)shellLink;
                hr = propertyStore.GetValue(PInvoke.PKEY_Title, out var pVar);
                if (FAILED(hr)) return null;

                // Get the thumbnail
                var bitmapImageData = ThumbnailHelper.GetThumbnail(shellLink);

                _autoDestList.GetUsageData(obj, out var accessCount, out var LastAccessedUtc);

                return new(type, bitmapImageData, new string(pVar.Anonymous.Anonymous.Anonymous.pwszVal), IsPinned(shellLink), JumpListDataType.Link, (uint)Math.Round(accessCount), DateTime.FromFileTimeUtc(LastAccessedUtc).ToLocalTime(), shellLink);
            }
            else
            {
                return null;
            }
        }
    }

    private bool IsPinned(object obj)
    {
        HRESULT hr = default;

        if (pinnedItemsObjectCollection is null)
        {
            hr = _autoDestList.GetList(DESTLISTTYPE.PINNED, 20, GETDESTLISTFLAGS.NONE, typeof(IObjectCollection).GUID, out var objectCollectionObj);
            if (FAILED(hr)) return false;

            pinnedItemsObjectCollection = (IObjectCollection)objectCollectionObj;
        }

        hr = pinnedItemsObjectCollection.GetCount(out uint cPinnedObjects);
        if (FAILED(hr)) return false;

        for (uint dwIndex = 0U; dwIndex < cPinnedObjects; dwIndex++)
        {
            hr = pinnedItemsObjectCollection.GetAt(dwIndex, IUnknown.IID_Guid, out var pinnedItemObj);

            if (IsSameObject(pinnedItemObj, obj))
                return true;
        }

        return false;
    }

    private bool IsSameObject(object pObj1, object pObj2)
    {
        HRESULT hr = default;

        if (pObj1 is IShellItem shellItem)
        {
            if (pObj2 is IShellItem shellItemOther)
            {
                hr = shellItem.Compare(shellItemOther, (uint)(_SICHINTF.SICHINT_CANONICAL | _SICHINTF.SICHINT_TEST_FILESYSPATH_IF_NOT_EQUAL), out var iOrder); // 0x30000000
                return iOrder is 0;
            }
        }
        else
        {
            if (pObj1 is IShellLinkW shellLink)
            {
                if (pObj2 is IShellLinkW shellLinkOther)
                {
                    char* pwszString1 = (char*)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);
                    char* pwszString2 = (char*)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);

                    COMPARESTRING_RESULT compStrResult = default;

                    hr = shellLink.GetPath(pwszString1, (int)PInvoke.MAX_PATH /*260*/, null, (uint)SLGP_FLAGS.SLGP_RAWPATH /*0x4*/);
                    if (SUCCEEDED(hr))
                    {
                        hr = shellLinkOther.GetPath(pwszString2, (int)PInvoke.MAX_PATH /*260*/, null, (uint)SLGP_FLAGS.SLGP_RAWPATH /*0x4*/);
                        if (SUCCEEDED(hr))
                        {
                            compStrResult = PInvoke.CompareStringOrdinal(pwszString1, -1, pwszString2, -1, BOOL.FALSE);
                            NativeMemory.Free(pwszString1);
                            NativeMemory.Free(pwszString2);

                            if (compStrResult is COMPARESTRING_RESULT.CSTR_EQUAL /*2*/)
                            {
                                hr = shellLink.GetArguments(pwszString1, (int)PInvoke.MAX_PATH);
                                if (SUCCEEDED(hr))
                                {
                                    hr = shellLinkOther.GetArguments(pwszString2, (int)PInvoke.MAX_PATH);
                                    if (SUCCEEDED(hr))
                                    {
                                        compStrResult = PInvoke.CompareStringOrdinal(pwszString1, -1, pwszString2, -1, BOOL.FALSE);
                                        NativeMemory.Free(pwszString1);
                                        NativeMemory.Free(pwszString2);

                                        if (compStrResult is COMPARESTRING_RESULT.CSTR_EQUAL /*2*/)
                                        {
                                            var propertyStore = (IPropertyStore)shellLink;
                                            hr = propertyStore.GetValue(PInvoke.PKEY_Title, out var propVar1);

                                            var propertyStoreOther = (IPropertyStore)shellLinkOther;
                                            hr = propertyStoreOther.GetValue(PInvoke.PKEY_Title, out var propVarOther);

                                            // TODO: Define and use the inline functions
                                            compStrResult = PInvoke.CompareStringOrdinal(propVar1.Anonymous.Anonymous.Anonymous.pwszVal, -1, propVarOther.Anonymous.Anonymous.Anonymous.pwszVal, -1, BOOL.FALSE);
                                            PInvoke.CoTaskMemFree(propVar1.Anonymous.Anonymous.Anonymous.pwszVal);
                                            PInvoke.CoTaskMemFree(propVarOther.Anonymous.Anonymous.Anonymous.pwszVal);

                                            return compStrResult is COMPARESTRING_RESULT.CSTR_EQUAL /*2*/;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
