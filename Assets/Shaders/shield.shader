Shader "Custom/shield" {
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
    }
	SubShader {
   		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha One
		// Blend SrcAlpha OneMinusSrcAlpha // alpha blending
		
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
 			#pragma target 3.0
 			
 			#include "UnityCG.cginc"

            uniform sampler2D _MainTex;
			uniform fixed4 _Colors[3];

 			struct appdata_custom {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float4 texcoord2 : TEXCOORD1;
			};

 			struct v2f {
 				float4 pos : SV_POSITION;
 				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
 			};
 			
			float  _CurrentTime;
			float3  _CamUp;
   
            v2f vert(appdata_custom v)
            {
				float size = 1;
				float elapsed = (_CurrentTime - v.texcoord2.x);
				float3 up = float3(0,1,0);
				float3 tangent = normalize(cross(v.normal.xyz, up));
				float3 binormal = cross(v.normal.xyz, tangent);
				float3 vec = ((v.texcoord.x-0.5f)*tangent + (v.texcoord.y-0.5f)*binormal) *size + v.vertex.xyz;

            	v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, float4(vec.xyz,1));
				int color_index = (int)v.texcoord2.y;
				o.color = _Colors[color_index];
				o.color.a = clamp(1-elapsed*4, 0, 1);
				o.texcoord = MultiplyUV(UNITY_MATRIX_TEXTURE0,
										v.texcoord);
            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				return tex2D(_MainTex, i.texcoord) * i.color;
            }

            ENDCG
        }
    }
}
