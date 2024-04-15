#pragma once

namespace sk { namespace pbvl { namespace image { namespace managed {

public ref class IImage abstract
{
public:
	IImage() { }
	IImage(void* _data, int _width, int _height) : data(_data), width(_width), height(_height) { }

protected:
	void* data;
	int width;
	int height;

};

public ref class Frame : public IImage
{
	Frame(void* _data, int _width, int _height) : IImage(_data, _width, _height) { }
};

}}}} //namespace sk { namespace pbvl { namespace image { namespace managed {