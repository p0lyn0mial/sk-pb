#include "imageutilscstyle.h"
#include <iostream>

namespace sk { namespace pbvl { namespace image { namespace processing { namespace cstyle {

    using namespace sk::pbvl::image::common;

    void ProcessImage(
            byte* sData, int sWidth, int sHeight,
            byte* bgData, int bgWidth, int bgHeight,
            byte* fgData, int fgWidth, int fgHeight,
            byte* fgDataSec, int fgWidthSec, int fgHeightSec,
            byte* oData)
    {
        int size = sWidth * sHeight * 3;
        byte *lsData = new byte[size];
        memcpy(lsData, sData, size);

        byte *bR, *bG, *bB;
        byte *oR, *oG, *oB;
        byte *sR, *sG, *sB;
        byte *fR, *fG, *fB, *fA;
        byte *fRs, *fGs, *fBs, *fAs;

        byte *sH, *sS, *sV;

        int i3b = 0; //global iterator
        int i4b = 0; //global iterator
        int i3bm = 0; //global iterator
        int i3db = 0;

        int offset = (6 * sWidth);

        cv::Mat inputHsv;

        if(pi_chromakey)
        {
            cv::Mat input(sHeight, sWidth, CV_8UC3, lsData);
            cv::cvtColor(input, inputHsv, CV_BGR2HSV);
        }

        for(int row = 0; row < sHeight; row++)
        {
            i3bm = ((sWidth * (row + 1) * 3) - 1); //global iterator

            for(int cell = 0; cell < sWidth; cell++, i3b += 3, i3bm -= 3, i4b += 4, i3db += 3)
            {
                //background
                bB = (byte*)(bgData + i3b);
                bG = (byte*)(bgData + i3b + 1);
                bR = (byte*)(bgData + i3b + 2);

                if(pi_stream_mirror == 1)
                {
                    sB = (byte*)(lsData + i3bm - 2);
                    sG = (byte*)(lsData + i3bm - 1);
                    sR = (byte*)(lsData + i3bm);
                    sH = (byte*)(inputHsv.data + i3bm - 2);
                    sS = (byte*)(inputHsv.data + i3bm - 1);
                    sV = (byte*)(inputHsv.data + i3bm);
                }
                else
                {
                    sB = (byte*)(lsData + i3b);
                    sG = (byte*)(lsData + i3b + 1);
                    sR = (byte*)(lsData + i3b + 2);
                    sH = (byte*)(inputHsv.data + i3b);
                    sS = (byte*)(inputHsv.data + i3b + 1);
                    sV = (byte*)(inputHsv.data + i3b + 2);
                }

                //foreground
                fB = (byte*)(fgData + i4b);
                fG = (byte*)(fgData + i4b + 1);
                fR = (byte*)(fgData + i4b + 2);
                fA = (byte*)(fgData + i4b + 3);

                //second foreground
                fBs = (byte*)(fgDataSec + i4b);
                fGs = (byte*)(fgDataSec + i4b + 1);
                fRs = (byte*)(fgDataSec + i4b + 2);
                fAs = (byte*)(fgDataSec + i4b + 3);

                //output
                oB = (byte*)(oData + i3db);
                oG = (byte*)(oData + i3db + 1);
                oR = (byte*)(oData + i3db + 2);

                if (pi_chromakey == 1)
                {
                    if (   (*sH >= lowerH && *sH <= upperH && 
                            *sS >= lowerS && *sS <= upperS && 
                            *sV >= lowerV && *sV <= upperV) ||
                            pi_stream_crop_left > cell ||
                            (cell - pi_stream_crop_right) < cell ||
                            pi_stream_crop_top > row ||
                            (row - pi_stream_crop_bottom) < row )
                    {
                        if (pi_background == 1) //background enabled?
                        {
                            *oR = *bR; *oG = *bG; *oB = *bB;
                        }
                        else
                        {
                            *oR = 0; *oG = 0; *oB = 0;
                        }
                    }
                    else
                    {
                        if (pi_stream_gray == 1)
                        {
                            *oR = *oG = *oB = (*sR + *sG + *sB) / 3;
                        }
                        else
                        {
                            *oR = *sR; *oG = *sG; *oB = *sB;
                        }
                    }
                }
                else
                {
                    if (pi_stream_gray == 1)
                    {
                        *oR = *oG = *oB = (*sR + *sG + *sB) / 3;
                    }
                    else
                    {
                        *oR = *sR; *oG = *sG; *oB = *sB;
                    }
                }

                if (pi_foreground == 1) //foreground enabled?
                {
                    if (*fA >= 235)
                    {
                        *oR = *fR; *oG = *fG; *oB = *fB;
                    }
                }

                if (pi_foreground_sec == 1) //foreground enabled?
                {
                    if (*fAs >= 235)
                    {
                        *oR = *fRs; *oG = *fGs; *oB = *fBs;
                    }
                }

                if (pi_stream_super == 1)
                {
                    memcpy(oData + i3db + 3, oData + i3db, 3);
                    i3db += 3;
                }
            }

            if (pi_stream_super == 1)
            {
                memcpy(oData + i3db, oData + i3db - offset, offset);
                i3db += offset;
            }
        }

        delete[] lsData;
    }


}}}}} // namespace sk { namespace pbvl { namespace image { namespace processing { namespace cstyle {