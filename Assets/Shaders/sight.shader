
Shader "Custom/sight" {
    Properties {
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

 			struct appdata_custom {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float4 texcoord2 : TEXCOORD1;
			};

 			struct v2f {
 				float4 pos : SV_POSITION;
 				fixed4 color : COLOR;
 			};
 			
			float _CurrentTime;
			float3 _CamUp;
			float3 _CamPos;

            v2f vert(appdata_custom v)
            {
				float size = 200;
				float dist = 100;

				float elapsed = _CurrentTime - v.texcoord2.x;
				elapsed = clamp(elapsed*4, 0, 1);
				size *= (1 - elapsed);

				float3 diff = v.vertex.xyz - _CamPos;
				float3 v0 = _CamPos + normalize(diff) * dist;

				float3 up = _CamUp;
				float3 eye = normalize(ObjSpaceViewDir(v.vertex));
				float3 side = cross(eye, up);

				// rotate
				float3 vec = ((v.texcoord.x-0.5f)*side + (v.texcoord.y-0.5f)*up) *size;
				float3 n = eye;
				// float theta = elapsed;
				float theta = 0; // not rotate for the time being.
				/* rotate matrix for an arbitrary axis
				 * Vx*Vx*(1-cos) + cos  	Vx*Vy*(1-cos) - Vz*sin	Vz*Vx*(1-cos) + Vy*sin;
				 * Vx*Vy*(1-cos) + Vz*sin	Vy*Vy*(1-cos) + cos 	Vy*Vz*(1-cos) - Vx*sin;
				 * Vz*Vx*(1-cos) - Vy*sin	Vy*Vz*(1-cos) + Vx*sin	Vz*Vz*(1-cos) + cos;
				 */
				float s = sin(theta);
				float c = cos(theta);
				float nx1c = n.x*(1-c);
				float ny1c = n.y*(1-c);
				float nz1c = n.z*(1-c);
				float nxs = n.x*s;
				float nys = n.y*s;
				float nzs = n.z*s;
				float3 rvec;
				rvec.x = ((n.x*nx1c + c) * vec.x +
						  (n.x*ny1c - nzs) * vec.y +
						  (n.z*nx1c + nys) * vec.z);
				rvec.y = ((n.x*ny1c + nzs) * vec.x +
						  (n.y*ny1c + c) * vec.y +
						  (n.y*nz1c - nxs) * vec.z);
				rvec.z = ((n.z*nx1c - nys) * vec.x +
						  (n.y*nz1c + nxs) * vec.y +
						  (n.z*nz1c + c) * vec.z);
				float3 tv = rvec + v0;

            	v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, float4(tv, 1));
				o.color = v.color;
            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				return i.color;
            }

            ENDCG
        }
    }
}
