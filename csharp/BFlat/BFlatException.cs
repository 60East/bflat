////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2016 60East Technologies Inc., All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFlat
{
    public class BFlatException : Exception
    {
        /// <summary>
        /// Constructs a default BFlatException.
        /// </summary>
        public BFlatException() { }
        /// <summary>
        /// Constructs a BFlatException with a reason string.
        /// </summary>
        /// <param name="message">The reason for this exception.</param>
        public BFlatException(string message) : base(message) { }

        /// <summary>
        /// Constructs a BFlatException for a format error while parsing.
        /// </summary>
        /// <param name="message">The reason for this exception</param>
        /// <param name="position">The position in the BFlat document where the
        ///                        error was detected.</param>
        public BFlatException(string message, int position)
            : base(String.Format("Error at position {0}: {1}", position,
                                 message))
        {
        }
    }
}
