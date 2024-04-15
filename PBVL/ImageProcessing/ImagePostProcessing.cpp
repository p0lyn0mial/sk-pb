#include "ImagePostProcessing.h"
#include "imagecommon.h"

#include <stdexcept>

namespace sk { namespace pbvl { namespace image { namespace posprocessing {

using namespace sk::pbvl::image::common;

void ProcessPhoto(cv::Mat& frame, cv::Mat& bg)
{
	cv::Mat mask;

	// White balance correction
	/*std::vector<cv::Mat> channels;
	cv::split(frame, channels);
	for(std::vector<cv::Mat>::iterator channel = channels.begin(); channel != channels.end(); channel++)
	{
		cv::normalize(*channel, *channel, 0, 255, cv::NORM_MINMAX, CV_8UC1);
	}
	cv::merge(channels, frame);*/

	// Mirror effect
	if(IsProcessImageHintSet(PI_STREAM_MIRROR))
	{
		cv::flip(frame, frame, 1);
	}

	if(IsProcessImageHintSet(PI_CHROMAKEY))
	{
		cv::Mat bgCopy;
		bg.copyTo(bgCopy);

		cv::Scalar lowerHsv = cv::Scalar(GetProcessImageHint(PI_HSV_H_TRESHOLD_LOW), 
										GetProcessImageHint(PI_HSV_S_TRESHOLD_LOW), 
										GetProcessImageHint(PI_HSV_V_TRESHOLD_LOW));

		cv::Scalar upperHsv = cv::Scalar(GetProcessImageHint(PI_HSV_H_TRESHOLD_UP), 
										GetProcessImageHint(PI_HSV_S_TRESHOLD_UP), 
										GetProcessImageHint(PI_HSV_V_TRESHOLD_UP));
	
		{
			cv::Mat frameHsv;
			cv::cvtColor(frame, frameHsv, CV_BGR2HSV);
			cv::Mat imgThresh;
			cv::inRange(frameHsv, lowerHsv, upperHsv, imgThresh); 
			double blobThreshold = 600;
			double contour_area;
			std::vector<std::vector<cv::Point>> contours;
			std::vector<cv::Vec4i> hierarchy;
			std::vector<int> small_blobs;
			cv::Mat temp_image;
			cv::Mat canny_output;
	
			imgThresh.copyTo(temp_image);
			cv::findContours(temp_image, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE);
			if ( !contours.empty()) 
			{
				for (size_t i=0; i<contours.size(); ++i) 
				{
					contour_area = cv::contourArea(contours[i]) ;
					if ( contour_area < blobThreshold)
						small_blobs.push_back(i);
				}
			}

			// Fill-in all small contours with zeros
			for (size_t i=0; i < small_blobs.size(); ++i) 
			{
				cv::drawContours(imgThresh, contours, small_blobs[i], CV_RGB( 255, 255, 255 ), CV_FILLED, 8);
			}

			int erosion_elem = 1;
			int erosion_size = 1;
			int dilation_elem = 1;
			int dilation_size = 2;
			int const max_elem = 2;
			int const max_kernel_size = 21;

			int erosion_type;
			if( erosion_elem == 0 ){ erosion_type = cv::MORPH_RECT; }
			else if( erosion_elem == 1 ){ erosion_type = cv::MORPH_CROSS; }
			else if( erosion_elem == 2) { erosion_type = cv::MORPH_ELLIPSE; }

			cv::Mat elementEro = cv::getStructuringElement( erosion_type,
										   cv::Size( erosion_size + 1, erosion_size+1 ),
										   cv::Point( erosion_size, erosion_size ) );

			// Apply the erosion operation
			cv::Mat erosion_dst;
			cv::erode( imgThresh, erosion_dst, elementEro );

			int dilation_type = 1;
			if( dilation_elem == 0 ){ dilation_type = cv::MORPH_RECT; }
			else if( dilation_elem == 1 ){ dilation_type = cv::MORPH_CROSS; }
			else if( dilation_elem == 2) { dilation_type = cv::MORPH_ELLIPSE; }

			cv::Mat element = cv::getStructuringElement( dilation_type,
									   cv::Size( dilation_size + 1, dilation_size+1 ),
									   cv::Point( dilation_size, dilation_size ) );
			// Apply the dilation operation
			cv::Mat dilation_dst;
			cv::dilate( erosion_dst, dilation_dst, element );

			mask = cv::Mat::ones(dilation_dst.size(), dilation_dst.type()) * 255 - dilation_dst;
		}
	}

	//
	if(IsProcessImageHintSet(PI_STREAM_GRAY))
	{
		// This is tricky - in the end grayscale image MUST have the same numer of chanels as frame img
		cv::Mat frameGrayOneChanel;
		cv::cvtColor(frame, frameGrayOneChanel, CV_BGR2GRAY);
		
		cv::Mat frameGray;
		cv::cvtColor(frameGrayOneChanel, frameGray, CV_GRAY2BGR);
		frameGray.copyTo(frame);
	}

	// Well if mask is not empty - chroma key was requested
	if(!mask.empty())
	{
		cv::Mat bgCopy;
		bg.copyTo(bgCopy);

		frame.copyTo(bgCopy, mask);
		bgCopy.copyTo(frame);

		// Applay blur corners
		int blurVal = 5;
		cv::Mat frameBlur;
		cv::GaussianBlur(frame, frameBlur, cv::Size(blurVal, blurVal), 0, 0);
		cv::GaussianBlur(mask, mask, cv::Size(blurVal, blurVal), 0, 0);
		cv::inRange(mask, cv::Scalar(1, 1, 1), cv::Scalar(254, 254, 254), mask); 
		frameBlur.copyTo(frame, mask);
	}
}

void PreparePhotoStrip(const std::vector<cv::Mat>& images, cv::Mat& strip, int imgWidth, int imgHeight, int margin)
{
	int x = margin;
	int y = margin;
	
	cv::Rect stripRoiRec;
	cv::Mat stripRoi;
	for(int i = 0; i < 2 ; i++)
	{
		int j = 0;
		for(j = 0; j < 4; j++)
		{
			stripRoiRec = cv::Rect(x, y, imgWidth, imgHeight);
			stripRoi = strip(stripRoiRec);

			cv::Mat image = images[j];
			image.copyTo(stripRoi);
		
			y += imgHeight + margin;
		}

		/* add logo*/
		cv::Mat logo = images[j];
		int row = strip.rows;
		stripRoiRec = cv::Rect(x, y, imgWidth, strip.rows - y - margin);
		stripRoi = strip(stripRoiRec);
		logo.copyTo(stripRoi);

		y = margin;
		x = imgWidth + 2 * margin;
	}
}

}}}} // namespace sk { namespace pbvl { namespace image { namespace posprocessing {
