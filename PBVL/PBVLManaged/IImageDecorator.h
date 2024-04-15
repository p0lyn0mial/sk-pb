#pragma once

#include "IImage.h"

namespace sk { namespace pbvl { namespace image { namespace managed {

public ref class IImageDecorator abstract : public IImage
{
public:
	//IImageDecorator(void* _data, int _width, int _height) : IImage(_data, _width, _height) { }
	IImageDecorator(const IImage^ _image) : image(_image) { }
	virtual bool Process() = 0;
protected:
	const IImage^ image;
};

}}}} //namespace sk { namespace pbvl { namespace image { namespace managed {


