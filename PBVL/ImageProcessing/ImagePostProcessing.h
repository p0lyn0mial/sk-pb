#pragma once

#include <opencv2/opencv.hpp>
#include <vector>

namespace sk { namespace pbvl { namespace image { namespace posprocessing {

	void ProcessPhoto(cv::Mat& frame, cv::Mat& bg);
	void PreparePhotoStrip(const std::vector<cv::Mat>& images, cv::Mat& strip, int imgWidth, int imgHeight, int margin);

}}}} // namespace sk { namespace pbvl { namespace image { namespace posprocessing {
