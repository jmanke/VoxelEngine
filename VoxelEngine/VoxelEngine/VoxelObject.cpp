#include "stdafx.h"
#include "VoxelObject.h"
#include <vector>
#include <string>


static const glm::vec3 UNIT_X = glm::vec3(1.f, 0.f, 0.f);
static const glm::vec3 UNIT_Y = glm::vec3(0.f, 1.f, 0.f);
static const glm::vec3 UNIT_Z = glm::vec3(0.f, 0.f, 1.f);


VoxelObject::VoxelObject(int dimX, int dimY, int dimZ, int blockSize) : m_grid(dimX, dimY, dimZ, blockSize) {

}


VoxelObject::~VoxelObject() {

}

// Fill isovalues in the grid
void VoxelObject::FillIsoValues(char isovalues[]) {
	m_grid.FillIsoValues(isovalues);
}


glm::vec3 VoxelObject::CalcNormal(const Block& block, const glm::vec3& coord) {
	return glm::vec3(m_grid.isovalueAt(block, coord + UNIT_X) - m_grid.isovalueAt(block, coord - UNIT_X) * 0.5f,
		m_grid.isovalueAt(block, coord + UNIT_Y) - m_grid.isovalueAt(block, coord - UNIT_Y) * 0.5f,
		m_grid.isovalueAt(block, coord + UNIT_Z) - m_grid.isovalueAt(block, coord - UNIT_Z) * 0.5f);
}


void getPreeceedingCellIndex(int& x, int& y, int& z, char direction) {
	if (direction & 0x1) x -= 1;
	if (direction & 0x2) y -= 1;
	if (direction & 0x4) z -= 1;
}
