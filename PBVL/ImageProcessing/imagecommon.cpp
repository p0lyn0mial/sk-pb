#include "imagecommon.h"

namespace sk { namespace pbvl { namespace image { namespace common {

    short int pi_chromakey;
    short int pi_background;
    short int pi_foreground;
    short int pi_stream_gray;
    short int pi_stream_mirror;
    short int pi_stream_super;
    short int pi_foreground_sec;

    short int pi_stream_crop_top;
    short int pi_stream_crop_right;
    short int pi_stream_crop_bottom;
    short int pi_stream_crop_left;

    short int lowerH;
    short int upperH;
    short int lowerS; 
    short int upperS;
    short int lowerV; 
    short int upperV;

    short int GetProcessImageHint(short int what)
    {
        switch(what)
        {
            case PI_CHROMAKEY:
                return pi_chromakey;
                break;
            case PI_BACKGROUND:
                return pi_background;
                break;
            case PI_FOREGROUND:
                return pi_foreground;
                break;
            case PI_FOREGROUND_SEC:
                return pi_foreground_sec;
                break;
            case PI_STREAM_GRAY:
                return pi_stream_gray;
                break;
            case PI_STREAM_MIRROR:
                return pi_stream_mirror;
                break;
            case PI_STREAM_SUPER:
                return pi_stream_super;
                break;
            case PI_STREAM_CROP_TOP:
                return pi_stream_crop_top;
                break;
            case PI_STREAM_CROP_RIGHT:
                return pi_stream_crop_right;
                break;
            case PI_STREAM_CROP_BOTTOM:
                return pi_stream_crop_bottom;
                break;
            case PI_STREAM_CROP_LEFT:
                return pi_stream_crop_left;
                break;
            case PI_HSV_H_TRESHOLD_LOW:
                return lowerH;
                break;
            case PI_HSV_H_TRESHOLD_UP:
                return upperH;
                break;
            case PI_HSV_S_TRESHOLD_LOW:
                return lowerS;
                break;
            case PI_HSV_S_TRESHOLD_UP:
                return upperS;
                break;
            case PI_HSV_V_TRESHOLD_LOW:
                return lowerV;
                break;
            case PI_HSV_V_TRESHOLD_UP:
                return upperV;
                break;
            default:
                return 0;
                break;
        }
    }

    bool IsProcessImageHintSet(short int what)
    {
        return (GetProcessImageHint(what) == 1);
    }

    void SetProcessImageHint(short int what, int value)
    {
        switch(what)
        {
            case PI_CHROMAKEY:
                pi_chromakey = value;
                break;
            case PI_BACKGROUND:
                pi_background = value;
                break;
            case PI_FOREGROUND:
                pi_foreground = value;
                break;
            case PI_FOREGROUND_SEC:
                pi_foreground_sec = value;
                break;
            case PI_STREAM_GRAY:
                pi_stream_gray = value;
                break;
            case PI_STREAM_MIRROR:
                pi_stream_mirror = value;
                break;
            case PI_STREAM_SUPER:
                pi_stream_super = value;
                break;
            case PI_STREAM_CROP_TOP:
                pi_stream_crop_top = value;
                break;
            case PI_STREAM_CROP_RIGHT:
                pi_stream_crop_right = value;
                break;
            case PI_STREAM_CROP_BOTTOM:
                pi_stream_crop_bottom = value;
                break;
            case PI_STREAM_CROP_LEFT:
                pi_stream_crop_left = value;
                break;
            case PI_HSV_H_TRESHOLD_LOW:
                lowerH = value;
                break;
            case PI_HSV_H_TRESHOLD_UP:
                upperH = value;
                break;
            case PI_HSV_S_TRESHOLD_LOW:
                lowerS = value;
                break;
            case PI_HSV_S_TRESHOLD_UP:
                upperS = value;
                break;
            case PI_HSV_V_TRESHOLD_LOW:
                lowerV = value;
                break;
            case PI_HSV_V_TRESHOLD_UP:
                upperV = value;
                break;
        }
    }
}}}}

