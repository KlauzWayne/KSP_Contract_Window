﻿#region license
/*The MIT License (MIT)
Contract Stock Toolbar- Addon for stock app launcher interface

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
using System.Collections;
using KSP.UI.Screens;
using ContractsWindow.PanelInterfaces;
using UnityEngine;

namespace ContractsWindow.Toolbar
{
	public class ContractStockToolbar : MonoBehaviour
	{
		private ApplicationLauncherButton toolbarButton = null;
		private ApplicationLauncherButton stockAppButton = null;
		private static ContractStockToolbar instance;
		
		public static ContractStockToolbar Instance
		{
			get { return instance; }
		}

		public ApplicationLauncherButton Button
		{
			get
			{
				if (ContractLoader.Settings != null && ContractLoader.Settings.replaceStockApp)
					return stockAppButton;

				return toolbarButton;
			}
		}

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			setupToolbar();
		}

		private void setupToolbar()
		{
			if (ContractLoader.Settings != null && ContractLoader.Settings.replaceStockApp)
				StartCoroutine(replaceStockContractApp());
			else
				StartCoroutine(addButton());
		}

		private void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);

			removeButton(HighLogic.LoadedScene);
		}
		
		private IEnumerator addButton()
		{
			while (!ApplicationLauncher.Ready)
				yield return null;

			toolbarButton = ApplicationLauncher.Instance.AddModApplication(open, close, null, null, null, null, (ApplicationLauncher.AppScenes)63, ContractLoader.ToolbarIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			if (toolbarButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
				toolbarButton = null;
			}
		}

		internal void replaceStockApp()
		{
			StartCoroutine(replaceStockContractApp());
		}

		private IEnumerator replaceStockContractApp()
		{
			while (!ApplicationLauncher.Ready || ContractsApp.Instance == null || ContractsApp.Instance.appLauncherButton == null)
				yield return null;

			if (toolbarButton == null)
			{
				ContractUtils.LogFormatted("Contracts Window + App Launcher Button Not Initialized; Starting It Now");
				toolbarButton = ApplicationLauncher.Instance.AddModApplication(open, close, null, null, null, null, (ApplicationLauncher.AppScenes)63, ContractLoader.ToolbarIcon);
			}

			stockAppButton = ContractsApp.Instance.appLauncherButton;

			if (stockAppButton != null)
			{
				stockAppButton.onDisable();

				stockAppButton.onTrue = toolbarButton.onTrue;
				stockAppButton.onFalse = toolbarButton.onFalse;
				stockAppButton.onHover = toolbarButton.onHover;
				stockAppButton.onHoverOut = toolbarButton.onHoverOut;
				stockAppButton.onEnable = toolbarButton.onEnable;
				stockAppButton.onDisable = toolbarButton.onDisable;

				ApplicationLauncher.Instance.DisableMutuallyExclusive(stockAppButton);

				ContractUtils.LogFormatted("Stock Contracts App Replaced With Contracts Window +");

				try
				{
					removeButton(HighLogic.LoadedScene);
				}
				catch (Exception e)
				{
					ContractUtils.LogFormatted("Error In Removing Contracts Window + Toolbar App After Replacing Stock App: {0}", e);
				}
			}
			else
			{
				ContractUtils.LogFormatted("Something went wrong while replacing the stock contract; attempting to add standard toolbar button");

				if (toolbarButton != null)
					GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
				else
					StartCoroutine(addButton());
			}
		}

		private void open()
		{
			if (ContractWindow.Instance == null || ContractScenario.Instance == null)
				return;

			int sceneInt = ContractUtils.currentScene(HighLogic.LoadedScene);

			ContractWindow.Instance.Open();
			ContractScenario.Instance.windowVisible[sceneInt] = true;
		}

		private void close()
		{
			if (ContractWindow.Instance == null || ContractScenario.Instance == null)
				return;

			int sceneInt = ContractUtils.currentScene(HighLogic.LoadedScene);

			ContractWindow.Instance.Close();
			ContractScenario.Instance.windowVisible[sceneInt] = false;
		}

	}
}
