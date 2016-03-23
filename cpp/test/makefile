GTEST_DIR=googletest-master/googletest
TARGET=bflat_tests
OBJECTS=test_leb128.o test_parser.o test_serializer.o
CXXFLAGS=-O0 -g -std=c++11 -I$(GTEST_DIR)/include -I../include
LDFLAGS= -L$(GTEST_DIR)/build -lgtest_main -lgtest -lpthread
$(TARGET): $(OBJECTS)
	g++ -o $@ $(OBJECTS) $(LDFLAGS)
clean:
	rm -f $(OBJECTS) $(TARGET)
