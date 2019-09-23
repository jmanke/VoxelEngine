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

struct Interpolators 
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	float3 localPos : TEXCOORD3;
	SHADOW_COORDS(5)

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD4;
#endif
};

float4 _Tint;
float _Metallic;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _SecondaryTex;
float4 SecondaryTex_ST;
float _Smoothness;

UnityLight CreateLight(Interpolators  i) {
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

void ComputeVertexLightColor(inout Interpolators  i) {
#if defined(VERTEXLIGHT_ON)
	i.vertexLightColor = Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb,
		unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, i.worldPos, i.normal
	);
#endif
}

UnityIndirect CreateIndirectLight(Interpolators  i) {
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

Interpolators MyVertexProgram(appdata v)
{
	Interpolators  o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.normal = normalize(o.normal);
	o.color = v.color;
	o.localPos = v.vertex;

	TRANSFER_SHADOW(o);
	ComputeVertexLightColor(o);
	return o;
}

UNITY_DECLARE_TEX2DARRAY(_TexArr);

float4 MyFragmentProgram(Interpolators  i) : SV_Target
{
	float3 dpdx = ddx(i.localPos);
	float3 dpdy = ddy(i.localPos);
	i.normal = normalize(cross(dpdy, dpdx));

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.localPos);

	// Find our UVs for each axis based on world position of the fragment.
	//half2 yUV = i.worldPos.xz;
	//half2 xUV = i.worldPos.zy;
	//half2 zUV = i.worldPos.xy;
	// Now do texture samples from our diffuse map with each of the 3 UV set's we've just made.

	//half3 yDiff;
	//half3 xDiff;
	//half3 zDiff;

	//yDiff = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(yUV, ind));
	//xDiff = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(xUV, ind));
	//zDiff = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(zUV, ind));

	//if (i.color.r < 1.5/255) {
	//	yDiff = tex2D(_MainTex, yUV);
	//	xDiff = tex2D(_MainTex, xUV);
	//	zDiff = tex2D(_MainTex, zUV);
	//}
	//else {
	//	yDiff = tex2D(_SecondaryTex, yUV);
	//	xDiff = tex2D(_SecondaryTex, xUV);
	//	zDiff = tex2D(_SecondaryTex, zUV);
	//}
	// Get the absolute value of the world normal.
	// Put the blend weights to the power of BlendSharpness, the higher the value, 
	// the sharper the transition between the planar maps will be.
	//half3 blendWeights = pow(abs(i.normal), 0.5);
	// Divide our blend mask by the sum of it's components, this will make x+y+z=1
	//blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
	// Finally, blend together all three samples based on the blend mask.

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

	float ind = i.color.r * 255;// +0.5;
	float3 albedo = 0;

	if (dotX > dotY && dotX > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.zy, ind)); //xDiff;
	else if (dotY > dotX && dotY > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xz, ind)); //yDiff;
	else
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xy, ind)); //zDiff;

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