// Copyright (c) Files Community
// Licensed under the MIT License.

using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;

[CustomMarshaller(typeof(DateTime), MarshalMode.ManagedToUnmanagedOut, typeof(FILETIMEDateTimeMarshaller.NativeToManaged))]
[CustomMarshaller(typeof(DateTime), MarshalMode.UnmanagedToManagedIn, typeof(FILETIMEDateTimeMarshaller.NativeToManaged))]
[CustomMarshaller(typeof(DateTime), MarshalMode.ManagedToUnmanagedIn, typeof(FILETIMEDateTimeMarshaller.ManagedToNative))]
[CustomMarshaller(typeof(DateTime), MarshalMode.UnmanagedToManagedOut, typeof(FILETIMEDateTimeMarshaller.ManagedToNative))]
internal static class FILETIMEDateTimeMarshaller
{
    public static class NativeToManaged
    {
        public static DateTime ConvertToManaged(FILETIME unmanaged)
        {
            long ft = ((long)unmanaged.dwHighDateTime << 32) | (uint)unmanaged.dwLowDateTime;
            return DateTime.FromFileTimeUtc(ft);
        }
    }

    public static class ManagedToNative
    {
        public static FILETIME ConvertToUnmanaged(DateTime managed)
        {
            long ft = managed.ToFileTimeUtc();
            return new FILETIME
            {
                dwLowDateTime = unchecked((int)ft),
                dwHighDateTime = unchecked((int)(ft >> 32))
            };
        }
    }
}
