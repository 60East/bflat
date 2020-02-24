#!/usr/bin/env python
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
import os, glob, sys
from distutils.core import setup
from distutils.extension import Extension
from distutils.sysconfig import get_config_var

if get_config_var("OPT") is not None:
  os.environ["OPT"] = get_config_var("OPT").replace("-Wstrict-prototypes","")

if sys.platform == "win32":
    if "VS90COMNTOOLS" not in os.environ:
        if "VS100COMNTOOLS" in os.environ:
            os.environ["VS90COMNTOOLS"] = os.environ["VS100COMNTOOLS"]
        elif "VS110COMNTOOLS" in os.environ:
            os.environ["VS90COMNTOOLS"] = os.environ["VS110COMNTOOLS"]
        elif "VS120COMNTOOLS" in os.environ:
            os.environ["VS90COMNTOOLS"] = os.environ["VS120COMNTOOLS"]
        else:
          print >>sys.stderr, "The Visual Studio environment variables do not appear to be set. Re-run this setup script from a Visual Studio Command Prompt."
          exit(-1)
else:
    compiler_args=["-std=c++11", "-ggdb"]

setup(name='bflat-python',
      description='BFlat Python Module',
      version='develop',
      maintainer='60East Technologies, Incorporated',
      maintainer_email='support@crankuptheamps.com',
      url='http://crankuptheamps.com',
      packages=['bflat'],
      package_dir= {'' : 'src'},
      ext_modules=[Extension('bflat._bflat_native',
          ['src/bflat_python.cpp'], extra_compile_args=compiler_args)]
      )




