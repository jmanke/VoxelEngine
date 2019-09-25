Shader "Custom/VoxelTerrain"
{
    Properties
    {
		_TexArr("Tex", 2DArray) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			#pragma require 2darray
			#pragma target 4.0

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram
			#pragma geometry MyGeometryProgram

			#define FORWARD_BASE_PASS

			#include "MyLighting.cginc"
		ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma require 2darray
			#pragma target 4.0

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram
			#pragma geometry MyGeometryProgram

			#include "MyLighting.cginc"
		ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma require 2darray
			#pragma target 4.0

			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram
			//#pragma geometry MyGeometryProgram

			#include "MyShadows.cginc"

			ENDCG
		}
    }
}
