// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public class ApplicationItem
{
	public byte[]? Icon { get; set; }

	public string? Name { get; set; }

	public string? AppUserModelID { get; set; }
}
