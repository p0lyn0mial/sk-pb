#pragma once

#include <opencv2/opencv.hpp>

namespace sk { namespace pbvl { namespace video { namespace processing {

class IVideoWriter
{
public:
	virtual bool Write(const cv::Mat& frame) = 0;
	virtual ~IVideoWriter() {};
};

}}}} // namespace sk { namespace pbvl { namespace video { namespace processing {