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
using Tasque.DateFormatters;

namespace Tasque {

	// Used to keep a relation of Regular Expression and the
	// formatters used to get the date.
	class RegularExpressionFormatter {

		public RegularExpressionFormatter ()
		{
		}

		public string RegularExpression {
			get { return regularExpression; }
			set {
				if (value == null)
					throw new ArgumentNullException ("RegularExpression");
				regularExpression = value;
			}
		}

		public IEnumerable<IDateFormatter> Formatters {
			get { return formatters; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Formatters");
				formatters = value;
			}
		}

		IEnumerable<IDateFormatter> formatters;
		string regularExpression;
	}

}
