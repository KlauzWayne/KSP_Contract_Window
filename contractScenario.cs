﻿#region license
/*The MIT License (MIT)
Contract Scenario - Scenario Module To Store Save Specific Info

Copyright (c) 2014 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

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
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;

namespace ContractsWindow
{

	#region Scenario Setup

	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.SPH)]
	class contractScenario : ScenarioModule
	{

		internal static contractScenario Instance
		{
			get
			{
				Game g = HighLogic.CurrentGame;
				try
				{
					var mod = g.scenarios.FirstOrDefault(m => m.moduleName == typeof(contractScenario).Name);
					if (mod != null)
						return (contractScenario)mod.moduleRef;
					else
						return null;
				}
				catch(Exception e)
				{
					print(string.Format("[Contracts+] Could not find Contracts Window Scenario Module: {0}", e));
					return null;
				}
			}
			private set { }
		}

		//Use this to reset settings on updates
		[KSPField(isPersistant = true)]
		public string version = "2.0.2.0";

		//Master contract storage
		private Dictionary<Guid, contractContainer> masterList = new Dictionary<Guid, contractContainer>();

		//Contract lists; for saving/loading and ordering
		internal List<Guid> showList = new List<Guid>();
		internal List<Guid> hiddenList = new List<Guid>();
		internal string showString;
		internal string hiddenString;

		//initialize data for each gamescene
		internal int[] orderMode = new int[4];
		internal int[] windowMode = new int[4];
		internal int[] showHideMode = new int[4];
		internal bool[] windowVisible = new bool[4];
		internal sortClass[] sortMode = new sortClass[4] { sortClass.Difficulty, sortClass.Difficulty, sortClass.Difficulty, sortClass.Difficulty };
		internal Rect[] windowRects = new Rect[4] { new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300) };
		private int[] windowPos = new int[16] { 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300 };

		//Global settings
		internal bool toolTips = true;
		internal bool fontSmall = true;
		internal int windowSize = 0;

		//Contract Config storage
		private static Dictionary<string, contractTypeContainer> cTypeList;
		private static Dictionary<string, paramTypeContainer> pTypeList;

		internal contractsWindow cWin;
		internal contractConfig cConfig;

		//Convert all of our saved strings into the appropriate arrays for each game scene
		public override void OnLoad(ConfigNode node)
		{
			if (version == contractAssembly.Version)
			{
				ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
				if (scenes != null)
				{
					//Contract lists
					showString = scenes.GetValue("DefaultListID");
					hiddenString = scenes.GetValue("HiddenListID");

					//Global Settings
					toolTips = stringBoolParse(scenes.GetValue("ToolTips"));
					fontSmall = stringBoolParse(scenes.GetValue("FontSize"));
					windowSize = stringintParse(scenes.GetValue("WindowSize"));

					//Scene settings
					showHideMode = stringSplit(scenes.GetValue("ShowListMode"));
					windowMode = stringSplit(scenes.GetValue("WindowMode"));
					orderMode = stringSplit(scenes.GetValue("SortOrder"));
					int[] sort = stringSplit(scenes.GetValue("SortMode"));
					sortMode = new sortClass[4] { (sortClass)sort[0], (sortClass)sort[1], (sortClass)sort[2], (sortClass)sort[3] };
					windowPos = stringSplit(scenes.GetValue("WindowPosition"));
					windowVisible = stringSplitBool(scenes.GetValue("WindowVisible"));
					int[] winPos = new int[4] { windowPos[4 * currentScene(HighLogic.LoadedScene)], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 1], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 2], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 3] };
					loadWindow(winPos);
				}
			}
			version = contractAssembly.Version;

			//Start the window object
			cWin = gameObject.AddComponent<contractsWindow>();
			cConfig = gameObject.AddComponent<contractConfig>();

			//Load the contract and parameter types
			if (cTypeList == null)
			{
				cTypeList = new Dictionary<string, contractTypeContainer>();
				foreach(AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
				{
					Type[] assemblyTypes = assembly.assembly.GetTypes();
					foreach (Type t in assemblyTypes)
					{
						if (t.IsSubclassOf(typeof(Contract)))
						{
							if (t != typeof(Contract))
							{
								if (!cTypeList.ContainsKey(t.Name))
									cTypeList.Add(t.Name, new contractTypeContainer(t));
								if (cTypeList[t.Name].ContractC == null)
									cTypeList.Remove(t.Name);
							}
						}
					}
				}
			}

			ConfigNode contractTypes = node.GetNode("Contracts_Window_Contract_Types");

			if (contractTypes != null)
			{
				foreach (ConfigNode contractType in contractTypes.GetNodes("Contract_Type"))
				{
					if (contractType != null)
					{
						string contractTypeName = contractType.GetValue("TypeName");
						string valuesString = contractType.GetValue("ContractValues");
						stringContractParse(valuesString, contractTypeName);
					}
				}
			}

			if (pTypeList == null)
			{
				pTypeList = new Dictionary<string, paramTypeContainer>();
				foreach(AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
				{
					Type[] assemblyTypes = assembly.assembly.GetTypes();
					foreach(Type t in assemblyTypes)
					{
						if (t.IsSubclassOf(typeof(ContractParameter)))
						{
							if (t.Name == "OR" || t.Name == "XOR" || t.Name == "RecoverPart")
								continue;
							if (t != typeof(ContractParameter))
							{
								if (!pTypeList.ContainsKey(t.Name))
									pTypeList.Add(t.Name, new paramTypeContainer(t));
								if (pTypeList[t.Name].Param == null)
									pTypeList.Remove(t.Name);
							}
						}
					}
				}
			}

			ConfigNode paramTypes = node.GetNode("Contracts_Window_Parameter_Types");

			if (paramTypes != null)
			{
				foreach (ConfigNode paramType in paramTypes.GetNodes("Parameter_Type"))
				{
					if (paramType != null)
					{
						string paramTypeName = paramType.GetValue("TypeName");
						string valuesString = paramType.GetValue("ParameterValues");
						stringParamParse(valuesString, paramTypeName);
					}
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			saveWindow(windowRects[currentScene(HighLogic.LoadedScene)]);

			ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");
			
			//Contract lists
			scenes.AddValue("DefaultListID", stringConcat(showList));
			scenes.AddValue("HiddenListID", stringConcat(hiddenList));

			//Scene settings
			scenes.AddValue("ShowListMode", stringConcat(showHideMode, showHideMode.Length));
			scenes.AddValue("WindowMode", stringConcat(windowMode, windowMode.Length));
			scenes.AddValue("SortOrder", stringConcat(orderMode, orderMode.Length));
			int[] sort = new int[4] { (int)sortMode[0], (int)sortMode[1], (int)sortMode[2], (int)sortMode[3] };
			scenes.AddValue("SortMode", stringConcat(sort, sort.Length));
			scenes.AddValue("WindowPosition", stringConcat(windowPos, windowPos.Length));
			scenes.AddValue("WindowVisible", stringConcat(windowVisible, windowVisible.Length));

			//Global settings
			scenes.AddValue("ToolTips", toolTips);
			scenes.AddValue("FontSize", fontSmall);
			scenes.AddValue("WindowSize", windowSize);

			node.AddNode(scenes);

			//Save values for each contract type
			ConfigNode contractTypes = new ConfigNode("Contracts_Window_Contract_Types");

			foreach (contractTypeContainer c in cTypeList.Values)
			{
				ConfigNode contractType = new ConfigNode("Contract_Type");

				contractType.AddValue("TypeName", c.ContractC.GetType().Name);
				contractType.AddValue("ContractValues", stringConcat(c));

				contractTypes.AddNode(contractType);
			}

			node.AddNode(contractTypes);

			//Save values for each parameter type
			ConfigNode paramTypes = new ConfigNode("Contracts_Window_Parameter_Types");

			foreach (paramTypeContainer p in pTypeList.Values)
			{
				ConfigNode paramType = new ConfigNode("Parameter_Type");

				paramType.AddValue("TypeName", p.Param.GetType().Name);
				paramType.AddValue("ParameterValues", stringConcat(p));

				paramTypes.AddNode(paramType);
			}

			node.AddNode(paramTypes);
		}

		//Remove our contract window object
		private void OnDestroy()
		{
			Destroy(cWin);
			Destroy(cConfig);
		}

	#endregion

		#region utilities

		internal static int currentScene(GameScenes s)
		{
			switch (s)
			{
				case GameScenes.FLIGHT:
					return 0;
				case GameScenes.EDITOR:
				case GameScenes.SPH:
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
		private string stringConcat(int[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString();
			}
			return string.Join(",", s);
		}

		private string stringConcat(List<Guid> source)
		{
			if (source.Count == 0)
				return "";
			List<string> s = new List<string>();
			for (int j = 0; j < source.Count; j++)
			{
				contractContainer c = getContract(source[j]);
				if (c == null)
					continue;
				string i;
				if (c.listOrder == null)
					i = "N";
				else
					i = c.listOrder.ToString();
				bool show = c.showParams;
				string id = string.Format("{0}|{1}|{2}", source[j], i, show);
				s.Add(id);
			}

			return string.Join(",", s.ToArray());
		}

		private string stringConcat(bool[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString();
			}
			return string.Join(",", s);
		}

		private string stringConcat(contractTypeContainer c)
		{
			string[] s = new string[9];
			s[0] = c.RewardFund.ToString("N2");
			s[1] = c.AdvanceFund.ToString("N2");
			s[2] = c.PenaltyFund.ToString("N2");
			s[3] = c.RewardRep.ToString("N2");
			s[4] = c.PenaltyRep.ToString("N2");
			s[5] = c.RewardScience.ToString("N2");
			s[6] = c.DurationTime.ToString("N2");
			s[7] = c.MaxOffer.ToString("N0");
			s[8] = c.MaxActive.ToString("N0");
			return string.Join(",", s);
		}

		private string stringConcat(paramTypeContainer p)
		{
			string[] s = new string[5];
			s[0] = p.RewardFund.ToString("N2");
			s[1] = p.PenaltyFund.ToString("N2");
			s[2] = p.RewardRep.ToString("N2");
			s[3] = p.PenaltyRep.ToString("N2");
			s[4] = p.RewardScience.ToString("N2");
			return string.Join(",", s);
		}

		//Convert strings into the appropriate arrays
		private int[] stringSplit(string source)
		{
			string[] s = source.Split(',');
			int[] i = new int[s.Length];
			for (int j = 0; j < s.Length; j++)
			{
				i[j] = int.Parse(s[j]);
			}
			return i;
		}

		private bool[] stringSplitBool(string source)
		{
			string[] s = source.Split(',');
			bool[] b = new bool[s.Length];
			for (int j = 0; j < s.Length; j++)
			{
				b[j] = bool.Parse(s[j]);
			}
			return b;
		}

		private bool stringBoolParse(string source)
		{
			bool b;
			if (bool.TryParse(source, out b))
				return b;
			return true;
		}

		private int? stringIntParse(string s)
		{
			int i;
			if (int.TryParse(s, out i)) return i;
			return null;
		}

		private int stringintParse(string s)
		{
			int i;
			if (int.TryParse(s, out i)) return i;
			return 0;
		}

		private float stringFloatParse(string s, bool b)
		{
			float f;
			if (float.TryParse(s, out f)) return f;
			if (b)
				return 1;
			else
				return 10;
		}

		private void stringContractParse(string s, string type)
		{
			contractTypeContainer c;
			string[] a = s.Split(',');
			if (cTypeList.ContainsKey(type))
				c = cTypeList[type];
			else
			{
				DMC_MBE.LogFormatted("Contract Type Not Found; Removing Type From List");
				return;
			}
			c.RewardFund = stringFloatParse(a[0], true);
			c.AdvanceFund = stringFloatParse(a[1], true);
			c.PenaltyFund = stringFloatParse(a[2], true);
			c.RewardRep = stringFloatParse(a[3], true);
			c.PenaltyRep = stringFloatParse(a[4], true);
			c.RewardScience = stringFloatParse(a[5], true);
			c.DurationTime = stringFloatParse(a[6], true);
			c.MaxOffer = stringFloatParse(a[7], false);
			c.MaxActive = stringFloatParse(a[8], false);
		}

		private void stringParamParse(string s, string type)
		{
			paramTypeContainer p;
			string[] a = s.Split(',');
			if (pTypeList.ContainsKey(type))
				p = pTypeList[type];
			else
			{
				DMC_MBE.LogFormatted("Parameter Type Not Found; Removing Type From List");
				return;
			}
			p.RewardFund = stringFloatParse(a[0], true);
			p.PenaltyFund = stringFloatParse(a[1], true);
			p.RewardRep = stringFloatParse(a[2], true);
			p.PenaltyRep = stringFloatParse(a[3], true);
			p.RewardScience = stringFloatParse(a[4], true);
		}

		#endregion

		#region internal methods

		//Populate the contract lists based on contract Guid values
		internal contractContainer getContract(Guid id)
		{
			if (masterList.ContainsKey(id))
				return masterList[id];
			else
				return null;
		}

		internal void addContract(Guid id, contractContainer c)
		{
			if (!masterList.ContainsKey(id))
				masterList.Add(id, c);
			else
				DMC_MBE.LogFormatted_DebugOnly("Contract Already Present In List");
		}

		internal void resetList()
		{
			masterList.Clear();
		}

		internal void loadContractLists(string s, int l)
		{
			if (string.IsNullOrEmpty(s))
			{
				if (l == 0)
					showList = new List<Guid>();
				else
					hiddenList = new List<Guid>();
			}
			else
			{
				string[] sA = s.Split(',');
				List<Guid> gID = new List<Guid>();
				for (int i = 0; i < sA.Length; i++)
				{
					contractContainer c = null;
					string[] sB = sA[i].Split('|');
					try
					{
						Guid g = new Guid(sB[0]);
						c = getContract(g);
						if (c != null)
							gID.Add(g);
						else
							continue;
					}
					catch (Exception e)
					{
						DMC_MBW.LogFormatted("Guid invalid: {0}", e);
						continue;
					}

					c.listOrder = stringIntParse(sB[1]);
					c.showParams = stringBoolParse(sB[2]);
				}
				if (l == 0)
					showList = gID;
				else
					hiddenList = gID;
			}
		}

		internal List<Guid> loadPinnedContracts(List<Guid> gID)
		{
			List<contractContainer> temp = new List<contractContainer>();
			List<Guid> idTemp = new List<Guid>();
			foreach (Guid id in gID)
			{
				contractContainer c = getContract(id);
				if (c != null)
				{
					if (c.listOrder != null)
						temp.Add(c);
				}
			}
			if (temp.Count > 0)
			{
				temp.Sort((a, b) =>
					{
						return Comparer<int?>.Default.Compare(a.listOrder, b.listOrder);
					});
				foreach (contractContainer c in temp)
				{
					idTemp.Add(c.contract.ContractGuid);
				}
			}
			return idTemp;
		}

		internal List<contractTypeContainer> setContractTypes(List<contractTypeContainer> cC)
		{
			cC = new List<contractTypeContainer>();
			foreach (contractTypeContainer c in cTypeList.Values)
				cC.Add(c);
			if (cC.Count > 0)
				cC.Sort((a,b) => string.Compare(a.Name, b.Name));
			return cC;
		}

		internal List<paramTypeContainer> setParamTypes(List<paramTypeContainer> pC)
		{
			pC = new List<paramTypeContainer>();
			foreach (paramTypeContainer p in pTypeList.Values)
				pC.Add(p);
			if (pC.Count > 0)
				pC.Sort((a, b) => string.Compare(a.Name, b.Name));
			return pC;
		}

		internal static string paramTypeCheck(ContractParameter param)
		{
			if (param.GetType() == typeof(PartTest))
				return "partTest";

			if (contractAssembly.FPLoaded)
			{
				if (param.GetType() == contractAssembly._FPType)
					return "FinePrint";
			}

			if (contractAssembly.MCELoaded)
			{
				if (param.GetType() == contractAssembly._MCEType)
					return "MCEScience";
			}

			if (contractAssembly.DMLoaded)
			{
				if (param.GetType() == contractAssembly._DMCType)
					return "DMcollectScience";
			}

			if (contractAssembly.DMALoaded)
			{
				if (param.GetType() == contractAssembly._DMAType)
					return "DManomalyScience";
				else
					return "";
			}

			if (contractAssembly.DMAstLoaded)
			{
				if (param.GetType() == contractAssembly._DMAstType)
					return "DMasteroidScience";
				else
					return "";
			}

			return "";
		}

		internal static string timeInDays(double D)
		{
			if (D <= 0)
				return "----";

			int[] time = KSPUtil.GetDateFromUT((int)D);
			string s = "";

			if (time[4] > 0)
				s = string.Format("{0}y", time[4]);
			if (time[3] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}d", time[3]);
			}
			if (time[4] <= 0 && time[2] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}h", time[2]);
			}
			if (time[4] <= 0 && time[3] <= 0 && time[2] <= 0 && time[1] > 0)
				s = string.Format("{0}m", time[1]);

			return s;
		}

		#endregion

		#region save/load methods

		//Save and load the window rectangle position
		private void saveWindow(Rect source)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowPos[i * 4] = (int)source.x;
			windowPos[(i * 4) + 1] = (int)source.y;
			if (windowMode[i] == 0)
				windowPos[(i * 4) + 2] = (int)source.width - (windowSize * 30);
			else
				windowPos[(i * 4) + 2] = (int)source.width - (windowSize * 60);
			windowPos[(i * 4) + 3] = (int)source.height;
		}

		private void loadWindow(int[] window)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowRects[i] = new Rect(window[0], window[1], window[2], window[3]);
			if (windowMode[i] == 0)
				windowRects[i].width += (windowSize * 30);
			else
				windowRects[i].width += (windowSize * 60);
		}

		#endregion

	}
}
