// ImageProcessingUT.cpp : Defines the entry point for the console application.
//
#include <iostream>
#include <tchar.h>

#include "gtest/gtest.h"
#include "SampleTest.h"

int _tmain(int argc, _TCHAR* argv[])
{
	testing::InitGoogleTest(&argc, argv);
    RUN_ALL_TESTS();
	return 0;
}

