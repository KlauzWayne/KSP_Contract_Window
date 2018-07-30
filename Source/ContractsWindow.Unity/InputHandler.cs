﻿#region license
/*The MIT License (MIT)
InputHandler - Script for handling Input field object replacement with Text Mesh Pro

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

using UnityEngine;
using UnityEngine.Events;

namespace ContractsWindow.Unity
{
	public class InputHandler : MonoBehaviour
	{
		private string _text;
		private bool _isFocused;

		public class OnTextEvent : UnityEvent<string> { }
		public class OnValueChanged : UnityEvent<string> { }

		private OnTextEvent _onTextUpdate = new OnTextEvent();
		private OnValueChanged _onValueChanged = new OnValueChanged();

		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}

		public bool IsFocused
		{
			get { return _isFocused; }
			set { _isFocused = value; }
		}

		public UnityEvent<string> OnTextUpdate
		{
			get { return _onTextUpdate; }
		}

		public UnityEvent<string> OnValueChange
		{
			get { return _onValueChanged; }
		}

	}
}
