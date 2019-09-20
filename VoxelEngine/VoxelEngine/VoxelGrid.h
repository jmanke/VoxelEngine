#pragma once
#include "glm/glm.hpp"
#include "Structs.h"
#include <vector>

class VoxelGrid
{
public:
	VoxelGrid(char* isovalues, char* materialIndices, int blockSize, int lod);
	~VoxelGrid();

	char IsovalueAt(int x, int y, int z) const;
	char IsovalueAt(Coord coord) const;

	int MaterialIndexAt(int x, int y, int z) const;
	int MaterialIndexAt(Coord coord) const;

	const char* isovalues;
	const char* materialIndices;
	const int isoSize;
	const int blockSize;
	const int spacing;
//private:
	//int indexFromCoord(int x, int y, int z);
};

