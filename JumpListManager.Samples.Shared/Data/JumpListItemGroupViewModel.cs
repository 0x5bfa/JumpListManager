// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public class JumpListItemGroupViewModel : List<JumpListItemViewModel>
{
    public required string Key { get; set; }
}
