Shader "Custom/vrsprite" {
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
    }
	SubShader {
   		Tags { "Queue"="Overlay+100" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		ZTest Always
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha // alpha blending
		
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
 			#pragma target 3.0
 			
 			#include "UnityCG.cginc"

            uniform sampler2D _MainTex;
			uniform fixed4 _Colors[13];

 			struct appdata_custom {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

 			struct v2f {
 				float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
 				fixed4 color:COLOR;
 			};
   
			float3 _CamUp;
			float3 _CamPos;

            v2f vert(appdata_custom v)
            {
				// float dist = 10;
				// float3 diff = v.vertex.xyz - _CamPos;
				// float3 tv = _CamPos + normalize(diff) * dist;
				float3 tv = v.vertex.xyz;
				float size = 0.005;
				size *= v.vertex.z;
				float3 up = _CamUp;
				float3 eye = normalize(ObjSpaceViewDir(v.vertex));
				float3 side = cross(eye, up);
				float3 vec = (v.normal.x*side + v.normal.y*up) * size;
				tv.xyz += vec;

            	v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, float4(tv.xyz, 1));
				o.texcoord = v.texcoord.xy;
				int color_index = (int)v.normal.z;
				o.color = _Colors[color_index];
            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				half4 col = tex2D(_MainTex, i.texcoord) * i.color;
				return col;
            }

            ENDCG
        }
    }
}
