// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SSR_ASE"
{
	Properties
	{

		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
			#if ASE_SRP_VERSION >=140007
            #pragma multi_compile _ _FORWARD_PLUS
			#endif
		
			
			
			#if ASE_SRP_VERSION >=140007
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif


			#if defined(LIGHTMAP_ON)
				#define DECLARE_LIGHTMAP_OR_SH_FOG(staticLightmapUV_fogFactor, vertexSH_fogFactor, index) float4 staticLightmapUV_fogFactor : TEXCOORD##index
			#else
				#define DECLARE_LIGHTMAP_OR_SH_FOG(staticLightmapUV_fogFactor, vertexSH_fogFactor, index) float4 vertexSH_fogFactor : TEXCOORD##index
			#endif

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#include "Packages/com.worldsystem/Shader/SSRR/SSR_Surface_Pass.hlsl"
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#define SSR_JITTER 1


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD0;
				#endif
				#if defined(ASE_FOG) || defined(REQUIRE_BAKEDGI)
					DECLARE_LIGHTMAP_OR_SH_FOG(staticLightmapUV_fogFactor, vertexSH_fogFactor, 1);
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _BaseMap_TexelSize;
			float4 _BaseMap_MipInfo;

			

			float3 TransformObjectToWorldNormal_Ref33_g1195( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			
			float3 SampleSHVertex_Ref376_g1( float3 normalWS )
			{
				return SampleSHVertex(normalWS);
			}
			
			float ComputeFogFactor_Ref381_g1( float4 positionCS )
			{
				return ComputeFogFactor(positionCS.z);
			}
			
			float4 GetShadowCoord_Ref384_g1( float4 positionCS, float3 positionWS )
			{
				#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
				    return ComputeScreenPos(positionCS);
				#else
				    return TransformWorldToShadowCoord(positionWS);
				#endif
			}
			
			float2 GetNormalizedScreenSpaceUV_Ref( float4 positionCS )
			{
				return GetNormalizedScreenSpaceUV(positionCS);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 temp_output_31_0_g1195 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g1195 = temp_output_31_0_g1195;
				float3 localTransformObjectToWorldNormal_Ref33_g1195 = TransformObjectToWorldNormal_Ref33_g1195( normalOS33_g1195 );
				float3 normalizeResult140_g1195 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g1195 );
				float3 temp_output_515_34_g1 = normalizeResult140_g1195;
				float3 VertexNormalWS314_g1 = temp_output_515_34_g1;
				float3 normalWS376_g1 = VertexNormalWS314_g1;
				float3 localSampleSHVertex_Ref376_g1 = SampleSHVertex_Ref376_g1( normalWS376_g1 );
				
				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float4 vertexPositionCS382_g1 = CS1_g1204;
				float4 positionCS381_g1 = vertexPositionCS382_g1;
				float localComputeFogFactor_Ref381_g1 = ComputeFogFactor_Ref381_g1( positionCS381_g1 );
				
				float4 positionCS384_g1 = vertexPositionCS382_g1;
				float3 temp_output_345_7_g1 = WS1_g1204;
				float3 vertexPositionWS386_g1 = temp_output_345_7_g1;
				float3 positionWS384_g1 = vertexPositionWS386_g1;
				float4 localGetShadowCoord_Ref384_g1 = GetShadowCoord_Ref384_g1( positionCS384_g1 , positionWS384_g1 );
				
				float4 temp_output_10_313 = vertexPositionCS382_g1;
				
				float3 break138_g1 = VertexNormalWS314_g1;
				float vertexToFrag323_g1 = break138_g1.x;
				o.ase_texcoord2.x = vertexToFrag323_g1;
				float vertexToFrag324_g1 = break138_g1.y;
				o.ase_texcoord2.y = vertexToFrag324_g1;
				float vertexToFrag325_g1 = break138_g1.z;
				o.ase_texcoord2.z = vertexToFrag325_g1;
				float3 break310_g1 = vertexPositionWS386_g1;
				float vertexToFrag320_g1 = break310_g1.x;
				o.ase_texcoord2.w = vertexToFrag320_g1;
				float vertexToFrag321_g1 = break310_g1.y;
				o.ase_texcoord3.x = vertexToFrag321_g1;
				float vertexToFrag322_g1 = break310_g1.z;
				o.ase_texcoord3.y = vertexToFrag322_g1;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

				//接口
				float2 StaticLightmapUV = (v.ase_texcoord1.xy*(unity_LightmapST).xy + (unity_LightmapST).zw);
				float3 VertexSH = localSampleSHVertex_Ref376_g1;
				#ifdef ASE_FOG
					float FogFactor = localComputeFogFactor_Ref381_g1;
				#else
					float FogFactor = 0;
				#endif
				float4 ShadowCoord = localGetShadowCoord_Ref384_g1;


				//输出,规范:此处不允许做任何计算仅允许输出
				#if defined(ASE_FOG) || defined(REQUIRE_BAKEDGI)
					#if defined(LIGHTMAP_ON)
						o.staticLightmapUV_fogFactor = float4(StaticLightmapUV,0,FogFactor);
					#else
						o.vertexSH_fogFactor = float4(VertexSH,FogFactor);
					#endif
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					o.shadowCoord = ShadowCoord;
				#endif
			
				o.positionCS = temp_output_10_313;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord1 = v.ase_texcoord1;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				/*ase_local_var:StaticLightmapUV*/float2 StaticLightmapUV = 0;
				/*ase_local_var:VertexSH*/half3 VertexSH = 0;
				/*ase_local_var:FogFactor*/float FogFactor = 0;
				/*ase_local_var:ShadowCoords*/float4 ShadowCoords = 0;
				/*ase_local_var:PositionDS*/float4 PositionDS = IN.positionCS;

				//初始化
				//ShadowCoords的初始化因为需要worldposition,已将将代码移至ASE中
				#if defined(REQUIRE_BAKEDGI) || defined(ASE_FOG)
					#if defined(LIGHTMAP_ON)
						StaticLightmapUV = IN.staticLightmapUV_fogFactor.xy;
						FogFactor = IN.staticLightmapUV_fogFactor.w;
					#else
						VertexSH = IN.vertexSH_fogFactor.xyz;
						FogFactor = IN.vertexSH_fogFactor.w;
					#endif
				#endif

				float4 temp_cast_0 = (1.0).xxxx;
				float4 positionCS289_g1 = PositionDS;
				float2 localGetNormalizedScreenSpaceUV_Ref289_g1 = GetNormalizedScreenSpaceUV_Ref( positionCS289_g1 );
				float3 appendResult291_g1 = (float3(localGetNormalizedScreenSpaceUV_Ref289_g1 , PositionDS.z));
				float3 PositionSS12 = appendResult291_g1;
				float2 uv9 = PositionSS12.xy;
				float vertexToFrag323_g1 = IN.ase_texcoord2.x;
				float vertexToFrag324_g1 = IN.ase_texcoord2.y;
				float vertexToFrag325_g1 = IN.ase_texcoord2.z;
				float3 appendResult142_g1 = (float3(vertexToFrag323_g1 , vertexToFrag324_g1 , vertexToFrag325_g1));
				float3 normalizeResult459_g1 = normalize( appendResult142_g1 );
				float3 NormalWS388_g1 = normalizeResult459_g1;
				float3 vNormalWS14 = NormalWS388_g1;
				float3 temp_output_31_0_g1229 = vNormalWS14;
				float3 worldToViewDir68_g1229 = mul( UNITY_MATRIX_V, float4( temp_output_31_0_g1229, 0 ) ).xyz;
				float3 normalizeResult142_g1229 = normalize( worldToViewDir68_g1229 );
				float3 _Vector4 = float3(1,1,-1);
				float3 normalVS9 = ( normalizeResult142_g1229 * _Vector4 );
				float localPosition1_g1228 = ( 0.0 );
				float localGetPositionTransformSpaceFromWorld1_g1226 = ( 0.0 );
				float vertexToFrag320_g1 = IN.ase_texcoord2.w;
				float vertexToFrag321_g1 = IN.ase_texcoord3.x;
				float vertexToFrag322_g1 = IN.ase_texcoord3.y;
				float3 appendResult311_g1 = (float3(vertexToFrag320_g1 , vertexToFrag321_g1 , vertexToFrag322_g1));
				float3 positionWS402_g1 = appendResult311_g1;
				float3 PositionWS19 = positionWS402_g1;
				float3 temp_output_14_0_g1220 = PositionWS19;
				float3 positionWS1_g1226 = temp_output_14_0_g1220;
				Position position1_g1226 =(Position)0;
				GetPositionTransformSpaceFromWorld_float( positionWS1_g1226 , position1_g1226 );
				Position position1_g1228 =(Position)position1_g1226;
				float3 OS1_g1228 = float3( 0,0,0 );
				float3 WS1_g1228 = float3( 0,0,0 );
				float3 VS1_g1228 = float3( 0,0,0 );
				float4 CS1_g1228 = float4( 0,0,0,0 );
				float4 NDC1_g1228 = float4( 0,0,0,0 );
				float3 SS1_g1228 = float3( 0,0,0 );
				float4 DS1_g1228 = float4( 0,0,0,0 );
				float3 LS1_g1228 = float3( 0,0,0 );
				float4 SC1_g1228 = float4( 0,0,0,0 );
				Position_float( position1_g1228 , OS1_g1228 , WS1_g1228 , VS1_g1228 , CS1_g1228 , NDC1_g1228 , SS1_g1228 , DS1_g1228 , LS1_g1228 , SC1_g1228 );
				float3 rayStart9 = ( VS1_g1228 * _Vector4 );
				float4 localSSR_Pass9 = SSR_Pass( uv9 , normalVS9 , rayStart9 );
				float4 lerpResult38 = lerp( temp_cast_0 , localSSR_Pass9 , (localSSR_Pass9).w);
				

				float3 Color = lerpResult38.xyz;
				float Alpha = 1;
				float AlphaClipThresholdShadow = 0.5;
				float3 WorldNormal = float3(0, 0, 1);
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float Roughness = 0;
                float Metallic = 0;
				
				#ifdef _ALPHATEST_ON
					clip( Alpha - 0.5 );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.positionCS, Color);
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, FogFactor );
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;

				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			

			float3 TransformObjectToWorldNormal_Ref33_g1195( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localOffsetShadow412_g1 = ( 0.0 );
				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float3 temp_output_345_7_g1 = WS1_g1204;
				float3 positionWS412_g1 = temp_output_345_7_g1;
				float3 temp_output_31_0_g1195 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g1195 = temp_output_31_0_g1195;
				float3 localTransformObjectToWorldNormal_Ref33_g1195 = TransformObjectToWorldNormal_Ref33_g1195( normalOS33_g1195 );
				float3 normalizeResult140_g1195 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g1195 );
				float3 temp_output_515_34_g1 = normalizeResult140_g1195;
				float3 normalWS412_g1 = temp_output_515_34_g1;
				float4 positionCS412_g1 = float4( 0,0,0,0 );
				{
				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS412_g1);
				#else
				float3 lightDirectionWS = _LightDirection;
				#endif
				positionCS412_g1 = TransformWorldToHClip(ApplyShadowBias(positionWS412_g1, normalWS412_g1, lightDirectionWS));
				#if UNITY_REVERSED_Z
				positionCS412_g1.z = min(positionCS412_g1.z, UNITY_NEAR_CLIP_VALUE);
				#else
				positionCS412_g1.z = max(positionCS412_g1.z, UNITY_NEAR_CLIP_VALUE);
				#endif
				}
				

				o.positionCS = positionCS412_g1;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				

				float Alpha = 1;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - 0.5);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float4 vertexPositionCS382_g1 = CS1_g1204;
				float4 temp_output_10_313 = vertexPositionCS382_g1;
				

				o.positionCS = temp_output_10_313;


				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				

				float Alpha = 1;

				#ifdef _ALPHATEST_ON
					clip(Alpha - 0.5);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif
				return IN.positionCS.z;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			

			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float4 vertexPositionCS382_g1 = CS1_g1204;
				float4 temp_output_10_313 = vertexPositionCS382_g1;
				

				o.positionCS = temp_output_10_313;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = 0.5;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			

			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float4 vertexPositionCS382_g1 = CS1_g1204;
				float4 temp_output_10_313 = vertexPositionCS382_g1;
				

				o.positionCS = temp_output_10_313;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = 0.5;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			
			#pragma vertex vert
			#pragma fragment frag

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;

				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				float localPosition1_g1204 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g1203 = ( 0.0 );
				float3 temp_output_14_0_g1196 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g1203 = temp_output_14_0_g1196;
				Position position1_g1203 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g1203 , position1_g1203 );
				Position position1_g1204 =(Position)position1_g1203;
				float3 OS1_g1204 = float3( 0,0,0 );
				float3 WS1_g1204 = float3( 0,0,0 );
				float3 VS1_g1204 = float3( 0,0,0 );
				float4 CS1_g1204 = float4( 0,0,0,0 );
				float4 NDC1_g1204 = float4( 0,0,0,0 );
				float3 SS1_g1204 = float3( 0,0,0 );
				float4 DS1_g1204 = float4( 0,0,0,0 );
				float3 LS1_g1204 = float3( 0,0,0 );
				float4 SC1_g1204 = float4( 0,0,0,0 );
				Position_float( position1_g1204 , OS1_g1204 , WS1_g1204 , VS1_g1204 , CS1_g1204 , NDC1_g1204 , SS1_g1204 , DS1_g1204 , LS1_g1204 , SC1_g1204 );
				float4 vertexPositionCS382_g1 = CS1_g1204;
				float4 temp_output_10_313 = vertexPositionCS382_g1;
				

				o.positionCS = temp_output_10_313;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0
			#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
			#endif
				 )
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
                float3 WorldNormal = float3(0, 0, 1);

				#if _ALPHATEST_ON
					clip(surfaceDescription.Alpha - 0.5);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float3 normalWS = normalize(WorldNormal);
					float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					float3 normalWS = WorldNormal;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}

			ENDHLSL
		}


	
	}
	
	CustomEditor "LogicalSGUI.LogicalSGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.FunctionNode;10;-749,52.5;Inherit;False;Model;-1;;1;db2a50576abc55b4182a85385bad6f66;8,430,0,511,0,512,0,514,0,513,0,505,0,431,0,552,0;7;307;FLOAT3;0,0,0;False;432;FLOAT3;0,0,0;False;377;FLOAT3;0,0,0;False;433;FLOAT3;0,0,0;False;554;FLOAT3;0,0,0;False;555;FLOAT3;0,0,0;False;556;FLOAT3;0,0,0;False;39;FLOAT2;543;FLOAT3;546;FLOAT;547;FLOAT4;347;FLOAT4;313;FLOAT4;413;FLOAT4;415;FLOAT2;273;FLOAT2;275;FLOAT3;277;FLOAT;410;FLOAT3;285;FLOAT4;243;FLOAT4;178;FLOAT3;0;FLOAT3;4;FLOAT3;5;FLOAT3x3;6;FLOAT3;312;FLOAT3;156;FLOAT3;271;FLOAT3;229;FLOAT3;7;FLOAT3;185;FLOAT3;194;FLOAT3;197;FLOAT3;11;FLOAT;21;FLOAT;540;FLOAT;541;FLOAT;542;FLOAT;22;INT;202;FLOAT4;224;FLOAT4;225;FLOAT3;220;INT;216;INT;217;INT;219
Node;AmplifyShaderEditor.TemplateMultiPassSwitchNode;11;-201,171.5;Inherit;False;0;0;9;9;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-202.2999,431.8524;Inherit;False;vNormalWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-199.2999,500.8524;Inherit;False;TBN;-1;True;1;0;FLOAT3x3;0,0,0,1,0,0,1,0,1;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-198.2999,566.8524;Inherit;False;PositionSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-195.8685,636.0928;Inherit;False;PositionWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;13;-1345.652,-869.6012;Inherit;False;12;PositionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;20;-2020.219,-622.3605;Inherit;False;19;PositionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;21;-1829.219,-621.3605;Inherit;False;PositionTransformSpace;-1;;1220;be470b7f25071594faeb0c9db5956ba5;1,13,1;1;14;FLOAT3;0,0,0;False;9;FLOAT3;0;FLOAT3;7;FLOAT3;8;FLOAT4;9;FLOAT3;10;FLOAT4;36;FLOAT4;11;FLOAT3;12;FLOAT4;35
Node;AmplifyShaderEditor.GetLocalVarNode;18;-1955.651,-751.6011;Inherit;False;17;TBN;1;0;OBJECT;;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.FunctionNode;16;-1763.651,-817.6011;Inherit;False;NormalTransformSpace;-1;;1229;49cf2c8d471f5a8469f03384352a304f;12,65,1,91,1,40,1,78,1,116,1,30,1,135,0,138,0,141,0,144,0,147,0,134,0;2;31;FLOAT3;0,0,0;False;52;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;6;FLOAT3;38;FLOAT3;34;FLOAT3;39;FLOAT3;36;FLOAT3;37;FLOAT3;35
Node;AmplifyShaderEditor.GetLocalVarNode;15;-1954.651,-819.6011;Inherit;False;14;vNormalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;24;-1551.22,-513.3605;Inherit;False;Constant;_Vector4;Vector 4;0;0;Create;True;0;0;0;False;0;False;1,1,-1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-1343.206,-771.9311;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1332.22,-576.3606;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1278.206,-380.9311;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;9;-1003.352,-531.9535;Inherit;False; ;4;File;3;True;uv;FLOAT2;0,0;In;;Inherit;False;True;normalVS;FLOAT3;0,0,0;In;;Inherit;False;True;rayStart;FLOAT3;0,0,0;In;;Inherit;False;SSR_Pass;False;False;0;bb7bf17031706d94a86a6549b93d084b;False;3;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;44;-626.9662,-466.8042;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-714.4802,-592.8974;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;45;-703.3835,-391.765;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;38;-410.5711,-429.894;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ExtraPrePass;0;0;ExtraPrePass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;SceneSelectionPass;0;5;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ScenePickingPass;0;6;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthNormals;0;7;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;SmoothnessMetallic;0;8;SmoothnessMetallic;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;130,-20;Float;False;True;-1;2;LogicalSGUI.LogicalSGUI;0;14;SSR_ASE;fa16f3565aea0f0448ed74c1d4f5407b;True;Forward;0;1;Forward;13;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;2;Include;;False;;Native;False;0;0;;Define;SSR_JITTER 1;False;;Custom;True;0;0;Forward;;0;0;Standard;20;Surface;0;0;  Blend;0;0;Forward Only;0;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;Meta Pass;0;0;RoughnessMetallic Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;0;9;False;True;True;True;False;True;True;True;False;False;;False;0
WireConnection;11;0;10;313
WireConnection;11;1;10;313
WireConnection;11;2;10;413
WireConnection;11;3;10;313
WireConnection;11;4;10;415
WireConnection;11;5;10;313
WireConnection;11;6;10;313
WireConnection;11;7;10;313
WireConnection;11;8;10;313
WireConnection;14;0;10;0
WireConnection;17;0;10;6
WireConnection;12;0;10;7
WireConnection;19;0;10;312
WireConnection;21;14;20;0
WireConnection;16;31;15;0
WireConnection;16;52;18;0
WireConnection;26;0;16;39
WireConnection;26;1;24;0
WireConnection;22;0;21;8
WireConnection;22;1;24;0
WireConnection;9;0;13;0
WireConnection;9;1;26;0
WireConnection;9;2;22;0
WireConnection;44;0;9;0
WireConnection;45;0;9;0
WireConnection;38;0;39;0
WireConnection;38;1;44;0
WireConnection;38;2;45;0
WireConnection;1;0;38;0
WireConnection;1;7;10;543
WireConnection;1;8;10;546
WireConnection;1;9;10;547
WireConnection;1;12;10;347
WireConnection;1;13;11;0
ASEEND*/
//CHKSM=AE911B106FCD7D63618F861034A3E2AA2AE15905