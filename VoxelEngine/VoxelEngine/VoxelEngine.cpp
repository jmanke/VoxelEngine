// VoxelEngine.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "VoxelEngine.h"

extern "C" {
	LIBRARY_EXPORT  VoxelObject* new_VoxelObject(int dimX, int dimY, int dimZ, int blockSize) {
		return new VoxelObject(dimX, dimY, dimZ, blockSize);
	}

	LIBRARY_EXPORT void delete_VoxelObject(VoxelObject* instance)
	{
		delete instance;
	}

	//LIBRARY_EXPORT void FillIsoValues(VoxelObject * instance, char isovalues[])
	//{
	//	return instance->FillIsoValues(isovalues);
	//}
}
