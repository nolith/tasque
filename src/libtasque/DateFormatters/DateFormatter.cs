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

using Tasque;

namespace Tasque.DateFormatters {

	class DateFormatter : IDateFormatter {
		
		// Formatting: Month and OrdinalNumber
		public DateTime GetDate (Match match)
		{
			// Checking ordinal and cardinal numbers
			bool dayProvided = !(string.IsNullOrEmpty (match.Groups ["O"].Value)
			                     && string.IsNullOrEmpty (match.Groups ["N"].Value));
			bool monthProvided = !string.IsNullOrEmpty (match.Groups ["M"].Value);
			if (!monthProvided && !dayProvided)
				return DateTime.MinValue;

			int month = 0;
			if (!monthProvided)
				month = DateTime.Now.Month;
			else {
				month = Array.IndexOf (TaskParser.MonthsArray,
				                       match.Groups ["M"].Value.ToLower ());
				if (month == -1)
					return DateTime.MinValue;
				// Months start in 1.
				month = (month / 2) + 1;
			}

			int day = -1;
			if (dayProvided) {
				if (!string.IsNullOrEmpty (match.Groups ["N"].Value))
					int.TryParse (match.Groups ["N"].Value, out day);
				else if (!match.Groups ["O"].Value.ToOrdinalNumber (out day))
					return DateTime.MinValue;
			}
			int year =  DateTime.Today.Year;
			if (DateTime.Today.Month > month)
				year++;

			// If no day is provided, default is last one of the month
			if (day == -1)
				day = DateTime.DaysInMonth (year, month);

			try {
				return new DateTime (year, month, day);
			} catch (Exception ex) {
				if (ex is ArgumentOutOfRangeException
				    || ex is ArgumentException) {
					return DateTime.MinValue;
				} else
					throw;
			}
		}
	}
}
