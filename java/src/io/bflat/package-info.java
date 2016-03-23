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
/**
 * This package contains classes that let you create and parse BFlat messages.
 * <p>
 *  BFlat is a data-interchange format designed for high-performance
 *  messaging. BFlat is self-describing, and does not support nested elements.
 * </p>
 * <p>Programs use two main classes from this package:
 * <ul>
 * <li>{@link io.bflat.BFlatBuilder} encodes Java values into a BFlat message.</li>
 * <li>{@link io.bflat.BFlatParser} decodes a BFlat message into Java types.</li>
 * </ul>
 * <p>
 *  This package also provides a set of supporting classes for use with the
 *  {@link io.bflat.BFlatBuilder} and {@link io.bflat.BFlatParser}.
 *  
 */

package io.bflat;
