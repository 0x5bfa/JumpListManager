// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text;

namespace JumpListManager.ViewModels
{
	internal class CrcCalculatorViewModel : ObservableObject
	{
		public string? CrcHash { get => field; set => SetProperty(ref field, value); }

		public void CalculateCrcHash(string input)
		{
			var hash = new AppIdCrcHash();

			CrcHash = BitConverter.ToUInt64(hash.ComputeHash(Encoding.Unicode.GetBytes(input.ToUpper()))).ToString("X16");
		}
	}
}
