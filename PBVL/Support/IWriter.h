#pragma once

namespace sk { namespace pbvl { namespace support {

class IWriter
{
public:
	virtual bool Write(const void* data, int size) = 0;
	virtual ~IWriter() {};
};

}}} // namespace sk { namespace pbvl { namespace support {