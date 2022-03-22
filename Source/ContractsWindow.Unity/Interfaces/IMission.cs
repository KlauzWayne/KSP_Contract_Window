﻿#region license
/*The MIT License (MIT)
IMissionSection - Interface for transferring information on contract missions

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public enum SortMode {
		planet,
		expiration,
		acceptance,
		reward
	}
	public interface IMission
	{
		string MissionTitle { get; set; }

		string ContractNumber { get; }

		bool IsVisible { get; set; }

		bool ContainsContract(Guid contract);

		bool OrderIsDescending { get; set; }

		bool ShowHidden { get; set; }

		IList<IContractSection> GetContracts { get; }

		void SetSort(int i);

		void AddContract(IContractSection contract);

		void RemoveContract(IContractSection contract);
	}
}
