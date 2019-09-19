Shader "Custom/FirstLighting"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
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

			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert
			#pragma fragment frag

			#define FORWARD_BASE_PASS

			#include "Lighting.cginc"
		ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
		ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "Shadows.cginc"

			ENDCG
		}
    }
}
