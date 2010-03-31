//
// Author:
//      Mario Carrion <mario@carrion.mx>
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

using Tasque;

namespace Tasque.Tests {

	[TestFixture]
	public class DateGuesserTest {
		
		#region TomorrowTodayGuesser

		[Test]
		public void Today ()
		{
			DateGuesser guesser = DateGuesser.Instance;
			
			DateTime dateTime = DateTime.MinValue;
			DateTime expected = DateTime.Today;
			string task = string.Empty;
			string text = string.Empty;

			// #1
			text = string.Format ("Sell something, {0}, on the internet", 
			                       Constants.Today);
			bool parsed = guesser.Parse (text, out task, out dateTime);
			Assert.IsTrue (parsed, "#0");
			Assert.AreEqual ("Sell something,, on the internet", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
			parsed = false;
			dateTime = DateTime.MinValue;

			// #2
			text = string.Format ("Buy beer {0}", Constants.Today);
			parsed = guesser.Parse (text, out task, out dateTime);
			Assert.IsTrue (parsed, "#5");
			Assert.AreEqual ("Buy beer", task, "#6");
			Assert.AreEqual (expected.Month, dateTime.Month, "#7");
			Assert.AreEqual (expected.Day, dateTime.Day, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			parsed = false;
			dateTime = DateTime.MinValue;
			
			// Due is separated by "|"
			string[] dues = Constants.Due.Split (new string[] { "|" }, 
			                                     StringSplitOptions.RemoveEmptyEntries);
			foreach (string due in dues) {
				if (string.IsNullOrEmpty (due))
					continue;

				// #3
				text = string.Format ("Print tickets {0} {1}", due, Constants.Today);
				parsed = guesser.Parse (text, out task, out dateTime);
				Assert.IsTrue (parsed, "#10");
				Assert.AreEqual ("Print tickets", 
				                 task, 
				                 string.Format ("#11 -> {0}", due));
				Assert.AreEqual (expected.Month,
				                 dateTime.Month, 
				                 string.Format ("#12 -> {0}", due));
				Assert.AreEqual (expected.Day, 
				                 dateTime.Day, 
				                 string.Format ("#13 -> {0}", due));
				Assert.AreEqual (expected.Year, 
				                 dateTime.Year, 
				                 string.Format ("#14 -> {0}", due));
				parsed = false;
				dateTime = DateTime.MinValue;
			}
		}
		
		[Test]
		public void Tomorrow ()
		{
			DateGuesser guesser = DateGuesser.Instance;
			
			DateTime dateTime = DateTime.MinValue;
			DateTime expected = DateTime.Today.AddDays (1);
			string task = string.Empty;
			string text = string.Empty;

			// #1
			text = string.Format ("Sell something, {0}, on the internet", 
			                       Constants.Tomorrow);
			bool parsed = guesser.Parse (text, out task, out dateTime);
			Assert.IsTrue (parsed, "#0");
			Assert.AreEqual ("Sell something,, on the internet", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
			parsed = false;
			dateTime = DateTime.MinValue;

			// #2
			text = string.Format ("Buy beer {0}", Constants.Tomorrow);
			parsed = guesser.Parse (text, out task, out dateTime);
			Assert.IsTrue (parsed, "#5");
			Assert.AreEqual ("Buy beer", task, "#6");
			Assert.AreEqual (expected.Month, dateTime.Month, "#7");
			Assert.AreEqual (expected.Day, dateTime.Day, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			parsed = false;
			dateTime = DateTime.MinValue;
			
			// Due is separated by "|"
			string[] dues = Constants.Due.Split (new string[] { "|" }, 
			                                     StringSplitOptions.RemoveEmptyEntries);
			foreach (string due in dues) {
				if (string.IsNullOrEmpty (due))
					continue;

				// #3
				text = string.Format ("Print tickets {0} {1}", due, Constants.Tomorrow);
				parsed = guesser.Parse (text, out task, out dateTime);
				Assert.IsTrue (parsed, "#10");
				Assert.AreEqual ("Print tickets", 
				                 task, 
				                 string.Format ("#11 -> {0}", due));
				Assert.AreEqual (expected.Month,
				                 dateTime.Month, 
				                 string.Format ("#12 -> {0}", due));
				Assert.AreEqual (expected.Day, 
				                 dateTime.Day, 
				                 string.Format ("#13 -> {0}", due));
				Assert.AreEqual (expected.Year, 
				                 dateTime.Year, 
				                 string.Format ("#14 -> {0}", due));
				parsed = false;
				dateTime = DateTime.MinValue;
			}
		}

		#endregion

		#region NextGuesser
		
		[Test]
		public void NextMonth ()
		{
			DateTime dateTime = DateTime.MinValue;
			string task = string.Empty;

			DateTime expected = DateTime.Today.AddMonths (1);
			string text = string.Format ("Sell something, {0} {1}, on the internet", 
			                              Constants.Next, 
			                              Constants.Month);
			Assert.IsTrue (DateGuesser.Instance.Parse (text, out task, out dateTime), "#0");
			Assert.AreEqual ("Sell something,, on the internet", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
		}

		[Test]
		public void NextWeek ()
		{
			DateTime dateTime = DateTime.MinValue;
			string task = string.Empty;
			DateTime expected = DateTime.Today.AddDays (7);

			string text = string.Format ("Buy beer {0} {1}",
			                              Constants.Next,
			                              Constants.Week);
			Assert.IsTrue (DateGuesser.Instance.Parse (text, out task, out dateTime), "#1");
			Assert.AreEqual ("Buy beer", task, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			Assert.AreEqual (expected.Year, dateTime.Year, "#5");
		}
		
		[Test]
		public void NextYear ()
		{
			DateTime dateTime = DateTime.MinValue;
			string task = string.Empty;

			DateTime expected = DateTime.Today.AddYears (1);
			string text = string.Format ("Buy a house {0} {1}",
			                              Constants.Next,
			                              Constants.Year);
			Assert.IsTrue (DateGuesser.Instance.Parse (text, out task, out dateTime), "#1");
			Assert.AreEqual ("Buy a house", task, "#2");
			Assert.AreEqual (expected.Month, dateTime.Month, "#3");
			Assert.AreEqual (expected.Day, dateTime.Day, "#4");
			Assert.AreEqual (expected.Year, dateTime.Year, "#5");
		}

		[Test]
		public void NextMonday ()
		{
			DateTime dateTime = DateTime.MinValue;
			string task = string.Empty;

			DateTime expected = DateTime.Today.AddDays (GetDayOffset (DayOfWeek.Monday));
			string text = string.Format ("Doctor appointment {0} {1}", 
			                             Constants.Next, 
			                             Utilities.GetLocalizedDayOfWeek (DayOfWeek.Monday));
			Assert.IsTrue (DateGuesser.Instance.Parse (text, out task, out dateTime), "#0");
			Assert.AreEqual ("Doctor appointment", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
		}

		[Test]
		public void OnNextMonday ()
		{
			DateTime dateTime = DateTime.MinValue;
			string task = string.Empty;

			DateTime expected = DateTime.Today.AddDays (GetDayOffset (DayOfWeek.Monday));
			string text = string.Format ("Doctor appointment {0} {1} {2}",
			                             Constants.On,
			                             Constants.Next,
			                             Utilities.GetLocalizedDayOfWeek (DayOfWeek.Monday));
			Assert.IsTrue (DateGuesser.Instance.Parse (text, out task, out dateTime), "#0");
			Assert.AreEqual ("Doctor appointment", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
		}

		#endregion
		
		#region DueGuesser 
		
		[Test]
		public void DueValid ()
		{
			DateGuesser guesser = DateGuesser.Instance;
			
			DateTime dateTime = DateTime.MinValue;
			DateTime expected = DateTime.MinValue;
			string task = string.Empty;

			// January
			string text = string.Format ("Sell something due {0} 12", Constants.Months [0]);
			expected = GetMonthDate (1, 12);

			Assert.IsTrue (guesser.Parse (text, out task, out dateTime), "#0");
			Assert.AreEqual ("Sell something", task, "#1");
			Assert.AreEqual (expected.Month, dateTime.Month, "#2");
			Assert.AreEqual (expected.Day, dateTime.Day, "#3");
			Assert.AreEqual (expected.Year, dateTime.Year, "#4");
			dateTime = DateTime.MinValue;

			// April 22
			expected = GetMonthDate (4, 22);
			text = string.Format ("Sell something due {0} 22nd", Constants.Months [7]);

			Assert.IsTrue (guesser.Parse (text, out task, out dateTime), "#5");
			Assert.AreEqual ("Sell something", task, "#6");
			Assert.AreEqual (expected.Month, dateTime.Month, "#7");
			Assert.AreEqual (expected.Day, dateTime.Day, "#8");
			Assert.AreEqual (expected.Year, dateTime.Year, "#9");
			dateTime = DateTime.MinValue;

			// April 13th
			expected = GetMonthDate (4, 13);
			text = string.Format ("Sell something due {0} 13th", Constants.Months [7]);

			Assert.IsTrue (guesser.Parse (text, out task, out dateTime), "#10");
			Assert.AreEqual ("Sell something", task, "#11");
			Assert.AreEqual (expected.Month, dateTime.Month, "#12");
			Assert.AreEqual (expected.Day, dateTime.Day, "#13");
			Assert.AreEqual (expected.Year, dateTime.Year, "#14");
			dateTime = DateTime.MinValue;
			
			// May
			expected = GetMonthDate (5, 1);
			text = string.Format ("Sell something due before {0}", Constants.Months [9]);

			Assert.IsTrue (guesser.Parse (text, out task, out dateTime), "#15");
			Assert.AreEqual ("Sell something", task, "#16");
			Assert.AreEqual (expected.Month, dateTime.Month, "#17");
			Assert.AreEqual (expected.Day, dateTime.Day, "#18");
			Assert.AreEqual (expected.Year, dateTime.Year, "#19");
			
			// April 13th
			expected = GetMonthDate (4, 12);
			text = string.Format ("Sell something due {0} 12th", Constants.Months [7]);

			Assert.IsTrue (guesser.Parse (text, out task, out dateTime), "#20");
			Assert.AreEqual ("Sell something", task, "#21");
			Assert.AreEqual (expected.Month, dateTime.Month, "#22");
			Assert.AreEqual (expected.Day, dateTime.Day, "#23");
			Assert.AreEqual (expected.Year, dateTime.Year, "#24");
			dateTime = DateTime.MinValue;
		}
		
		[Test]
		public void DueInvalid ()
		{
			string task = string.Empty;
			DateTime dateTime = DateTime.MinValue;
			
			// April 22rd - invalid
			string text = string.Format ("Sell something due {0} 22rd", Constants.Months [7]);
			Assert.IsFalse (DateGuesser.Instance.Parse (text, 
			                                            out task,
			                                            out dateTime), 
			                "#10");

			// April 32nd
			text = string.Format ("Sell something due {0} 32nd", Constants.Months [7]);
			Assert.IsFalse (DateGuesser.Instance.Parse (text, 
			                                            out task, 
			                                            out dateTime), 
			                "#10");
		}

		#endregion

		#region Helper methods

		DateTime GetMonthDate (int month, int day)
		{
			if (DateTime.Now.Month > month)
				return new DateTime (DateTime.Now.Year + 1, month, day);
			else
				return new DateTime (DateTime.Now.Year, month, day);
		}

		uint GetDayOffset (DayOfWeek futureDay)
		{
			uint future = DateGuesser.DayOfWeekToUInt (futureDay);
			uint today = DateGuesser.DayOfWeekToUInt (DateTime.Today.DayOfWeek);
			
			if (future > today)
				return future - today;
			else if (today > future)
				return 7 - (today - future);
			else
				return 7;
		}

		#endregion
	}
}
