#ifndef IMAGECOMMON_H
#define IMAGECOMMON_H

namespace sk { namespace pbvl { namespace image { namespace common {

#define byte unsigned char

class pixel {
    public:
        pixel() { }
        void assign3b(byte* raw) {              r = raw + 2; g = raw + 1; b = raw; }
        void assign4b(byte* raw) { a = raw + 3; r = raw + 2; g = raw + 1; b = raw; }
        byte *r;
        byte *g;
        byte *b;
        byte *a;
};

//struct pixel {
//    byte *r;
//    byte *g;
//    byte *b;
//};

extern short int pi_chromakey;
extern short int pi_background;
extern short int pi_foreground;
extern short int pi_foreground_sec;
extern short int pi_stream_gray;
extern short int pi_stream_mirror;
extern short int pi_stream_super;

extern short int pi_stream_crop_top;
extern short int pi_stream_crop_right;
extern short int pi_stream_crop_bottom;
extern short int pi_stream_crop_left;

extern short int lowerH;
extern short int upperH;
extern short int lowerS; 
extern short int upperS;
extern short int lowerV; 
extern short int upperV;

enum
{
    PI_CHROMAKEY    = 1,
    PI_BACKGROUND   = 2,
    PI_FOREGROUND   = 3,
    PI_STREAM_GRAY  = 4,
    PI_STREAM_MIRROR= 5, //apply mirror to input stream
    PI_STREAM_SUPER = 6, //output image in super size
    PI_FOREGROUND_SEC = 7,

    PI_STREAM_CROP_TOP     = 50,
    PI_STREAM_CROP_RIGHT   = 51,
    PI_STREAM_CROP_BOTTOM  = 52,
    PI_STREAM_CROP_LEFT    = 53,

    PI_HSV_H_TRESHOLD_LOW   = 100,
    PI_HSV_H_TRESHOLD_UP    = 101,
    PI_HSV_S_TRESHOLD_LOW   = 102,
    PI_HSV_S_TRESHOLD_UP    = 103,
    PI_HSV_V_TRESHOLD_LOW   = 104,
    PI_HSV_V_TRESHOLD_UP    = 105,
};


bool IsProcessImageHintSet(short int what);

short int GetProcessImageHint(short int what);

void SetProcessImageHint(short int what, int value);

}}}}

#endif //IMAGECOMMON_H

