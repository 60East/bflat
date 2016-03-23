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
def test_encode_ints():
    data = [0,-1,1,127,128,-127,-128,-32767,-32768,-65535,-65536,sys.maxint, -1*sys.maxint]
    data = dict([(str(i),data[i]) for i in range(len(data))])
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_encode_doubles():
    data = [0.0,-1.0,1.0,127.0,128.01,-127.001,-128.0001,-32767.1,-32768.1,-65535.01,-65536.001,sys.maxint - 0.001, -1.01*sys.maxint]
    data = dict([(str(i),data[i]) for i in range(len(data))])
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_encode_strings():
    data = ["","\"","zz","zzz","zzzz","\x00\x01\x02dddd","zzz",""]
    data = dict([(str(i),data[i]) for i in range(len(data))])
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_encode_int_array():
    data = [0,-1,1,127,128,-127,-128,-32767,-32768,-65535,-65536,sys.maxint, -1*sys.maxint]
    data = {"values":data}
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_encode_double_array():
    data = [0.0,-1.0,1.0,127.0,128.01,-127.001,-128.0001,-32767.1,-32768.1,-65535.01,-65536.001,sys.maxint - 0.001, -1.01*sys.maxint]
    data = {"values":data}
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_encode_string_array():
    data = ["","\"","zz","zzz","zzzz","\x00\x01\x02dddd","zzz",""]
    data = {"values":data}
    encoded = bflat.dumps(data)
    assert bflat.loads(encoded) == data
def test_big_strings():
    for i in xrange(16):
        data = {"t"*(2**i):"d"*(2**i)}
        assert bflat.loads(bflat.dumps(data)) == data


def test_unicode():
    data = {"data":[u'xxx',u'',u'12345']}
    expected = {"data":['xxx','','12345']}
    assert expected == bflat.loads(bflat.dumps(data))

def test_bools():
    data = {"value_1":True,"value_2":False}
    expected = {"value_1":1, "value_2":0}
    assert expected == bflat.loads(bflat.dumps(data))



