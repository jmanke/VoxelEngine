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

	void GenerateRegularCells(VoxelGrid& grid, Mesh& mesh);
	Cell MakeCell(const Coord& coord, const VoxelGrid& grid);
	void ComputeMesh(char* isovalues, int blockSize, int lod, glm::vec3* vertices, int* vertexCount, int* triangles, int* triangleCount, glm::vec3* normals);

private:
	glm::vec3 CalcNormal(const Coord& coord, const VoxelGrid& grid);
};

