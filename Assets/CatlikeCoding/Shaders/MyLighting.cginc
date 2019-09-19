#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float3 normal: NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators 
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	SHADOW_COORDS(5)

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD4;
#endif
};

float4 _Tint;
float _Metallic;
sampler2D _MainTex;
float4 _MainTex_ST;
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

	TRANSFER_SHADOW(o);
	ComputeVertexLightColor(o);
	return o;
}

fixed4 MyFragmentProgram(Interpolators  i) : SV_Target
{
	float3 dpdx = ddx(i.worldPos);
	float3 dpdy = ddy(i.worldPos);
	i.normal = normalize(cross(dpdy, dpdx));

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
	float3 specularTint;
	float oneMinusReflectivity;

	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	return UNITY_BRDF_PBS(albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i));
}
#endif