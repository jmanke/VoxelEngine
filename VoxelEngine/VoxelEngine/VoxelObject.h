#pragma once
#include "glm/glm.hpp"
#include "Structs.h"
#include "VoxelGrid.h"
#include <vector>

class VoxelObject
{
public:
	VoxelObject(int dimX, int dimY, int dimZ, int blockSize);
	~VoxelObject();

	// isovalues needs to be the same dimensions as the size of this voxel object
	void FillIsoValues(char isovalues[]);

private:
	glm::vec3 CalcNormal(const Block& block, const glm::vec3& coord);
	VoxelGrid m_grid;
};

