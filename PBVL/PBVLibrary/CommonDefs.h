#pragma once

#ifdef PBVLIBRARY_EXPORTS
#define DLL_API extern "C" __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif