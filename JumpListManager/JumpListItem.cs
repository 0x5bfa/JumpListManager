// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

namespace JumpListManager
{
	public class JumpListItem(JumpListItemType type, byte[]? iconData, string? text, bool isPinned, JumpListDataType dataType, uint accessCount, DateTime lastAccessed, object comObject)
	{
		public JumpListItemType Type { get; set; } = type;

		public byte[]? IconData { get; set; } = iconData;

		public string? Text { get; set; } = text;

		public bool IsPinned { get; set; } = isPinned;

		public JumpListDataType DataType { get; set; } = dataType;

		public uint AccessCount { get; set; } = accessCount;

		public DateTime LastAccessed { get; set; } = lastAccessed;

		public object ComObject { get; set; } = comObject;
	}
}
