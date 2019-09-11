#pragma once
#include "glm/glm.hpp"
#include "Structs.h"
#include <vector>

class VoxelGrid
{
public:
	VoxelGrid(int dimX, int dimY, int dimZ, int blockSize);
	~VoxelGrid();
	void FillIsoValues(char isovalues[]);
	char isovalueAt(int x, int y, int z);
	char isovalueAt(glm::vec3 globalCoord);
	char isovalueAt(const Block& block, glm::vec3 localCoord);
private:
	const int dimX;
	const int dimY;
	const int dimZ;

	std::vector<char> grid;
	int indexFromCoord(int x, int y, int z);
};

