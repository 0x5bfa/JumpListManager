// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.Shell.PropertiesSystem;
using Windows.Win32.UI.WindowsAndMessaging;

#if WASDK
using Microsoft.UI.Xaml.Data;
#elif UWP
using Windows.UI.Xaml.Data;
#endif


#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public partial class MainPageViewModel : ObservableObject
{
    public ObservableCollection<ApplicationItem> ApplicationItems { get; set; } = [];

    private ObservableCollection<JumpListItemGroupViewModel> GroupedJumpListItems { get; set; } = [];

    public CollectionViewSource? JumpListItems { get => field; set => SetProperty(ref field, value); }

    public ObservableCollection<BaseCommandItem> CommandItems { get; set; } = [];

    public int SelectedIndexOfApplicationItems { get; set; } = 0;

    [ObservableProperty]
    public partial JumpListItemViewModel? SelectedJumpListItem { get; set; }

    public string? SelectedApplicationUserModelID
    {
        get
        {
            if (SelectedIndexOfApplicationItems is -1)
                return null;
            return ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;
        }
    }

    public ICommand OpenCrcHashCalculatorCommand { get; }

    public ICommand OpenAboutDialogCommand { get; }

    public MainPageViewModel()
    {
        EnumerateApplicationItems();

        OpenCrcHashCalculatorCommand = new AsyncRelayCommand(ExecuteOpenCrcHashCalculatorCommand);
        OpenAboutDialogCommand = new AsyncRelayCommand(ExecuteOpenAboutDialogCommand);
    }

    public unsafe void EnumerateApplicationItems()
    {
        ApplicationItems.Clear();

        HRESULT hr = default;

        // Get the shell folder item
        hr = PInvoke.SHCreateItemFromParsingName("Shell:AppsFolder", null, typeof(IShellItem).GUID, out var shellItemObj);
        var shellItem = (IShellItem)shellItemObj;

        // Get the enumerator of the shell folder
        hr = shellItem.BindToHandler(null, PInvoke.BHID_EnumItems, typeof(IEnumShellItems).GUID, out var enumShellItemsObj);
        var enumShellItems = (IEnumShellItems)enumShellItemsObj;

        // Enumerate all child items one by one
        var childItemsArray = new IShellItem[1];
        while (enumShellItems.Next(1, childItemsArray) == HRESULT.S_OK)
        {
            var childItem = childItemsArray[0];

            // Get the application name
            using ComHeapPtr<char> pName = default;
            hr = childItem.GetDisplayName(SIGDN.SIGDN_PARENTRELATIVEFORUI, (PWSTR*)pName.GetAddressOf());

            // Get the AMUID from the property store of the item
            hr = childItem.BindToHandler(null, PInvoke.BHID_PropertyStore, typeof(IPropertyStore).GUID, out var propertyStoreObj);

            var propertyStore = (IPropertyStore)propertyStoreObj;

            hr = propertyStore.GetValue(PInvoke.PKEY_AppUserModel_ID, out var pVar);

            // Get the thumbnail
            var bitmapImageData = ThumbnailHelper.GetThumbnail(childItem, 64);

            // Insert the new item
            ApplicationItems.Add(new() { Icon = bitmapImageData, Name = new(pName.Get()), AppUserModelID = new(pVar.Anonymous.Anonymous.Anonymous.pwszVal) });

            // Dispose the unmanaged memory
            PInvoke.CoTaskMemFree(pVar.Anonymous.Anonymous.Anonymous.pwszVal);
        }
    }

    public void EnumerateJumpListItems()
    {
        JumpListItems = new() { IsSourceGrouped = true };
        GroupedJumpListItems.Clear();

        var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;

        // Initialize the jump list manager
        JumpList manager = JumpList.Create(amuid)
            ?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

        // Insert the pinned items
        if (manager.EnumeratePinnedItems() is { } pinnedItems && pinnedItems.Any())
        {
            var grouped = new JumpListItemGroupViewModel() { Key = "Pinned" };
            grouped.AddRange(pinnedItems.Select(x => JumpListItemViewModel.Create(x)));
            GroupedJumpListItems.Add(grouped);
        }

        // Insert the recent items
        if (manager.EnumerateRecentItems() is { } recentItems && recentItems.Any())
        {
            var grouped = new JumpListItemGroupViewModel() { Key = "Recent" };
            grouped.AddRange(recentItems.Select(x => JumpListItemViewModel.Create(x)));
            GroupedJumpListItems.Add(grouped);
        }

        // Insert the frequent items
        if (manager.EnumerateFrequentItems() is { } frequentItems && frequentItems.Any())
        {
            var grouped = new JumpListItemGroupViewModel() { Key = "Frequent" };
            grouped.AddRange(frequentItems.Select(x => JumpListItemViewModel.Create(x)));
            GroupedJumpListItems.Add(grouped);
        }

        // Insert the custom destination items
        int count = manager.GetCustomDestinationsCount();
        for (uint index = 0U; index < (uint)count; index++)
        {
            if (manager.EnumerateCustomDestinationsAt(index, out var categoryName) is { } customDestItems && customDestItems.Any())
            {
                var grouped = new JumpListItemGroupViewModel() { Key = categoryName };
                grouped.AddRange(customDestItems.Select(x => JumpListItemViewModel.Create(x)));
                GroupedJumpListItems.Add(grouped);
            }
        }

        // Insert the task items
        if (manager.EnumerateTasks() is { } taskItems && taskItems.Any())
        {
            var grouped = new JumpListItemGroupViewModel() { Key = "Tasks" };
            grouped.AddRange(taskItems.Select(x => JumpListItemViewModel.Create(x)));
            GroupedJumpListItems.Add(grouped);
        }

        JumpListItems.Source = new ObservableCollection<JumpListItemGroupViewModel>(GroupedJumpListItems);
    }

    public void EnumerateContextMenuItems()
    {
        CommandItems.Clear();

        if (SelectedJumpListItem is null)
            return;

        CommandItems.Add(new CommandButtonItem("\uE894", "Delete the all MRU", new RelayCommand(ExecuteClearAutomaticDestinationsCommand)));

        //CommandItems.Add(new CommandButtonItem("\uE894", "Delete this category", new RelayCommand(ExecuteClearAutomaticDestinationsCommand)));

        CommandItems.Add(new CommandSeparatorItem());

        CommandItems.Add(new CommandButtonItem("\uE737", "Open", new RelayCommand(ExecuteOpenCommand)));

        if (SelectedJumpListItem.Type is not JumpListItemType.Task)
        {
            if (SelectedJumpListItem.Type is JumpListItemType.Automatic &&
                SelectedJumpListItem.DataType is JumpListDataType.Item)
            {
                CommandItems.Add(new CommandButtonItem("\uED43", "Open file location", new RelayCommand(ExecuteOpenFileLocationCommand)));
            }

            CommandItems.Add(new CommandSeparatorItem());

            if (SelectedJumpListItem.Item.IsPinned)
            {
                CommandItems.Add(new CommandButtonItem("\uE718", "Unpin from the list", new RelayCommand(ExecuteUnpinItemCommand)));
            }
            else
            {
                CommandItems.Add(new CommandButtonItem("\uE718", "Pin to the list", new RelayCommand(ExecutePinItemCommand)));
                CommandItems.Add(new CommandButtonItem("\uE74D", "Remove from the list", new RelayCommand(ExecuteRemoveItemCommand)));
            }

            if (SelectedJumpListItem.Type is JumpListItemType.Automatic)
            {
                CommandItems.Add(new CommandSeparatorItem());

                CommandItems.Add(new CommandButtonItem("\uE90F", "Properties", new RelayCommand(ExecuteOpenPropertiesCommand)));
            }
        }
    }

    public unsafe void ExecuteClearAutomaticDestinationsCommand()
    {
        var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;

        JumpList manager = JumpList.Create(amuid)
            ?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

        manager.ClearAutomaticDestinations();

        EnumerateJumpListItems();
    }

    private unsafe void ExecuteOpenCommand()
    {
        if (SelectedJumpListItem is null)
            return;

        HRESULT hr = default;

        if (SelectedJumpListItem.DataType is JumpListDataType.Item)
        {
            SHELLEXECUTEINFOW info = default;
            info.cbSize = (uint)sizeof(SHELLEXECUTEINFOW);
            info.fMask = PInvoke.SEE_MASK_INVOKEIDLIST | PInvoke.SEE_MASK_FLAG_NO_UI | PInvoke.SEE_MASK_FLAG_DDEWAIT;
            info.nShow = (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL;

            using ComHeapPtr<ITEMIDLIST> thisPidl = default;
            hr = PInvoke.SHGetIDListFromObject(SelectedJumpListItem.Item.ComObject, thisPidl.GetAddressOf());

            fixed (char* pwszVerb = "open")
            {
                info.lpVerb = pwszVerb;
                info.lpIDList = thisPidl.Get();

                PInvoke.ShellExecuteEx(&info);
            }
        }
        else if (SelectedJumpListItem.DataType is JumpListDataType.Link)
        {
            PWSTR pwszPath = (PWSTR)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);
            PWSTR pwszArgs = (PWSTR)NativeMemory.AllocZeroed(PInvoke.MAX_PATH);

            hr = ((IShellLinkW)SelectedJumpListItem.Item.ComObject).GetPath(pwszPath, (int)PInvoke.MAX_PATH, null, 4U);
            hr = ((IShellLinkW)SelectedJumpListItem.Item.ComObject).GetArguments(pwszArgs, (int)PInvoke.MAX_PATH);

            SHELLEXECUTEINFOW info = default;
            info.cbSize = (uint)sizeof(SHELLEXECUTEINFOW);
            info.fMask = PInvoke.SEE_MASK_INVOKEIDLIST | PInvoke.SEE_MASK_FLAG_NO_UI | PInvoke.SEE_MASK_FLAG_DDEWAIT;
            info.nShow = (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL;

            using ComHeapPtr<ITEMIDLIST> thisPidl = default;
            hr = PInvoke.SHGetIDListFromObject(SelectedJumpListItem.Item.ComObject, thisPidl.GetAddressOf());

            info.lpFile = pwszPath;
            info.lpParameters = pwszArgs;

            PInvoke.ShellExecuteEx(&info);
        }
    }

    private unsafe void ExecuteOpenFileLocationCommand()
    {
        if (SelectedJumpListItem is null ||
            SelectedJumpListItem.Type is not JumpListItemType.Automatic ||
            SelectedJumpListItem.DataType is not JumpListDataType.Item ||
            SelectedJumpListItem.Item.ComObject is null)
            return;

        HRESULT hr = default;

        using ComHeapPtr<ITEMIDLIST> thisPidl = default;
        using ComHeapPtr<ITEMIDLIST> parentPidl = default;

        hr = ((IShellItem)SelectedJumpListItem.Item.ComObject).GetParent(out var parentItem);
        hr = PInvoke.SHGetIDListFromObject(SelectedJumpListItem.Item.ComObject, thisPidl.GetAddressOf());
        hr = PInvoke.SHGetIDListFromObject(parentItem, parentPidl.GetAddressOf());
        ITEMIDLIST* childPidl = PInvoke.ILFindLastID(thisPidl.Get());

        hr = PInvoke.SHOpenFolderAndSelectItems(parentPidl.Get(), 1, &childPidl, 0U);
    }

    private void ExecutePinItemCommand()
    {
        if (SelectedJumpListItem is null)
            return;

        var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;

        JumpList manager = JumpList.Create(amuid)
            ?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

        manager.PinItem(SelectedJumpListItem.Item);

        EnumerateJumpListItems();
    }

    private void ExecuteUnpinItemCommand()
    {
        if (SelectedJumpListItem is null)
            return;

        var amuid = ApplicationItems.ElementAt(SelectedIndexOfApplicationItems).AppUserModelID;

        JumpList manager = JumpList.Create(amuid)
            ?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

        manager.UnpinItem(SelectedJumpListItem.Item);

        EnumerateJumpListItems();
    }

    private void ExecuteRemoveItemCommand()
    {
        if (SelectedJumpListItem is null)
            return;

        JumpList manager = JumpList.Create(SelectedApplicationUserModelID!)
            ?? throw new InvalidOperationException($"Failed to initialize {nameof(JumpListManager)}.");

        manager.RemoveItem(SelectedJumpListItem.Item);

        EnumerateJumpListItems();
    }

    private unsafe void ExecuteOpenPropertiesCommand()
    {
        if (SelectedJumpListItem is null ||
            SelectedJumpListItem.Type is not JumpListItemType.Automatic ||
            SelectedJumpListItem.DataType is not JumpListDataType.Item ||
            SelectedJumpListItem.Item.ComObject is null)
            return;

        HRESULT hr = default;

        SHELLEXECUTEINFOW info = default;
        info.cbSize = (uint)sizeof(SHELLEXECUTEINFOW);
        info.fMask = PInvoke.SEE_MASK_INVOKEIDLIST | PInvoke.SEE_MASK_FLAG_NO_UI | PInvoke.SEE_MASK_FLAG_DDEWAIT;
        info.nShow = (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL;

        using ComHeapPtr<ITEMIDLIST> thisPidl = default;
        hr = PInvoke.SHGetIDListFromObject(SelectedJumpListItem.Item.ComObject, thisPidl.GetAddressOf());

        fixed (char* pwszVerb = "properties")
        {
            info.lpVerb = pwszVerb;
            info.lpIDList = thisPidl.Get();

            PInvoke.ShellExecuteEx(&info);
        }
    }

    private async Task ExecuteOpenCrcHashCalculatorCommand()
    {
        var dialog = new CrcCalculatorDialog
        {
#if WASDK
            XamlRoot = App.MainWindow!.Content.XamlRoot
#endif
        };

        await dialog.ShowAsync();
    }

    private async Task ExecuteOpenAboutDialogCommand()
    {
        var dialog = new AboutDialog
        {
#if WASDK
            XamlRoot = App.MainWindow!.Content.XamlRoot
#endif
        };

        await dialog.ShowAsync();
    }

    partial void OnSelectedJumpListItemChanged(JumpListItemViewModel? oldValue, JumpListItemViewModel? newValue)
    {
        oldValue?.IsSelected = false;
        newValue?.IsSelected = true;
    }
}
