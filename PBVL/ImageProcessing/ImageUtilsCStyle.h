#ifndef IMAGEUTILSCSTYLE_H
#define IMAGEUTILSCSTYLE_H

#include <opencv2/opencv.hpp>
#include "imagecommon.h"

namespace sk { namespace pbvl { namespace image { namespace processing { namespace cstyle {

using namespace sk::pbvl::image::common;

void CopyPixel(byte *fromr, byte *fromg, byte *fromb, byte *tor, byte *tog, byte *tob);
void CopyGrayPixel(byte *fromr, byte *fromg, byte *fromb, byte *tor, byte *tog, byte *tob);
void ApplyChromaKey(byte *bR, byte *bG, byte *bB, byte *oR, byte *oG, byte *oB, byte *h, byte *s, byte *v);
void Assign3b(byte **r, byte **g, byte **b, byte *raw);
void Assign4b(byte **r, byte **g, byte **b, byte **a, byte *raw);

void ProcessImage(
            byte* sData, int sWidth, int sHeight,
            byte* bgData, int bgWidth, int bgHeight,
            byte* fgData, int fgWidth, int fgHeight,
            byte* fgDataSec, int fgWidthSec, int fgHeightSec,
            byte* oData);

}}}}} // namespace sk { namespace pbvl { namespace image { namespace processing { namespace cstyle {

#endif //IMAGEUTILSCSTYLE_H
