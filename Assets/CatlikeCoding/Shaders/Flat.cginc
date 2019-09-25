#if !defined(FLAT_INCLUDED)
#define FLAT_INCLUDED

#define CUSTOM_GEOMETRY_INTERPOLATORS \
float2 barycentricCoordinates : TEXCOORD4;

#include "MyLightingInput.cginc"
#include "MyLighting.cginc"


struct InterpolatorsGeometry {
	InterpolatorsVertex data;
	CUSTOM_GEOMETRY_INTERPOLATORS
};

[maxvertexcount(3)]
void MyGeometryProgram(triangle InterpolatorsVertex i[3],
	inout TriangleStream<InterpolatorsVertex> stream) {
	float3 p0 = i[0].worldPos.xyz;
	float3 p1 = i[1].worldPos.xyz;
	float3 p2 = i[2].worldPos.xyz;

	float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));

	i[0].normal = triangleNormal;
	i[1].normal = triangleNormal;
	i[2].normal = triangleNormal;

	i[0].barycentricCoordinates = float2(1, 0);
	i[1].barycentricCoordinates = float2(0, 1);
	i[2].barycentricCoordinates = float2(0, 0);

#if defined(VOXEL_INCLUDED)
	i[0].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;
	i[1].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;
	i[2].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;
#endif

	stream.Append(i[0]);
	stream.Append(i[1]);
	stream.Append(i[2]);
}

#endif