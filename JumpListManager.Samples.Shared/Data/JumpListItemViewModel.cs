// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using System;

#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public partial class JumpListItemViewModel(
    JumpListItem item,
    JumpListItemType type,
    JumpListDataType dataType,
    byte[]? iconData,
    string? text,
    uint usagePoints,
    DateTime lastAccessed,
    uint actionCount
    ) : ObservableObject
{
    public JumpListItem Item { get; set; } = item;

    [ObservableProperty]
    public partial JumpListItemType Type { get; set; } = type;

    [ObservableProperty]
    public partial JumpListDataType DataType { get; set; } = dataType;

    [ObservableProperty]
    public partial byte[]? IconData { get; set; } = iconData;

    [ObservableProperty]
    public partial string? Text { get; set; } = text;

    [ObservableProperty]
    public partial uint UsagePoints { get; set; } = usagePoints;

    [ObservableProperty]
    public partial DateTime LastAccessed { get; set; } = lastAccessed;

    [ObservableProperty]
    public partial uint ActionCount { get; set; } = actionCount;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public static JumpListItemViewModel Create(JumpListItem item)
    {
        return new JumpListItemViewModel(
            item,
            item.Type,
            item.DataType,
            item.IconData,
            item.Text,
            item.AccessCount,
            item.LastAccessed,
            0
            );
    }
}
