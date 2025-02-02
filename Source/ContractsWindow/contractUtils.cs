﻿#region license
/*The MIT License (MIT)
Contract Utilities - Public utilities for accessing and altering internal information

Copyright (c) 2014 DMagic

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
using System.Linq;
using Contracts;
using UnityEngine;
using ContractParser;

namespace ContractsWindow
{
	/// <summary>
	/// A static helper class intended primarily for use by external assemblies through reflection
	/// </summary>
	public static class ContractUtils
	{
        private static readonly char[] separator = { ','};

        /// <summary>
        /// A method for manually resetting a locally cached contract title.
        /// </summary>
        /// <param name="contract">Instance of the contract in question</param>
        /// <param name="name">The new contract title</param>
        public static void setContractTitle (Contract contract, string name)
		{
			try
			{
				if (string.IsNullOrEmpty(name))
					return;
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					c.Title = name;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Contract Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract notes.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <param name="notes">The new contract notes</param>
		public static void setContractNotes (Contract contract, string notes)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					c.Notes = notes;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign new Contract Notes: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting a locally cached contract parameter title.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="name">The new contract parameter title</param>
		public static void setParameterTitle (Contract contract, ContractParameter parameter, string name)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				parameterContainer pC = c?.AllParamList.SingleOrDefault(a => a.CParam == parameter);
				if (pC != null)
				{
					pC.Title = name;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract parameter notes.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="notes">The new contract parameter notes</param>
		public static void setParameterNotes(Contract contract, ContractParameter parameter, string notes)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				parameterContainer pC = c?.AllParamList.SingleOrDefault(a => a.CParam == parameter);
				pC?.setNotes(notes);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Notes: " + e);
			}
		}

		/// <summary>
		/// A method for returning a contractContainer object. The contract in question must be loaded by Contracts
		/// Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <returns>contractContainer object</returns>
		public static contractContainer getContractContainer(Contract contract)
		{
			try
			{
				return contractParser.getActiveContract(contract.ContractGuid);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Contract Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for returning a parameterContainer object. The contract and parameter in question must be loaded by 
		/// Contracts Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <returns>parameterContainer object</returns>
		public static parameterContainer getParameterContainer(Contract contract, ContractParameter parameter)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				return c?.AllParamList.SingleOrDefault(a => a.CParam == parameter);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Parameter Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for updating all reward values for active contracts
		/// </summary>
		/// <param name="contractType">Type of contract that needs to be updated; must be a subclass of Contracts.Contract</param>
		public static void UpdateContractType(Type contractType)
		{
			if (contractType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update contract method is null");
				return;
			}
			if (ContractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				ContractScenario.Instance.contractChanged(contractType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating contract values: " + e);
			}
		}

		/// <summary>
		/// A method for updating contract parameter reward values for active contracts
		/// </summary>
		/// <param name="parameterType">Type of parameter that needs to be updated; must be a subclass of Contracts.ContractParameter</param>
		public static void UpdateParameterType(Type parameterType)
		{
			if (parameterType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update parameter method is null");
				return;
			}
			if (ContractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				ContractScenario.Instance.paramChanged(parameterType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating parameter values: " + e);
			}
		}

        internal static int currentScene(GameScenes s)
        {
            switch (s)
            {
                case GameScenes.FLIGHT:
                    return 0;
                case GameScenes.EDITOR:
                    return 1;
                case GameScenes.SPACECENTER:
                    return 2;
                case GameScenes.TRACKSTATION:
                    return 3;
                default:
                    return 0;
            }
        }

        //Convert array types into strings for storage
        public static string stringConcat<T>(T[] source)
		{
			string result = "";
			if ((source?.Length ?? 0) > 0)
            {
                foreach (T item in source)
                {
					result += item.ToString()+",";
                }
            }
			return result;
        }

        //Convert strings into the appropriate arrays
        public static int[] stringSplit(string source)
        {
            string[] s = source.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return s.Select(x => int.Parse(x)).ToArray();
        }

        public static bool[] stringSplitBool(string source)
		{
			string[] s = source.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			return s.Select(x => bool.Parse(x)).ToArray();
        }

        public static void LogFormatted(String Message, params object[] strParams)
        {
            Message = String.Format(Message, strParams);
            String strMessageLine = String.Format("[Contracts_Window_+] {0}, {1}", DateTime.Now, Message);
            Debug.Log(strMessageLine);
        }
    }
}
