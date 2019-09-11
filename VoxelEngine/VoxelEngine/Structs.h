#pragma once
#include "glm/glm.hpp"
#include <vector>
#include <string>

static const glm::vec3 cornerPositions[8] = {
	glm::vec3(0,0,0),
	glm::vec3(1,0,0),
	glm::vec3(0,1,0),
	glm::vec3(1,1,0),
	glm::vec3(0,0,1),
	glm::vec3(1,0,1),
	glm::vec3(0,1,1),
	glm::vec3(1,1,1)
};

struct Cell {
	char corner[8];
	glm::vec3 localPosition;
	// point to vertex position in array
	int reuseVertexIndicies[4];

	static unsigned long generateCaseCode(const char corner[8]) {
		return ((corner[0] >> 7) & 0x01)
			| ((corner[1] >> 6) & 0x02)
			| ((corner[2] >> 5) & 0x04)
			| ((corner[3] >> 4) & 0x08)
			| ((corner[4] >> 3) & 0x10)
			| ((corner[5] >> 2) & 0x20)
			| ((corner[6] >> 1) & 0x40)
			| (corner[7] & 0x80);
	}

	static glm::vec3 getCornerCoord(const float& spacing, const glm::vec3& localCoord, const char& cornerInd) {
		return (localCoord + cornerPositions[cornerInd]) * spacing;
	}
};

struct Block
{
	int x;
	int y;
	int z;
	int lod;
	float spacing;
	int size;
	std::vector<Cell> cellBuffer;

	static std::string generateId(int x, int y, int z, int depth) {
		return std::string(std::to_string(x) + '_' + std::to_string(y) + '_' + std::to_string(z) + '_' + std::to_string(depth));
	}

	static glm::vec3 blockGlobalCoord(const Block& block) {
		return glm::vec3(block.x, +block.y, +block.z) * (float)block.size;
	}

	static int cellInd(int x, int y, int z, int size) {
		return x + y * size + z * size * size;
	}
};

struct Mesh {
	int vertInd;
	glm::vec3* verticies;
	int triInd;
	int* triangles;
	glm::vec3* normals;
	int reuseNum;

	Mesh(glm::vec3* verticies, int* triangles, glm::vec3* normals) : vertInd(0), verticies(verticies), triInd(0), triangles(triangles), reuseNum(0), normals(normals) {}
};