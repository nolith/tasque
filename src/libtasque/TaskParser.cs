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

using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tasque.DateFormatters;

namespace Tasque {
	
	// TODO: Support for time parsing

	public class TaskParser {

		#region Public Members

		public static TaskParser Instance {
			get {
				if (instance == null)
					instance = new TaskParser ();
				return instance;
			}
		}
		
		public static string OrdinalSuffixes = Catalog.GetString ("th,st,nd,rd");
		
		public static string Today = Catalog.GetString ("today");

		public static string Tomorrow = Catalog.GetString ("tomorrow");

		// L10N: Don't forget to include the plural value, if any
		public static string Month = Catalog.GetString ("month|months");

		// L10N: Don't forget to include the plural value, if any
		public static string Week = Catalog.GetString ("week|weeks");

		// L10N: Don't forget to include the plural value, if any
		public static string Year = Catalog.GetString ("year|years");

		// L10N: Don't forget to include the plural value, if any
		public static string Day = Catalog.GetString ("day|days");

		public static string[] MonthsArray {
			get {
				if (months == null) {
					List<string> list  = new List<string> (12);
					foreach (int month in Enumerable.Range (1, 12)) {
						DateTime date = new DateTime (1900, month, 1);
						list.Add (date.ToString ("MMMM").ToLower ());
						list.Add (date.ToString ("MMM").ToLower ());
					}
					months = list.ToArray ();
				}
				return months;
			}
		}

		public static string[] WeekdaysArray {
			get {
				if (weekdays == null) {
					List<string> list  = new List<string> (7);
					// To begin on Sunday January 7th 1900.
					// Sunday is our index 0
					foreach (int day in Enumerable.Range (7, 7)) {
						DateTime date = new DateTime (1900, 1, day);
						list.Add (date.ToString ("dddd").ToLower ());
						list.Add (date.ToString ("ddd").ToLower ());
					}
					weekdays = list.ToArray ();
				}
				return weekdays;
			}
		}

		public bool TryParse (string enteredTaskText,
		                        out string parsedTaskText,
		                        out DateTime dueDateTime)
		{
			if (fullExpression == null) {
				fullExpression = string.Format ("{0}|{1}",
				                                tokensExpression,
				                                dateGroupExpression);
	
				// 1. Get all translatable token
				translatableTokens = TranslatableTokens;
	
				// 2. Get all regular expressions, these are also translatable
				//    and are based on the translatable tokens.
				regExFormats = RegularExpressions;
			}

			dueDateTime = DateTime.MinValue;
			parsedTaskText = enteredTaskText;
			foreach (RegularExpressionFormatter format in regExFormats) {
				string regEx = string.Format (@"^(?<task>.+?)\s*?{0}(?<eol>.*)$",
				                               format.RegularExpression);
				Match match = Regex.Match (enteredTaskText,
				                           regEx,
				                           RegexOptions.IgnoreCase);
				if (match.Success) {
					string trimmedTaskText = match.Groups ["task"].Value;

					if (!string.IsNullOrEmpty (trimmedTaskText)) {
						foreach (IDateFormatter formatter in format.Formatters) {
							dueDateTime = formatter.GetDate (match);
							if (dueDateTime != DateTime.MinValue)
								break;
						}
						if (dueDateTime == DateTime.MinValue)
							return false;

						string trimmedEofText = match.Groups ["eol"].Value;
						parsedTaskText = string.Format ("{0}{1}", trimmedTaskText, trimmedEofText);

						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region Static Members

		static TaskParser instance;
		static string[] months;
		static string[] weekdays;

		#endregion
		
		#region Private Members

		// All the translatable tokens and their possible formatters
		// are defined here
		Dictionary<char, TranslatableToken> TranslatableTokens {
			get {
				Dictionary<char, TranslatableToken> tokens
					= new Dictionary<char, TranslatableToken> ();
				// Months
				tokens.Add ('M', new TranslatableToken () {
					Formatter = typeof (DateFormatter),
					Expression = string.Join ("|", MonthsArray)
				});
				// Weekdays
				tokens.Add ('D', new TranslatableToken () {
					Formatter = typeof (WeekdayFormatter),
					Expression = string.Join ("|", WeekdaysArray)
				});
				// Date separated Formatter
				tokens.Add ('A', new TranslatableToken () {
					Formatter = typeof (DateSeparatedFormatter),
					Expression = string.Format ("{0}",
					                            @"[0-9]{1,2}(/|-)[0-9]{1,2}(((/|-)[0-9]{2,4}|/s))?")
				});
				// Today and Tomorrow
				tokens.Add ('T', new TranslatableToken () {
					Formatter = typeof (TodayTomorrowFormatter),
					Expression = string.Format ("{0}|{1}", Today, Tomorrow)
				});
				// Ordinal Number
				tokens.Add ('O', new TranslatableToken () {
					Formatter = typeof (DateFormatter),
					Expression = string.Format ("{0}({1})",
					                            @"\d{1,2}",
					                            OrdinalSuffixes.Replace (",", "|"))
				});
				// Cardinal Number
				tokens.Add ('N', new TranslatableToken () {
					Expression = string.Format ("{0}", @"[1-9][0-9]?")
				});
				// Due
				tokens.Add ('u', new TranslatableToken () {
					Expression = Catalog.GetString ("due before|due by|due")
				});
				// Day
				tokens.Add ('d', new TranslatableToken () {
					Formatter = typeof (DayFormatter),
					Expression = Day
				});
				// Week
				tokens.Add ('w', new TranslatableToken () {
					Formatter = typeof (WeekFormatter),
					Expression = Week
				});
				// Month
				tokens.Add ('m', new TranslatableToken () {
					Formatter = typeof (MonthFormatter),
					Expression = Month
				});
				// Year
				tokens.Add ('y', new TranslatableToken () {
					Formatter = typeof (YearFormatter),
					Expression = Year
				});
				// Next
				tokens.Add ('n', new TranslatableToken () {
					// L10N: Examples could be: "Next Month", "Next Monday"
					Expression = Catalog.GetString ("next")
				});
				// On
				tokens.Add ('o', new TranslatableToken () {
					// L10N: Examples could be: "On April 1st", "On Wednesday"
					Expression = Catalog.GetString ("on")
				});
				// In
				tokens.Add ('i', new TranslatableToken () {
					// L10N: Examples could be: "In 2 weeks", "In 3 months"
					Expression = Catalog.GetString ("in")
				});

				return tokens;
			}
		}

		List<RegularExpressionFormatter> RegularExpressions {
			get {
				List<RegularExpressionFormatter> regularExpressions
					= new List<RegularExpressionFormatter> ();
				List<string> translatableExpressions 
					= new List<string> () {
					// "Today and Tomorrow" expression. More information in TRANSLATORS
					Catalog.GetString ("u T"),
					// Represents: "Next" expression. More information in TRANSLATORS
					Catalog.GetString ("n w|n m|n y|n D"),
					// Represents "Due" expression. More information in TRANSLATORS
					Catalog.GetString ("u o D|u M O|u M N|u O|u M"),
					// Represents "On" expression. More information in TRANSLATORS
					Catalog.GetString (@"o D|o O"),
					// Represents "In" expression. More information in TRANSLATORS
					Catalog.GetString (@"i N d|i d|i N w|i w|i N m|i m|i N y|i y"),
					// Represents all other expressions not using tokens. More information in TRANSLATORS
					Catalog.GetString (@"T|D|M O|M N|O|A")
				};

				foreach (string expression in translatableExpressions) {
					foreach (string alternativeExpression 
					         in GetAlternativesExpressions (expression)) {
						regularExpressions.Add (new RegularExpressionFormatter () {
						                           RegularExpression = GenerateRegularExpression (alternativeExpression),
						                           Formatters = GetFormatters (alternativeExpression)
						                        });
					}
				}
				return regularExpressions;
			}
		}

		#endregion

		#region Private Members
		
		TaskParser ()
		{
		}

		string GenerateRegularExpression (string translatedExpression)
		{
			List<string> expressionList = new List<string> ();
			Regex regex = new Regex (fullExpression, RegexOptions.IgnoreCase);
			foreach (Match match in regex.Matches (translatedExpression)) {
				// We found a match for a string like this: "\Something\"
				if (Regex.IsMatch (match.Value, dateGroupExpression)) {
					Regex dateRegEx = new Regex (tokensExpression,
					                             RegexOptions.IgnoreCase);
					string date = "(?<date>";
					List<string> dateList = new List<string> ();
					foreach (Match token in dateRegEx.Matches (match.Value))
						dateList.Add (GetFormatedToken (token));
					date += string.Join (@"\s+", dateList.ToArray ()) + ")";
					expressionList.Add (date);
				// Using translated tokens
				} else
					expressionList.Add (GetFormatedToken (match));
			}
			return String.Join (@"\s+", expressionList.ToArray ());
		}

		string GetFormatedToken (Match match)
		{
			// It's a translatable token, we need it named.
			if (match.Value.Length == 1) {
				char token = match.Value [0];
				string savedTokens = translatableTokens [token].Expression;
				return string.Format (@"(?<{0}>\b({1})\b)", token, savedTokens);
			// It's a "hardcoded" translated string. We need the complete word
			} else
				return string.Format (@"({0})", match.Value.Replace ("'", @"\b"));
		}
		
		List<IDateFormatter> GetFormatters (string expression)
		{
			List<IDateFormatter> formatters = new List<IDateFormatter> ();
			foreach (string str in expression.Split (' ')) {
				char token = char.MinValue;
				if (!char.TryParse (str, out token))
					continue;

				TranslatableToken translatableToken = null;
				if (!translatableTokens.TryGetValue (token,
				                                     out translatableToken))
					continue;

				if (translatableToken.Formatter == null)
					continue;

				IDateFormatter formatter = DateFormatterFactory
					.Instance.Get (translatableToken.Formatter);
				if (!formatters.Contains (formatter))
					formatters.Add (formatter);
			}
			if (formatters.Count == 0)
				throw new Exception ("No formatters");

			return formatters;
		}

		// Spliting all "|" separated expressions (RegularExpressions)
		List<string> GetAlternativesExpressions (string expression)
		{
			// Matches anything like this: "a b cd|e f g 'xy'"
			string alternativesRegEx = @"(\w|\s|\'|\\)+((?<=\|)(\w|\s|\'|\\)+)*";

			List<string> expressionList = new List<string> ();
			Regex regex = new Regex (alternativesRegEx,
			                         RegexOptions.IgnoreCase);
			foreach (Match match in regex.Matches (expression))
				expressionList.Add (match.Value);
			return expressionList;
		}

		// Matches format and explicit '' delimited text
		string tokensExpression = @"(\b\w\b|'\w+')+((?<=\s)(\b\w\b|'\w+'))*";
		// Expression that defines the named group "<?date>"
		string dateGroupExpression = @"(?>\\)(\b\w\b|'\w+')+(?>.*?\\)";

		string fullExpression;
		List<RegularExpressionFormatter> regExFormats;
		Dictionary<char, TranslatableToken> translatableTokens;

		#endregion
	}
}
