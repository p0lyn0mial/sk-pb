#pragma once

namespace sk { namespace pbvl { namespace image { namespace processing { namespace facade {
	
	/*
	*/
	DLL_API void SetProcessImageHint(int what, int value);

	/*
	*/
	DLL_API bool ProcessFrame(
            void* bgData, int bgWidth, int bgHeight,
            void* sgData, int sgWidth, int sgHeight,
            void* fgData, int fgWidth, int fgHeight,
			void* fgDataSec, int fgWidthSec, int fgHeightSec,
            void* oData);
	
	/*
	*/
	DLL_API bool ProcessPhoto(
            void* photoData, int photoWidth, int photoHeight,
			void* bgData, int bgWidth, int bgHeight);

	/*
	*/
	DLL_API bool PreparePhotoStrip(
            void* imgOne, int imgOneWidth, int imgOneHeight,
			void* imgTwo, int imgTwoWidth, int imgTwoHeight,
			void* imgThree, int imgThreeWidth, int imgThreeHeight,
			void* imgFour, int imgFourWidth, int imgFourHeight,
			void* imgLogo, int imgLogoWidth, int imgLogoHeight,
            void* imgStrip, int imgStripWidth, int imgStripHeight);

}}}}} // namespace sk { namespace pbvl { namespace image { namespace processing { namespace facade
