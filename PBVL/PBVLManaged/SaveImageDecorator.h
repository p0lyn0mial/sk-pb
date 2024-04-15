#pragma once

#include "IImageDecorator.h"

namespace sk { namespace pbvl { namespace image { namespace managed {

public ref class SaveImageDecorator : public IImageDecorator
{
public:
	SaveImageDecorator(const IImage^ _image) : IImageDecorator(_image) { }
	virtual bool Process() override;
};

}}}} //namespace sk { namespace pbvl { namespace image { namespace managed {
