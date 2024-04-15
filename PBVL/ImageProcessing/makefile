CPP     = g++
SWIG    = /c/Users/adrian/Downloads/swigwin-2.0.11/swig.exe
BASENAME= $(shell basename $(CURDIR))
TARGET  = main.exe

CPP_SRC	= main.cpp imageutilscstyle.cpp imagecommon.cpp imagepostprocessing.cpp
#CPP_SRC = $(filter-out stdafx.cpp, $(wildcard *.cpp))
CPP_OBJ = $(CPP_SRC:.cpp=.o)

CXX_SRC = $(wildcard *.cxx)
CXX_OBJ = $(CXX_SRC:.cxx=.o)

CCFLAGS = -Wall -Werror -O3 \
		  -I. \
		  -I../../Externals/OpenCv/include
LDFLAGS = -static-libstdc++ \
		  -mwindows \
		  -static \
		  -L../../Externals/OpenCv/x86/mingw/lib

#LIBS	= -lopencv_core246 
LIBS	= -lopencv_calib3d246 \
		  -lopencv_features2d246 \
		  -lopencv_flann246 \
		  -lopencv_imgproc246 \
		  -lopencv_core246 \
		  -lopencv_highgui246 \
		  -lopencv_imgproc246 \
		  -llibtiff \
		  -llibpng \
		  -llibjpeg \
		  -llibjasper \
		  -lIlmImf \
		  -lzlib

.PHONY: all clean distclean 

all: $(TARGET)

$(TARGET): $(CPP_OBJ) $(CXX_OBJ)
	$(CPP) $(LDFLAGS) -o $@ $^ $(LIBS)

$(CPP_OBJ): %.o: %.cpp
	$(CPP) $(CCFLAGS) -o $@ -c $<

clean:
	rm -f *~ *.o *wrap.cxx *.cs $(TARGET)

distclean: clean

