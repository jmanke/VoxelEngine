using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public class Cell
    {
        public sbyte[] corner;
        Vector3 coord;
        public int[] reuseVertices;

        public Cell(sbyte[] corner, Vector3 coord)
        {
            this.corner = corner;
            this.coord = coord;
            this.reuseVertices = new int[4];
        }

        public static Vector3[] cornerPositions = new Vector3[] 
        {
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(0,1,0),
            new Vector3(1,1,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(0,1,1),
            new Vector3(1,1,1)
        };

        public static int GenerateCaseCode(sbyte[] corner)
        {
            return ((corner[0] >> 7) & 0x01)
                | ((corner[1] >> 6) & 0x02)
                | ((corner[2] >> 5) & 0x04)
                | ((corner[3] >> 4) & 0x08)
                | ((corner[4] >> 3) & 0x10)
                | ((corner[5] >> 2) & 0x20)
                | ((corner[6] >> 1) & 0x40)
                | (corner[7] & 0x80);
        }

        public static Vector3 GetCornerCoord(float spacing, Vector3 localCoord, byte cornerInd)
        {
		    return (localCoord + cornerPositions[cornerInd]) * spacing;
	    }
    }
}
