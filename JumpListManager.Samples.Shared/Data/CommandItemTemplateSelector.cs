// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System;

#if WASDK
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

#if WASDK
namespace JumpListManager.Samples.WinUI;
#elif UWP
namespace JumpListManager.Samples.Uwp;
#endif

public partial class CommandItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate CommandButtonTemplate { get; set; } = null!;

	public DataTemplate CommandSeparatorTemplate { get; set; } = null!;

	protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
	{
		return item switch
		{
			CommandButtonItem => CommandButtonTemplate,
			CommandSeparatorItem => CommandSeparatorTemplate,
			_ => throw new ArgumentException($@"Type of ""{nameof(item)}"" is not a type expected."),
		};
	}
}
