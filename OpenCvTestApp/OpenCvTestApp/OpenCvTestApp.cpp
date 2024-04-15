// OpenCvTestApp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <windows.h>

#include <iostream>
#include <vector>

#include<opencv2/opencv.hpp>
#include "opencv2/imgproc/imgproc.hpp"
#include <opencv2/highgui/highgui.hpp>
//#include <opencv2/highgui/highgui.h>

#include <ImageProcessing/ImageUtils.h>
#include <ImageProcessing/ImageUtilsCStyle.h>
#include <ImageProcessing/ImagePostProcessing.h>

#include "mixedpoisson.h"

using namespace std;


void CreatePhotoStrip(vector<cv::Mat>& images, cv::Mat& strip, int imgWidth, int imgHeight, int margin)
{
	int x = margin;
	int y = margin;
	
	cv::Rect stripRoiRec;
	cv::Mat stripRoi;

	cv::Mat logo = images.back();
	images.pop_back();

	for(int i = 0; i < 2 ; i++)
	{
		int j = 0;

		for(j = 0; j < 1; j++)
		{
			stripRoiRec = cv::Rect(x, y, imgWidth, imgHeight);
			stripRoi = strip(stripRoiRec);

			cv::Mat image = images[j];
			image.copyTo(stripRoi);
		
			y += imgHeight + margin;

			// add logo
			stripRoiRec = cv::Rect(x, y, imgWidth, logo.rows);
			stripRoi = strip(stripRoiRec);
			logo.copyTo(stripRoi);

			y += logo.rows + margin;
		}

		for(j = 0; j < 3; j++)
		{
			stripRoiRec = cv::Rect(x, y, imgWidth, imgHeight);
			stripRoi = strip(stripRoiRec);

			cv::Mat image = images[j];
			image.copyTo(stripRoi);
		
			y += imgHeight + margin;
		}

		y = margin;
		x = imgWidth + 2 * margin;
	}
}

void TryPoisonBlend()
{
	cv::Mat bg, frame;

	bg = cv::imread("c:\\home\\workspace\\repos\\photobooth\\backgrounds\\green_txt.jpg", CV_LOAD_IMAGE_COLOR);
	frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\24_7_2014_13_51_29\\0\\IMG_0003.JPG", CV_LOAD_IMAGE_COLOR);



	cv::Rect frameRec = cv::Rect(880, 156, frame.rows - 880, frame.cols - 156);
	cv::Mat frameRoi = frame(frameRec);
	//IplImage* src =cvCloneImage(&(IplImage)bg);
	//IplImage* overlay =cvCloneImage(&(IplImage)frame);

	IplImage src = bg;
	IplImage overlay = frameRoi;

	IplImage* result = poisson_blend(&src, &overlay, 0, 0);

	cv::Mat matResult =  cvarrToMat(result);

	
	cv::namedWindow("PoisonBlend", cv::WINDOW_NORMAL);
	cv::imshow("PoisonBlend", matResult);

	//delete src;
	//delete overlay;
	delete result;
}

cv::Mat HistEqOnColorImg(cv::Mat& img)
{
	cv::vector<cv::Mat> channels; 
    cv::Mat img_hist_equalized;
    cvtColor(img, img_hist_equalized, CV_BGR2YCrCb); //change the color image from BGR to YCrCb format

    split(img_hist_equalized,channels); //split the image into channels
    equalizeHist(channels[0], channels[0]); //equalize histogram on the 1st channel (Y)

    merge(channels,img_hist_equalized); //merge 3 channels including the modified 1st channel into one image
    cvtColor(img_hist_equalized, img_hist_equalized, CV_YCrCb2BGR); //change the color image from YCrCb to BGR format (to display image properly)


	cv::namedWindow("normal", cv::WINDOW_NORMAL);
	cv::imshow("normal", img);
	cv::namedWindow("histEq", cv::WINDOW_NORMAL);
	cv::imshow("histEq", img_hist_equalized);
	

	return img_hist_equalized;
}

void Normalize(cv::Mat frame)
{

	cv::normalize(frame, frame, 0, 255, cv::NORM_MINMAX, CV_8UC3);

	// White balance correction
	/*std::vector<cv::Mat> channels;
	cv::split(frame, channels);
	for(std::vector<cv::Mat>::iterator channel = channels.begin(); channel != channels.end(); channel++)
	{
		cv::normalize(*channel, *channel, 0, 255, cv::NORM_MINMAX, CV_8UC3);
	}
	cv::merge(channels, frame);*/

	cv::namedWindow("frame", cv::WINDOW_NORMAL);
	cv::imshow("frame", frame);
}

void NormalizeSplit(cv::Mat frame)
{

	//White balance correction
	std::vector<cv::Mat> channels;
	cv::split(frame, channels);
	for(std::vector<cv::Mat>::iterator channel = channels.begin(); channel != channels.end(); channel++)
	{
		cv::normalize(*channel, *channel, 0, 255, cv::NORM_MINMAX, CV_8UC1);
	}
	cv::merge(channels, frame);

	cv::namedWindow("frameSplit", cv::WINDOW_NORMAL);
	cv::imshow("frameSplit", frame);
}

inline int _tmain(int argc, _TCHAR* argv[])
{
	//cv::Mat image = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\18_7_2014_21_07_44\\0\\IMG_0001.JPG", CV_LOAD_IMAGE_COLOR);
	//cv::Mat image = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\24_7_2014_13_51_29\\0\\IMG_0003.JPG", CV_LOAD_IMAGE_COLOR);
	//HistEqOnColorImg(image);
	//Normalize(image);
	//NormalizeSplit(image);

	//cv::waitKey();
	//return 0;

	//vector<cv::Mat> images;
	//cv::Mat procesed = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\Temp\\IMG_0003.JPG", CV_LOAD_IMAGE_COLOR);
	//
	//const int margin = 10;
	//const int imgWidth = 584;
	//const int imgHeight = 388;

	//const int logoWidth = imgWidth;
	//const int logoHeight = 188;
	//{
	//	cv::Mat imgResized(imgHeight, imgWidth,  procesed.type());
	//	cv::resize(procesed, imgResized, imgResized.size());

	//	images.push_back(imgResized); /* header */
	//	images.push_back(imgResized);
	//	images.push_back(imgResized);
	//	images.push_back(imgResized);
	//}
	//{
	//	/* add logo */
	//	cv::Mat imgResized(logoHeight, logoWidth,  procesed.type());
	//	cv::resize(procesed, imgResized, imgResized.size());

	//	images.push_back(imgResized);
	//}
	//cv::Mat strip(1800, 1200, procesed.type());
	//strip.setTo(cv::Scalar(255, 255, 255)); // fill strip with white color


	////sk::pbvl::image::posprocessing::PreparePhotoStrip(images, strip, imgWidth, imgHeight, margin);
	//CreatePhotoStrip(images, strip, imgWidth, imgHeight, margin);


	//cv::imwrite("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\Temp\\PhotoStrip.JPG", strip);

	//cv::namedWindow("strip", cv::WINDOW_NORMAL);
	//cv::imshow("strip", strip);
	//cv::waitKey();
	//return 0;
	
	enum
	{
		PI_CHROMAKEY    = 1,
		PI_BACKGROUND   = 2,
		PI_FOREGROUND   = 3,
		PI_STREAM_GRAY  = 4,
		PI_STREAM_MIRROR= 5, //apply mirror to input stream
		PI_STREAM_SUPER = 6, //output image in super size
		PI_FOREGROUND_SEC = 7,

		PI_STREAM_CROP_TOP     = 50,
		PI_STREAM_CROP_RIGHT   = 51,
		PI_STREAM_CROP_BOTTOM  = 52,
		PI_STREAM_CROP_LEFT    = 53,
		/*PhHTresholdLow" value="32" />
    <add key="PhHTresholdUp" value="80" />
    <add key="PhSTresholdLow" value="100" />
    <add key="PhSTresholdUp" value="227" />
    <add key="PhVTresholdLow" value="15" />
    <add key="PhVTresholdUp" value="250" />*/


		PI_HSV_H_TRESHOLD_LOW   = 100,
		PI_HSV_H_TRESHOLD_UP    = 101,
		PI_HSV_S_TRESHOLD_LOW   = 102,
		PI_HSV_S_TRESHOLD_UP    = 103,
		PI_HSV_V_TRESHOLD_LOW   = 104,
		PI_HSV_V_TRESHOLD_UP    = 105,
	};

	cv::Mat frame, frameHsv, frameCopy, fg, bg, bgHsv;
	//bg = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\Temp\\hollywood.jpg", CV_LOAD_IMAGE_UNCHANGED);

	//bg = cv::imread("C:\\home\\workspace\\repos\\photobooth\\PB\\App\\bin\\Release\\Img\\bg\\blurry_city_lights.jpg ", CV_LOAD_IMAGE_COLOR);
	bg = cv::imread("c:\\home\\workspace\\repos\\photobooth\\backgrounds\\konfeti_titles_bg.jpg", CV_LOAD_IMAGE_COLOR);
	
	//frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\from_stefan\\D4S9323.jpg", CV_LOAD_IMAGE_COLOR);
	//frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\18_7_2014_21_07_44\\0\\IMG_0001.JPG ", CV_LOAD_IMAGE_COLOR);
	//frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\18_7_2014_21_07_44\\0\\IMG_0001.JPG ", CV_LOAD_IMAGE_COLOR);
	
	//frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\17_7_2014_09_31_35\\0\\IMG_0001.JPG", CV_LOAD_IMAGE_COLOR);
	//frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\12_5_2014_21_33_38\\0\\IMG_0004.JPG", CV_LOAD_IMAGE_COLOR);
	frame = cv::imread("c:\\Users\\Lukasz\\Pictures\\RemotePhoto\\08_8_2014_18_26_45\\1\\IMG_0006.JPG", CV_LOAD_IMAGE_COLOR);
	
	
	int lowerH=52;
	int upperH=80;

	int lowerS=100;
	int upperS=180;
	
	int lowerV=55;
	int upperV=150;

	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_H_TRESHOLD_LOW, lowerH);
	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_H_TRESHOLD_UP, upperH);
	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_S_TRESHOLD_LOW, lowerS);
	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_S_TRESHOLD_UP, upperS);
	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_V_TRESHOLD_LOW, lowerV);
	sk::pbvl::image::common::SetProcessImageHint(PI_HSV_V_TRESHOLD_UP, upperV);

	sk::pbvl::image::common::SetProcessImageHint(PI_STREAM_MIRROR, 1);
	sk::pbvl::image::common::SetProcessImageHint(PI_CHROMAKEY, 1);
	//sk::pbvl::image::common::SetProcessImageHint(PI_STREAM_GRAY, 1);

	sk::pbvl::image::posprocessing::ProcessPhoto(frame, bg);

	cv::namedWindow("Frame", cv::WINDOW_NORMAL);
	//cv::namedWindow("Bg", cv::WINDOW_NORMAL);
	cv::imshow("Frame", frame);
	//cv::imshow("Bg", bg);
	cv::imwrite("c:\\home\\workspace\\repos\\photobooth\\Demo\\IMG_0006_konfeti_titles_bg.jpg", frame);
	cv::waitKey();
	return 0;
}

