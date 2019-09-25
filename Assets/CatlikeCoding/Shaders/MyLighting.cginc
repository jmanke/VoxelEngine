#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED

#include "MyLightingInput.cginc"

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
	o.color = v.color;
	o.localPos = v.vertex;
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.normal = normalize(o.normal);
#if defined(VOXEL_INCLUDED)
	o.matInd = float3(0, 0, 0);
#else
	o.uv = v.uv;
#endif
#if defined(CUSTOM_GEOMETRY_INTERPOLATORS)
	o.barycentricCoordinates = 0;
#endif
	TRANSFER_SHADOW(o);
	ComputeVertexLightColor(o);
	return o;
}

#if defined(VOXEL_INCLUDED)
	UNITY_DECLARE_TEX2DARRAY(_TexArr);
#else
	sampler2D _MainTex;
#endif

float4 MyFragmentProgram(InterpolatorsVertex  i) : SV_Target
{
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

	float3 albedo = 0;

#if defined (VOXEL_INCLUDED)
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

	if (dotX > dotY && dotX > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.zy, ind));
	else if (dotY > dotX && dotY > dotZ)
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xz, ind));
	else
		albedo = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.localPos.xy, ind));

#else
	albedo = tex2D(_MainTex, i.uv).rgb * _Tint;
	//albedo = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * _Tint;
#endif
	//float minBary = min(barys.x, min(barys.y, barys.z));
	//minBary = smoothstep(0, 0.02, minBary);
	//albedo = albedo * minBary;

	//float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
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