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
using NUnit.Framework;

namespace Tasque.Tests {

	[TestFixture]
	public class TaskParserFixture {

		#region TodayTomorrowFormatter tests

		[Test]
		public void TomorrowTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now.AddDays (1);
			Assert.IsTrue (parser.TryParse ("Something tomorrow",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#0");
			Assert.AreEqual (expected.Year, dateTime.Year, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			dateTime = DateTime.MinValue;

			Assert.IsTrue (parser.TryParse ("Buy meat tomorrow for lunch",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Buy meat for lunch", parsedText, "#4");
			Assert.AreEqual (expected.Year, dateTime.Year, "#5");
			Assert.AreEqual (expected.Month, dateTime.Month, "#6");
			Assert.AreEqual (expected.Day, dateTime.Day, "#7");
			dateTime = DateTime.MinValue;

			Assert.IsTrue (parser.TryParse ("GET BEER TOMORROW!",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("GET BEER!", parsedText, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			Assert.AreEqual (expected.Month, dateTime.Month, "#10");
			Assert.AreEqual (expected.Day, dateTime.Day, "#11");
		}

		[Test]
		public void TodayTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now;
			Assert.IsTrue (parser.TryParse ("Something today",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#0");
			Assert.AreEqual (expected.Year, dateTime.Year, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			dateTime = DateTime.MinValue;

			Assert.IsTrue (parser.TryParse ("Buy meat today for lunch",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Buy meat for lunch", parsedText, "#4");
			Assert.AreEqual (expected.Year, dateTime.Year, "#5");
			Assert.AreEqual (expected.Month, dateTime.Month, "#6");
			Assert.AreEqual (expected.Day, dateTime.Day, "#7");
			dateTime = DateTime.MinValue;

			Assert.IsTrue (parser.TryParse ("GET BEER TODAY, RIGHT NOW!",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("GET BEER, RIGHT NOW!", parsedText, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			Assert.AreEqual (expected.Month, dateTime.Month, "#10");
			Assert.AreEqual (expected.Day, dateTime.Day, "#11");
		}

		#endregion

		#region MonthFormatter tests

		[Test]
		public void MonthTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now.AddMonths (1);
			Assert.IsTrue (parser.TryParse ("Something next month",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = DateTime.Now.AddMonths (3);
			Assert.IsTrue (parser.TryParse ("Something in 3 months",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			Assert.IsFalse (parser.TryParse ("Something in -1 months",
			                                  out parsedText,
			                                  out dateTime),
			               "#10");
			dateTime = DateTime.MinValue;

			// Even though this is invalid English, we accept it
			expected = DateTime.Now.AddMonths (1);
			Assert.IsTrue (parser.TryParse ("Something in months",
			                                 out parsedText,
			                                 out dateTime),
			               "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;
		}

		#endregion
		
		#region DayFormatter tests

		[Test]
		public void DayTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now.AddDays (3);
			Assert.IsTrue (parser.TryParse ("Something in 3 days",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = DateTime.Now.AddDays (1);
			Assert.IsTrue (parser.TryParse ("Something in 1 day",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			Assert.IsFalse (parser.TryParse ("Something in -1 days",
			                                  out parsedText,
			                                  out dateTime),
			               "#10");
			dateTime = DateTime.MinValue;

			// Even though this is invalid English, we accept it
			expected = DateTime.Now.AddDays (1);
			Assert.IsTrue (parser.TryParse ("Something in days",
			                                 out parsedText,
			                                 out dateTime),
			               "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;
		}

		#endregion

		#region WeekFormatter tests

		[Test]
		public void WeekTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now.AddDays (7);
			Assert.IsTrue (parser.TryParse ("Something next week",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = DateTime.Now.AddDays (7 * 2);
			Assert.IsTrue (parser.TryParse ("Something in 2 weeks",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			Assert.IsFalse (parser.TryParse ("Something in -1 week",
			                                  out parsedText,
			                                  out dateTime),
			               "#10");
			dateTime = DateTime.MinValue;

			// Even though this is invalid English, we accept it
			expected = DateTime.Now.AddDays (7);
			Assert.IsTrue (parser.TryParse ("Something in weeks",
			                                 out parsedText,
			                                 out dateTime),
			               "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;
		}

		#endregion

		#region YearFormatter tests

		[Test]
		public void YearTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = DateTime.Now.AddYears (1);
			Assert.IsTrue (parser.TryParse ("Something next year",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = DateTime.Now.AddYears (5);
			Assert.IsTrue (parser.TryParse ("Buy a house in 5 years",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Buy a house", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			Assert.IsFalse (parser.TryParse ("Sleep in -1 years",
			                                  out parsedText,
			                                  out dateTime),
			               "#10");
			dateTime = DateTime.MinValue;

			// Even though this is invalid English, we accept it
			expected = DateTime.Now.AddYears (1);
			Assert.IsTrue (parser.TryParse ("Something in years",
			                                 out parsedText,
			                                 out dateTime),
			               "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;
		}

		#endregion

		#region DateFormatter tests

		[Test]
		public void DateTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = GetMonthDate (12, 1);
			Assert.IsTrue (parser.TryParse ("Something due by December 1st",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#0");
			Assert.AreEqual (expected.Year, dateTime.Year, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (12, 1);
			Assert.IsTrue (parser.TryParse ("Something due Dec 1",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#4");
			Assert.AreEqual (expected.Year, dateTime.Year, "#5");
			Assert.AreEqual (expected.Month, dateTime.Month, "#6");
			Assert.AreEqual (expected.Day, dateTime.Day, "#7");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (12, -1);
			Assert.IsTrue (parser.TryParse ("Something due before December",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			Assert.AreEqual (expected.Month, dateTime.Month, "#10");
			Assert.AreEqual (expected.Day, dateTime.Day, "#11");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (-1, 15);
			Assert.IsTrue (parser.TryParse ("Something due by 15th",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#12");
			Assert.AreEqual (expected.Year, dateTime.Year, "#13");
			Assert.AreEqual (expected.Month, dateTime.Month, "#14");
			Assert.AreEqual (expected.Day, dateTime.Day, "#15");
			dateTime = DateTime.MinValue;

			// Invalid date, it should be 15th, but we accept it
			expected = GetMonthDate (-1, 15);
			Assert.IsTrue (parser.TryParse ("Something important due by 15nd",
			                                out parsedText,
			                                out dateTime),
			               "#16");
			Assert.AreEqual ("Something important", parsedText, "#17");
			Assert.AreEqual (expected.Year, dateTime.Year, "#18");
			Assert.AreEqual (expected.Month, dateTime.Month, "#19");
			Assert.AreEqual (expected.Day, dateTime.Day, "#20");
			
			expected = GetMonthDate (3, 15);
			Assert.IsTrue (parser.TryParse ("Something important March 15th",
			                                out parsedText,
			                                out dateTime),
			               "#21");
			Assert.AreEqual ("Something important", parsedText, "#22");
			Assert.AreEqual (expected.Year, dateTime.Year, "#23");
			Assert.AreEqual (expected.Month, dateTime.Month, "#24");
			Assert.AreEqual (expected.Day, dateTime.Day, "#25");

			expected = GetMonthDate (DateTime.Now.Month, 2);
			Assert.IsTrue (parser.TryParse (string.Format ("Something important {0} 2nd",
			                                               expected.ToString ("MMMM")),
			                                out parsedText,
			                                out dateTime),
			               "#26");
			Assert.AreEqual ("Something important", parsedText, "#27");
			Assert.AreEqual (expected.Year, dateTime.Year, "#28");
			Assert.AreEqual (expected.Month, dateTime.Month, "#29");
			Assert.AreEqual (expected.Day, dateTime.Day, "#30");
		}

		#endregion

		#region OrdinalFormatter tests

		[Test]
		public void OrdinalTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;

			DateTime expected = GetMonthDate (DateTime.Now.Month, 1);
			Assert.IsTrue (parser.TryParse ("Something on 1st",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (DateTime.Now.Month, 2);
			Assert.IsTrue (parser.TryParse ("Something due 2nd",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#5");
			Assert.AreEqual (expected.Year, dateTime.Year, "#6");
			Assert.AreEqual (expected.Month, dateTime.Month, "#7");
			Assert.AreEqual (expected.Day, dateTime.Day, "#8");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (-1, 15);
			Assert.IsTrue (parser.TryParse ("Something due by 15th",
			                                 out parsedText,
			                                 out dateTime));
			Assert.AreEqual ("Something", parsedText, "#9");
			Assert.AreEqual (expected.Year, dateTime.Year, "#10");
			Assert.AreEqual (expected.Month, dateTime.Month, "#11");
			Assert.AreEqual (expected.Day, dateTime.Day, "#12");
			dateTime = DateTime.MinValue;

			// Event this is an invalid date in English
			// we don't care about it, and we parse it
			expected = GetMonthDate (-1, 15);
			Assert.IsTrue (parser.TryParse ("Something important on 15nd",
			                                out parsedText,
			                                out dateTime),
			                "#13");
			Assert.AreEqual ("Something important", parsedText, "#14");
			Assert.AreEqual (expected.Year, dateTime.Year, "#15");
			Assert.AreEqual (expected.Month, dateTime.Month, "#16");
			Assert.AreEqual (expected.Day, dateTime.Day, "#17");
		}
		
		#endregion

		#region WeekdayFormatter tests

		[Test]
		public void WeekdayTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;
			
			DateTime expected = GetWeekdayDate (DayOfWeek.Monday);
			Assert.IsTrue (parser.TryParse ("Something on Monday",
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = GetWeekdayDate (DayOfWeek.Friday);
			Assert.IsTrue (parser.TryParse ("Something due on Friday",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			expected = GetWeekdayDate (DayOfWeek.Sunday);
			Assert.IsTrue (parser.TryParse ("Something next sunday",
			                                 out parsedText,
			                                 out dateTime),
			               "#10");
			Assert.AreEqual ("Something", parsedText, "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;

			expected = GetWeekdayDate (DayOfWeek.Wednesday);
			Assert.IsTrue (parser.TryParse ("Something next wed",
			                                 out parsedText,
			                                 out dateTime),
			               "#15");
			Assert.AreEqual ("Something", parsedText, "#16");
			Assert.AreEqual (expected.Year, dateTime.Year, "#17");
			Assert.AreEqual (expected.Month, dateTime.Month, "#18");
			Assert.AreEqual (expected.Day, dateTime.Day, "#19");
		}

		#endregion
		
		#region Others tests

		[Test]
		public void OthersTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;
			
			DateTime expected = GetWeekdayDate (DayOfWeek.Monday);
			Assert.IsTrue (parser.TryParse ("Something mon", // as in Monday
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (3, 11);
			Assert.IsTrue (parser.TryParse ("Something March 11",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (4, 22);
			Assert.IsTrue (parser.TryParse ("Something April 22nd",
			                                 out parsedText,
			                                 out dateTime),
			               "#10");
			Assert.AreEqual ("Something", parsedText, "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (-1, 12);
			Assert.IsTrue (parser.TryParse ("Something 12th",
			                                 out parsedText,
			                                 out dateTime),
			               "#15");
			Assert.AreEqual ("Something", parsedText, "#16");
			Assert.AreEqual (expected.Year, dateTime.Year, "#17");
			Assert.AreEqual (expected.Month, dateTime.Month, "#18");
			Assert.AreEqual (expected.Day, dateTime.Day, "#19");
		}

		#endregion
		
		#region DateSeparatedFormatter Tests

		[Test]
		public void DateSeparatedTest ()
		{
			string parsedText = null;
			DateTime dateTime = DateTime.MinValue;
			TaskParser parser = TaskParser.Instance;
			
			DateTime expected = GetMonthDate (11, 12);
			Assert.IsTrue (parser.TryParse ("Something 11/12", // as in Monday
			                                 out parsedText,
			                                 out dateTime),
			               "#0");
			Assert.AreEqual ("Something", parsedText, "#1");
			Assert.AreEqual (expected.Year, dateTime.Year, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			dateTime = DateTime.MinValue;

			expected = GetMonthDate (3, 11);
			Assert.IsTrue (parser.TryParse ("Something 3-11",
			                                 out parsedText,
			                                 out dateTime),
			               "#5");
			Assert.AreEqual ("Something", parsedText, "#6");
			Assert.AreEqual (expected.Year, dateTime.Year, "#7");
			Assert.AreEqual (expected.Month, dateTime.Month, "#8");
			Assert.AreEqual (expected.Day, dateTime.Day, "#9");
			dateTime = DateTime.MinValue;

			expected = new DateTime (DateTime.Now.Year + 1, 1, 22);
			Assert.IsTrue (parser.TryParse (string.Format ("Something 1/22/{0}",
			                                               DateTime.Now.Year + 1),
			                                 out parsedText,
			                                 out dateTime),
			               "#10");
			Assert.AreEqual ("Something", parsedText, "#11");
			Assert.AreEqual (expected.Year, dateTime.Year, "#12");
			Assert.AreEqual (expected.Month, dateTime.Month, "#13");
			Assert.AreEqual (expected.Day, dateTime.Day, "#14");
			dateTime = DateTime.MinValue;
			
			expected = new DateTime (2011, 11, 11);
			Assert.IsTrue (parser.TryParse ("Something 11-11-11 soon!",
			                                out parsedText,
			                                out dateTime),
			                "#15");
			Assert.AreEqual ("Something soon!", parsedText, "#16");
			Assert.AreEqual (expected.Year, dateTime.Year, "#17");
			Assert.AreEqual (expected.Month, dateTime.Month, "#18");
			Assert.AreEqual (expected.Day, dateTime.Day, "#19");
			dateTime = DateTime.MinValue;
			
			// The year doesn't make sense, but is still valid
			expected = new DateTime (102, 1, 13);
			Assert.IsTrue (parser.TryParse ("Something 1-13-102 soon!!",
			                                out parsedText,
			                                out dateTime),
			                "#20");
			Assert.AreEqual ("Something soon!!", parsedText, "#21");
			Assert.AreEqual (expected.Year, dateTime.Year, "#22");
			Assert.AreEqual (expected.Month, dateTime.Month, "#23");
			Assert.AreEqual (expected.Day, dateTime.Day, "#24");
			dateTime = DateTime.MinValue;

			// Matches "11/11"
			expected = GetMonthDate (11, 11);
			Assert.IsTrue (parser.TryParse ("Buy beer 11/11/3 for party",
			                                out parsedText,
			                                out dateTime),
			                "#25");
			Assert.AreEqual ("Buy beer/3 for party", parsedText, "#26");
			Assert.AreEqual (expected.Year, dateTime.Year, "#27");
			Assert.AreEqual (expected.Month, dateTime.Month, "#28");
			Assert.AreEqual (expected.Day, dateTime.Day, "#29");
		}

		#endregion

		#region Helper methods

		DateTime GetMonthDate (int month, int day)
		{
			if (month == -1)
				month = DateTime.Now.Month;

			if (DateTime.Now.Month > month) {
				if (day == -1)
					day = DateTime.DaysInMonth (DateTime.Now.Year + 1, month);
				return new DateTime (DateTime.Now.Year + 1, month, day);
			}
			else {
				if (day == -1)
					day = DateTime.DaysInMonth (DateTime.Now.Year, month);
				return new DateTime (DateTime.Now.Year, month, day);
			}
		}
		
		DateTime GetWeekdayDate (DayOfWeek weekday)
		{
			DateTime todayDateTime = DateTime.Now;
			uint today = ToUint (todayDateTime.DayOfWeek);
			uint future = ToUint (weekday);
			if (future > today) 
				return DateTime.Now.AddDays (future - today);
			else if (today > future)
				return DateTime.Now.AddDays (7 - (today - future));
			else // future is in one week
				return DateTime.Now.AddDays (7);
		}

		uint ToUint (DayOfWeek dayOfWeek)
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
	}
}
