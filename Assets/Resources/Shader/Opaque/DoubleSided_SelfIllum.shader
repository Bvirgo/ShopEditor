Shader "MyShader/Opaque/DoubleSided_SelfIllum" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (0.8,0.8,0.8,1)
		_Emission ("Emmisive Color", Color) = (0.3,0.3,0.3,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{   
		Tags { "RenderType"="Opaque" }
		Material
			{
				Diffuse [_Color]
				Ambient (1,1,1,1)
				Emission [_Emission]
			}
		Pass
		{			
			
			Lighting On
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha

			SetTexture [_MainTex] 
			{
				constantColor [_Color]
				Combine texture * primary DOUBLE, texture * constant 
			}
		}
   }
   FallBack "Diffuse", 1
}