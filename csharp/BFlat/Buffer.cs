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
    /// <summary>
    /// Represents a byte array and current position in the array.
    /// </summary>
    public class Buffer
    {
        /// <summary>
        /// Create a new Buffer with no underlying byte array.
        /// </summary>
        public Buffer() { }
        /// <summary>
        /// Create a new Buffer wrapping an existing byte array.
        /// </summary>
        /// <param name="data">The byte array to wrap.</param>
        /// <param name="start">The position in this byte array where
        ///   the Buffer begins.</param>
        public Buffer(byte[] data, int start)
        {
            this.data = data;
            this.position = start;
            this.start = start;
        }

        /// <summary>
        /// Returns the number of bytes remaining in this buffer.
        /// </summary>
        /// <returns>The number of bytes remaining.</returns>
        public int remaining()
        {
            return data.Length - position;
        }

        /// <summary>
        /// Rewinds this buffer to the original starting position.
        /// </summary>
        /// <returns>This Buffer.</returns>
        public Buffer rewind()
        {
            this.position = this.start;
            return this;
        }

        /// <summary>
        /// The underlying byte array for this buffer.
        /// </summary>
        public byte[] data;
        /// <summary>
        /// The current position in the buffer.
        /// </summary>
        public int position;
        /// <summary>
        /// The original starting position in the buffer.
        /// </summary>
        public int start;
    }
}
