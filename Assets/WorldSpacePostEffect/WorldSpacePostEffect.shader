Shader "Hidden/WorldSpacePostEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			//depth buffer
			sampler2D _CameraDepthTexture;

			//view/projection matrices proved by VolumetricSphere.cs (in none vr, only left eye is used)
			float4x4 _LeftEyeToWorld;
			float4x4 _RightEyeToWorld;
			float4x4 _LeftEyeProjection;
			float4x4 _RightEyeProjection;

			//simple vs output
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			//simple vertex shader
			//transforms vertex from 0:1 into screen space -1:1
			//flips y of tex coord 
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = v.vertex * float4(2,2,1,1) + float4(-1,-1,0,0);
				o.uv = v.texcoord;
				o.uv.y = 1.0f - o.uv.y; //blit flips the uv for some reason
				return o;
			}

			//world space fragment shader
			fixed4 frag (v2f i) : SV_Target
			{
				//read none linear depth texture, accounting for 
				float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv)); // non-linear Z

				//pick one of the passed in projection/view matrices based on stereo eye selection (always left if not vr)
				float4x4 proj, eyeToWorld;
				if (unity_StereoEyeIndex == 0)
				{
					proj = _LeftEyeProjection;
					eyeToWorld = _LeftEyeToWorld;
				}
				else
				{
					proj = _RightEyeProjection;
					eyeToWorld = _RightEyeToWorld;
				}

				//bit of matrix math to take the screen space coord (u,v,depth) and transform to world space
				float2 uvClip = i.uv * 2.0 - 1.0;
				float4 clipPos = float4(uvClip, d, 1.0);
				float4 viewPos = mul(proj, clipPos); // inverse projection by clip position
				viewPos /= viewPos.w; // perspective division
				float3 worldPos = mul(eyeToWorld, viewPos).xyz;

				//output result
				fixed3 color = pow(abs(cos(worldPos * UNITY_PI * 4)), 20); // visualize grid
				return fixed4(color, 1);
			}

			ENDCG
		}
	}
}
