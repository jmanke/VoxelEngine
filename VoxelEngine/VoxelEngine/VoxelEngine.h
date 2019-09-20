#pragma once
#include "VoxelObject.h"

#define LIBRARY_EXPORT __declspec(dllexport)

extern "C" {
	//! constructor
	LIBRARY_EXPORT  VoxelObject* new_VoxelObject(int dimX, int dimY, int dimZ, int blockSize);

	//! Destructor
	LIBRARY_EXPORT  void delete_VoxelObject(VoxelObject* instance);

	//! Computes mesh
	LIBRARY_EXPORT void ComputeMesh(VoxelObject* instance, char* isovalues, int blockSize, int lod, glm::vec3* vertices, int* vertexCount, int* triangles, int* triangleCount, char* materialIndices, int* vertexMaterialIndicies);
}