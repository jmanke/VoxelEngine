#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED
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
	//float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	float3 localPos : TEXCOORD3;
	float2 barycentricCoordinates : TEXCOORD4;
	SHADOW_COORDS(5)
	float3 matInd : TEXCOORD6;

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD4;
#endif
};
//SamplerState sampler_MainTex;


float4 _Tint;
float _Metallic;
//sampler2D _MainTex;
//float4 _MainTex_ST;
//sampler2D _SecondTex;
//float4 _SecondTex_ST;
float _Smoothness;

UnityLight CreateLight(InterpolatorsVertex  i) {
	UnityLight light;
#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
	light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
#else
	light.dir = _WorldSpaceLightPos0.xyz;
#endif
#if defined(SHADOWS_SCREEN)
	float attenuation = SHADOW_ATTENUATION(i);
#else
	UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
#endif
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

void ComputeVertexLightColor(inout InterpolatorsVertex  i) {
#if defined(VERTEXLIGHT_ON)
	i.vertexLightColor = Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb,
		unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, i.worldPos, i.normal
	);
#endif
}

UnityIndirect CreateIndirectLight(InterpolatorsVertex  i) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

#if defined(VERTEXLIGHT_ON)
	indirectLight.diffuse = i.vertexLightColor;
#endif

#if defined(FORWARD_BASE_PASS)
	indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
#endif

	return indirectLight;
}

InterpolatorsVertex MyVertexProgram(appdata v)
{
	InterpolatorsVertex  o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.normal = normalize(o.normal);
	o.color = v.color;
	o.localPos = v.vertex;
	o.barycentricCoordinates = float2(0, 0);
	o.matInd = float3(0,0,0);
	TRANSFER_SHADOW(o);
	ComputeVertexLightColor(o);
	return o;
}

UNITY_DECLARE_TEX2DARRAY(_TexArr);

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

	i[0].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;
	i[1].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;
	i[2].matInd = float3(i[0].color.r, i[1].color.r, i[2].color.r) * 255;

	stream.Append(i[0]);
	stream.Append(i[1]);
	stream.Append(i[2]);
}

float4 MyFragmentProgram(InterpolatorsVertex  i) : SV_Target
{
	//float3 dpdx = ddx(i.localPos);
	//float3 dpdy = ddy(i.localPos);
	//i.normal = normalize(cross(dpdy, dpdx));

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.localPos);

	float dotX = abs(dot(float3(1, 0, 0), i.normal));
	float dotY = abs(dot(float3(0, 1, 0), i.normal));
	float dotZ = abs(dot(float3(0, 0, 1), i.normal));

	// Check similarity
	if (abs(dotX - dotY) < 0.01) {
		dotX = dotY;
		dotY = dotX;
		dotX += 0.01;
	}

	if (abs(dotX - dotZ) < 0.01) {
		dotX = dotZ;
		dotZ = dotX;
		dotX += 0.01;
	}

	if (abs(dotY - dotZ) < 0.01) {
		dotY = dotZ;
		dotZ = dotY;
		dotZ += 0.02;
	}

	float3 barys;
	barys.xy = i.barycentricCoordinates;
	barys.z = 1 - barys.x - barys.y;

	float ind = 0;// i.color.r * 255;//0;
	float m0 = i.matInd.x;
	float m1 = i.matInd.y;
	float m2 = i.matInd.z;
	
	if (m0 == m1 && m0 == m2 && m1 == m2)
		ind = m0;
	else if (m0 != m1 && m0 != m2 && m1 != m2) {
		if (barys.x > 0.5) {
			ind = m0;
		}
		else if (barys.y > 0.5) {
			ind = m1;
		}
		else if (barys.z > 0.5) {
			ind = m2;
		}
		else {
			ind = m1;
		}
	}
	else {
		if (m0 != m1 && m0 != m2) {
			if (barys.x > 0.5)
				ind = m0;
			else
				ind = m1;
		}
		else if (m1 != m0 && m1 != m2) {
			if (barys.y > 0.5)
				ind = m1;
			else
				ind = m0;
		}
		else {
			if (barys.z > 0.5)
				ind = m2;
			else
				ind = m0;
		}
	}

	//if (barys.x > c)
	//	ind = i.matInd.x;
	//else if (barys.y > c)
	//	ind = i.matInd.y;
	//else
	//	ind = i.matInd.z;

	float3 albedo = 0;// _MainTex.Sample(sampler_MainTex, i.localPos.zy);

	if (dotX > dotY && dotX > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.zy, ind)); //xDiff;
	else if (dotY > dotX && dotY > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xz, ind)); //yDiff;
	else
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xy, ind)); //zDiff;

	float minBary = min(barys.x, min(barys.y, barys.z));
	minBary = smoothstep(0, 0.02, minBary);
	//albedo = albedo * minBary;

	//float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
	float3 specularTint;
	float oneMinusReflectivity;

	//albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.worldPos.xz, ind)) * _Tint;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	return UNITY_BRDF_PBS(albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i));
}
#endif