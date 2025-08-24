// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JumpListManager.Data;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.PropertiesSystem;
using Windows.Win32.UI.WindowsAndMessaging;

namespace JumpListManager.ViewModels
{
	public class MainPageViewModel : ObservableObject
	{
		public ObservableCollection<ApplicationItem> ApplicationItems { get; set; } = [];

		private ObservableCollection<JumpListGroupItem> GroupedJumpListItems { get; set; } = [];
		public CollectionViewSource? JumpListItems { get => field; set => SetProperty(ref field, value); }

		public ObservableCollection<BaseCommandItem> CommandItems { get; set; } = [];

		public int SelectedIndexOfApplicationItems { get; set; } = 0;

		public int SelectedIndexOfJumpListItems { get; set; } = 0;

		public JumpListItem? SelectedJumpListItem
		{
			get
			{
				if (SelectedIndexOfJumpListItems is -1)
					return null;
				var flattenItems = GroupedJumpListItems.SelectMany(group => group).ToList();
				return flattenItems.ElementAt(SelectedIndexOfJumpListItems);
			}
		}

		public string? SelectedApplicationUserModelID
		{
			get
			{
				if (SelectedIndexOfApplicationItems is -1)
					return null;
				return ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;
			}
		}

		public ICommand OpenAboutDialogCommand { get; }

		public MainPageViewModel()
		{
			EnumerateApplicationItems();

			OpenAboutDialogCommand = new AsyncRelayCommand(ExecuteOpenAboutDialogCommand);
		}

		public unsafe void EnumerateApplicationItems()
		{
			ApplicationItems.Clear();

			HRESULT hr = default;

			// Get the shell folder item
			using ComPtr<IShellItem> pShellItem = default;
			fixed (char* pwszShellAppsFolderPath = "Shell:AppsFolder")
				hr = PInvoke.SHCreateItemFromParsingName(pwszShellAppsFolderPath, null, IID.IID_IShellItem, (void**)pShellItem.GetAddressOf());

			// Get the enumerator of the shell folder
			using ComPtr<IEnumShellItems> pEnumShellItems = default;
			hr = pShellItem.Get()->BindToHandler(null, BHID.BHID_EnumItems, IID.IID_IEnumShellItems, (void**)pEnumShellItems.GetAddressOf());

			// Enumerate all child items one by one
			ComPtr<IShellItem> pChildShellItem = default;
			while (pEnumShellItems.Get()->Next(1, pChildShellItem.GetAddressOf()) == HRESULT.S_OK)
			{
				// Get the application name
				using ComHeapPtr<char> pName = default;
				hr = pChildShellItem.Get()->GetDisplayName(SIGDN.SIGDN_PARENTRELATIVEFORUI, (PWSTR*)pName.GetAddressOf());

				// Get the AMUID from the property store of the item
				ComPtr<IPropertyStore> pPropertyStore = default;
				PROPERTYKEY pKey = PInvoke.PKEY_AppUserModel_ID;
				PROPVARIANT pVar = default;
				hr = pChildShellItem.Get()->BindToHandler(null, BHID.BHID_PropertyStore, IID.IID_IPropertyStore, (void**)pPropertyStore.GetAddressOf());
				hr = pPropertyStore.Get()->GetValue(&pKey, &pVar);

				// Get the thumbnail
				var bitmapImageData = ThumbnailHelper.GetThumbnail(pChildShellItem.Get(), 64);

				// Insert the new item
				ApplicationItems.Add(new() { Icon = bitmapImageData, Name = new(pName.Get()), AppUserModelID = new(pVar.Anonymous.Anonymous.Anonymous.pwszVal) });

				// Dispose the unmanaged memory
				PInvoke.CoTaskMemFree(pVar.Anonymous.Anonymous.Anonymous.pwszVal);
				pChildShellItem.Dispose();
			}
		}

		public unsafe void EnumerateJumpListItems()
		{
			JumpListItems = new() { IsSourceGrouped = true, };
			foreach (var list in GroupedJumpListItems) foreach (var item in list) item.Dispose();
			GroupedJumpListItems.Clear();

			var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;

			// Initialize the jump list manager
			using JumpList manager = JumpList.Create(amuid)
				?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

			// Insert the pinned items
			if (manager.EnumeratePinnedItems() is { Count: not 0 } pinnedItems)
				GroupedJumpListItems.Add(pinnedItems);

			// Insert the recent items
			if (manager.EnumerateRecentItems() is { Count: not 0 } recentItems)
				GroupedJumpListItems.Add(recentItems);

			// Insert the custom destination items
			int count = manager.GetCustomDestinationsCount();
			for (uint index = 0U; index < (uint)count; index++)
				if (manager.EnumerateCustomDestinationsAt(index) is { } list)
					GroupedJumpListItems.Add(list);

			// Insert the task items
			if (manager.EnumerateTasks() is { Count: not 0 } taskItems)
				GroupedJumpListItems.Add(taskItems);

			JumpListItems.Source = new ObservableCollection<JumpListGroupItem>(GroupedJumpListItems);
		}

		public unsafe void EnumerateContextMenuItems()
		{
			CommandItems.Clear();

			if (SelectedIndexOfJumpListItems is -1)
				return;

			var flattenItems = GroupedJumpListItems.SelectMany(group => group).ToList();
			var selectedJumpListItem = flattenItems.ElementAt(SelectedIndexOfJumpListItems);

			CommandItems.Add(new CommandButtonItem("\uE737", "Open", new RelayCommand(ExecuteOpenCommand)));

			if (selectedJumpListItem.Type is not JumpListItemType.Task)
			{
				if (selectedJumpListItem.Type is JumpListItemType.Automatic &&
					selectedJumpListItem.DataType is JumpListDataType.IShellItem)
				{
					CommandItems.Add(new CommandButtonItem("\uED43", "Open file location", new RelayCommand(ExecuteOpenFileLocationCommand)));
				}

				CommandItems.Add(new CommandSeparatorItem());

				if (selectedJumpListItem.IsPinned)
				{
					CommandItems.Add(new CommandButtonItem("\uE718", "Unpin from the list", new RelayCommand(ExecuteUnpinItemCommand)));
				}
				else
				{
					CommandItems.Add(new CommandButtonItem("\uE718", "Pin to the list", new RelayCommand(ExecutePinItemCommand)));
					CommandItems.Add(new CommandButtonItem("\uE74D", "Remove from the list", new RelayCommand(ExecuteRemoveItemCommand)));
				}

				if (selectedJumpListItem.Type is JumpListItemType.Automatic)
				{
					CommandItems.Add(new CommandSeparatorItem());

					CommandItems.Add(new CommandButtonItem("\uE90F", "Properties", new RelayCommand(ExecuteOpenPropertiesCommand)));
				}
			}
		}

		private void ExecuteOpenCommand()
		{
		}

		private void ExecuteOpenFileLocationCommand()
		{
		}

		private void ExecutePinItemCommand()
		{
			var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;
			var flattenItems = GroupedJumpListItems.SelectMany(group => group).ToList();
			var selectedJumpListItem = flattenItems.ElementAt(SelectedIndexOfJumpListItems);

			using JumpList manager = JumpList.Create(amuid)
				?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

			manager.PinItem(selectedJumpListItem);

			EnumerateJumpListItems();
		}

		private void ExecuteUnpinItemCommand()
		{
			var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;
			var flattenItems = GroupedJumpListItems.SelectMany(group => group).ToList();
			var selectedJumpListItem = flattenItems.ElementAt(SelectedIndexOfJumpListItems);

			using JumpList manager = JumpList.Create(amuid)
				?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

			manager.UnpinItem(selectedJumpListItem);

			EnumerateJumpListItems();
		}

		private void ExecuteRemoveItemCommand()
		{
			using JumpList manager = JumpList.Create(SelectedApplicationUserModelID!)
				?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

			manager.RemoveItem(SelectedJumpListItem!);

			EnumerateJumpListItems();
		}

		private unsafe void ExecuteOpenPropertiesCommand()
		{
			//SHELLEXECUTEINFOW info = default;
			//info.cbSize = (uint)sizeof(SHELLEXECUTEINFOW);
			//info.lpVerb = "properties";
			//info.lpFile = Filename;
			//info.nShow = (int)SHOW_WINDOW_CMD.SW_SHOW;
			//info.fMask = PInvoke.SEE_MASK_INVOKEIDLIST;

			//fixed (char* pwszVerb = "properties", pwszFilePath = "")
			//{
			//	PInvoke.ShellExecuteEx(&info);
			//}
		}

		private async Task ExecuteOpenAboutDialogCommand()
		{
			var dialog = new Views.SettingsDialog
			{
				XamlRoot = App.MainWindow!.Content.XamlRoot
			};

			await dialog.ShowAsync();
		}
	}
}
