// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyShader/Opaque/Lightmap"{
	Properties{
	  _MainTex("Base (RGB)", 2D) = "white" {}
	  _LightMap("Lightmap (RGB)", 2D) = "white" {}
	}

		SubShader{
			Tags{ "RenderType" = "Opaque" }
		  LOD 100

		  Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
			  float4 vertex : POSITION;
			  half2 uv : TEXCOORD0;
			  half2 uv1: TEXCOORD1;
			};

			struct v2f {
			  float4 pos : SV_POSITION;
			  half2 uv :TEXCOORD0;
			  half2 uv1:TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _LightMap;

			v2f vert(appdata v)
			{
			  v2f o;
			  o.pos = UnityObjectToClipPos(v.vertex);
			  o.uv = v.uv;
			  o.uv1 = v.uv1;
			  return o;
			}

			half4 frag(v2f i) : COLOR
			{
			  half4 c = tex2D(_MainTex, i.uv);
			  half4 c1 = tex2D(_LightMap, i.uv1);
			  half4 o;
			  o.rgb = c.rgb * c1.rgb;
			  o.a = 1;
			  return o;
			}
			ENDCG
		  }
	}
}