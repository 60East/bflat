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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFlat
{

    /// <summary>
    /// Parser for BFlat messages that provides an {@link Iterator} interface
    /// to the values in a message. BFlatParser parses BFlat-encoded data from
    /// a byte array acting as an iterator over the values contained in the
    /// message.
    ///
    /// Example of parsing a BFlat message using foreach:
    /// <code>
    /// // Assume rawMessage contains only a BFlat-encoded message with scalar
    /// //  values -- see BFlatValue for more information on using arrays.
    /// byte[] rawMessage = ...;
    /// BFlatParser parser = new BFlatParser();
    ///
    /// foreach(BFlatValue value in parser.parse(rawMessage))
    /// {
    ///   // Note, if you just want to convert the value to a string,
    ///   //  you could simply say 'value.ToString()'.
    ///   //
    ///   // print the "tag" string from this value.
    ///   Console.Format("{0}: ", value.getTag());
    ///   // Use the type to invoke the right getter and format correctly.
    ///   switch(value.getType())
    ///   {
    ///       case BFlatEncoding.String:
    ///       case BFlatEncoding.Binary:
    ///         Console.WriteLine("{0}", value.getString());
    ///         break;
    ///       case BFlatEncoding.Int8:
    ///       case BFlatEncoding.Int16:
    ///       case BFlatEncoding.Int32:
    ///       case BFlatEncoding.Int64:
    ///       case BFlatEncoding.Leb128:
    ///       case BFlatEncoding.Datetime:
    ///         Console.WriteLine("{0}", value.getLong());
    ///         break;
    ///       case BFlatEncoding.Null:
    ///         Console.WriteLine("null");
    ///         break;
    ///       case BFlatEncoding.Double:
    ///         Console.WriteLine("{0}", value.getDouble());
    ///         break;
    ///   }
    ///   // Optional -- since we are finished with "value", calling
    ///   //  BFlatValue.reuse() allows it to be reused by the parser
    ///   //  for better performance/less garbage creation.
    ///   value.reuse();
    /// }
    /// </code>
    /// </summary>
    public class BFlatParser : Buffer, IEnumerable<BFlatValue>,
                                       IEnumerator<BFlatValue>
    {
        /// <summary>
        /// Construct a default BFlatParser. The
        /// <see cref="parse(byte[])"/> or
        /// <see cref="parse(byte[], int, int)"/> method must be called before
        /// iteration can begin.
        /// </summary>
        public BFlatParser()
        {
        }

        /// <summary>
        /// Begin parsing a BFlat message contained in a byte array.
        /// Upon successful return, this parser object may be used to iterate
        /// over the values in the message.
        /// </summary>
        ///
        /// <param name="data">The byte array containing a BFlat-encoded
        /// message to parse.</param>
        ///
        /// <returns>This parser object, which may be used as an iterator over
        ///          the values in this message.</returns>
        public BFlatParser parse(byte[] data)
        {
            return parse(data, 0, data.Length);
        }

        /// <summary>
        /// Begin parsing a BFlat message contained in a byte array.
        /// Upon successful return, this parser object may be used to iterate
        /// over the values in the message.
        /// </summary>
        /// <param name="data">The byte array containing a BFlat-encoded
        /// message to parse.</param>
        /// <param name="position">The position in data where BFlat
        /// data begins.</param>
        /// <param name="length">The length of BFlat data contained in
        /// data.</param>
        ///
        /// <returns>This parser object, which may be used as an iterator
        /// over the values in this message.</returns>
        /// @throws BFlatException An error occurred parsing this message.
        ///
        public BFlatParser parse(byte[] data, int position, int length)
        {

            this.data = data;
            this.position = position;
            this.start = this.position;

            _end = position + length;
            return this;
        }

        BFlatValue parseNext()
        {
            if (position >= _end)
            {
                return null;
            }
            BFlatValue value = (_current != null && _current.isReuse())
          ? _current.reset(data) : new BFlatValue(data);


            byte byte0 = data[position++];
            byte type = (byte)(byte0 & BFlatEncoding.TypeMask);
            bool isArray = (byte0 & BFlatEncoding.ArrayMask) != 0;
            int tagLength = byte0 & BFlatEncoding.LengthMask;
            int tagStart = position;

            if (tagLength == 0)
            {
                tagLength = (int)(Leb128.decodeUnsigned(this));
                if (tagLength == 0)
                {
                    throw new BFlatException("zero-length tag", position);


                }
                tagStart = position;
            }
            value.setTag(tagStart, tagLength);
            position += tagLength;
            int dataStart = position;

            int elementCount = 1;
            if (isArray)
            {
                elementCount = (int)(Leb128.decodeUnsigned(this));
                dataStart = position;
            }

            value.setData(position, byte0, elementCount);
            // for variable length types we have to parse the array contents
            switch ((BFlatEncoding.Type)type)
            {
                case BFlatEncoding.Type.String:
                case BFlatEncoding.Type.Binary:
                    for (int i = 0; i < elementCount; ++i)
                    {
                        int length = (int)(Leb128.decodeUnsigned(this));
                        value.setStringOffsetAndLen(i, position, length);
                        position += length;
                    }
                    break;
                case BFlatEncoding.Type.Leb128:
                    for (int i = 0; i < elementCount; ++i)
                    {
                        long leb128 = Leb128.decodeSigned(this);
                        value.setLeb128Value(i, leb128);
                    }
                    break;
                case BFlatEncoding.Type.Int8:
                    position += elementCount;
                    break;
                case BFlatEncoding.Type.Int16:
                    position += elementCount * 2;
                    break;
                case BFlatEncoding.Type.Int32:
                    position += elementCount * 4;
                    break;
                case BFlatEncoding.Type.Int64:
                case BFlatEncoding.Type.Double:
                case BFlatEncoding.Type.Datetime:
                    position += elementCount * 8;
                    break;
                case BFlatEncoding.Type.Null:
                    break;
                default:
                    throw new BFlatException("unknown value type", position);
            }
            return value;
        }

        /// <summary>
        /// The current value parsed by this BFlatParser.
        /// </summary>
        public BFlatValue Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// The current value parsed by this BFlatParser.
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Returns self as an enumerator over the parsed BFlat values.
        /// </summary>
        /// <returns>Self.</returns>
        public IEnumerator<BFlatValue> GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Returns self as an enumerator over the parsed BFlat values.
        /// </summary>
        /// <returns>Self.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Frees all resources associated with self.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Moves this parser to the next BFlatValue in the supplied BFlat
        /// message.
        /// </summary>
        /// <returns>true if another value is found, false otherwise.</returns>
        public bool MoveNext()
        {
            _current = parseNext();
            return _current != null;
        }

        /// <summary>
        /// Rewinds this parser to the beginning of the BFlat document.
        /// </summary>
        public void Reset()
        {
            _current = null;
            rewind();
        }
        BFlatValue _current;
        int _end;
    }

}
