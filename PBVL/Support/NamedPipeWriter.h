#pragma once

#include "IWriter.h"

#include <string>
#include <Windows.h>

namespace sk { namespace pbvl { namespace support {

class NamedPipeWriter : public IWriter
{
public:
	NamedPipeWriter(const std::string& pipeName, DWORD timeOut_ = 10000 /*10 s.*/);
	virtual bool Write(const void* data, int size);
	virtual ~NamedPipeWriter();

private:
	NamedPipeWriter(const NamedPipeWriter&);
	NamedPipeWriter& operator=(const NamedPipeWriter&);
	NamedPipeWriter(NamedPipeWriter&&);
	NamedPipeWriter& operator=(NamedPipeWriter&&);

	bool WaitForClientToConnect() const;
	bool WriteInternal(const void* data, int size) const;
	void DisconnectClient() const;

	HANDLE pipe;
	bool clientConnected;
	DWORD timeOut;
};

}}} // namespace sk { namespace pbvl { namespace support {