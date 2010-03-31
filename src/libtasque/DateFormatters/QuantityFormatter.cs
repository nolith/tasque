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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Tasque.DateFormatters {

	abstract class QuantityFormatter : IDateFormatter {

		#region Constructor

		protected QuantityFormatter (string token)
		{
			this.token = token;
		}

		#endregion

		#region IDateFormatter

		public DateTime GetDate (Match match)
		{
			if (string.IsNullOrEmpty (match.Groups [token].Value)
			    || !Translations.Contains (match.Groups [token].Value,
			                               new StringInsensitiveComparer ()))
				return DateTime.MinValue;

			return GetDate (GetCardinal (match));
		}

		#endregion
		
		#region Public Members

		public abstract DateTime GetDate (int quantity);

		#endregion
		
		#region Protected Members

		protected List<string> Translations {
			get {
				if (translations == null)
					translations = new List<string> (RegularExpression.Split ('|'));
				return translations;
			}
		}

		protected abstract string RegularExpression {
			get;
		}

		protected int GetCardinal (Match match)
		{
			int cardinal = 1;
			string cardinalStr = match.Groups ["N"].Value;
			// We don't validate "valid" literal value, 
			// for example, in English:
			// "Something in months" or "Something in 3 month"
			if (!string.IsNullOrEmpty (cardinalStr)) {
				if (!int.TryParse (cardinalStr, out cardinal))
					throw new ArgumentException ("cardinal");
			}
			return cardinal;
		}

		#endregion
		
		#region Private Members

		List<string> translations;
		string token;

		#endregion
	}
}
