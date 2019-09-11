using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelObject
    {
        static readonly Vector3 UNIT_X = new Vector3(1, 0, 0);
        static readonly Vector3 UNIT_Y = new Vector3(0, 1, 0);
        static readonly Vector3 UNIT_Z = new Vector3(0, 0, 1);

        public readonly int dimX;
        public readonly int dimY;
        public readonly int dimZ;

        public readonly int isoX;
        public readonly int isoY;
        public readonly int isoZ;

        public readonly int size;

        public readonly Block[] blocks;

        private sbyte[] isovalues;
        private FastNoiseSIMD fastNoise;

        public VoxelObject(int dimX, int dimY, int dimZ, int size)
        {
            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;
            this.size = size;

            this.isoX = dimX * size;
            this.isoY = dimY * size;
            this.isoZ = dimZ * size;

            blocks = new Block[dimX * dimY * dimZ];
            isovalues = new sbyte[isoX * isoY * isoZ];

            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        blocks[CoordToIndex(x, y, z)] = new Block(x, y, z, 0, size);
                    }
                }
            }

            fastNoise = new FastNoiseSIMD();
            fastNoise.SetFrequency(0.05f);
            fastNoise.SetFractalGain(1.0f);
        }

        public void GenerateIsovalues()
        {
            // Get a set of 16 x 16 x 16 Simplex Fractal noise
            float[] noiseSet = fastNoise.GetNoiseSet(0, 0, 0, isoX, isoY, isoZ);
            int index = 0;

            for (int x = 0; x < isoX; x++)
            {
                for (int y = 0; y < isoY; y++)
                {
                    for (int z = 0; z < isoZ; z++)
                    {
                        // Pseudo function to process data in noise set
                        //ProcessVoxelData(x, y, z, noiseSet[index++]);
                        isovalues[IsoCoordToIndex(x, y, z)] = (noiseSet[index++] < 0) ? (sbyte)-1 : (sbyte)1;
                    }
                }
            }

            Debug.Log("Filled");
        }

        public int CoordToIndex(int x, int y, int z)
        {
            return x + y * dimX + z * dimY * dimY;
        }

        public int IsoCoordToIndex(int x, int y, int z)
        {
            return x + y * isoX + z * isoY * isoY;
        }

        public sbyte IsovalueAt(int x, int y, int z)
        {
            if (x < 0)
                x = 0;
            else if (x > isoX - 1)
                x = isoX - 1;
            if (y < 0)
                y = 0;
            else if (y > isoY - 1)
                y = isoY - 1;
            if (z < 0)
                z = 0;
            else if (z > isoZ - 1)
                z = isoZ - 1;

            return isovalues[IsoCoordToIndex(x, y, z)]; //(fastNoise.GetPerlin(x, y, z) < 0) ? (sbyte)-1 : (sbyte)1;
        }

        public sbyte IsovalueAt(float x, float y, float z)
        {
            return IsovalueAt((int)x, (int)y, (int)z);//(fastNoise.GetPerlin(x, y, z) < 0) ? (sbyte)-1 : (sbyte)1;
        }

        public sbyte IsovalueAt(Block block, Vector3 coord)
        {
            return IsovalueAt(block.x * block.size + coord.x, block.y * block.size + coord.y, block.z * block.size + coord.z);
        }

        public VoxelMesh GenerateMesh(Block block)
        {
            var mesh = new VoxelMesh();
            int numVoxels = block.size * block.size * block.size;
            int maxTriCount = numVoxels * 5;
            int vertInd = 0;
            int triInd = 0;
            var vertices = new Vector3[maxTriCount * 3];
            var triangles = new int[maxTriCount * 3];
            var normals = new Vector3[maxTriCount * 3];
            int[] triangleVertexIndex = new int[15];
            byte reuseMask = 0;

            for (int z = 0; z < block.size; z++)
            {
                for (int y = 0; y < block.size; y++)
                {
                    for (int x = 0; x < block.size; x++)
                    {
                        var localCoord = new Vector3(x, y, z);
                        var cell = MakeCell(block, localCoord);

                        // TODO: Do in the loop
                        reuseMask = 0;
                        if (x > 0) reuseMask |= 1;
                        if (y > 0) reuseMask |= 0x2;
                        if (z > 0) reuseMask |= 0x4;

                        int caseCode = Cell.GenerateCaseCode(cell.corner);

                        if ((caseCode ^ ((cell.corner[7] >> 7) & 0xFF)) == 0)
                            continue;

                        var classIndex = TransVoxelTables.regularCellClass[caseCode];
                        var cellData = TransVoxelTables.regularCellData[classIndex];
                        var regVertexData = TransVoxelTables.regularVertexData[caseCode];

                        for (long vertexIndex = 0; vertexIndex < cellData.GetVertexCount(); vertexIndex++)
                        {
                            byte edgeInd = (byte)(regVertexData[vertexIndex] & 0xFF);
                            byte direction = (byte)(regVertexData[vertexIndex] >> 12);
                            byte vIndexInCell = (byte)((regVertexData[vertexIndex] >> 8) & 0x0F);
                            byte v0 = (byte)((edgeInd >> 4) & 0X0F);
                            byte v1 = (byte)(edgeInd & 0X0F);

                            var p0 = Cell.GetCornerCoord(block.spacing, localCoord, v0);
                            var p1 = Cell.GetCornerCoord(block.spacing, localCoord, v1);

                            var d0 = IsovalueAt(block, p0);
                            var d1 = IsovalueAt(block, p1);
                            long t;

                            if (d0 != d1)
                                t = (d1 << 8) / (d1 - d0);
                            else
                                t = 0;

                            Vector3 Q;
                            bool createVertex = false;

                            // Vertex lies in the interior of the edge.
                            if ((t & 0x00FF) != 0)
                            {
                                // check if preceeding cell exists
                                if ((direction & reuseMask) != direction)
                                {
                                    createVertex = true;
                                }
                            }
                            // Vertex lies on the endpoint of an edge (for -1, 1 points, this doesn't apply)
                            else
                            {
                                //mesh.reuseNum++;
                                if ((direction & reuseMask) != direction)
                                {
                                    createVertex = true;
                                }
                            }

                            if (createVertex)
                            {
                                long u = 0x0100 - t;
                                Q = (t * p0 + u * p1) / 256.0f;

                                // only add the maximal edges
                                if ((direction & 0x8) != 0)
                                    cell.reuseVertices[vIndexInCell] = vertInd;

                                triangleVertexIndex[vertexIndex] = vertInd;
                                normals[vertInd] = CalcNormal(block, Q);
                                vertices[vertInd] = Q;
                                vertInd++;
                            }
                            else
                            {
                                int preX = x; int preY = y; int preZ = z;
                                GetPreeceedingCellIndex(ref preX, ref preY, ref preZ, direction);
                                var preCell = block.cells[Block.CellInd(preX, preY, preZ, block.size)];
                                // get preceeding cell index
                                triangleVertexIndex[vertexIndex] = preCell.reuseVertices[vIndexInCell];
                            }
                        }

                        for (int i = 0; i < cellData.GetTriangleCount(); i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                int k = i * 3 + j;
                                triangles[triInd] = triangleVertexIndex[cellData.vertexIndex[k]];
                                triInd++;
                            }
                        }

                        // add to cell reuse
                        block.cells[Block.CellInd(x, y, z, block.size)] = cell;
                    }
                }
            }

            mesh.vertices = vertices.Take(vertInd).ToArray();
            mesh.triangles = triangles.Take(triInd).ToArray();
            mesh.normals = normals.Take(vertInd).ToArray();

            return mesh;
        }

        private Vector3 CalcNormal(Block block, Vector3 coord) {
	        return new Vector3(IsovalueAt(block, coord + UNIT_X) - IsovalueAt(block, coord - UNIT_X) * 0.5f,
                                IsovalueAt(block, coord + UNIT_Y) - IsovalueAt(block, coord - UNIT_Y) * 0.5f,
                                IsovalueAt(block, coord + UNIT_Z) - IsovalueAt(block, coord - UNIT_Z) * 0.5f);
}

    private Cell MakeCell(Block block, Vector3 coord)
        {
            var cell = new Cell(new sbyte[8]
            {   IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 0)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 1)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 2)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 3)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 4)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 5)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 6)),
                IsovalueAt(block, Cell.GetCornerCoord(block.spacing, coord, 7)),
            }, coord);

            return cell;
        }

        private void GetPreeceedingCellIndex(ref int x, ref int y, ref int z, byte direction)
        {
            if ((direction & 0x1) != 0) x -= 1;
            if ((direction & 0x2) != 0) y -= 1;
            if ((direction & 0x4) != 0) z -= 1;
        }
    }
}
