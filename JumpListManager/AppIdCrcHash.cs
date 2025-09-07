// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;

namespace JumpListManager
{
	public class AppIdCrcHash : HashAlgorithm
	{
		public const ulong CrcPolynomial = 0x92C64265D32139A4;

		private static readonly ulong[] _lookupTable = InitializeTable();

		private ulong _hash = 0xFFFFFFFFFFFFFFFF;

		public override int HashSize
			=> 64;

		public AppIdCrcHash() { }

		/// <inheritdoc/>
		public override void Initialize()
		{
			_hash = 0xFFFFFFFFFFFFFFFF;
		}

		/// <inheritdoc/>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			for (int i = ibStart; i < ibStart + cbSize; i++)
				unchecked { _hash = _hash >> 8 ^ _lookupTable[(_hash ^ array[i]) & 0xFF]; }
		}

		protected override byte[] HashFinal()
		{
			HashValue = BitConverter.GetBytes(_hash);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(HashValue);

			return HashValue;
		}

		private static ulong[] InitializeTable()
		{
			var table = new ulong[256];
			for (uint i = 0U; i < 256; i++)
			{
				ulong entry = (ulong)i;

				for (uint j = 0U; j < 8U; j++)
					entry = (entry & 1) is 1 ? entry >> 1 ^ CrcPolynomial : entry >> 1;

				table[i] = entry;
			}

			return table;
		}
	}
}
