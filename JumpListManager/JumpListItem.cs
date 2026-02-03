// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace JumpListManager
{
	public unsafe class JumpListItem(JumpListItemType type, byte[]? iconData, string? text, bool isPinned, JumpListDataType dataType, object comObject)
	{
		public JumpListItemType Type { get; set; } = type;

		public byte[]? IconData { get; set; } = iconData;

		public string? Text { get; set; } = text;

		public bool IsPinned { get; set; } = isPinned;

		public JumpListDataType DataType { get; set; } = dataType;

		public object ComObject { get; set; } = comObject;

		public IContextMenu ContextMenu { get; set; }
	}
}
