#include "stdafx.h"
#include "VoxelGrid.h"


VoxelGrid::VoxelGrid(int dimX, int dimY, int dimZ, int blockSize) : dimX(dimX), dimY(dimY), dimZ(dimZ) {
	grid.resize(dimX * dimY * dimZ);
}


VoxelGrid::~VoxelGrid() {
}

void VoxelGrid::FillIsoValues(char isovalues[]) {
	for (int i = 0; i < grid.size(); i++) {
		grid[i] = isovalues[i];
	}
}


// Gets an index given a coordinate
int VoxelGrid::indexFromCoord(int x, int y, int z) {
	return x + y * dimX + z * dimY * dimY;
}


char VoxelGrid::isovalueAt(int x, int y, int z) {
	if (x < 0)
		x = 0;
	else if (x > dimX - 1)
		x = dimX - 1;
	if (y < 0)
		y = 0;
	else if (y > dimY - 1)
		y = dimY - 1;
	if (z < 0)
		z = 0;
	else if (z > dimZ - 1)
		z = dimZ - 1;

	return grid[indexFromCoord(x, y, z)];
}

char VoxelGrid::isovalueAt(glm::vec3 coord) {
	return isovalueAt(coord.x, coord.y, coord.z);
}

char VoxelGrid::isovalueAt(const Block& block, glm::vec3 localCoord) {
	return isovalueAt(block.x * block.size + localCoord.x, block.y * block.size + localCoord.y, block.z * block.size + localCoord.z);
}
