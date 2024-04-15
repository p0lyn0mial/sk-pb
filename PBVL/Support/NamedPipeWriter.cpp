#include "NamedPipeWriter.h"

#include <boost/log/trivial.hpp>

namespace sk { namespace pbvl { namespace support {

NamedPipeWriter::NamedPipeWriter(const std::string& pipeName, DWORD timeOut_) 
	: clientConnected(false), pipe(nullptr), timeOut(timeOut_)
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter ctor called.";
	BOOST_LOG_TRIVIAL(debug)  << "Creating named pipe. Pipe name = " << pipeName;

    pipe = CreateNamedPipe(pipeName.c_str(), PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED, PIPE_TYPE_BYTE,
        1, /* allow 1 instance of this pipe*/ 0, /*no outbound buffer*/ 0, /*no inbound buffer*/
        timeOut, /*time wait*/ NULL /*use default security attributes*/
    );

	if (pipe == NULL || pipe == INVALID_HANDLE_VALUE) 
	{
        BOOST_LOG_TRIVIAL(error) << "Failed to create named pipe instance. Error = " << GetLastError();
		throw std::exception("Failed to create named pipe instance.");
    }

	BOOST_LOG_TRIVIAL(info) << "Named pipe instance created successfully.";
}

bool NamedPipeWriter::Write(const void* data, int size)
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter::Write called.";

	if(clientConnected)
	{
		BOOST_LOG_TRIVIAL(debug) << "Client connected calling WriteInternal method.";
		clientConnected =  WriteInternal(data, size);
		return clientConnected;
	}

	BOOST_LOG_TRIVIAL(debug) << "Client NOT connected. Waiting for client to connect.";
	if(WaitForClientToConnect())
	{
		clientConnected = WriteInternal(data, size);
		return clientConnected;
	}

	BOOST_LOG_TRIVIAL(error) << "Can not write data to the name pipe. Client NOT connected.";
	return false;
}

bool NamedPipeWriter::WriteInternal(const void* data, int size) const
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter::WriteInternal called.";

	DWORD numBytesWritten = 0;
	BOOL result = WriteFile(pipe, data, size, &numBytesWritten, NULL);

	if (result) 
	{
		BOOST_LOG_TRIVIAL(info) << "Successfully wrote " << numBytesWritten <<" bytes to the named pipe.";
	} 
	else
	{
		BOOST_LOG_TRIVIAL(info) << "Error writing data to the named pipe. Error = " << GetLastError();
		DisconnectClient();
	}

	return result != 0;
}

bool NamedPipeWriter::WaitForClientToConnect() const
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter::WaitForClientToConnect called.";

	OVERLAPPED ol = {0,0,0,0,NULL};
	BOOL ret = 0;
 
	ol.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	ret = ConnectNamedPipe(pipe, &ol);

	if(ret == 0) 
	{
		switch(GetLastError()) 
		{
		    case ERROR_PIPE_CONNECTED:
				{
					BOOST_LOG_TRIVIAL(info) << "Client connected to the name pipe instance.";
					ret = TRUE;
					break;
				}
		    case ERROR_IO_PENDING:
				{
					BOOST_LOG_TRIVIAL(info) << "Waiting " << timeOut << " s. for client to connect.";

					if( WaitForSingleObject(ol.hEvent, timeOut) == WAIT_OBJECT_0 ) 
					{
						DWORD dwIgnore;
						ret = GetOverlappedResult(pipe, &ol, &dwIgnore, FALSE);
					} 
					else 
					{
						CancelIo(pipe);
					}
				}
			default:
				{
					break;
				}
		}
	}
	CloseHandle(ol.hEvent);
	return ret != 0;
}

void NamedPipeWriter::DisconnectClient() const
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter::DisconnectClient called.";

	BOOST_LOG_TRIVIAL(info) << "Disconnecting client from the named pipe.";
	FlushFileBuffers(pipe);
    if (!DisconnectNamedPipe(pipe)) 
	{
		BOOST_LOG_TRIVIAL(error) <<  "Disconnecting client failed. Error = "<< GetLastError() ;
    } 
	else 
	{
		BOOST_LOG_TRIVIAL(info) << "Disconnecting client completed successfuly";
    }
}

NamedPipeWriter::~NamedPipeWriter()
{
	BOOST_LOG_TRIVIAL(debug) << "NamedPipeWriter dtor called. Closing named pipe instance.";

	DisconnectClient();
	BOOL ret = CloseHandle(pipe); 

	BOOST_LOG_TRIVIAL(debug) << "Named pipe closing status = " << ret;
}

}}} // namespace sk { namespace pbvl { namespace support {