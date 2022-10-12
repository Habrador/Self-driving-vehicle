//Car paint with just one pass
Shader "Custom/CarPaintSimple" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		//_Smoothness ("Smoothness", Range(0,1)) = 0.5
		_CarPaintNoiseTex ("Car Paint Noise", 2D) = "white" {}
		//_CarPaintNoiseScale ("Car Paint Noise Scale", Range(0,1)) = 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		
		#pragma surface surf StandardSpecular fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		half _Smoothness;
		fixed4 _Color;
		sampler2D _CarPaintNoiseTex;
		float _CarPaintNoiseScale;

		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) 
		{
			//Should be hardcoded in the final version so we only need to change the parameters here
			_Smoothness = 0.6;
			_CarPaintNoiseScale = 0.05;
			
			//Color with ambient occlusion in it
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			//Color with facing angle in it
			float NdotV = dot(normalize(IN.viewDir), o.Normal);

			c *= NdotV * 1.0;

			//Car paint noise which is affecting the specular color
			fixed3 specularColor = tex2D(_CarPaintNoiseTex, IN.uv_MainTex / _CarPaintNoiseScale) * 0.6;

			
			//Output
			o.Albedo = c.rgb;
			o.Specular = specularColor;
			o.Smoothness = _Smoothness;
			o.Alpha = c.a;
		}
		ENDCG		
	}
	FallBack "Diffuse"
}
