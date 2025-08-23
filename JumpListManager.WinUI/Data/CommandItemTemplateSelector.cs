﻿// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace JumpListManager.Data
{
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
}
