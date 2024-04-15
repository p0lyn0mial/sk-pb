#include <iostream>
#include "imageutilscstyle.h"
#include "imagepostprocessing.h"

using namespace sk::pbvl::image::processing::cstyle;
using namespace sk::pbvl::image::common;
using namespace sk::pbvl::image::posprocessing;

int main(int argc, char* argv[])
{
    cv::Mat bgImg = cv::imread("bgdata.png");
    cv::Mat fgImg = cv::imread("fgdata.png", -1);
    cv::Mat fg2Img = cv::imread("fgdata2.png", -1);
    cv::Mat sImg = cv::imread("sdata.png");
    cv::Mat oImg(704, 1056, CV_8UC3);
    //cv::Mat oImg(1408, 2112, CV_8UC3);
    //cv::Mat oImg(400, 640, CV_8UC3);
    //cv::Mat oImg(800, 1280, CV_8UC3);

    SetProcessImageHint(PI_HSV_H_TRESHOLD_LOW, 25);
    SetProcessImageHint(PI_HSV_H_TRESHOLD_UP, 95);
    SetProcessImageHint(PI_HSV_S_TRESHOLD_LOW, 50);
    SetProcessImageHint(PI_HSV_S_TRESHOLD_UP, 255);
    SetProcessImageHint(PI_HSV_V_TRESHOLD_LOW, 30);
    SetProcessImageHint(PI_HSV_V_TRESHOLD_UP, 255);

    //SetProcessImageHint(PI_STREAM_CROP_LEFT, 200);
    //SetProcessImageHint(PI_STREAM_CROP_TOP, 100);

    SetProcessImageHint(PI_CHROMAKEY, 1);
    //SetProcessImageHint(PI_FOREGROUND, 1);
    //SetProcessImageHint(PI_FOREGROUND_SEC, 1);
    SetProcessImageHint(PI_BACKGROUND, 1);
    //SetProcessImageHint(PI_STREAM_GRAY, 1);
    //SetProcessImageHint(PI_STREAM_MIRROR, 1);
    //SetProcessImageHint(PI_STREAM_SUPER, 1);

    //for(int i=0; i<1000; i++)
    //for(int i=0; i<1; i++)
    //{
    //    ProcessImage(
    //        sImg.data, 
    //        sImg.cols, sImg.rows, 
    //        bgImg.data,
    //        bgImg.cols, bgImg.rows, 
    //        fgImg.data, 
    //        fgImg.cols, fgImg.rows,
    //        fg2Img.data, 
    //        fg2Img.cols, fg2Img.rows,
    //        oImg.data);
    //}

    int lowerH=32;
    int lowerS=100;
    int lowerV=15;

    int upperH=80;
    int upperS=227;
    int upperV=250;

    cv::Scalar lower = cv::Scalar(lowerH, lowerS, lowerV);
    cv::Scalar upper = cv::Scalar(upperH, upperS, upperV);

    Process(sImg, bgImg, lower, upper);

    cv::imwrite("out.png", sImg);

    return 0;
}
