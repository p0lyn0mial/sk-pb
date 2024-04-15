#include "stdafx.h"
#include "JpegVideoWriter.h"

#include <boost/log/trivial.hpp>

namespace sk { namespace pbvl { namespace video { namespace processing {

JpegVideoWriter::JpegVideoWriter(sk::pbvl::support::IWriter& writer_, 
								 int quality, 
								 int commpresionLevel) : writer(writer_)
{
	BOOST_LOG_TRIVIAL(debug) << "JpegVideoWriter ctor called. quality = " << quality << " , commpresionLevel = " << commpresionLevel;
	conversionParams.push_back(quality);
	conversionParams.push_back(commpresionLevel);
}


bool JpegVideoWriter::Write(const cv::Mat& frame)
{
	BOOST_LOG_TRIVIAL(debug) << "JpegVideoWriter::Write called.";

	BOOST_LOG_TRIVIAL(info) << "Converting given frame to jpeg image.";
	std::vector<uchar> buffer;
	cv::imencode(".jpg", frame, buffer, conversionParams);
	
	BOOST_LOG_TRIVIAL(info) << "Writing jpeg image.";
	return writer.Write(buffer.data(), buffer.size());
}

JpegVideoWriter::~JpegVideoWriter(void)
{
	BOOST_LOG_TRIVIAL(debug) << "JpegVideoWriter dtor called.";
}

}}}} // namespace sk { namespace pbvl { namespace video { namespace processing {