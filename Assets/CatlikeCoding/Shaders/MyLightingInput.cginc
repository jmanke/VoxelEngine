#if !defined(MY_LIGHTING_INPUT_INCLUDED)
#define MY_LIGHTING_INPUT_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float3 normal: NORMAL;
	float4 color : COLOR;
	float2 uv : TEXCOORD0;
};

struct TriplanarUV {
	float2 x, y, z;
};

struct InterpolatorsVertex 
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float3 normal : TEXCOORD1;
#if defined(VOXEL_INCLUDED)
	float3 matInd : TEXCOORD6;
#else
	float2 uv : TEXCOORD0;
#endif
	float3 worldPos : TEXCOORD2;
	float3 localPos : TEXCOORD3;
#if defined (CUSTOM_GEOMETRY_INTERPOLATORS)
	CUSTOM_GEOMETRY_INTERPOLATORS
#endif
	SHADOW_COORDS(5)

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD4;
#endif
};
//SamplerState sampler_MainTex;

float4 _Tint;
float _Metallic;
float _Smoothness;

#endif