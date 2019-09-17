#include "stdafx.h"
#include "VoxelObject.h"
#include "TransVoxelTables.h"
#include <vector>
#include <string>


static const Coord UNIT_X = Coord(1.f, 0.f, 0.f);
static const Coord UNIT_Y = Coord(0.f, 1.f, 0.f);
static const Coord UNIT_Z = Coord(0.f, 0.f, 1.f);


VoxelObject::VoxelObject(int dimX, int dimY, int dimZ, int blockSize) {

}


VoxelObject::~VoxelObject() {

}


glm::vec3 VoxelObject::CalcNormal(const Coord& coord, const VoxelGrid& grid) {
	return glm::vec3(grid.IsovalueAt(coord + UNIT_X) - grid.IsovalueAt(coord - UNIT_X) * 0.5f,
		grid.IsovalueAt(coord + UNIT_Y) - grid.IsovalueAt(coord - UNIT_Y) * 0.5f,
		grid.IsovalueAt(coord + UNIT_Z) - grid.IsovalueAt(coord - UNIT_Z) * 0.5f);
}


void getPreeceedingCellIndex(int& x, int& y, int& z, char direction) {
	if (direction & 0x1) x -= 1;
	if (direction & 0x2) y -= 1;
	if (direction & 0x4) z -= 1;
}


Cell VoxelObject::MakeCell(const Coord& coord, const VoxelGrid& grid)
{
	Cell cell = {
			{
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 0)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 1)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 2)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 3)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 4)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 5)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 6)),
				grid.IsovalueAt(Cell::getCornerCoord(grid.spacing, coord, 7)),
			},
			{0, 0, 0, 0}
	};

	return cell;
}

void VoxelObject::ComputeMesh(char* isovalues, int blockSize, int lod, glm::vec3 * vertices, int * vertexCount, int * triangles, int * triangleCount, glm::vec3 * normals) {
	auto mesh = Mesh(vertices, triangles, normals);
	auto grid = VoxelGrid(isovalues, blockSize, lod);

	GenerateRegularCells(grid, mesh);

	vertexCount[0] = mesh.vertInd;
	triangleCount[0] = mesh.triInd;
}

int ReuseCellInd(int x, int y, int z, int blockSize) {
	return x + y * blockSize + z * blockSize * blockSize;
}


void VoxelObject::GenerateRegularCells(VoxelGrid& grid, Mesh& mesh) {
	int min = 1;
	int max = grid.blockSize + 1;
	unsigned long triangleVertexIndex[15];
	unsigned char reuseMask = 0;
	std::vector<Cell> reuseCells;
	reuseCells.resize(max * max * max);

	for (int z = min; z < max; z++) {
		for (int y = min; y < max; y++) {
			for (int x = min; x < max; x++) {
				Coord localCoord = Coord{ x-1.f, y-1.f, z-1.f };
				auto cell = MakeCell(localCoord, grid);

				// TODO: Do in the loop
				reuseMask = 0;
				if (x > min) reuseMask |= 1;
				if (y > min) reuseMask |= 0x2;
				if (z > min) reuseMask |= 0x4;

				unsigned long caseCode = Cell::generateCaseCode(cell.corner);

				if ((caseCode ^ ((cell.corner[7] >> 7) & 0xFF)) == 0)
					continue;

				const auto classIndex = regularCellClass[caseCode];
				const auto cellData = regularCellData[classIndex];
				const unsigned short* regVertexData = regularVertexData[caseCode];

				for (long vertexIndex = 0; vertexIndex < cellData.GetVertexCount(); vertexIndex++) {
					const char edgeInd = regVertexData[vertexIndex] & 0xFF;
					char direction = regVertexData[vertexIndex] >> 12;
					char vIndexInCell = (regVertexData[vertexIndex] >> 8) & 0x0F;
					const unsigned char v0 = (edgeInd >> 4) & 0X0F;
					const unsigned char v1 = edgeInd & 0X0F;

					glm::vec3 p0 = Cell::getCornerCoord(grid.spacing, localCoord, v0);
					glm::vec3 p1 = Cell::getCornerCoord(grid.spacing, localCoord, v1);

					const auto d0 = grid.IsovalueAt(p0);
					const auto d1 = grid.IsovalueAt(p1);
					long t;

					if (d0 != d1)
						t = (d1 << 8) / (d1 - d0);
					else
						t = 0;

					glm::vec3 Q;
					bool createVertex = false;

					// Vertex lies in the interior of the edge.
					if ((t & 0x00FF) != 0) {
						// check if preceeding cell exists
						if ((direction & reuseMask) != direction) {
							createVertex = true;
						}
					}
					// Vertex lies on the endpoint of an edge (for -1, 1 points, this doesn't apply)
					else {
						//mesh.reuseNum++;
						if ((direction & reuseMask) != direction) {
							createVertex = true;
						}
					}

					if (createVertex) {
						long u = 0x0100 - t;
						Q = ((float)t * p0 + (float)u * p1) / 256.0f;

						// only add the maximal edges
						if (direction & 0x8)
							cell.reuseVertexIndicies[vIndexInCell] = mesh.vertInd;

						triangleVertexIndex[vertexIndex] = mesh.vertInd;
						mesh.normals[mesh.vertInd] = CalcNormal(Q, grid);
						mesh.verticies[mesh.vertInd] = Q;
						mesh.vertInd++;
					}
					else {
						int preX = x; int preY = y; int preZ = z;
						getPreeceedingCellIndex(preX, preY, preZ, direction);
						const auto& preCell = reuseCells[ReuseCellInd(preX, preY, preZ, grid.blockSize)];
						// get preceeding cell index
						triangleVertexIndex[vertexIndex] = preCell.reuseVertexIndicies[vIndexInCell];
					}
				}

				for (int i = 0; i < cellData.GetTriangleCount(); i++) {
					for (int j = 0; j < 3; j++) {
						int triInd = i * 3 + j;
						mesh.triangles[mesh.triInd] = triangleVertexIndex[cellData.vertexIndex[triInd]];
						mesh.triInd++;
					}
				}

				// add to cell reuse
				reuseCells[ReuseCellInd(x, y, z, grid.blockSize)] = cell;
			}
		}
	}
}
