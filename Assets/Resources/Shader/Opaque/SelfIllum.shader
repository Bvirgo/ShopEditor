Shader "MyShader/Opaque/SelfIllum" 
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
		LOD 200
		Material
			{
				Diffuse [_Color]
				Ambient (1,1,1,1)
				Emission [_Emission]
			}
		Pass
		{			
			Lighting On
		//	Tags { "LightMode" = "ForwardAdd" }
			SetTexture [_MainTex] 
			{
				constantColor [_Color]
				Combine texture * primary DOUBLE, texture * constant 
			}
		}
   }
   FallBack "Diffuse", 1
}