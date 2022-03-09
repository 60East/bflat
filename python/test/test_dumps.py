##
## Copyright (c) 2016 60East Technologies Inc., All Rights Reserved.
##
## Permission is hereby granted, free of charge, to any person obtaining
## a copy of this software and associated documentation files (the
## "Software"), to deal in the Software without restriction, including
## without limitation the rights to use, copy, modify, merge, publish,
## distribute, sublicense, and/or sell copies of the Software, and to
## permit persons to whom the Software is furnished to do so, subject to
## the following conditions:
##
## The above copyright notice and this permission notice shall be
## included in all copies or substantial portions of the Software.
##
## THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
## EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
## MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
## NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
## LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
## OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
## WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
##
import bflat
import sys
import unittest

if sys.version_info[0] < 3:
    MAX = sys.maxint
else:
    MAX = sys.maxsize


class TestBflatDumps(unittest.TestCase):
    def setup(self):
        pass

    def test_encode_ints(self):
        intarray = [0, -1, 1, 127, 128, -127, -128, -32767, -32768]
        for x in [[], [-65535, -65536, MAX, -1 * MAX], [-2147483647, -2147483648], [9223372036854775807, -9223372036854775808]]:
            intarray.extend(x)
            data = dict([(str(i), intarray[i]) for i in range(len(intarray))])
            encoded = bflat.dumps(data)
            assert bflat.loads(encoded) == data

    def test_encode_doubles(self):
        data = [0.0, -1.0, 1.0, 127.0, 128.01, -127.001, -128.0001, -32767.1, -32768.1, -65535.01, -65536.001, MAX - 0.001, -1.01 * MAX]
        data = dict([(str(i), data[i]) for i in range(len(data))])
        encoded = bflat.dumps(data)
        assert bflat.loads(encoded) == data

    def test_encode_strings(self):
        data = ["", "\"", "zz", "zzz", "zzzz", "\x00\x01\x02dddd", "zzz", ""]
        data = dict([(str(i), data[i]) for i in range(len(data))])
        encoded = bflat.dumps(data)
        assert bflat.loads(encoded) == data

    def test_encode_int_array(self):
        intarray = [0, -1, 1, 127, 128, -127, -128, -32767, -32768]
        for x in [[], [-65535, -65536, MAX, -1 * MAX], [-2147483647, -2147483648], [9223372036854775807, -9223372036854775808]]:
            intarray.extend(x)
            data = {"values": intarray}
            encoded = bflat.dumps(data)
            assert bflat.loads(encoded) == data

    def test_encode_double_array(self):
        data = [0.0, -1.0, 1.0, 127.0, 128.01, -127.001, -128.0001, -32767.1, -32768.1, -65535.01, -65536.001, MAX - 0.001, -1.01 * MAX]
        data = {"values": data}
        encoded = bflat.dumps(data)
        assert bflat.loads(encoded) == data

    def test_encode_string_array(self):
        data = ["", "\"", "zz", "zzz", "zzzz", "\x00\x01\x02dddd", "zzz", ""]
        data = {"values": data}
        encoded = bflat.dumps(data)
        assert bflat.loads(encoded) == data

    def test_big_strings(self):
        for i in range(16):
            data = {"t" * (2**i): "d" * (2**i)}
            assert bflat.loads(bflat.dumps(data)) == data

    def test_unicode(self):
        data = {"data": [u'xxx', u'', u'12345']}
        expected = {"data": ['xxx', '', '12345']}
        assert expected == bflat.loads(bflat.dumps(data))

    def test_bools(self):
        data = {"value_1": True, "value_2": False}
        expected = {"value_1": 1, "value_2": 0}
        assert expected == bflat.loads(bflat.dumps(data))


if __name__ == '__main__':
    unittest.main()
