// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System;

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

internal sealed partial class BooleanToHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return System.Convert.ToBoolean(value) ? 64D : 40D;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
