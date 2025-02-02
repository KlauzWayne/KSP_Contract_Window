﻿#region license
/*The MIT License (MIT)
CW_StandardNode - Controls progress node UI elements

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
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_StandardNode : MonoBehaviour
	{
		[SerializeField]
		private TextHandler Title = null;
		[SerializeField]
		private TextHandler Reward = null;
		[SerializeField]
		private Toggle NoteToggle = null;
		[SerializeField]
		private GameObject NoteContainer = null;
		[SerializeField]
		private TextHandler NoteText = null;
		[SerializeField]
		private TooltipHandler NoteTooltip = null;
		[SerializeField]
		private LayoutElement NodeLayout = null;

		private IStandardNode standardInterface;

		public bool IsComplete
		{
			get
            {
                if (standardInterface == null)
                    return false;

                return standardInterface.IsComplete;
            }
		}

		public void setNode(IStandardNode node)
		{
			if (node == null)
				return;

			standardInterface = node;

			if (Title != null)
				Title.OnTextUpdate.Invoke(node.NodeText);
			
			if (Reward != null)
				Reward.OnTextUpdate.Invoke(node.RewardText);

			setNote();
		}

		public void UpdateText()
		{
			if (standardInterface == null)
				return;

			if (Title != null)
				Title.OnTextUpdate.Invoke(standardInterface.NodeText);

			if (Reward != null)
				Reward.OnTextUpdate.Invoke(standardInterface.RewardText);
		}

		private void setNote()
		{
			if (standardInterface == null)
				return;

			if (NoteContainer == null || NoteText == null)
				return;

			if (string.IsNullOrEmpty(standardInterface.GetNote))
				return;

			if (NoteToggle != null)
				NoteToggle.gameObject.SetActive(true);

			NoteText.OnTextUpdate.Invoke(standardInterface.GetNote);

			NoteContainer.gameObject.SetActive(false);

			if (NodeLayout == null)
				return;

			NodeLayout.minWidth -= 12;
			NodeLayout.preferredWidth -= 12;
		}

		public void NoteOn(bool isOn)
		{
			if (standardInterface == null)
				return;

			if (NoteContainer == null || NoteText == null)
				return;

			if (isOn)
				NoteText.OnTextUpdate.Invoke(standardInterface.GetNote);

			NoteContainer.gameObject.SetActive(isOn);

			if (NoteTooltip != null)
				NoteTooltip.TooltipIndex = isOn ? 1 : 0;
		}
	}
}
