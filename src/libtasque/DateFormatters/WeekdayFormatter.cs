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
using System.Text.RegularExpressions;

namespace Tasque.DateFormatters {

	class WeekdayFormatter : IDateFormatter {

		public DateTime GetDate (Match match)
		{
			string weekDay = match.Groups ["D"].Value;
			if (string.IsNullOrEmpty (weekDay))
				return DateTime.MinValue;

			DateTime todayDateTime = DateTime.Now;
			uint today = todayDateTime.DayOfWeek.ToUint ();
			uint future = weekDay.ToDayOfWeek ().ToUint ();
			if (future > today) 
				return DateTime.Now.AddDays (future - today);
			else if (today > future)
				return DateTime.Now.AddDays (7 - (today - future));
			else // future is in one week
				return DateTime.Now.AddDays (7);
		}

	}
}
