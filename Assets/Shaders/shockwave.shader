Shader "Custom/shockwave" {
	SubShader {
   		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
				float4 texcoord : TEXCOORD0;
			};

 			struct v2f
 			{
 				float4 pos:SV_POSITION;
				float4 uvgrab : TEXCOORD0;
 			};
 			
			float _CurrentTime;
			float3  _CamUp;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
   
            v2f vert(appdata_custom v)
            {
				float distortion_level = 0.1;
				float elapsed = (_CurrentTime - v.texcoord.x) + 0.02;
				float radius = elapsed * 8;
				float theta = v.texcoord.y;

				float s = sin(theta);
				float c = cos(theta);
				float3 tv = float3(radius * c, radius * s, 0);
				float3 eye = normalize(ObjSpaceViewDir(v.vertex));
				float3 up = _CamUp;
				float3 side = cross(eye, up);
				float3 vec2 = (tv.x*side + tv.y*up) + v.vertex.xyz;

            	v2f o;
            	o.pos = mul(UNITY_MATRIX_MVP, float4(vec2, 1));
				o.uvgrab.xy = (float2(o.pos.x+c*distortion_level, o.pos.y+s*distortion_level) + o.pos.w) * 0.5;
				o.uvgrab.zw = o.pos.zw;
            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				fixed4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				return col;
            }

            ENDCG
        }
    }
}
