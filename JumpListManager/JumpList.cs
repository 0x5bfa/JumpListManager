// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace JumpListManager
{
	/// <summary>
	/// Represents a native wrapper for accessing and managing Windows Jump Lists using unmanaged COM interfaces.
	/// </summary>
	public unsafe partial class JumpList : IDisposable
	{
		private IAutomaticDestinationList* _autoDestListPtr = null;
		private ICustomDestinationList* _customDestListPtr = null;
		private IInternalCustomDestinationList* _customDestList2Ptr = null;

		private IObjectCollection* pPinnedItemsObjectCollection = null;

		/// <summary>
		/// Creates an instance of <see cref="JumpList"/> for the specified Application User Model ID (AppUserModelID).
		/// </summary>
		/// <param name="szAppId">The AMUID to initialize an instance with.</param>
		/// <returns>If succeeded, returns a valid instance of <see cref="JumpList"/>; otherwise, returns null.</returns>
		public static JumpList? Create(string? szAppId)
		{
			if (string.IsNullOrEmpty(szAppId))
				return null;

			HRESULT hr = default;

			IAutomaticDestinationList* autoDestListPtr = default;
			ICustomDestinationList* customDestListPtr = default;
			IInternalCustomDestinationList* customDestList2Ptr = default;

			hr = PInvoke.CoCreateInstance(CLSID.CLSID_AutomaticDestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_IAutomaticDestinationList, (void**)&autoDestListPtr).ThrowOnFailure();
			if (FAILED(hr))
				return null;

			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList, (void**)&customDestListPtr).ThrowOnFailure();
			if (FAILED(hr))
				return null;

			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList2, (void**)&customDestList2Ptr).ThrowOnFailure();
			if (FAILED(hr))
				return null;

			// These internally convert the passed AMUID string to the corresponding CRC hash and initialize the path to the destination lists with FOLDERID_Recent.
			fixed (char* pwszAppId = szAppId)
			{
				hr = autoDestListPtr->Initialize(pwszAppId, default, default).ThrowOnFailure();
				if (FAILED(hr))
					return null;

				hr = customDestListPtr->SetAppID(pwszAppId).ThrowOnFailure();
				if (FAILED(hr))
					return null;

				hr = customDestList2Ptr->SetApplicationID(pwszAppId).ThrowOnFailure();
				if (FAILED(hr))
					return null;
			}

			return new() { _autoDestListPtr = autoDestListPtr, _customDestListPtr = customDestListPtr, _customDestList2Ptr = customDestList2Ptr };
		}

		public bool HasAutomaticDestinationsOf(DESTLISTTYPE type)
		{
			HRESULT hr = default;

			using ComPtr<IObjectCollection> pObjectCollection = default;

			BOOL fHasList = default;
			hr = _autoDestListPtr->HasList(&fHasList);
			if (FAILED(hr) || !(bool)fHasList) return false;

			hr = _autoDestListPtr->GetList(type, 1, GETDESTLISTFLAGS.NONE, IID.IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());
			if (FAILED(hr))
				return false;

			hr = pObjectCollection.Get()->GetCount(out uint pcObjects);
			if (FAILED(hr))
				return false;

			return pcObjects is not 0U;
		}

		public JumpListGroupItem? EnumeratePinnedItems(int count = 20)
		{
			HRESULT hr = default;

			IObjectCollection* pObjectCollection = default;
			JumpListGroupItem items = new() { Key = "Pinned" };

			hr = _autoDestListPtr->GetList(DESTLISTTYPE.PINNED, count, GETDESTLISTFLAGS.NONE, IID.IID_IObjectCollection, (void**)&pObjectCollection);
			if (FAILED(hr)) return null;

			// Cache the pinned items collection for later use
			if (pPinnedItemsObjectCollection is not null) pPinnedItemsObjectCollection->Release();
			pPinnedItemsObjectCollection = pObjectCollection;

			items.AddRange(CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, pObjectCollection));

			return items;
		}

		public JumpListGroupItem? EnumerateRecentItems(int count = 20)
		{
			HRESULT hr = default;

			IObjectCollection* pObjectCollection = default;
			JumpListGroupItem items = new() { Key = "Recent" };

			try
			{
				hr = _autoDestListPtr->GetList(DESTLISTTYPE.RECENT, count, GETDESTLISTFLAGS.NONE, IID.IID_IObjectCollection, (void**)&pObjectCollection);
				if (FAILED(hr)) return null;

				items.AddRange(CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, pObjectCollection));
				return items;
			}
			finally
			{
				pObjectCollection->Release();
			}
		}

		public JumpListGroupItem? EnumerateFrequentItems(int count = 20)
		{
			HRESULT hr = default;

			IObjectCollection* pObjectCollection = default;
			JumpListGroupItem items = new() { Key = "Frequent" };

			try
			{
				hr = _autoDestListPtr->GetList(DESTLISTTYPE.FREQUENT, count, GETDESTLISTFLAGS.NONE, IID.IID_IObjectCollection, (void**)&pObjectCollection);
				if (FAILED(hr)) return null;

				items.AddRange(CreateCollectionFromIObjectCollection(JumpListItemType.Automatic, pObjectCollection));
				return items;
			}
			finally
			{
				pObjectCollection->Release();
			}
		}

		public int GetCustomDestinationsCount()
		{
			HRESULT hr = default;

			uint dwCategoryCount = 0U;
			hr = _customDestList2Ptr->GetCategoryCount(&dwCategoryCount);
			if (FAILED(hr))
				return -1;

			return (int)dwCategoryCount;
		}

		public JumpListGroupItem? EnumerateCustomDestinationsAt(uint dwIndex, int count = 20)
		{
			HRESULT hr = default;

			APPDESTCATEGORY category = default;
			char* pszCategoryName = null;
			JumpListGroupItem groupedCollection = new() { Key = string.Empty };

			try
			{
				// Get the category data (e.g., the type, the name, and the count of the destinations)
				hr = _customDestList2Ptr->GetCategory(dwIndex, GETCATFLAG.DEFAULT, &category);
				if (FAILED(hr) || category.Type is not APPDESTCATEGORYTYPE.CUSTOM)
					return null;

				// Get the category name
				pszCategoryName = (char*)NativeMemory.AllocZeroed(256);
				hr = PInvoke.SHLoadIndirectString(category.Anonymous.Name, pszCategoryName, 256);
				if (FAILED(hr))
					return null;

				groupedCollection.Key = new string(pszCategoryName);

				using ComPtr<IObjectCollection> pObjectCollection = default;

				// Enumerate the destinations in the category
				hr = _customDestList2Ptr->EnumerateCategoryDestinations(dwIndex, IID.IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());
				if (FAILED(hr))
					return null;

				groupedCollection.AddRange(CreateCollectionFromIObjectCollection(JumpListItemType.Custom, pObjectCollection.Get()));

				return groupedCollection;
			}
			finally
			{
				if (pszCategoryName is not null) NativeMemory.Free(pszCategoryName);
				if (category.Anonymous.Name.Value is not null && category.Type is APPDESTCATEGORYTYPE.CUSTOM) PInvoke.CoTaskMemFree(category.Anonymous.Name);
				if (FAILED(hr)) foreach (var item in groupedCollection) item.Dispose();
			}
		}

		public JumpListGroupItem? EnumerateTasks(int count = 20)
		{
			HRESULT hr = default;

			int dwCategoryCount = GetCustomDestinationsCount();
			if (dwCategoryCount is -1)
				return null;

			JumpListGroupItem groupedCollection = new() { Key = "Tasks" };

			for (uint dwCategoryIndex = 0U; dwCategoryIndex < (uint)dwCategoryCount; dwCategoryIndex++)
			{
				APPDESTCATEGORY category = default;
				char* pszCategoryName = null;
				try
				{
					// Get the category data (e.g., the type, the name, and the count of the destinations)
					hr = _customDestList2Ptr->GetCategory(dwCategoryIndex, GETCATFLAG.DEFAULT, &category);
					if (FAILED(hr) || category.Type is not APPDESTCATEGORYTYPE.TASKS)
						continue;

					// Enumerate and add the destinations in the category to the list
					using ComPtr<IObjectCollection> pObjectCollection = default;
					hr = _customDestList2Ptr->EnumerateCategoryDestinations(dwCategoryIndex, IID.IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());
					if (FAILED(hr))
						return null;

					groupedCollection.AddRange(CreateCollectionFromIObjectCollection(JumpListItemType.Task, pObjectCollection.Get()));
					return groupedCollection;
				}
				finally
				{
					if (pszCategoryName is not null) NativeMemory.Free(pszCategoryName);
					if (category.Anonymous.Name.Value is not null) PInvoke.CoTaskMemFree(category.Anonymous.Name);
					if (FAILED(hr)) foreach (var item in groupedCollection) item.Dispose();
				}
			}

			return null;
		}

		public bool PinItem(JumpListItem item)
		{
			if (item is null || item.IsPinned || item.Type is JumpListItemType.Task)
				return false;

			HRESULT hr = default;

			hr = _autoDestListPtr->PinItem((IUnknown*)item.NativeObjectPtr, -1 /*Pin at the last*/);
			if (FAILED(hr)) return false;

			item.IsPinned = true;
			return true;
		}

		public bool UnpinItem(JumpListItem item)
		{
			if (item is null || !item.IsPinned || item.Type is JumpListItemType.Task)
				return false;

			HRESULT hr = default;

			hr = _autoDestListPtr->PinItem((IUnknown*)item.NativeObjectPtr, -2 /*Unpin*/);
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
				hr = _autoDestListPtr->RemoveDestination((IUnknown*)item.NativeObjectPtr);
				if (FAILED(hr)) return false;
			}
			else if (item.Type is JumpListItemType.Custom)
			{
				hr = _customDestList2Ptr->RemoveDestination((IUnknown*)item.NativeObjectPtr);
				if (FAILED(hr)) return false;
			}

			return true;
		}

		private IEnumerable<JumpListItem> CreateCollectionFromIObjectCollection(JumpListItemType type, IObjectCollection* pObjectCollection)
		{
			HRESULT hr = default;

			List<JumpListItem> items = [];

			hr = pObjectCollection->GetCount(out uint pcObjects);

			for (uint dwIndex = 0U; dwIndex < pcObjects; dwIndex++)
			{
				using ComPtr<IUnknown> pObj = default;
				hr = pObjectCollection->GetAt(dwIndex, IID.IID_IUnknown, (void**)pObj.GetAddressOf());

				if (CreateItemFromIUnknown(type, pObj.Get()) is { } item)
					items.Add(item);
			}

			return items;
		}

		private JumpListItem? CreateItemFromIUnknown(JumpListItemType type, IUnknown* pObj)
		{
			HRESULT hr = default;

			using ComPtr<IShellItem> pShellItem = default;
			hr = pObj->QueryInterface(IID.IID_IShellItem, (void**)pShellItem.GetAddressOf());
			if (SUCCEEDED(hr))
			{
				// Get the display name
				using ComHeapPtr<char> pwszName = default;
				hr = pShellItem.Get()->GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, (PWSTR*)pwszName.GetAddressOf());
				if (FAILED(hr))
					return null;

				// Get the thumbnail
				var bitmapImageData = ThumbnailHelper.GetThumbnail(pShellItem.Get());

				IShellItem* pShellItemGlobal = null;
				pShellItem.CopyTo(&pShellItemGlobal);

				return new(type, bitmapImageData, new string(pwszName.Get()), IsPinned((IUnknown*)pShellItem.Get()), JumpListDataType.IShellItem, (IUnknown*)pShellItemGlobal);
			}
			else
			{
				using ComPtr<IShellLinkW> pShellLink = default;
				hr = pObj->QueryInterface(IID.IID_IShellLinkW, (void**)pShellLink.GetAddressOf()).ThrowOnFailure();
				if (SUCCEEDED(hr))
				{
					// Get the display name
					using ComPtr<IPropertyStore> pPropertyStore = default;
					PROPERTYKEY pKey = PInvoke.PKEY_Title;
					PROPVARIANT pVar = default;
					pShellLink.Get()->QueryInterface(IID.IID_IPropertyStore, (void**)pPropertyStore.GetAddressOf());
					hr = pPropertyStore.Get()->GetValue(&pKey, &pVar);
					if (FAILED(hr))
						return null;

					// Get the thumbnail
					var bitmapImageData = ThumbnailHelper.GetThumbnail(pShellLink.Get());

					IShellLinkW* pShellLinkGlobal = null;
					pShellLink.CopyTo(&pShellLinkGlobal);

					return new(type, bitmapImageData, new string(pVar.Anonymous.Anonymous.Anonymous.pwszVal), IsPinned((IUnknown*)pShellLinkGlobal), JumpListDataType.IShellLink, (IUnknown*)pShellLinkGlobal);
				}
				else
				{
					return null;
				}
			}
		}

		private bool IsPinned(IUnknown* pObject)
		{
			HRESULT hr = default;

			if (pPinnedItemsObjectCollection is null)
			{
				IObjectCollection* pObjectCollection = default;
				hr = _autoDestListPtr->GetList(DESTLISTTYPE.PINNED, 20, GETDESTLISTFLAGS.NONE, IID.IID_IObjectCollection, (void**)&pObjectCollection);
				if (FAILED(hr))
					return false;

				pPinnedItemsObjectCollection = pObjectCollection;
			}

			hr = pPinnedItemsObjectCollection->GetCount(out uint cPinnedObjects);
			if (FAILED(hr))
				return false;

			for (uint dwIndex = 0U; dwIndex < cPinnedObjects; dwIndex++)
			{
				using ComPtr<IUnknown> pPinnedItemObject = default;
				hr = pPinnedItemsObjectCollection->GetAt(dwIndex, IID.IID_IUnknown, (void**)pPinnedItemObject.GetAddressOf());

				if (IsSameObject(pPinnedItemObject.Get(), pObject))
					return true;
			}

			return false;
		}

		private bool IsSameObject(IUnknown* pObj1, IUnknown* pObj2)
		{
			HRESULT hr = default;

			using ComPtr<IShellItem> pShellItem1 = default;
			hr = pObj1->QueryInterface(IID.IID_IShellItem, (void**)pShellItem1.GetAddressOf());
			if (SUCCEEDED(hr))
			{
				using ComPtr<IShellItem> pShellItem2 = default;
				hr = pObj2->QueryInterface(IID.IID_IShellItem, (void**)pShellItem2.GetAddressOf());
				if (SUCCEEDED(hr))
				{
					int iOrder = 0;

					hr = pShellItem1.Get()->Compare(
						pShellItem2.Get(),
						(uint)(_SICHINTF.SICHINT_CANONICAL | _SICHINTF.SICHINT_TEST_FILESYSPATH_IF_NOT_EQUAL), // 0x30000000
						&iOrder);

					return iOrder is 0;
				}
			}
			else
			{
				using ComPtr<IShellLinkW> pShellLink1 = default;
				hr = pObj1->QueryInterface(IID.IID_IShellLinkW, (void**)pShellLink1.GetAddressOf());
				if (SUCCEEDED(hr))
				{
					using ComPtr<IShellLinkW> pShellLink2 = default;
					hr = pObj2->QueryInterface(IID.IID_IShellLinkW, (void**)pShellLink2.GetAddressOf());
					if (SUCCEEDED(hr))
					{
						char* pwszString1 = (char*)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);
						char* pwszString2 = (char*)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);

						COMPARESTRING_RESULT compStrResult = default;

						hr = pShellLink1.Get()->GetPath(pwszString1, (int)PInvoke.MAX_PATH /*260*/, null, (uint)SLGP_FLAGS.SLGP_RAWPATH /*0x4*/);
						if (SUCCEEDED(hr))
						{
							hr = pShellLink2.Get()->GetPath(pwszString2, (int)PInvoke.MAX_PATH /*260*/, null, (uint)SLGP_FLAGS.SLGP_RAWPATH /*0x4*/);
							if (SUCCEEDED(hr))
							{
								compStrResult = PInvoke.CompareStringOrdinal(pwszString1, -1, pwszString2, -1, BOOL.FALSE);
								NativeMemory.Free(pwszString1);
								NativeMemory.Free(pwszString2);

								if (compStrResult is COMPARESTRING_RESULT.CSTR_EQUAL /*2*/)
								{
									hr = pShellLink1.Get()->GetArguments(pwszString1, (int)PInvoke.MAX_PATH);
									if (SUCCEEDED(hr))
									{
										hr = pShellLink2.Get()->GetArguments(pwszString2, (int)PInvoke.MAX_PATH);
										if (SUCCEEDED(hr))
										{
											compStrResult = PInvoke.CompareStringOrdinal(pwszString1, -1, pwszString2, -1, BOOL.FALSE);
											NativeMemory.Free(pwszString1);
											NativeMemory.Free(pwszString2);

											if (compStrResult is COMPARESTRING_RESULT.CSTR_EQUAL /*2*/)
											{
												PROPERTYKEY pKey = PInvoke.PKEY_Title;

												using ComPtr<IPropertyStore> pPropertyStore1 = default;
												PROPVARIANT pPropVar1 = default;
												pShellLink1.Get()->QueryInterface(IID.IID_IPropertyStore, (void**)pPropertyStore1.GetAddressOf());
												hr = pPropertyStore1.Get()->GetValue(&pKey, &pPropVar1);

												using ComPtr<IPropertyStore> pPropertyStore2 = default;
												PROPVARIANT pPropVar2 = default;
												pShellLink2.Get()->QueryInterface(IID.IID_IPropertyStore, (void**)pPropertyStore2.GetAddressOf());
												hr = pPropertyStore2.Get()->GetValue(&pKey, &pPropVar2);

												compStrResult = PInvoke.CompareStringOrdinal(pPropVar1.Anonymous.Anonymous.Anonymous.pwszVal, -1, pPropVar2.Anonymous.Anonymous.Anonymous.pwszVal, -1, BOOL.FALSE);
												PInvoke.CoTaskMemFree(pPropVar1.Anonymous.Anonymous.Anonymous.pwszVal);
												PInvoke.CoTaskMemFree(pPropVar2.Anonymous.Anonymous.Anonymous.pwszVal);

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

		public void Dispose()
		{
			if (_autoDestListPtr is not null) ((IUnknown*)_autoDestListPtr)->Release();
			if (_customDestListPtr is not null) ((IUnknown*)_customDestListPtr)->Release();
			if (_customDestList2Ptr is not null) ((IUnknown*)_customDestList2Ptr)->Release();
			if (pPinnedItemsObjectCollection is not null) ((IUnknown*)pPinnedItemsObjectCollection)->Release();
		}
	}
}
