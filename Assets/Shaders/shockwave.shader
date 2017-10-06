// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/shockwave" {
	SubShader {
   		Tags { "Queue"="Transparent+10" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		// Blend SrcAlpha OneMinusSrcAlpha // alpha blending
		// Blend SrcAlpha One 				// alpha additive
		
		GrabPass { }

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
 			#pragma target 3.0
 
 			#include "UnityCG.cginc"
 			struct appdata_custom {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

 			struct v2f
 			{
 				float4 pos:SV_POSITION;
				float4 uvgrab : TEXCOORD0;
				float2 uv2:TEXCOORD1;
 			};
 			
			float _CurrentTime;
			float3  _CamUp;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
   
            v2f vert(appdata_custom v)
            {
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1;
				#else
				float scale = 1;
				#endif		

				float elapsed = (_CurrentTime - v.normal.x) + 0.02;
				float radius = elapsed * 32;
				float theta = v.normal.y;
				float distortion_level = v.normal.z*0.5;
				float luminance_level = 1 + v.normal.z;

				float s = sin(theta);
				float c = cos(theta);
				float3 tv = float3(radius * c, radius * s, 0);
				float3 eye = normalize(ObjSpaceViewDir(v.vertex));
				float3 up = _CamUp;
				float3 side = cross(eye, up);
				float3 vec2 = (tv.x*side + tv.y*up) + v.vertex.xyz;

            	v2f o;
            	o.pos = UnityObjectToClipPos(float4(vec2, 1));
				o.uvgrab.xy = (((float2(o.pos.x, o.pos.y*scale) + o.pos.w) * 0.5) +
							   float2(c, s)*distortion_level);
				o.uvgrab.zw = o.pos.zw;
				o.uv2.x = luminance_level;
				o.uv2.y = 0;
            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				fixed4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab))*i.uv2.x;
				return col;
            }

            ENDCG
        }
    }
}
