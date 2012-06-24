/*
 *  NumberModifyPopover.xib.cs
 *
 *  Copyright (C) 2010-2012 Kyle Olson, kyle@kyleolson.com
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */


using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CombatManagerMono
{
	public class NumberModifyEventArgs : EventArgs
	{
		public NumberModifyEventArgs()
		{
		}
		
		
		public int? Value{get; set;}
		public bool Set{get; set;}
		
		public int? ModifyValue(int ? startValue)
		{
			return ModifyValue(startValue, int.MinValue, int.MaxValue);	
		}
		
		public int? ModifyValue(int ? startValue, int min, int max)
		{
			if (Set || Value == null)
			{
				return Value;	
			}
			else
			{
				if (startValue == null)
				{
					return null;
				}
				else
				{
					int start = startValue.Value;
					int mod = Value.Value;
					
					int newVal = start  + mod;
					
					if (newVal < min)
					{
						newVal = min;
					}
					if (newVal > max)
					{
						newVal = max;
					}
					return newVal;
				}
			}
		}
	}
	
	public delegate void NumberModifyEvent (object sender, NumberModifyEventArgs args);
	
	public partial class NumberModifyPopover : DialogPopoverViewController
	{
		private int? _Value;
		private string _ValueType;
		private string _Format;
		private bool _Nullable;
		
		private int _NumPadValue;
		
		
		private static Dictionary<string, int?> _LastValues = new Dictionary<string, int?>();
		private static bool _NumPadMode = false;
		
		public event NumberModifyEvent NumberModified;
		
		
		public NumberModifyPopover (IntPtr handle) : base(handle)
		{
			Initialize ();
			
			
		}

		[Export("initWithCoder:")]
		public NumberModifyPopover (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public NumberModifyPopover () : base("NumberModifyPopover", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
			pickerView.Delegate = new PickerDelegate(this);
			pickerView.DataSource = new PickerDataSource(this);
			
			StyleButton(addButton);
			StyleButton(subtractButton);
			StyleButton(setButton);
			StyleButton(NullButton);
			StylePadButton(NumberButton0);
			StylePadButton(NumberButton1);
			StylePadButton(NumberButton2);
			StylePadButton(NumberButton3);
			StylePadButton(NumberButton4);
			StylePadButton(NumberButton5);
			StylePadButton(NumberButton6);
			StylePadButton(NumberButton7);
			StylePadButton(NumberButton8);
			StylePadButton(NumberButton9);
			StylePadButton(ClearButton);
			StyleButton(SwapButton);
			
			SwapButton.SetImage(UIExtensions.GetSmallIcon("sort"), UIControlState.Normal);
			NullButton.Hidden = !this.Nullable;
			
			UpdatePadView();
			
		}
		
		void UpdatePadView()
		{
			NumPadView.Hidden = !_NumPadMode;
			pickerView.Hidden = _NumPadMode;
			UpdateNumPadLabel();
		}
		
		void UpdateNumPadLabel()
		{
			NumPadLabel.Text = _NumPadValue.ToString();	
		}
		
		void StyleButton(GradientButton button)
		{
			button.CornerRadius = 0;
			button.SetTitleColor(UIColor.White, UIControlState.Normal);
		}
		
		void StylePadButton(GradientButton button)
		{
			
			button.CornerRadius = 0;
			button.SetTitleColor(UIColor.White, UIControlState.Normal);
						
			button.Gradient = new GradientHelper(CMUIColors.SecondaryColorAMedium, CMUIColors.SecondaryColorADarker);

		}
		
		public int? Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				UpdateValueLabel();
			}
		}
		
		public string ValueFormat
		{
			get
			{
				return _Format;
			}
			set
			{
				if (_Format != value)
				{
					_Format = value;
					UpdateValueLabel();
				}
			}
		}
		
		private void UpdateValueLabel()
		{
			if (_Value == null)
			{
				numberLabel.Text = "-";	
			}
			else if (_Format == null)
			{
				numberLabel.Text = _Value.ToString();
			}
			else
			{
				numberLabel.Text = String.Format(_Format, new object[] {_Value});
			}
		}
		
	    partial void AddClicked (MonoTouch.Foundation.NSObject sender)
		{
			if (NumberModified != null && _Value != null)
			{
				NumberModified(this, new NumberModifyEventArgs() {Value = SelectedValue});
			}
		}
		
		partial void SubtractClicked (MonoTouch.Foundation.NSObject sender)
		{
			if (NumberModified != null && _Value != null)
			{
				NumberModified(this, new NumberModifyEventArgs() {Value = -SelectedValue});
			}
			
		}
		
		partial void SetClicked(MonoTouch.Foundation.NSObject sender)
		{
			if (NumberModified != null)
			{
				NumberModified(this, new NumberModifyEventArgs() {Value = SelectedValue, Set = true});
			}
				
		}
		
		partial void NullButtonTouchUpInside (MonoTouch.Foundation.NSObject sender)
		{
			if (_Nullable)
			{
				if (NumberModified != null)
				{
					NumberModified(this, new NumberModifyEventArgs() {Value = null, Set = true});
				}
			}
		}
		
		partial void NumberTouchUpInside (MonoTouch.Foundation.NSObject sender)
		{
			if (_NumPadValue < 1000)
			{
				_NumPadValue *= 10;
			}
			else
			{
				_NumPadValue = (_NumPadValue%1000)*10;
			}
			_NumPadValue += ((GradientButton)sender).Tag;
			UpdateNumPadLabel();
		}
		
		partial void ClearButtonTouchUpInside (MonoTouch.Foundation.NSObject sender)
		{
			_NumPadValue = 0;
			UpdateNumPadLabel();
		}
		
		partial void SwapButtonTouchUpInside (MonoTouch.Foundation.NSObject sender)
		{
			_NumPadMode = !_NumPadMode;
			UpdatePadView();
		}
		
		public int? SelectedValue
		{
			get
			{
				if (_NumPadMode)
				{
					return _NumPadValue;
				}
				else
				{
					return PickerValue;
				}
			}
			set
			{	
				PickerValue = value;
				if (value != null)
				{
					_NumPadValue = value.Value;	
				}
				
			}
		}
		
		
		public int? PickerValue
		{
			get
			{
				int val = 0;
				for (int i=0; i<4; i++)
				{
					int mod = (int)Math.Pow(10, 3-i);
					
					val+= pickerView.SelectedRowInComponent(i) * mod;
					
				}
				
				return val;
			}
			set
			{
				if (value != null)
				{
					for (int i=0; i<4; i++)
					{
						int mod = (int)Math.Pow(10, 3-i);
						
						int digit = (value.Value/mod)%10;
						
						pickerView.Select(digit, i, false);
					}
				}
			}			
		}
		
		public string ValueType
		{
			get
			{
				return _ValueType;
			}
			set
			{
				if (_ValueType != value)
				{
					_ValueType = value;
					if (_ValueType != null)
					{
						if (_LastValues.ContainsKey(_ValueType))
						{
							SelectedValue = _LastValues[_ValueType];	
						}
					}
				}
			}
				
		}
		
		public bool Nullable
		{
			get
			{
				return _Nullable;
			}
			set
			{
				if (_Nullable != value)
				{
					_Nullable = value;
					if (NullButton != null)
					{
						NullButton.Hidden = !_Nullable;	
					}
				}
			}
		}
		
		
		public class PickerDelegate : UIPickerViewDelegate
		{
			public NumberModifyPopover _Parent;
			public PickerDelegate(NumberModifyPopover parent)	
			{
				_Parent = parent;	
			}
			
			public override string GetTitle (UIPickerView pickerView, int row, int component)
			{
				return row.ToString();
			}
			
			public override void Selected (UIPickerView pickerView, int row, int component)
			{
				if (_Parent.ValueType != null)
				{
					NumberModifyPopover._LastValues[_Parent._ValueType] = _Parent.SelectedValue;	
				}
			}
			
		}
		
		public class PickerDataSource : UIPickerViewDataSource
		{
			public NumberModifyPopover _Parent;
			public PickerDataSource(NumberModifyPopover parent)	
			{
				_Parent = parent;	
			}
			
			public override int GetComponentCount (UIPickerView pickerView)
			{
				return 4;
			}
			
			public override int GetRowsInComponent (UIPickerView pickerView, int component)
			{
				return 10;
			}
		}
		
		
		
	}
}

