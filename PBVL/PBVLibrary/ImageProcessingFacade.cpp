#include "stdafx.h"
#include "ImageProcessingFacade.h"

#include <ImageProcessing/ImageUtils.h>
#include <ImageProcessing/ImageUtilsCStyle.h>
#include <ImageProcessing/ImagePostProcessing.h>

namespace sk { namespace pbvl { namespace image { namespace processing { namespace facade {

void SetProcessImageHint(int what, int value)
{
    sk::pbvl::image::common::SetProcessImageHint(what, value);
}

bool ProcessFrame(
            void* bgData, int bgWidth, int bgHeight,
            void* sgData, int sgWidth, int sgHeight,
            void* fgData, int fgWidth, int fgHeight,
			void* fgDataSec, int fgWidthSec, int fgHeightSec,
            void* oData)
{
	try
	{
		sk::pbvl::image::processing::cstyle::ProcessImage(
				(byte*)bgData,  bgWidth,  bgHeight,
				(byte*)sgData,  sgWidth,  sgHeight,
				(byte*)fgData,  fgWidth,  fgHeight, 
				(byte*) fgDataSec, fgWidthSec, fgHeightSec,
				(byte*)oData);

		return true;
	}
	catch(const std::exception&)
	{
		return false;
	}

	
}

bool ProcessPhoto(
            void* photoData, int photoWidth, int photoHeight,
			void* bgData, int bgWidth, int bgHeight)
{
	try
	{
		cv::Mat photo(photoHeight, photoWidth, CV_8UC3, photoData);
		cv::Mat bg(bgHeight, bgWidth, CV_8UC3, bgData);

		sk::pbvl::image::posprocessing::ProcessPhoto(photo, bg);

		return true;
	}
	catch(const std::exception&)
	{
		return false;
	}

}


bool PreparePhotoStrip(
            void* imgOne, int imgOneWidth, int imgOneHeight,
			void* imgTwo, int imgTwoWidth, int imgTwoHeight,
			void* imgThree, int imgThreeWidth, int imgThreeHeight,
			void* imgFour, int imgFourWidth, int imgFourHeight,
			void* imgLogo, int imgLogoWidth, int imgLogoHeight,
            void* imgStrip, int imgStripWidth, int imgStripHeight)
{
	try
	{
		const int margin = 10;
		const int imgWidth = 584;
		const int imgHeight = 388;

		cv::Mat imgOne(imgOneHeight, imgOneWidth, CV_8UC3, imgOne);
		cv::Mat imgTwo(imgTwoHeight, imgTwoWidth, CV_8UC3, imgTwo);
		cv::Mat imgThree(imgThreeHeight, imgThreeWidth, CV_8UC3, imgThree);
		cv::Mat imgFour(imgFourHeight, imgFourWidth, CV_8UC3, imgFour);
		cv::Mat imgLogo(imgLogoHeight, imgLogoWidth, CV_8UC3, imgLogo);
		cv::Mat strip(imgStripHeight, imgStripWidth, CV_8UC3, imgStrip);
		
		std::vector<cv::Mat> images;
		{
			int imgType = imgOne.type();
			cv::Mat imgOneResized(imgHeight, imgWidth,  imgType);
			cv::resize(imgOne, imgOneResized, imgOneResized.size());
			images.push_back(imgOneResized);

			cv::Mat imgTwoResized(imgHeight, imgWidth,  imgType);
			cv::resize(imgTwo, imgTwoResized, imgTwoResized.size());
			images.push_back(imgTwoResized);
			
			cv::Mat imgThreeResized(imgHeight, imgWidth,  imgType);
			cv::resize(imgThree, imgThreeResized, imgThreeResized.size());
			images.push_back(imgThreeResized);

			cv::Mat imgFourResized(imgHeight, imgWidth,  imgType);
			cv::resize(imgFour, imgFourResized, imgFourResized.size());
			images.push_back(imgFourResized);

			images.push_back(imgLogo);
		}

		/* fill strip with white color */
		strip.setTo(cv::Scalar(255, 255, 255));

		sk::pbvl::image::posprocessing::PreparePhotoStrip(images, strip, imgWidth, imgHeight, margin);

		return true;
	}
	catch(const std::exception&)
	{
		return false;
	}
}

}}}}} // namespace sk { namespace pbvl { namespace image { namespace processing { namespace facade
