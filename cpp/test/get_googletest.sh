#!/bin/bash
# Downloads and builds googletest in the place where this project expects to
# find it. Requires cmake, make, g++, wget and unzip.
wget https://github.com/google/googletest/archive/master.zip
unzip master.zip
pushd googletest-master/googletest
mkdir build
cd build
cmake ..
make
popd

