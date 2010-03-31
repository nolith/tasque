// Author:
//       Mario Carrion <mario@carrion.mx>
// 
// Copyright (c) 2010 Mario Carrion
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;

namespace Tasque {

	internal static class Extensions {

		#region DayOfWeek Extensions

		internal static uint ToUint (this DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek) {
			case DayOfWeek.Sunday:
				return 0;
			case DayOfWeek.Monday:
				return 1;
			case DayOfWeek.Tuesday:
				return 2;
			case DayOfWeek.Wednesday:
				return 3;
			case DayOfWeek.Thursday:
				return 4;
			case DayOfWeek.Friday:
				return 5;
			case DayOfWeek.Saturday:
			default:
				return 6;
			}
		}

		#endregion

		#region String Extensions

		internal static DayOfWeek ToDayOfWeek (this string str)
		{
			str = str.ToLower ();
			int indexOf
				= Array.IndexOf (TaskParser.WeekdaysArray, str);
			if (indexOf == -1)
				throw new ArgumentException (string.Format ("Wrong day {0}",
				                                             str));

			switch (indexOf) {
			case 0:
			case 1:
				return DayOfWeek.Sunday;
			case 2:
			case 3:
				return DayOfWeek.Monday;
			case 4:
			case 5:
				return DayOfWeek.Tuesday;
			case 6:
			case 7:
				return DayOfWeek.Wednesday;
			case 8:
			case 9:
				return DayOfWeek.Thursday;
			case 10:
			case 11:
				return DayOfWeek.Friday;
			case 12:
			case 13:
			default:
				return DayOfWeek.Saturday;
			}
		}

		internal static bool IsMonth (this string str)
		{
			return TaskParser.MonthsArray.Contains (str.ToLower ());
		}

		internal static bool IsWeekday (this string str)
		{
			return TaskParser.WeekdaysArray.Contains (str.ToLower ());
		}

		internal static bool ToOrdinalNumber (this string ordinalDate,
		                                        out int day)
		{
			day = -1;

			int cardinal = 0;

			int index = 0;
			foreach (char c in ordinalDate) {
				if (char.IsDigit (c))
					index++;
			}

			string number = ordinalDate.Substring (0, index);
			if (!int.TryParse (number, out cardinal))
				return false;

			day = cardinal;
			return true;
		}
		
		#endregion

	}
}
