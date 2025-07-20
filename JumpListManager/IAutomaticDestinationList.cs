// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com
{
	/// <summary>
	/// Defines unmanaged raw vtable for the <see cref="IAutomaticDestinationList"/> interface.
	/// </summary>
	public unsafe partial struct IAutomaticDestinationList : IComIID
	{
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
		private void** lpVtbl;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

		/// <summary>
		/// Initializes this instance of <see cref="IAutomaticDestinationList"/> with the specified Application User Model ID (AMUID).
		/// </summary>
		/// <param name="szAppId">The Application User Model ID to initialize this instance of <see cref="IAutomaticDestinationList"/> with.</param>
		/// <param name="a2">Unknown argument. Apparently this can be NULL.</param>
		/// <param name="a3">Unknown argument. Apparently this can be NULL.</param>
		/// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT Initialize(PCWSTR szAppId, PCWSTR a2, PCWSTR a3)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, PCWSTR, PCWSTR, PCWSTR, int>)lpVtbl[3])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), szAppId, a2, a3);

		/// <summary>
		/// Gets a value that determines whether this <see cref="IAutomaticDestinationList"/> has any list.
		/// </summary>
		/// <param name="pfHasList">A pointer to a <see cref="BOOL"/> that receives the result. <see cref="BOOL.TRUE"/> if there's any list; otherwise, <see cref="BOOL.FALSE"/>.</param>
		/// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT HasList(BOOL* pfHasList)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, BOOL*, int>)lpVtbl[4])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pfHasList);

		/// <summary>
		/// Gets the list of automatic destinations of the specified type.
		/// </summary>
		/// <param name="type">The type to get the automatic destinations of.</param>
		/// <param name="maxCount">The max count to get the automatic destinations up to.</param>
		/// <param name="flags">The flags to filter up the queried destinations.</param>
		/// <param name="riid">A reference to the interface identifier (IID) of the interface being queried for.</param>
		/// <param name="ppvObject">The address of a pointer to an interface with the IID specified in the riid parameter.</param>
		/// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetList(DESTLISTTYPE type, int maxCount, GETDESTLISTFLAGS flags, Guid* riid, void** ppvObject)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, DESTLISTTYPE, int, GETDESTLISTFLAGS, Guid*, void**, int>)lpVtbl[5])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), type, maxCount, flags, riid, ppvObject);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT AddUsagePoint(IUnknown* pObject)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, int>)lpVtbl[6])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pObject);

		/// <summary>
		/// Pins an item to the list.
		/// </summary>
		/// <param name="pObject">The native object to pin to the list.</param>
		/// <param name="pinIndex">-1 to pin to the last, -2 to unpin, zero or positive numbers (>= 0) indicate the index to pin to the list at. Other numbers are not handled *at all*.</param>
		/// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT PinItem(IUnknown* pObject, int pinIndex)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, int, int>)lpVtbl[7])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pObject, pinIndex);

		/// <summary>
		/// Gets the index of a pinned item in the Pinned list.
		/// </summary>
		/// <param name="pObj">The native object to get its index in the list.</param>
		/// <param name="pIndex">A pointer that points to an int value that takes the index of the item passed.</param>
		/// <returns>Returns <see cref="HRESULT.S_OK"/> if successful, or an error value otherwise.
		/// If the passed item doesn't belong to the Pinned list, HRESULT.E_NOT_SET is returned.</returns>
		// NOTE:
		//  According to the debug symbols, this method is called "IsPinned" and other definitions out there also define so
		//  but it is inappropriate based on the fact it actually calls an internal method that gets the index of a pinned item
		//  and returns it in the second argument. If you want to check if an item is pinned, you should use IShellItem::Compare for IShellItem,
		//  or compare IShellLinkW::GetPath, IShellLinkW::GetArguments and PKEY_Title for IShellLinkW, which is actually done, at least, in Windows 7 era.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetPinIndex(IUnknown* pObj, int* pIndex)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, int*, int>)lpVtbl[8])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pObj, pIndex);

		[GuidRVAGen.Guid("E9C5EF8D-FD41-4F72-BA87-EB03BAD5817C")]
		public static partial ref readonly Guid Guid { get; }
	}

	public enum DESTLISTTYPE : uint
	{
		PINNED,
		RECENT,
		FREQUENT,
	}

	public enum GETDESTLISTFLAGS : uint
	{
		NONE,
		EXCLUDE_UNNAMED_DESTINATIONS,
	}
}
