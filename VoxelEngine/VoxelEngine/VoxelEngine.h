#pragma once
#include "VoxelObject.h"

#define LIBRARY_EXPORT __declspec(dllexport)

extern "C" {
	//! constructor
	LIBRARY_EXPORT  VoxelObject* new_VoxelObject(int dimX, int dimY, int dimZ, int blockSize);

	//! Destructor
	LIBRARY_EXPORT  void delete_VoxelObject(VoxelObject* instance);

	//! Destructor
	LIBRARY_EXPORT  void FillIsoValues(VoxelObject* instance, char isovalues[]);
}