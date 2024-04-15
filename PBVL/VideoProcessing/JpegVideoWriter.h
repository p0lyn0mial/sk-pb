#pragma once

#include <Support/IWriter.h>
#include "IVideoWriter.h"

namespace sk { namespace pbvl { namespace video { namespace processing {

class JpegVideoWriter : public IVideoWriter
{
public:
	JpegVideoWriter(sk::pbvl::support::IWriter& writer_, int quality, int commpresionLevel);
	virtual bool Write(const cv::Mat& frame);
	virtual ~JpegVideoWriter();

private:

	sk::pbvl::support::IWriter& writer;
	std::vector<int> conversionParams;
};

}}}} // namespace sk { namespace pbvl { namespace video { namespace processing {