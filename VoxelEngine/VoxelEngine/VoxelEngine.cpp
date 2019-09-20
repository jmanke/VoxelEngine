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

	LIBRARY_EXPORT void ComputeMesh(VoxelObject* instance, char* isovalues, int blockSize, int lod, glm::vec3* vertices, int* vertexCount, int* triangles, int* triangleCount, char* materialIndices, int* vertexMaterialIndices)
	{
		instance->ComputeMesh(isovalues, blockSize, lod, vertices, vertexCount, triangles, triangleCount, materialIndices, vertexMaterialIndices);
	}
}
