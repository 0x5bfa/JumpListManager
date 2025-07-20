// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Windows.Input;

namespace JumpListManager.Data
{
	public abstract record BaseCommandItem();

	public record CommandButtonItem(string? Glyph, string Label, ICommand Command) : BaseCommandItem;

	public record CommandSeparatorItem() : BaseCommandItem;
}
