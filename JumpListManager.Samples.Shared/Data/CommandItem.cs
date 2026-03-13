// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Windows.Input;

#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public abstract record BaseCommandItem();

public record CommandButtonItem(string? Glyph, string Label, ICommand Command) : BaseCommandItem;

public record CommandSeparatorItem() : BaseCommandItem;
