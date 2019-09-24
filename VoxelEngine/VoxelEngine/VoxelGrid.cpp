#include "stdafx.h"
#include "VoxelGrid.h"
#include<cmath>


VoxelGrid::VoxelGrid(char* isovalues, unsigned char* materialIndices, int blockSize, int lod) : isovalues(isovalues),
																						materialIndices(materialIndices), 
																						isoSize(blockSize + 1), 
																						blockSize(blockSize), 
																						spacing(pow(2.0, lod)){}


VoxelGrid::~VoxelGrid() {
}


char VoxelGrid::IsovalueAt(int x, int y, int z) const {
	if (x < 0)
		x = 0;
	else if (x == isoSize - 1)
		x = isoSize - 1;
	if (y < 0)
		y = 0;
	else if (y == isoSize - 1)
		y = isoSize - 1;
	if (z < 0)
		z = 0;
	else if (z == isoSize - 1)
		z = isoSize - 1;

	return isovalues[x + y * isoSize + z * isoSize * isoSize];
}

char VoxelGrid::IsovalueAt(Coord coord) const {
	return IsovalueAt((int)coord.x, (int)coord.y, (int)coord.z);
}

unsigned char VoxelGrid::MaterialIndexAt(int x, int y, int z) const {
	if (x < 0)
		x = 0;
	else if (x == isoSize - 1)
		x = isoSize - 1;
	if (y < 0)
		y = 0;
	else if (y == isoSize - 1)
		y = isoSize - 1;
	if (z < 0)
		z = 0;
	else if (z == isoSize - 1)
		z = isoSize - 1;

	return materialIndices[x + y * isoSize + z * isoSize * isoSize];
}

unsigned char VoxelGrid::MaterialIndexAt(Coord coord) const {
	return MaterialIndexAt((int)coord.x, (int)coord.y, (int)coord.z);
}

