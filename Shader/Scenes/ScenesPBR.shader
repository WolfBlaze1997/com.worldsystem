// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ScenesPBR"
{
	Properties
	{

		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[BuiltinEnum(_, CullMode)]_Cull("剔除模式", Float) = 2
		[Toggle(_ALPHATEST_ON)] ALPHATEST_ON("Alpha裁剪", Float) = 0
		_AlphaClipOffset("Alpha剪辑偏移", Range( -1 , 1)) = 0
		[LogicalTex(_,true,RGB_A,_)]_BaseColorMap("基础贴图", 2D) = "white" {}
		[MainColor]_BaseColor("基础颜色", Color) = (1,1,1,1)
		_SpecColor("高光颜色", Color) = (1,1,1,1)
		[LogicalTex(_,false,RG_B_A,_)]_NormalMap("NRA贴图", 2D) = "bump" {}
		_NormalStrength("(N)法线强度", Range( 0 , 2)) = 1
		_Roughness1("(R)粗糙度", Range( 0 , 2)) = 1
		_OcclusionBaked("(A)环境光遮蔽", Range( -1 , 1)) = 0
		[LogicalTex(_,false,RGB_A,_)]Smoothness("EM贴图", 2D) = "white" {}
		_EmissionStrength0("(E)自发光强度", Range( 0 , 5)) = 1
		_Metallic01("(M)金属度", Range( 0 , 2)) = 1
		[Toggle(_SCREENREFLECTION_ON)] _ScreenReflection("屏幕空间实时反射", Float) = 1
		_MoistDiffuseCoeff("湿润漫反射系数", Range( 0.1 , 1)) = 0.3
		_MoistRoughnessCoeff("湿润粗糙度系数", Range( 0.05 , 1)) = 0.2
		_MoistAccumulatedwaterCoeff("积水系数", Range( 0 , 1)) = 1
		_AccumulatedwaterReflectStrength("积水环境反射强度", Range( 0 , 1)) = 0.5
		_OnAccumulatedWaterEnvEeflectAtten("非积水区域环境反射衰减", Range( 0 , 1)) = 0
		_OnAccumulatedWaterLightingAtten("非积水区域直接光照衰减", Range( 0 , 1)) = 0
		[LogicalKeywordList(_)]_Float6("关键字列表", Float) = 0


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

		Cull [_Cull]
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
			Tags { "LightMode"="UniversalForward" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#define ASE_SRP_VERSION 140011
			#define ASE_USING_SAMPLING_MACROS 1


			CBUFFER_START(UnityPerMaterial)
			float4 _SpecColor;
			float4 _BaseColorMap_ST;
			float4 _BaseColor;
			float _OnAccumulatedWaterLightingAtten;
			float _OnAccumulatedWaterEnvEeflectAtten;
			float _MoistDiffuseCoeff;
			float _MoistRoughnessCoeff;
			float _Roughness1;
			float _Float6;
			float _AccumulatedwaterReflectStrength;
			float _Metallic01;
			float _NormalStrength;
			float _OcclusionBaked;
			float _MoistAccumulatedwaterCoeff;
			float _Cull;
			float _EmissionStrength0;
			float _AlphaClipOffset;
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
			#pragma multi_compile_fragment __ _ACCUMULATEDWATER_ON
			#pragma multi_compile_fragment __ _RAINDROPS_ON
			#pragma multi_compile_local_fragment __ _SCREENREFLECTION_ON
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#define REQUIRE_BAKEDGI 1
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;
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
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _BaseMap_TexelSize;
			float4 _BaseMap_MipInfo;

			TEXTURE2D(_NormalMap);
			TEXTURE2D(_BaseColorMap);
			TEXTURE2D(_AccumulatedWaterMask);
			float _AccumulatedWaterMaskTiling;
			float _GlobalMoist;
			float _AccumulatedWaterContrast;
			float _AccumulatedWaterSteepHillExtinction;
			TEXTURE2D(_RipplesNormalAtlas);
			float _RipplesMainTiling;
			float4 _XColumnsYRowsZSpeedWStrartFrame;
			float _RipplesMainStrength;
			TEXTURE2D(_WaterWaveNormal);
			float _WaterWaveMainSpeed;
			float _WaterWaveRotate;
			float _WaterWaveMainTiling;
			float _WaterWaveMainStrength;
			float _WaterWaveDetailSpeed;
			float _WaterWaveDetailTiling;
			float _WaterWaveDetailStrength;
			TEXTURE2D(_FlowMap);
			float3 _FlowTiling;
			float _FlowStrength;
			float _AccumulatedWaterParallaxStrength;
			float _ApproxRealtimeGI_LightingMapContrast;
			float _RealtimeGIStrength;
			TEXTURE2D(Smoothness);
			TEXTURE2D(_RaindropsGradientMap);
			float _RaindropsSplashSpeed;
			float _RaindropsTiling;
			float _RaindropsSize;
			float4 _ApproxRealtimeGI_SkyColor;
			float _ApproxRealtimeGI_MixCoeff;
			float _ApproxRealtimeGI_ReflectionStrength;
			float _ApproxRealtimeGI_AOMin;
			float _ApproxRealtimeGI_AOMax;


			float3 TransformObjectToWorldNormal_Ref33_g7196( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			
			float3 SampleSHVertex_Ref376_g7193( float3 normalWS )
			{
				return SampleSHVertex(normalWS);
			}
			
			float ComputeFogFactor_Ref381_g7193( float4 positionCS )
			{
				return ComputeFogFactor(positionCS.z);
			}
			
			float4 GetShadowCoord_Ref384_g7193( float4 positionCS, float3 positionWS )
			{
				#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
				    return ComputeScreenPos(positionCS);
				#else
				    return TransformWorldToShadowCoord(positionWS);
				#endif
			}
			
			float3 TransformWorldToTangentDir_Ref133_g7194( float3 directionWS, float3x3 TBN )
			{
				return TransformWorldToTangentDir(directionWS, TBN);
			}
			
			float2 IterativeParallaxLegacy1_g7379( float height, float2 UVs, float2 plane, float refp, float scale )
			{
				UVs += plane * scale * refp;
				UVs += (height - 1) * plane * scale;
				return UVs;
			}
			
			float2 GetNormalizedScreenSpaceUV_Ref( float4 positionCS )
			{
				return GetNormalizedScreenSpaceUV(positionCS);
			}
			
			float4 AmbientGround46_g7491(  )
			{
				return unity_AmbientGround;
			}
			
			float4 AmbientSky42_g7491(  )
			{
				return unity_AmbientSky;
			}
			
			float4 AmbientEquator44_g7491(  )
			{
				return unity_AmbientEquator;
			}
			
			float4 ShadowMask28_g7488( float2 StaticLightMapUV )
			{
				half4 shadowMask =half4(1, 1, 1, 1); 
				shadowMask = SAMPLE_SHADOWMASK(StaticLightMapUV);
				return shadowMask;
			}
			
			float3 GetBakedGI39_g7491( float2 staticLightmapUV, float2 dynamicLightmapUV, float3 vertexSH, float3 normalWS, Light mainLight )
			{
				half3 bakedGI = 0;
				#if defined(DYNAMICLIGHTMAP_ON)
				    bakedGI = SAMPLE_GI(staticLightmapUV, dynamicLightmapUV, vertexSH, normalWS);
				#else
				    bakedGI = SAMPLE_GI(staticLightmapUV, vertexSH, normalWS);
				#endif
				MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);
				return bakedGI;
			}
			
					float2 voronoihash199( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi199( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0;
						for ( int j = -1; j <= 1; j++ )
						{
							for ( int i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash199( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			
			float3 GetBakedReflect26_g7526( float3 reflectDirWS, float3 positionWS, float perceptualRoughness, float2 normalizedScreenSpaceUV )
			{
				half3 BakedReflect = 0;
				BakedReflect = GlossyEnvironmentReflection(reflectDirWS, positionWS, perceptualRoughness, 1.0h, normalizedScreenSpaceUV);
				return BakedReflect;
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float3 GetViewForwardDir_Ref2_g7487(  )
			{
				 return GetViewForwardDir();
			}
			
			float3 AppendClearCoatReflect41_g7529( float3 bakedReflectClearCoat, float fresnel, BRDFData brdfDataClearCoat, float clearCoatMask, float3 mainBakedReflect )
			{
				#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
				    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, bakedReflectClearCoat, fresnel);
				    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnel;
				    return (mainBakedReflect * (1.0 - coatFresnel * clearCoatMask) + coatColor);
				#else
				    return mainBakedReflect ;
				#endif
			}
			
			int IsUseLightLayer11_g7508(  )
			{
				int useLightLayer = 0;
				#ifdef _LIGHT_LAYERS
				useLightLayer = 1;
				#endif
				return useLightLayer;
			}
			
			int GetLightLayer_WBhrsge19_g7502( Light light )
			{
				 return light.layerMask;
			}
			
			int GetMeshRenderingLayer_Ref14_g7508(  )
			{
				return GetMeshRenderingLayer();
			}
			
			int IsMatchingLightLayer_Ref10_g7508( int lightLayers, int renderingLayers )
			{
				return IsMatchingLightLayer(lightLayers,renderingLayers);
			}
			
			int IsUseLightLayer11_g7503(  )
			{
				int useLightLayer = 0;
				#ifdef _LIGHT_LAYERS
				useLightLayer = 1;
				#endif
				return useLightLayer;
			}
			
			int GetMeshRenderingLayer_Ref14_g7503(  )
			{
				return GetMeshRenderingLayer();
			}
			
			int IsMatchingLightLayer_Ref10_g7503( int lightLayers, int renderingLayers )
			{
				return IsMatchingLightLayer(lightLayers,renderingLayers);
			}
			
			float3 BrdfDataToDiffuse19_g7513( BRDFData BrdfData )
			{
				return BrdfData.diffuse;
			}
			
			float3 VertexLighting_Ref398_g7193( float3 positionWS, float3 normalWS )
			{
				return VertexLighting(positionWS,normalWS);
			}
			
			float4 CalculateFinalColor_Ref7_g7531( LightingData lightingData, float alpha )
			{
				#if REAL_IS_HALF
				    // Clamp any half.inf+ to HALF_MAX
				    return min(CalculateFinalColor(lightingData, alpha), HALF_MAX);
				#else
				    return CalculateFinalColor(lightingData, alpha);
				#endif
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 temp_output_31_0_g7196 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g7196 = temp_output_31_0_g7196;
				float3 localTransformObjectToWorldNormal_Ref33_g7196 = TransformObjectToWorldNormal_Ref33_g7196( normalOS33_g7196 );
				float3 normalizeResult140_g7196 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g7196 );
				float3 temp_output_515_34_g7193 = normalizeResult140_g7196;
				float3 VertexNormalWS314_g7193 = temp_output_515_34_g7193;
				float3 normalWS376_g7193 = VertexNormalWS314_g7193;
				float3 localSampleSHVertex_Ref376_g7193 = SampleSHVertex_Ref376_g7193( normalWS376_g7193 );
				
				float localPosition1_g7205 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g7204 = ( 0.0 );
				float3 temp_output_14_0_g7197 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g7204 = temp_output_14_0_g7197;
				Position position1_g7204 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g7204 , position1_g7204 );
				Position position1_g7205 =(Position)position1_g7204;
				float3 OS1_g7205 = float3( 0,0,0 );
				float3 WS1_g7205 = float3( 0,0,0 );
				float3 VS1_g7205 = float3( 0,0,0 );
				float4 CS1_g7205 = float4( 0,0,0,0 );
				float4 NDC1_g7205 = float4( 0,0,0,0 );
				float3 SS1_g7205 = float3( 0,0,0 );
				float4 DS1_g7205 = float4( 0,0,0,0 );
				float3 LS1_g7205 = float3( 0,0,0 );
				float4 SC1_g7205 = float4( 0,0,0,0 );
				Position_float( position1_g7205 , OS1_g7205 , WS1_g7205 , VS1_g7205 , CS1_g7205 , NDC1_g7205 , SS1_g7205 , DS1_g7205 , LS1_g7205 , SC1_g7205 );
				float4 vertexPositionCS382_g7193 = CS1_g7205;
				float4 positionCS381_g7193 = vertexPositionCS382_g7193;
				float localComputeFogFactor_Ref381_g7193 = ComputeFogFactor_Ref381_g7193( positionCS381_g7193 );
				
				float4 positionCS384_g7193 = vertexPositionCS382_g7193;
				float3 temp_output_345_7_g7193 = WS1_g7205;
				float3 vertexPositionWS386_g7193 = temp_output_345_7_g7193;
				float3 positionWS384_g7193 = vertexPositionWS386_g7193;
				float4 localGetShadowCoord_Ref384_g7193 = GetShadowCoord_Ref384_g7193( positionCS384_g7193 , positionWS384_g7193 );
				
				float4 temp_output_21_313 = vertexPositionCS382_g7193;
				
				float2 break15 = ( ( v.ase_texcoord.xy * _BaseColorMap_ST.xy ) + _BaseColorMap_ST.zw );
				float vertexToFrag17 = break15.x;
				o.ase_texcoord2.x = vertexToFrag17;
				float vertexToFrag16 = break15.y;
				o.ase_texcoord2.y = vertexToFrag16;
				float3 break310_g7193 = vertexPositionWS386_g7193;
				float vertexToFrag320_g7193 = break310_g7193.x;
				o.ase_texcoord2.z = vertexToFrag320_g7193;
				float vertexToFrag321_g7193 = break310_g7193.y;
				o.ase_texcoord2.w = vertexToFrag321_g7193;
				float vertexToFrag322_g7193 = break310_g7193.z;
				o.ase_texcoord3.x = vertexToFrag322_g7193;
				float3 break138_g7193 = VertexNormalWS314_g7193;
				float vertexToFrag323_g7193 = break138_g7193.x;
				o.ase_texcoord3.y = vertexToFrag323_g7193;
				float vertexToFrag324_g7193 = break138_g7193.y;
				o.ase_texcoord3.z = vertexToFrag324_g7193;
				float vertexToFrag325_g7193 = break138_g7193.z;
				o.ase_texcoord3.w = vertexToFrag325_g7193;
				float3 normalizeResult129_g7193 = ASESafeNormalize( ( _WorldSpaceCameraPos - vertexPositionWS386_g7193 ) );
				float3 temp_output_43_0_g7194 = normalizeResult129_g7193;
				float3 directionWS133_g7194 = temp_output_43_0_g7194;
				float3 temp_output_43_0_g7195 = ( v.ase_tangent.xyz + float3( 0,0,0 ) );
				float3 objToWorldDir42_g7195 = mul( GetObjectToWorldMatrix(), float4( temp_output_43_0_g7195, 0 ) ).xyz;
				float3 normalizeResult128_g7195 = ASESafeNormalize( objToWorldDir42_g7195 );
				float3 VertexTangentlWS474_g7193 = normalizeResult128_g7195;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 normalizeResult473_g7193 = ASESafeNormalize( ( cross( VertexNormalWS314_g7193 , VertexTangentlWS474_g7193 ) * ase_vertexTangentSign ) );
				float3 VertexBitangentWS476_g7193 = normalizeResult473_g7193;
				float3x3 temp_output_103_0_g7194 = float3x3(VertexTangentlWS474_g7193, VertexBitangentWS476_g7193, VertexNormalWS314_g7193);
				float3x3 TBN133_g7194 = temp_output_103_0_g7194;
				float3 localTransformWorldToTangentDir_Ref133_g7194 = TransformWorldToTangentDir_Ref133_g7194( directionWS133_g7194 , TBN133_g7194 );
				float3 normalizeResult132_g7194 = ASESafeNormalize( localTransformWorldToTangentDir_Ref133_g7194 );
				float3 break336_g7193 = normalizeResult132_g7194;
				float vertexToFrag264_g7193 = break336_g7193.x;
				o.ase_texcoord4.x = vertexToFrag264_g7193;
				float vertexToFrag337_g7193 = break336_g7193.y;
				o.ase_texcoord4.y = vertexToFrag337_g7193;
				float vertexToFrag338_g7193 = break336_g7193.z;
				o.ase_texcoord4.z = vertexToFrag338_g7193;
				float3 break141_g7193 = VertexTangentlWS474_g7193;
				float vertexToFrag326_g7193 = break141_g7193.x;
				o.ase_texcoord4.w = vertexToFrag326_g7193;
				float vertexToFrag327_g7193 = break141_g7193.y;
				o.ase_texcoord5.x = vertexToFrag327_g7193;
				float vertexToFrag328_g7193 = break141_g7193.z;
				o.ase_texcoord5.y = vertexToFrag328_g7193;
				float3 break148_g7193 = VertexBitangentWS476_g7193;
				float vertexToFrag329_g7193 = break148_g7193.x;
				o.ase_texcoord5.z = vertexToFrag329_g7193;
				float vertexToFrag330_g7193 = break148_g7193.y;
				o.ase_texcoord5.w = vertexToFrag330_g7193;
				float vertexToFrag331_g7193 = break148_g7193.z;
				o.ase_texcoord6.x = vertexToFrag331_g7193;
				float2 break517_g7193 = (v.ase_texcoord2.xy*(unity_DynamicLightmapST).xy + (unity_DynamicLightmapST).zw);
				float vertexToFrag521_g7193 = break517_g7193.x;
				o.ase_texcoord6.y = vertexToFrag521_g7193;
				float vertexToFrag522_g7193 = break517_g7193.y;
				o.ase_texcoord6.z = vertexToFrag522_g7193;
				float3 break145_g7193 = normalizeResult129_g7193;
				float vertexToFrag332_g7193 = break145_g7193.x;
				o.ase_texcoord6.w = vertexToFrag332_g7193;
				float vertexToFrag333_g7193 = break145_g7193.y;
				o.ase_texcoord7.x = vertexToFrag333_g7193;
				float vertexToFrag334_g7193 = break145_g7193.z;
				o.ase_texcoord7.y = vertexToFrag334_g7193;
				float3 positionWS398_g7193 = vertexPositionWS386_g7193;
				float3 normalWS398_g7193 = VertexNormalWS314_g7193;
				float3 localVertexLighting_Ref398_g7193 = VertexLighting_Ref398_g7193( positionWS398_g7193 , normalWS398_g7193 );
				float3 break533_g7193 = localVertexLighting_Ref398_g7193;
				float vertexToFrag530_g7193 = break533_g7193.x;
				o.ase_texcoord7.z = vertexToFrag530_g7193;
				float vertexToFrag531_g7193 = break533_g7193.y;
				o.ase_texcoord7.w = vertexToFrag531_g7193;
				float vertexToFrag532_g7193 = break533_g7193.z;
				o.ase_texcoord8.x = vertexToFrag532_g7193;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.yzw = 0;

				//接口
				float2 StaticLightmapUV = (v.ase_texcoord1.xy*(unity_LightmapST).xy + (unity_LightmapST).zw);
				float3 VertexSH = localSampleSHVertex_Ref376_g7193;
				#ifdef ASE_FOG
					float FogFactor = localComputeFogFactor_Ref381_g7193;
				#else
					float FogFactor = 0;
				#endif
				float4 ShadowCoord = localGetShadowCoord_Ref384_g7193;


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
			
				o.positionCS = temp_output_21_313;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord2 : TEXCOORD2;

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
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord2 = v.ase_texcoord2;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
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

				float localGetLightingData1_g7531 = ( 0.0 );
				float vertexToFrag17 = IN.ase_texcoord2.x;
				float vertexToFrag16 = IN.ase_texcoord2.y;
				float2 appendResult18 = (float2(vertexToFrag17 , vertexToFrag16));
				float vertexToFrag320_g7193 = IN.ase_texcoord2.z;
				float vertexToFrag321_g7193 = IN.ase_texcoord2.w;
				float vertexToFrag322_g7193 = IN.ase_texcoord3.x;
				float3 appendResult311_g7193 = (float3(vertexToFrag320_g7193 , vertexToFrag321_g7193 , vertexToFrag322_g7193));
				float3 positionWS402_g7193 = appendResult311_g7193;
				float3 positionWS36 = positionWS402_g7193;
				float3 break479 = positionWS36;
				float2 appendResult480 = (float2(break479.x , break479.z));
				float2 PositionWSxz481 = appendResult480;
				float localGlobalSampler2_g7220 = ( 0.0 );
				SamplerState PointClamp2_g7220 = sampler_PointClamp;
				SamplerState LinearClamp2_g7220 = sampler_LinearClamp;
				SamplerState PointRepeat2_g7220 = sampler_PointRepeat;
				SamplerState LinearRepeat2_g7220 = sampler_LinearRepeat;
				{
				PointClamp2_g7220 = sampler_PointClamp;
				LinearClamp2_g7220 = sampler_LinearClamp;
				PointRepeat2_g7220 = sampler_PointRepeat;
				LinearRepeat2_g7220 = sampler_LinearRepeat;
				}
				float GlobalMoist12401 = saturate( ( _GlobalMoist - 1.0 ) );
				float lerpResult519 = lerp( 0.5 , ( SAMPLE_TEXTURE2D( _AccumulatedWaterMask, LinearRepeat2_g7220, ( PositionWSxz481 * _AccumulatedWaterMaskTiling ) ).r + (GlobalMoist12401*2.0 + -1.0) ) , _AccumulatedWaterContrast);
				float vertexToFrag323_g7193 = IN.ase_texcoord3.y;
				float vertexToFrag324_g7193 = IN.ase_texcoord3.z;
				float vertexToFrag325_g7193 = IN.ase_texcoord3.w;
				float3 appendResult142_g7193 = (float3(vertexToFrag323_g7193 , vertexToFrag324_g7193 , vertexToFrag325_g7193));
				float3 normalizeResult459_g7193 = normalize( appendResult142_g7193 );
				float3 NormalWS388_g7193 = normalizeResult459_g7193;
				float3 vNormalWS39 = NormalWS388_g7193;
				float dotResult408 = dot( vNormalWS39 , float3(0,1,0) );
				float smoothstepResult413 = smoothstep( _AccumulatedWaterSteepHillExtinction , 1.0 , dotResult408);
				float FlatArea522 = smoothstepResult413;
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch962 = ( saturate( (lerpResult519).x ) * FlatArea522 * _MoistAccumulatedwaterCoeff );
				#else
				float staticSwitch962 = 0.0;
				#endif
				float AccumulatedWaterMask286 = staticSwitch962;
				float height1_g7379 = AccumulatedWaterMask286;
				float2 break135_g7377 = ( PositionWSxz481 * _RipplesMainTiling );
				float2 appendResult206_g7377 = (float2(frac( break135_g7377.x ) , frac( break135_g7377.y )));
				float temp_output_4_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.x;
				float temp_output_5_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.y;
				float2 appendResult116_g7377 = (float2(temp_output_4_0_g7377 , temp_output_5_0_g7377));
				float temp_output_122_0_g7377 = ( temp_output_4_0_g7377 * temp_output_5_0_g7377 );
				float2 appendResult175_g7377 = (float2(temp_output_122_0_g7377 , temp_output_5_0_g7377));
				float Columns213_g7377 = temp_output_4_0_g7377;
				float Rows212_g7377 = temp_output_5_0_g7377;
				float temp_output_133_0_g7377 = ( fmod( _TimeParameters.x , ( Columns213_g7377 * Rows212_g7377 ) ) * _XColumnsYRowsZSpeedWStrartFrame.z );
				float clampResult129_g7377 = clamp( _XColumnsYRowsZSpeedWStrartFrame.w , 1E-05 , ( temp_output_122_0_g7377 - 1.0 ) );
				float temp_output_185_0_g7377 = frac( ( ( temp_output_133_0_g7377 + ( clampResult129_g7377 + 1E-05 ) ) / temp_output_122_0_g7377 ) );
				float2 appendResult186_g7377 = (float2(temp_output_185_0_g7377 , ( 1.0 - temp_output_185_0_g7377 )));
				float2 temp_output_203_0_g7377 = ( ( appendResult206_g7377 / appendResult116_g7377 ) + ( floor( ( appendResult175_g7377 * appendResult186_g7377 ) ) / appendResult116_g7377 ) );
				float3 unpack233 = UnpackNormalScale( SAMPLE_TEXTURE2D( _RipplesNormalAtlas, LinearRepeat2_g7220, temp_output_203_0_g7377 ), _RipplesMainStrength );
				unpack233.z = lerp( 1, unpack233.z, saturate(_RipplesMainStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch964 = unpack233;
				#else
				float3 staticSwitch964 = float3(0,0,1);
				#endif
				float3 RipplesNormal294 = staticSwitch964;
				float2 _MainWaterWaveDir = float2(1,0);
				float cos278 = cos( radians( _WaterWaveRotate ) );
				float sin278 = sin( radians( _WaterWaveRotate ) );
				float2 rotator278 = mul( PositionWSxz481 - float2( 0.5,0.5 ) , float2x2( cos278 , -sin278 , sin278 , cos278 )) + float2( 0.5,0.5 );
				float2 panner259 = ( 1.0 * _Time.y * ( _MainWaterWaveDir * _WaterWaveMainSpeed ) + ( rotator278 * _WaterWaveMainTiling ));
				float3 unpack251 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner259 ), _WaterWaveMainStrength );
				unpack251.z = lerp( 1, unpack251.z, saturate(_WaterWaveMainStrength) );
				float2 panner269 = ( 1.0 * _Time.y * ( -_MainWaterWaveDir * _WaterWaveDetailSpeed ) + ( rotator278 * _WaterWaveDetailTiling ));
				float3 unpack275 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner269 ), _WaterWaveDetailStrength );
				unpack275.z = lerp( 1, unpack275.z, saturate(_WaterWaveDetailStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch972 = BlendNormal( unpack251 , unpack275 );
				#else
				float3 staticSwitch972 = float3(0,0,1);
				#endif
				float3 WaterWaveNormal306 = staticSwitch972;
				float3 temp_output_430_0 = ( positionWS36 * _FlowTiling );
				half3 Position160_g7376 = temp_output_430_0;
				float3 break170_g7376 = Position160_g7376;
				float2 appendResult171_g7376 = (float2(break170_g7376.z , break170_g7376.y));
				half3 Normal168_g7376 = vNormalWS39;
				float2 temp_output_180_0_g7376 = abs( (Normal168_g7376).xz );
				float2 temp_output_205_0_g7376 = ( temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 );
				float2 break183_g7376 = temp_output_205_0_g7376;
				float2 break185_g7376 = ( temp_output_205_0_g7376 / max( ( break183_g7376.x + break183_g7376.y ) , 1E-05 ) );
				float3 break186_g7376 = Position160_g7376;
				float2 appendResult191_g7376 = (float2(break186_g7376.x , break186_g7376.y));
				float4 break438 = ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7376 ) * break185_g7376.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7376 ) * break185_g7376.y ) );
				float2 appendResult437 = (float2(break438.b , break438.a));
				float2 normalMapRG1_g7378 = appendResult437;
				float4 localDecodeNormalRG1_g7378 = DecodeNormalRG( normalMapRG1_g7378 );
				float3 unpack4_g7378 = UnpackNormalScale( localDecodeNormalRG1_g7378, 0.25 );
				unpack4_g7378.z = lerp( 1, unpack4_g7378.z, saturate(0.25) );
				float3 normalizeResult449 = normalize( unpack4_g7378 );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch970 = normalizeResult449;
				#else
				float3 staticSwitch970 = float3(0,0,1);
				#endif
				float3 FlowNormal450 = staticSwitch970;
				half3 Position160_g7375 = ( temp_output_430_0 + ( _TimeParameters.x * float3(0,5,0) * 0.1 ) );
				float3 break170_g7375 = Position160_g7375;
				float2 appendResult171_g7375 = (float2(break170_g7375.z , break170_g7375.y));
				half3 Normal168_g7375 = vNormalWS39;
				float2 temp_output_180_0_g7375 = abs( (Normal168_g7375).xz );
				float2 temp_output_205_0_g7375 = ( temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 );
				float2 break183_g7375 = temp_output_205_0_g7375;
				float2 break185_g7375 = ( temp_output_205_0_g7375 / max( ( break183_g7375.x + break183_g7375.y ) , 1E-05 ) );
				float3 break186_g7375 = Position160_g7375;
				float2 appendResult191_g7375 = (float2(break186_g7375.x , break186_g7375.y));
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch968 = ( saturate( ( ( break438.r - saturate( ( ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g ) ) ) * 5.0 ) ) * GlobalMoist12401 );
				#else
				float staticSwitch968 = 0.0;
				#endif
				float FlowMask454 = ( staticSwitch968 * _FlowStrength );
				float2 UVs1_g7379 = ( appendResult18 + ( (( RipplesNormal294 + WaterWaveNormal306 )).xy * AccumulatedWaterMask286 * 0.02 ) + ( 0.02 * (FlowNormal450).xy * FlowMask454 ) );
				float vertexToFrag264_g7193 = IN.ase_texcoord4.x;
				float vertexToFrag337_g7193 = IN.ase_texcoord4.y;
				float vertexToFrag338_g7193 = IN.ase_texcoord4.z;
				float3 appendResult340_g7193 = (float3(vertexToFrag264_g7193 , vertexToFrag337_g7193 , vertexToFrag338_g7193));
				float3 normalizeResult451_g7193 = normalize( appendResult340_g7193 );
				float3 viewDirTS41 = normalizeResult451_g7193;
				float3 break6_g7379 = viewDirTS41;
				float2 appendResult5_g7379 = (float2(break6_g7379.x , break6_g7379.y));
				float2 plane1_g7379 = ( appendResult5_g7379 / break6_g7379.z );
				float refp1_g7379 = 1.0;
				float scale1_g7379 = ( _AccumulatedWaterParallaxStrength * 0.01 );
				float2 localIterativeParallaxLegacy1_g7379 = IterativeParallaxLegacy1_g7379( height1_g7379 , UVs1_g7379 , plane1_g7379 , refp1_g7379 , scale1_g7379 );
				#ifdef _ACCUMULATEDWATER_ON
				float2 staticSwitch974 = localIterativeParallaxLegacy1_g7379;
				#else
				float2 staticSwitch974 = appendResult18;
				#endif
				float2 DistortionUV298 = staticSwitch974;
				float4 tex2DNode116 = SAMPLE_TEXTURE2D( _NormalMap, LinearRepeat2_g7220, DistortionUV298 );
				float lerpResult122 = lerp( 1.0 , tex2DNode116.a , ( _OcclusionBaked + 1.0 ));
				float BakedAOTex113 = lerpResult122;
				float BakedAO40_g7527 = BakedAOTex113;
				float localGetScreenSpaceAmbientOcclusion_Ref4_g7527 = ( 0.0 );
				float4 positionCS289_g7193 = PositionDS;
				float2 localGetNormalizedScreenSpaceUV_Ref289_g7193 = GetNormalizedScreenSpaceUV_Ref( positionCS289_g7193 );
				float3 appendResult291_g7193 = (float3(localGetNormalizedScreenSpaceUV_Ref289_g7193 , PositionDS.z));
				float3 positionSS42 = appendResult291_g7193;
				float2 positionSS4_g7527 = positionSS42.xy;
				float IndirectAmbientOcclusion4_g7527 = 0;
				float DirectAmbientOcclusion4_g7527 = 0;
				{
				AmbientOcclusionFactor SSAO = (AmbientOcclusionFactor)0;
				SSAO = GetScreenSpaceAmbientOcclusion(positionSS4_g7527);
				IndirectAmbientOcclusion4_g7527 = SSAO.indirectAmbientOcclusion;
				DirectAmbientOcclusion4_g7527 = SSAO.directAmbientOcclusion;
				}
				float IndirectSSAO43_g7527 = IndirectAmbientOcclusion4_g7527;
				float temp_output_34_0_g7527 = min( BakedAO40_g7527 , IndirectSSAO43_g7527 );
				float temp_output_18_0_g7531 = temp_output_34_0_g7527;
				float4 localAmbientGround46_g7491 = AmbientGround46_g7491();
				float4 AmbientGround920 = localAmbientGround46_g7491;
				float4 localAmbientSky42_g7491 = AmbientSky42_g7491();
				float4 AmbientSky918 = localAmbientSky42_g7491;
				float3 desaturateInitialColor665 = AmbientSky918.xyz;
				float desaturateDot665 = dot( desaturateInitialColor665, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar665 = lerp( desaturateInitialColor665, desaturateDot665.xxx, ( 1.0 - (AmbientSky918).w ) );
				float4 localAmbientEquator44_g7491 = AmbientEquator44_g7491();
				float4 AmbientEquator919 = localAmbientEquator44_g7491;
				float3 desaturateInitialColor666 = AmbientEquator919.xyz;
				float desaturateDot666 = dot( desaturateInitialColor666, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar666 = lerp( desaturateInitialColor666, desaturateDot666.xxx, ( 1.0 - (AmbientEquator919).w ) );
				float3 AmbientSkyMixEquator938 = ( ( desaturateVar665 * desaturateVar666 ) + ( ( desaturateVar665 + desaturateVar666 ) * 0.5 ) );
				float2 normalMapRG1_g7380 = (tex2DNode116).rg;
				float4 localDecodeNormalRG1_g7380 = DecodeNormalRG( normalMapRG1_g7380 );
				float3 unpack4_g7380 = UnpackNormalScale( localDecodeNormalRG1_g7380, _NormalStrength );
				unpack4_g7380.z = lerp( 1, unpack4_g7380.z, saturate(_NormalStrength) );
				float3 PrimitiveNormalTS878 = unpack4_g7380;
				float3 lerpResult284 = lerp( PrimitiveNormalTS878 , BlendNormal( RipplesNormal294 , WaterWaveNormal306 ) , AccumulatedWaterMask286);
				float3 lerpResult470 = lerp( lerpResult284 , FlowNormal450 , FlowMask454);
				float3 NormalTs25 = lerpResult470;
				float vertexToFrag326_g7193 = IN.ase_texcoord4.w;
				float vertexToFrag327_g7193 = IN.ase_texcoord5.x;
				float vertexToFrag328_g7193 = IN.ase_texcoord5.y;
				float3 appendResult134_g7193 = (float3(vertexToFrag326_g7193 , vertexToFrag327_g7193 , vertexToFrag328_g7193));
				float3 normalizeResult448_g7193 = normalize( appendResult134_g7193 );
				float3 TangentWS315_g7193 = normalizeResult448_g7193;
				float vertexToFrag329_g7193 = IN.ase_texcoord5.z;
				float vertexToFrag330_g7193 = IN.ase_texcoord5.w;
				float vertexToFrag331_g7193 = IN.ase_texcoord6.x;
				float3 appendResult144_g7193 = (float3(vertexToFrag329_g7193 , vertexToFrag330_g7193 , vertexToFrag331_g7193));
				float3 normalizeResult449_g7193 = normalize( appendResult144_g7193 );
				float3 BitangentWS316_g7193 = normalizeResult449_g7193;
				float3x3 TBN24 = float3x3(TangentWS315_g7193, BitangentWS316_g7193, NormalWS388_g7193);
				float3 normalizeResult29 = normalize( mul( NormalTs25, TBN24 ) );
				float3 normalWS58 = normalizeResult29;
				float dotResult616 = dot( normalWS58 , float3(0,1,0) );
				float4 lerpResult649 = lerp( AmbientGround920 , float4( AmbientSkyMixEquator938 , 0.0 ) , (dotResult616*0.5 + 0.5));
				float2 staticLightMapUV32 = StaticLightmapUV;
				float2 staticLightmapUV39_g7491 = staticLightMapUV32;
				float vertexToFrag521_g7193 = IN.ase_texcoord6.y;
				float vertexToFrag522_g7193 = IN.ase_texcoord6.z;
				float2 appendResult523_g7193 = (float2(vertexToFrag521_g7193 , vertexToFrag522_g7193));
				float2 dynamicLightMapUV33 = appendResult523_g7193;
				float2 dynamicLightmapUV39_g7491 = dynamicLightMapUV33;
				float3 vertexSH34 = VertexSH;
				float3 vertexSH39_g7491 = vertexSH34;
				float3 normalWS39_g7491 = normalWS58;
				float localGetMainLight_Ref1_g7488 = ( 0.0 );
				float localShadowCoordInputFragment352_g7193 = ( 0.0 );
				float4 ShadowCoords352_g7193 = float4( 0,0,0,0 );
				float3 WorldPosition352_g7193 = appendResult311_g7193;
				{
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				ShadowCoords352_g7193 = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				ShadowCoords352_g7193 = TransformWorldToShadowCoord(WorldPosition352_g7193);
				#endif
				}
				float4 shadowCoord37 = ShadowCoords352_g7193;
				float4 shadowCoord1_g7488 = shadowCoord37;
				float3 positionWS1_g7488 = positionWS36;
				float2 StaticLightMapUV28_g7488 = staticLightMapUV32;
				float4 localShadowMask28_g7488 = ShadowMask28_g7488( StaticLightMapUV28_g7488 );
				float4 shadowMask1_g7488 = localShadowMask28_g7488;
				float3 Direction1_g7488 = float3( 0,0,0 );
				float3 Color1_g7488 = float3( 0,0,0 );
				float DistanceAttenuation1_g7488 = 0;
				float ShadowAttenuation1_g7488 = 0;
				int LayerMask1_g7488 = 0;
				Light light1_g7488 = (Light)0;
				{
				Light mainlight= (Light)0;
				mainlight = GetMainLight(shadowCoord1_g7488, positionWS1_g7488, shadowMask1_g7488);
				Direction1_g7488 = mainlight.direction;
				Color1_g7488 = mainlight.color;
				DistanceAttenuation1_g7488 = mainlight.distanceAttenuation;
				ShadowAttenuation1_g7488 = mainlight.shadowAttenuation;
				LayerMask1_g7488 = mainlight.layerMask;
				light1_g7488 = mainlight;
				}
				Light mainLight39_g7491 =(Light)light1_g7488;
				float3 localGetBakedGI39_g7491 = GetBakedGI39_g7491( staticLightmapUV39_g7491 , dynamicLightmapUV39_g7491 , vertexSH39_g7491 , normalWS39_g7491 , mainLight39_g7491 );
				float luminance925 = Luminance(localGetBakedGI39_g7491);
				float BakedGI916 = luminance925;
				float lerpResult728 = lerp( 0.5 , BakedGI916 , _ApproxRealtimeGI_LightingMapContrast);
				float4 ApproxRealtimeGI942 = ( lerpResult649 * lerpResult728 * _RealtimeGIStrength );
				float localBRDFDataSplit12_g7530 = ( 0.0 );
				float localBRDFDataMerge1_g7523 = ( 0.0 );
				float localBRDFDataSplit12_g7518 = ( 0.0 );
				float localCreateClearCoatBRDFData_Ref139_g7519 = ( 0.0 );
				float localSurfaceDataMerge6_g7521 = ( 0.0 );
				float4 tex2DNode110 = SAMPLE_TEXTURE2D( _BaseColorMap, LinearRepeat2_g7220, DistortionUV298 );
				float4 Albedo57 = tex2DNode110;
				float3 albedo107_g7519 = Albedo57.rgb;
				float3 Albedo6_g7521 = albedo107_g7519;
				float3 temp_cast_6 = (0.04).xxx;
				float4 tex2DNode112 = SAMPLE_TEXTURE2D( Smoothness, LinearRepeat2_g7220, DistortionUV298 );
				float PrimitiveMetallic881 = saturate( ( _Metallic01 * tex2DNode112.a ) );
				float vertexToFrag332_g7193 = IN.ase_texcoord6.w;
				float vertexToFrag333_g7193 = IN.ase_texcoord7.x;
				float vertexToFrag334_g7193 = IN.ase_texcoord7.y;
				float3 appendResult335_g7193 = (float3(vertexToFrag332_g7193 , vertexToFrag333_g7193 , vertexToFrag334_g7193));
				float3 normalizeResult484_g7193 = normalize( appendResult335_g7193 );
				float3 viewDirWS40 = normalizeResult484_g7193;
				float dotResult810 = dot( vNormalWS39 , viewDirWS40 );
				float temp_output_814_0 = ( 1.0 - saturate( dotResult810 ) );
				float WaterMask861 = max( AccumulatedWaterMask286 , FlowMask454 );
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch966 = ( ( temp_output_814_0 * temp_output_814_0 * temp_output_814_0 * temp_output_814_0 ) * WaterMask861 );
				#else
				float staticSwitch966 = 0.0;
				#endif
				float ModifWaterMask867 = staticSwitch966;
				float lerpResult818 = lerp( PrimitiveMetallic881 , _AccumulatedwaterReflectStrength , ModifWaterMask867);
				float Metallic50 = lerpResult818;
				float temp_output_70_0_g7519 = Metallic50;
				float metallic82_g7519 = temp_output_70_0_g7519;
				float3 lerpResult28_g7519 = lerp( temp_cast_6 , albedo107_g7519 , metallic82_g7519);
				float3 specular43_g7519 = ( lerpResult28_g7519 * _SpecColor.rgb );
				float3 Specular6_g7521 = specular43_g7519;
				float Metallic6_g7521 = metallic82_g7519;
				float PrimitiveRoughness830 = saturate( ( _Roughness1 * tex2DNode116.b ) );
				float GlobalMoist01393 = saturate( _GlobalMoist );
				float2 temp_cast_8 = (_RaindropsSplashSpeed).xx;
				float time199 = 0.001;
				float2 voronoiSmoothId199 = 0;
				float2 coords199 = PositionWSxz481 * _RaindropsTiling;
				float2 id199 = 0;
				float2 uv199 = 0;
				float fade199 = 0.5;
				float voroi199 = 0;
				float rest199 = 0;
				for( int it199 = 0; it199 <2; it199++ ){
				voroi199 += fade199 * voronoi199( coords199, time199, id199, uv199, 0,voronoiSmoothId199 );
				rest199 += fade199;
				coords199 *= 2;
				fade199 *= 0.5;
				}//Voronoi199
				voroi199 /= rest199;
				float2 panner203 = ( ( GlobalMoist01393 * 10.0 ) * temp_cast_8 + ( step( voroi199 , 0.1 ) * id199 ));
				#ifdef _RAINDROPS_ON
				float staticSwitch961 = max( GlobalMoist01393 , ( saturate( ( SAMPLE_TEXTURE2D( _RaindropsGradientMap, LinearRepeat2_g7220, panner203 ).r * step( voroi199 , ( _RaindropsSize * 0.05 ) ) ) ) * saturate( ( GlobalMoist01393 * 5.0 ) ) * FlatArea522 ) );
				#else
				float staticSwitch961 = GlobalMoist01393;
				#endif
				float MoistMask310 = staticSwitch961;
				float lerpResult391 = lerp( PrimitiveRoughness830 , ( PrimitiveRoughness830 * _MoistRoughnessCoeff ) , MoistMask310);
				float lerpResult314 = lerp( lerpResult391 , 0.005 , WaterMask861);
				float Roughness52 = lerpResult314;
				float temp_output_68_0_g7519 = Roughness52;
				float perceptualRoughness75_g7519 = temp_output_68_0_g7519;
				float perceptualSmoothness40_g7519 = ( 1.0 - perceptualRoughness75_g7519 );
				float Smoothness6_g7521 = perceptualSmoothness40_g7519;
				float3 NormalTS6_g7521 = NormalTs25;
				float3 Emission6_g7521 = float3( 0,0,0 );
				float Occlusion6_g7521 = BakedAOTex113;
				float Alpha6_g7521 = 1.0;
				float ClearCoatMask6_g7521 = 0.0;
				float ClearCoatSmoothness6_g7521 = 0.0;
				SurfaceData surfaceData6_g7521 = (SurfaceData)0;
				{
				surfaceData6_g7521.albedo = Albedo6_g7521;
				surfaceData6_g7521.specular = Specular6_g7521;
				surfaceData6_g7521.metallic = Metallic6_g7521;
				surfaceData6_g7521.smoothness = Smoothness6_g7521;
				surfaceData6_g7521.normalTS = NormalTS6_g7521;
				surfaceData6_g7521.emission = Emission6_g7521;
				surfaceData6_g7521.occlusion = Occlusion6_g7521;
				surfaceData6_g7521.alpha = Alpha6_g7521;
				surfaceData6_g7521.clearCoatMask = ClearCoatMask6_g7521;
				surfaceData6_g7521.clearCoatSmoothness = ClearCoatSmoothness6_g7521;
				}
				SurfaceData surfaceData139_g7519 = surfaceData6_g7521;
				float localBRDFDataMerge1_g7520 = ( 0.0 );
				float3 Albedo1_g7520 = albedo107_g7519;
				float oneMinusReflectivity48_g7519 = ( ( 1.0 - metallic82_g7519 ) * 0.96 );
				float3 diffuse49_g7519 = ( oneMinusReflectivity48_g7519 * albedo107_g7519 * _BaseColor.rgb );
				float3 Diffuse1_g7520 = diffuse49_g7519;
				float3 Specular1_g7520 = specular43_g7519;
				float Reflectivity47_g7519 = ( 1.0 - oneMinusReflectivity48_g7519 );
				float Reflectivity1_g7520 = Reflectivity47_g7519;
				float PerceptualRoughness1_g7520 = perceptualRoughness75_g7519;
				float roughness44_g7519 = max( ( perceptualRoughness75_g7519 * perceptualRoughness75_g7519 ) , 0.0078125 );
				float Roughness1_g7520 = roughness44_g7519;
				float roughness2116_g7519 = max( ( roughness44_g7519 * roughness44_g7519 ) , 6.103516E-05 );
				float Roughness21_g7520 = roughness2116_g7519;
				float grazingTerm41_g7519 = saturate( ( perceptualSmoothness40_g7519 + Reflectivity47_g7519 ) );
				float GrazingTerm1_g7520 = grazingTerm41_g7519;
				float normalizationTerm42_g7519 = ( ( roughness44_g7519 * 4.0 ) + 2.0 );
				float NormalizationTerm1_g7520 = normalizationTerm42_g7519;
				float roughness2MinusOne46_g7519 = ( roughness2116_g7519 - 1.0 );
				float Roughness2MinusOne1_g7520 = roughness2MinusOne46_g7519;
				BRDFData brdfData1_g7520 = (BRDFData)0;
				{
				brdfData1_g7520 = (BRDFData)0;
				brdfData1_g7520.albedo = Albedo1_g7520;
				brdfData1_g7520.diffuse = Diffuse1_g7520;
				brdfData1_g7520.specular = Specular1_g7520;
				brdfData1_g7520.reflectivity = Reflectivity1_g7520;
				brdfData1_g7520.perceptualRoughness = PerceptualRoughness1_g7520;
				brdfData1_g7520.roughness = Roughness1_g7520;
				brdfData1_g7520.roughness2 = Roughness21_g7520;
				brdfData1_g7520.grazingTerm = GrazingTerm1_g7520;
				brdfData1_g7520.normalizationTerm = NormalizationTerm1_g7520;
				brdfData1_g7520.roughness2MinusOne = Roughness2MinusOne1_g7520;
				}
				BRDFData brdfData139_g7519 = brdfData1_g7520;
				BRDFData brdfDataClearCoat139_g7519 = (BRDFData)1;
				{
				brdfDataClearCoat139_g7519 = CreateClearCoatBRDFData(surfaceData139_g7519, brdfData139_g7519);
				}
				BRDFData brdfData12_g7518 = brdfData139_g7519;
				float3 Albedo12_g7518 = float3( 0,0,0 );
				float3 Diffuse12_g7518 = float3( 0,0,0 );
				float3 Specular12_g7518 = float3( 0,0,0 );
				float Reflectivity12_g7518 = 0;
				float PerceptualRoughness12_g7518 = 0;
				float Roughness12_g7518 = 0;
				float Roughness212_g7518 = 0;
				float GrazingTerm12_g7518 = 0;
				float NormalizationTerm12_g7518 = 0;
				float Roughness2MinusOne12_g7518 = 0;
				{
				Albedo12_g7518 = brdfData12_g7518.albedo;
				Diffuse12_g7518 = brdfData12_g7518.diffuse;
				Specular12_g7518 = brdfData12_g7518.specular;
				Reflectivity12_g7518 = brdfData12_g7518.reflectivity;
				PerceptualRoughness12_g7518 = brdfData12_g7518.perceptualRoughness;
				Roughness12_g7518 = brdfData12_g7518.roughness ;
				Roughness212_g7518 = brdfData12_g7518.roughness2;
				GrazingTerm12_g7518 = brdfData12_g7518.grazingTerm;
				NormalizationTerm12_g7518 = brdfData12_g7518.normalizationTerm;
				Roughness2MinusOne12_g7518 = brdfData12_g7518.roughness2MinusOne;
				}
				float3 Albedo1_g7523 = Albedo12_g7518;
				float3 temp_output_527_39 = Diffuse12_g7518;
				float3 lerpResult327 = lerp( temp_output_527_39 , ( temp_output_527_39 * _MoistDiffuseCoeff ) , MoistMask310);
				float3 Diffuse1_g7523 = lerpResult327;
				float3 Specular1_g7523 = Specular12_g7518;
				float Reflectivity1_g7523 = Reflectivity12_g7518;
				float PerceptualRoughness1_g7523 = PerceptualRoughness12_g7518;
				float Roughness1_g7523 = Roughness12_g7518;
				float Roughness21_g7523 = Roughness212_g7518;
				float GrazingTerm1_g7523 = GrazingTerm12_g7518;
				float NormalizationTerm1_g7523 = NormalizationTerm12_g7518;
				float Roughness2MinusOne1_g7523 = Roughness2MinusOne12_g7518;
				BRDFData brdfData1_g7523 = (BRDFData)0;
				{
				brdfData1_g7523 = (BRDFData)0;
				brdfData1_g7523.albedo = Albedo1_g7523;
				brdfData1_g7523.diffuse = Diffuse1_g7523;
				brdfData1_g7523.specular = Specular1_g7523;
				brdfData1_g7523.reflectivity = Reflectivity1_g7523;
				brdfData1_g7523.perceptualRoughness = PerceptualRoughness1_g7523;
				brdfData1_g7523.roughness = Roughness1_g7523;
				brdfData1_g7523.roughness2 = Roughness21_g7523;
				brdfData1_g7523.grazingTerm = GrazingTerm1_g7523;
				brdfData1_g7523.normalizationTerm = NormalizationTerm1_g7523;
				brdfData1_g7523.roughness2MinusOne = Roughness2MinusOne1_g7523;
				}
				BRDFData brdfData12_g7530 = brdfData1_g7523;
				float3 Albedo12_g7530 = float3( 0,0,0 );
				float3 Diffuse12_g7530 = float3( 0,0,0 );
				float3 Specular12_g7530 = float3( 0,0,0 );
				float Reflectivity12_g7530 = 0;
				float PerceptualRoughness12_g7530 = 0;
				float Roughness12_g7530 = 0;
				float Roughness212_g7530 = 0;
				float GrazingTerm12_g7530 = 0;
				float NormalizationTerm12_g7530 = 0;
				float Roughness2MinusOne12_g7530 = 0;
				{
				Albedo12_g7530 = brdfData12_g7530.albedo;
				Diffuse12_g7530 = brdfData12_g7530.diffuse;
				Specular12_g7530 = brdfData12_g7530.specular;
				Reflectivity12_g7530 = brdfData12_g7530.reflectivity;
				PerceptualRoughness12_g7530 = brdfData12_g7530.perceptualRoughness;
				Roughness12_g7530 = brdfData12_g7530.roughness ;
				Roughness212_g7530 = brdfData12_g7530.roughness2;
				GrazingTerm12_g7530 = brdfData12_g7530.grazingTerm;
				NormalizationTerm12_g7530 = brdfData12_g7530.normalizationTerm;
				Roughness2MinusOne12_g7530 = brdfData12_g7530.roughness2MinusOne;
				}
				float3 desaturateInitialColor669 = ( ApproxRealtimeGI942.xyz * Diffuse12_g7530 );
				float desaturateDot669 = dot( desaturateInitialColor669, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar669 = lerp( desaturateInitialColor669, desaturateDot669.xxx, ( 1.0 - (AmbientGround920).w ) );
				float3 IndirectDiffuse946 = desaturateVar669;
				float3 bakedReflectClearCoat41_g7529 = float3( 0,0,0 );
				float dotResult51_g7519 = dot( normalWS58 , viewDirWS40 );
				float temp_output_53_0_g7519 = ( 1.0 - saturate( dotResult51_g7519 ) );
				float fresnel54_g7519 = ( temp_output_53_0_g7519 * temp_output_53_0_g7519 * temp_output_53_0_g7519 * temp_output_53_0_g7519 );
				float fresnel94 = fresnel54_g7519;
				float temp_output_20_0_g7529 = fresnel94;
				float fresnel41_g7529 = temp_output_20_0_g7529;
				float localMyCustomExpression39_g7529 = ( 0.0 );
				BRDFData brdfData39_g7529 = (BRDFData)0;
				{
				 
				}
				BRDFData brdfDataClearCoat41_g7529 =(BRDFData)brdfData39_g7529;
				float clearCoatMask41_g7529 = 0.0;
				float3 ReflectionDirWS902 = reflect( -viewDirWS40 , normalWS58 );
				float3 temp_output_15_0_g7526 = ReflectionDirWS902;
				float3 reflectDirWS26_g7526 = temp_output_15_0_g7526;
				float3 temp_output_16_0_g7526 = positionWS36;
				float3 positionWS26_g7526 = temp_output_16_0_g7526;
				float temp_output_17_0_g7526 = saturate( Roughness52 );
				float perceptualRoughness26_g7526 = temp_output_17_0_g7526;
				float2 temp_output_18_0_g7526 = positionSS42.xy;
				float2 normalizedScreenSpaceUV26_g7526 = temp_output_18_0_g7526;
				float3 localGetBakedReflect26_g7526 = GetBakedReflect26_g7526( reflectDirWS26_g7526 , positionWS26_g7526 , perceptualRoughness26_g7526 , normalizedScreenSpaceUV26_g7526 );
				float3 BakedReflect905 = localGetBakedReflect26_g7526;
				float3 desaturateInitialColor702 = BakedReflect905;
				float desaturateDot702 = dot( desaturateInitialColor702, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar702 = lerp( desaturateInitialColor702, desaturateDot702.xxx, 0.0 );
				float3 hsvTorgb704 = RGBToHSV( AmbientSkyMixEquator938 );
				float3 hsvTorgb705 = HSVToRGB( float3(hsvTorgb704.x,hsvTorgb704.y,1.0) );
				float dotResult707 = dot( ReflectionDirWS902 , float3(0,1,0) );
				float temp_output_709_0 = saturate( dotResult707 );
				float4 lerpResult710 = lerp( float4( ( desaturateVar702 * hsvTorgb705 ) , 0.0 ) , _ApproxRealtimeGI_SkyColor , ( temp_output_709_0 * temp_output_709_0 * temp_output_709_0 * temp_output_709_0 ));
				float4 lerpResult720 = lerp( float4( BakedReflect905 , 0.0 ) , lerpResult710 , _ApproxRealtimeGI_MixCoeff);
				float4 ApproxRealtimeReflection894 = ( lerpResult720 * _ApproxRealtimeGI_ReflectionStrength );
				float smoothstepResult724 = smoothstep( _ApproxRealtimeGI_AOMin , _ApproxRealtimeGI_AOMax , BakedGI916);
				float BakedGItoAO931 = smoothstepResult724;
				float4 temp_output_933_0 = ( ApproxRealtimeReflection894 * BakedGItoAO931 );
				float2 uv763 = positionSS42.xy;
				float3 temp_output_31_0_g7477 = normalWS58;
				float3 worldToViewDir68_g7477 = mul( UNITY_MATRIX_V, float4( temp_output_31_0_g7477, 0 ) ).xyz;
				float3 normalizeResult142_g7477 = normalize( worldToViewDir68_g7477 );
				float3 _Vector7 = float3(1,1,-1);
				float3 normalVS763 = ( normalizeResult142_g7477 * _Vector7 );
				float localPosition1_g7486 = ( 0.0 );
				float localGetPositionTransformSpaceFromWorld1_g7484 = ( 0.0 );
				float3 temp_output_14_0_g7478 = positionWS36;
				float3 positionWS1_g7484 = temp_output_14_0_g7478;
				Position position1_g7484 =(Position)0;
				GetPositionTransformSpaceFromWorld_float( positionWS1_g7484 , position1_g7484 );
				Position position1_g7486 =(Position)position1_g7484;
				float3 OS1_g7486 = float3( 0,0,0 );
				float3 WS1_g7486 = float3( 0,0,0 );
				float3 VS1_g7486 = float3( 0,0,0 );
				float4 CS1_g7486 = float4( 0,0,0,0 );
				float4 NDC1_g7486 = float4( 0,0,0,0 );
				float3 SS1_g7486 = float3( 0,0,0 );
				float4 DS1_g7486 = float4( 0,0,0,0 );
				float3 LS1_g7486 = float3( 0,0,0 );
				float4 SC1_g7486 = float4( 0,0,0,0 );
				Position_float( position1_g7486 , OS1_g7486 , WS1_g7486 , VS1_g7486 , CS1_g7486 , NDC1_g7486 , SS1_g7486 , DS1_g7486 , LS1_g7486 , SC1_g7486 );
				float3 rayStart763 = ( VS1_g7486 * _Vector7 );
				float4 localSSR_Pass763 = SSR_Pass( uv763 , normalVS763 , rayStart763 );
				float4 ReflectionSS898 = localSSR_Pass763;
				float3 localGetViewForwardDir_Ref2_g7487 = GetViewForwardDir_Ref2_g7487();
				float3 ForwardDirWS889 = localGetViewForwardDir_Ref2_g7487;
				float dotResult841 = dot( ForwardDirWS889 , float3(0,-1,0) );
				float temp_output_845_0 = ( 1.0 - saturate( dotResult841 ) );
				float smoothstepResult980 = smoothstep( 0.0 , 0.9 , temp_output_845_0);
				float4 lerpResult767 = lerp( ApproxRealtimeReflection894 , ( ( ApproxRealtimeReflection894 * ReflectionSS898 ) + ReflectionSS898 ) , ( (ReflectionSS898).w * AccumulatedWaterMask286 * smoothstepResult980 ));
				#ifdef _SCREENREFLECTION_ON
				float4 staticSwitch768 = ( BakedGItoAO931 * lerpResult767 );
				#else
				float4 staticSwitch768 = temp_output_933_0;
				#endif
				#ifdef _ACCUMULATEDWATER_ON
				float4 staticSwitch976 = staticSwitch768;
				#else
				float4 staticSwitch976 = temp_output_933_0;
				#endif
				float4 ApproxRealtimeFinalReflect936 = staticSwitch976;
				float3 temp_cast_17 = (GrazingTerm12_g7530).xxx;
				float3 lerpResult8_g7529 = lerp( Specular12_g7530 , temp_cast_17 , temp_output_20_0_g7529);
				float3 mainBakedReflect41_g7529 = ( ApproxRealtimeFinalReflect936.rgb * ( lerpResult8_g7529 / ( Roughness212_g7530 + 1.0 ) ) );
				float3 localAppendClearCoatReflect41_g7529 = AppendClearCoatReflect41_g7529( bakedReflectClearCoat41_g7529 , fresnel41_g7529 , brdfDataClearCoat41_g7529 , clearCoatMask41_g7529 , mainBakedReflect41_g7529 );
				float3 temp_output_196_0 = localAppendClearCoatReflect41_g7529;
				float3 lerpResult826 = lerp( temp_output_196_0 , ( temp_output_196_0 * ( 1.0 - ( PrimitiveRoughness830 * _OnAccumulatedWaterEnvEeflectAtten ) ) ) , ( GlobalMoist01393 * ( 1.0 - ModifWaterMask867 ) ));
				float3 IndirectSpecular947 = lerpResult826;
				float3 GIColor1_g7531 = ( ( temp_output_18_0_g7531 * IndirectDiffuse946 ) + ( temp_output_34_0_g7527 * IndirectSpecular947 ) );
				float DirectSSAO44_g7527 = DirectAmbientOcclusion4_g7527;
				float temp_output_28_0_g7527 = min( DirectSSAO44_g7527 , saturate( ( BakedAO40_g7527 * 2.0 ) ) );
				float temp_output_21_0_g7531 = temp_output_28_0_g7527;
				int localIsUseLightLayer11_g7508 = IsUseLightLayer11_g7508();
				Light light19_g7502 =(Light)light1_g7488;
				int localGetLightLayer_WBhrsge19_g7502 = GetLightLayer_WBhrsge19_g7502( light19_g7502 );
				int lightLayers10_g7508 = localGetLightLayer_WBhrsge19_g7502;
				int localGetMeshRenderingLayer_Ref14_g7508 = GetMeshRenderingLayer_Ref14_g7508();
				int renderingLayers10_g7508 = localGetMeshRenderingLayer_Ref14_g7508;
				int localIsMatchingLightLayer_Ref10_g7508 = IsMatchingLightLayer_Ref10_g7508( lightLayers10_g7508 , renderingLayers10_g7508 );
				float localLightingPhysicallyBased5_g7502 = ( 0.0 );
				float3 normalWS5_g7502 = normalWS58;
				float3 viewDirWS5_g7502 = viewDirWS40;
				Light light5_g7502 =(Light)light1_g7488;
				BRDFData brdfData5_g7502 =(BRDFData)brdfData1_g7523;
				BRDFData brdfDataClearCoat5_g7502 =(BRDFData)0;
				float clearCoatMask5_g7502 = 0.0;
				float3 outDirectDiffuse5_g7502 = float3( 0,0,0 );
				float3 outDirectSpecular5_g7502 = float3( 0,0,0 );
				LightingPhysicallyBased( normalWS5_g7502 , viewDirWS5_g7502 , light5_g7502 , brdfData5_g7502 , brdfDataClearCoat5_g7502 , clearCoatMask5_g7502 , outDirectDiffuse5_g7502 , outDirectSpecular5_g7502 );
				float3 temp_output_7_0_g7508 = outDirectDiffuse5_g7502;
				float3 temp_cast_20 = (0.0).xxx;
				float3 MainDirectDiffuse952 = ( (float)localIsUseLightLayer11_g7508 != 0.0 ? ( (float)localIsMatchingLightLayer_Ref10_g7508 != 0.0 ? temp_output_7_0_g7508 : temp_cast_20 ) : temp_output_7_0_g7508 );
				float temp_output_20_0_g7531 = temp_output_28_0_g7527;
				int localIsUseLightLayer11_g7503 = IsUseLightLayer11_g7503();
				int lightLayers10_g7503 = localGetLightLayer_WBhrsge19_g7502;
				int localGetMeshRenderingLayer_Ref14_g7503 = GetMeshRenderingLayer_Ref14_g7503();
				int renderingLayers10_g7503 = localGetMeshRenderingLayer_Ref14_g7503;
				int localIsMatchingLightLayer_Ref10_g7503 = IsMatchingLightLayer_Ref10_g7503( lightLayers10_g7503 , renderingLayers10_g7503 );
				float3 temp_output_7_0_g7503 = outDirectSpecular5_g7502;
				float3 temp_cast_23 = (0.0).xxx;
				float3 clampResult477 = clamp( ( (float)localIsUseLightLayer11_g7503 != 0.0 ? ( (float)localIsMatchingLightLayer_Ref10_g7503 != 0.0 ? temp_output_7_0_g7503 : temp_cast_23 ) : temp_output_7_0_g7503 ) , float3( 0,0,0 ) , float3( 2,2,2 ) );
				float3 lerpResult834 = lerp( clampResult477 , ( clampResult477 * ( 1.0 - ( PrimitiveRoughness830 * _OnAccumulatedWaterLightingAtten ) ) ) , ( GlobalMoist01393 * ( 1.0 - AccumulatedWaterMask286 ) ));
				float3 MainDirectSpecular955 = lerpResult834;
				float3 MainLightColor1_g7531 = ( ( temp_output_21_0_g7531 * MainDirectDiffuse952 ) + ( temp_output_20_0_g7531 * MainDirectSpecular955 ) );
				float localLightingPhysicallyBased_AdditionaLighting7_g7513 = ( 0.0 );
				float3 positionWS7_g7513 = positionWS36;
				float2 positionSS7_g7513 = positionSS42.xy;
				float3 normalWS7_g7513 = normalWS58;
				float3 viewDirWS7_g7513 = viewDirWS40;
				float4 shadowMask49 = localShadowMask28_g7488;
				float4 shadowMask7_g7513 = shadowMask49;
				BRDFData brdfData7_g7513 =(BRDFData)brdfData1_g7523;
				float localMyCustomExpression18_g7513 = ( 0.0 );
				BRDFData BrdfData18_g7513 = (BRDFData)1;
				{
				 
				}
				BRDFData brdfDataClearCoat7_g7513 =(BRDFData)BrdfData18_g7513;
				float clearCoatMask7_g7513 = 0.0;
				float3 outAddDirectDiffuse7_g7513 = float3( 0,0,0 );
				float3 outAddDirectSpecular7_g7513 = float3( 0,0,0 );
				LightingPhysicallyBased_AdditionaLighting( positionWS7_g7513 , positionSS7_g7513 , normalWS7_g7513 , viewDirWS7_g7513 , shadowMask7_g7513 , brdfData7_g7513 , brdfDataClearCoat7_g7513 , clearCoatMask7_g7513 , outAddDirectDiffuse7_g7513 , outAddDirectSpecular7_g7513 );
				float3 clampResult671 = clamp( outAddDirectSpecular7_g7513 , float3( 0,0,0 ) , float3( 2,2,2 ) );
				float3 AdditionalLightsColor1_g7531 = ( ( temp_output_21_0_g7531 * outAddDirectDiffuse7_g7513 ) + ( temp_output_20_0_g7531 * clampResult671 ) );
				BRDFData BrdfData19_g7513 =(BRDFData)brdfData1_g7523;
				float3 localBrdfDataToDiffuse19_g7513 = BrdfDataToDiffuse19_g7513( BrdfData19_g7513 );
				float vertexToFrag530_g7193 = IN.ase_texcoord7.z;
				float vertexToFrag531_g7193 = IN.ase_texcoord7.w;
				float vertexToFrag532_g7193 = IN.ase_texcoord8.x;
				float3 appendResult534_g7193 = (float3(vertexToFrag530_g7193 , vertexToFrag531_g7193 , vertexToFrag532_g7193));
				float3 vertexlight35 = appendResult534_g7193;
				float3 VertexLightingColor1_g7531 = ( temp_output_18_0_g7531 * ( localBrdfDataToDiffuse19_g7513 * vertexlight35 ) );
				float3 temp_cast_25 = (0.1).xxx;
				float3 Emission572 = ( saturate( ( (tex2DNode112).rgb - temp_cast_25 ) ) * 1.111111 * _EmissionStrength0 );
				float3 EmissionColor1_g7531 = Emission572;
				LightingData lightingData1_g7531 = (LightingData)0;
				{
				lightingData1_g7531 = (LightingData)0;
				lightingData1_g7531.giColor = GIColor1_g7531;
				lightingData1_g7531.mainLightColor = MainLightColor1_g7531;
				lightingData1_g7531.additionalLightsColor = AdditionalLightsColor1_g7531;
				lightingData1_g7531.vertexLightingColor = VertexLightingColor1_g7531;
				lightingData1_g7531.emissionColor = EmissionColor1_g7531;
				}
				LightingData lightingData7_g7531 =(LightingData)lightingData1_g7531;
				float alpha7_g7531 = 1.0;
				float4 localCalculateFinalColor_Ref7_g7531 = CalculateFinalColor_Ref7_g7531( lightingData7_g7531 , alpha7_g7531 );
				float4 FinalColor914 = localCalculateFinalColor_Ref7_g7531;
				
				float Alpha149 = tex2DNode110.a;
				#ifdef _ALPHATEST_ON
				float staticSwitch156 = ( Alpha149 - _AlphaClipOffset );
				#else
				float staticSwitch156 = 1.0;
				#endif
				

				float3 Color = FinalColor914.xyz;
				float Alpha = staticSwitch156;
				float AlphaClipThresholdShadow = 0.5;
				float3 WorldNormal = normalWS58;
				float3 BakedAlbedo = Albedo57.rgb;
				float3 BakedEmission = Emission572;
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

			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 140011
			#define ASE_USING_SAMPLING_MACROS 1


			CBUFFER_START(UnityPerMaterial)
			float4 _SpecColor;
			float4 _BaseColorMap_ST;
			float4 _BaseColor;
			float _OnAccumulatedWaterLightingAtten;
			float _OnAccumulatedWaterEnvEeflectAtten;
			float _MoistDiffuseCoeff;
			float _MoistRoughnessCoeff;
			float _Roughness1;
			float _Float6;
			float _AccumulatedwaterReflectStrength;
			float _Metallic01;
			float _NormalStrength;
			float _OcclusionBaked;
			float _MoistAccumulatedwaterCoeff;
			float _Cull;
			float _EmissionStrength0;
			float _AlphaClipOffset;
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
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile_fragment __ _ACCUMULATEDWATER_ON
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;

				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D(_BaseColorMap);
			TEXTURE2D(_AccumulatedWaterMask);
			float _AccumulatedWaterMaskTiling;
			float _GlobalMoist;
			float _AccumulatedWaterContrast;
			float _AccumulatedWaterSteepHillExtinction;
			TEXTURE2D(_RipplesNormalAtlas);
			float _RipplesMainTiling;
			float4 _XColumnsYRowsZSpeedWStrartFrame;
			float _RipplesMainStrength;
			TEXTURE2D(_WaterWaveNormal);
			float _WaterWaveMainSpeed;
			float _WaterWaveRotate;
			float _WaterWaveMainTiling;
			float _WaterWaveMainStrength;
			float _WaterWaveDetailSpeed;
			float _WaterWaveDetailTiling;
			float _WaterWaveDetailStrength;
			TEXTURE2D(_FlowMap);
			float3 _FlowTiling;
			float _FlowStrength;
			float _AccumulatedWaterParallaxStrength;


			float3 TransformObjectToWorldNormal_Ref33_g7196( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			
			float3 TransformWorldToTangentDir_Ref133_g7194( float3 directionWS, float3x3 TBN )
			{
				return TransformWorldToTangentDir(directionWS, TBN);
			}
			
			float2 IterativeParallaxLegacy1_g7379( float height, float2 UVs, float2 plane, float refp, float scale )
			{
				UVs += plane * scale * refp;
				UVs += (height - 1) * plane * scale;
				return UVs;
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localOffsetShadow412_g7193 = ( 0.0 );
				float localPosition1_g7205 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g7204 = ( 0.0 );
				float3 temp_output_14_0_g7197 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g7204 = temp_output_14_0_g7197;
				Position position1_g7204 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g7204 , position1_g7204 );
				Position position1_g7205 =(Position)position1_g7204;
				float3 OS1_g7205 = float3( 0,0,0 );
				float3 WS1_g7205 = float3( 0,0,0 );
				float3 VS1_g7205 = float3( 0,0,0 );
				float4 CS1_g7205 = float4( 0,0,0,0 );
				float4 NDC1_g7205 = float4( 0,0,0,0 );
				float3 SS1_g7205 = float3( 0,0,0 );
				float4 DS1_g7205 = float4( 0,0,0,0 );
				float3 LS1_g7205 = float3( 0,0,0 );
				float4 SC1_g7205 = float4( 0,0,0,0 );
				Position_float( position1_g7205 , OS1_g7205 , WS1_g7205 , VS1_g7205 , CS1_g7205 , NDC1_g7205 , SS1_g7205 , DS1_g7205 , LS1_g7205 , SC1_g7205 );
				float3 temp_output_345_7_g7193 = WS1_g7205;
				float3 positionWS412_g7193 = temp_output_345_7_g7193;
				float3 temp_output_31_0_g7196 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g7196 = temp_output_31_0_g7196;
				float3 localTransformObjectToWorldNormal_Ref33_g7196 = TransformObjectToWorldNormal_Ref33_g7196( normalOS33_g7196 );
				float3 normalizeResult140_g7196 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g7196 );
				float3 temp_output_515_34_g7193 = normalizeResult140_g7196;
				float3 normalWS412_g7193 = temp_output_515_34_g7193;
				float4 positionCS412_g7193 = float4( 0,0,0,0 );
				{
				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS412_g7193);
				#else
				float3 lightDirectionWS = _LightDirection;
				#endif
				positionCS412_g7193 = TransformWorldToHClip(ApplyShadowBias(positionWS412_g7193, normalWS412_g7193, lightDirectionWS));
				#if UNITY_REVERSED_Z
				positionCS412_g7193.z = min(positionCS412_g7193.z, UNITY_NEAR_CLIP_VALUE);
				#else
				positionCS412_g7193.z = max(positionCS412_g7193.z, UNITY_NEAR_CLIP_VALUE);
				#endif
				}
				
				float2 break15 = ( ( v.ase_texcoord.xy * _BaseColorMap_ST.xy ) + _BaseColorMap_ST.zw );
				float vertexToFrag17 = break15.x;
				o.ase_texcoord.x = vertexToFrag17;
				float vertexToFrag16 = break15.y;
				o.ase_texcoord.y = vertexToFrag16;
				float3 vertexPositionWS386_g7193 = temp_output_345_7_g7193;
				float3 break310_g7193 = vertexPositionWS386_g7193;
				float vertexToFrag320_g7193 = break310_g7193.x;
				o.ase_texcoord.z = vertexToFrag320_g7193;
				float vertexToFrag321_g7193 = break310_g7193.y;
				o.ase_texcoord.w = vertexToFrag321_g7193;
				float vertexToFrag322_g7193 = break310_g7193.z;
				o.ase_texcoord1.x = vertexToFrag322_g7193;
				float3 VertexNormalWS314_g7193 = temp_output_515_34_g7193;
				float3 break138_g7193 = VertexNormalWS314_g7193;
				float vertexToFrag323_g7193 = break138_g7193.x;
				o.ase_texcoord1.y = vertexToFrag323_g7193;
				float vertexToFrag324_g7193 = break138_g7193.y;
				o.ase_texcoord1.z = vertexToFrag324_g7193;
				float vertexToFrag325_g7193 = break138_g7193.z;
				o.ase_texcoord1.w = vertexToFrag325_g7193;
				float3 normalizeResult129_g7193 = ASESafeNormalize( ( _WorldSpaceCameraPos - vertexPositionWS386_g7193 ) );
				float3 temp_output_43_0_g7194 = normalizeResult129_g7193;
				float3 directionWS133_g7194 = temp_output_43_0_g7194;
				float3 temp_output_43_0_g7195 = ( v.ase_tangent.xyz + float3( 0,0,0 ) );
				float3 objToWorldDir42_g7195 = mul( GetObjectToWorldMatrix(), float4( temp_output_43_0_g7195, 0 ) ).xyz;
				float3 normalizeResult128_g7195 = ASESafeNormalize( objToWorldDir42_g7195 );
				float3 VertexTangentlWS474_g7193 = normalizeResult128_g7195;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 normalizeResult473_g7193 = ASESafeNormalize( ( cross( VertexNormalWS314_g7193 , VertexTangentlWS474_g7193 ) * ase_vertexTangentSign ) );
				float3 VertexBitangentWS476_g7193 = normalizeResult473_g7193;
				float3x3 temp_output_103_0_g7194 = float3x3(VertexTangentlWS474_g7193, VertexBitangentWS476_g7193, VertexNormalWS314_g7193);
				float3x3 TBN133_g7194 = temp_output_103_0_g7194;
				float3 localTransformWorldToTangentDir_Ref133_g7194 = TransformWorldToTangentDir_Ref133_g7194( directionWS133_g7194 , TBN133_g7194 );
				float3 normalizeResult132_g7194 = ASESafeNormalize( localTransformWorldToTangentDir_Ref133_g7194 );
				float3 break336_g7193 = normalizeResult132_g7194;
				float vertexToFrag264_g7193 = break336_g7193.x;
				o.ase_texcoord2.x = vertexToFrag264_g7193;
				float vertexToFrag337_g7193 = break336_g7193.y;
				o.ase_texcoord2.y = vertexToFrag337_g7193;
				float vertexToFrag338_g7193 = break336_g7193.z;
				o.ase_texcoord2.z = vertexToFrag338_g7193;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;

				o.positionCS = positionCS412_g7193;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;

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
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float vertexToFrag17 = IN.ase_texcoord.x;
				float vertexToFrag16 = IN.ase_texcoord.y;
				float2 appendResult18 = (float2(vertexToFrag17 , vertexToFrag16));
				float vertexToFrag320_g7193 = IN.ase_texcoord.z;
				float vertexToFrag321_g7193 = IN.ase_texcoord.w;
				float vertexToFrag322_g7193 = IN.ase_texcoord1.x;
				float3 appendResult311_g7193 = (float3(vertexToFrag320_g7193 , vertexToFrag321_g7193 , vertexToFrag322_g7193));
				float3 positionWS402_g7193 = appendResult311_g7193;
				float3 positionWS36 = positionWS402_g7193;
				float3 break479 = positionWS36;
				float2 appendResult480 = (float2(break479.x , break479.z));
				float2 PositionWSxz481 = appendResult480;
				float localGlobalSampler2_g7220 = ( 0.0 );
				SamplerState PointClamp2_g7220 = sampler_PointClamp;
				SamplerState LinearClamp2_g7220 = sampler_LinearClamp;
				SamplerState PointRepeat2_g7220 = sampler_PointRepeat;
				SamplerState LinearRepeat2_g7220 = sampler_LinearRepeat;
				{
				PointClamp2_g7220 = sampler_PointClamp;
				LinearClamp2_g7220 = sampler_LinearClamp;
				PointRepeat2_g7220 = sampler_PointRepeat;
				LinearRepeat2_g7220 = sampler_LinearRepeat;
				}
				float GlobalMoist12401 = saturate( ( _GlobalMoist - 1.0 ) );
				float lerpResult519 = lerp( 0.5 , ( SAMPLE_TEXTURE2D( _AccumulatedWaterMask, LinearRepeat2_g7220, ( PositionWSxz481 * _AccumulatedWaterMaskTiling ) ).r + (GlobalMoist12401*2.0 + -1.0) ) , _AccumulatedWaterContrast);
				float vertexToFrag323_g7193 = IN.ase_texcoord1.y;
				float vertexToFrag324_g7193 = IN.ase_texcoord1.z;
				float vertexToFrag325_g7193 = IN.ase_texcoord1.w;
				float3 appendResult142_g7193 = (float3(vertexToFrag323_g7193 , vertexToFrag324_g7193 , vertexToFrag325_g7193));
				float3 normalizeResult459_g7193 = normalize( appendResult142_g7193 );
				float3 NormalWS388_g7193 = normalizeResult459_g7193;
				float3 vNormalWS39 = NormalWS388_g7193;
				float dotResult408 = dot( vNormalWS39 , float3(0,1,0) );
				float smoothstepResult413 = smoothstep( _AccumulatedWaterSteepHillExtinction , 1.0 , dotResult408);
				float FlatArea522 = smoothstepResult413;
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch962 = ( saturate( (lerpResult519).x ) * FlatArea522 * _MoistAccumulatedwaterCoeff );
				#else
				float staticSwitch962 = 0.0;
				#endif
				float AccumulatedWaterMask286 = staticSwitch962;
				float height1_g7379 = AccumulatedWaterMask286;
				float2 break135_g7377 = ( PositionWSxz481 * _RipplesMainTiling );
				float2 appendResult206_g7377 = (float2(frac( break135_g7377.x ) , frac( break135_g7377.y )));
				float temp_output_4_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.x;
				float temp_output_5_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.y;
				float2 appendResult116_g7377 = (float2(temp_output_4_0_g7377 , temp_output_5_0_g7377));
				float temp_output_122_0_g7377 = ( temp_output_4_0_g7377 * temp_output_5_0_g7377 );
				float2 appendResult175_g7377 = (float2(temp_output_122_0_g7377 , temp_output_5_0_g7377));
				float Columns213_g7377 = temp_output_4_0_g7377;
				float Rows212_g7377 = temp_output_5_0_g7377;
				float temp_output_133_0_g7377 = ( fmod( _TimeParameters.x , ( Columns213_g7377 * Rows212_g7377 ) ) * _XColumnsYRowsZSpeedWStrartFrame.z );
				float clampResult129_g7377 = clamp( _XColumnsYRowsZSpeedWStrartFrame.w , 1E-05 , ( temp_output_122_0_g7377 - 1.0 ) );
				float temp_output_185_0_g7377 = frac( ( ( temp_output_133_0_g7377 + ( clampResult129_g7377 + 1E-05 ) ) / temp_output_122_0_g7377 ) );
				float2 appendResult186_g7377 = (float2(temp_output_185_0_g7377 , ( 1.0 - temp_output_185_0_g7377 )));
				float2 temp_output_203_0_g7377 = ( ( appendResult206_g7377 / appendResult116_g7377 ) + ( floor( ( appendResult175_g7377 * appendResult186_g7377 ) ) / appendResult116_g7377 ) );
				float3 unpack233 = UnpackNormalScale( SAMPLE_TEXTURE2D( _RipplesNormalAtlas, LinearRepeat2_g7220, temp_output_203_0_g7377 ), _RipplesMainStrength );
				unpack233.z = lerp( 1, unpack233.z, saturate(_RipplesMainStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch964 = unpack233;
				#else
				float3 staticSwitch964 = float3(0,0,1);
				#endif
				float3 RipplesNormal294 = staticSwitch964;
				float2 _MainWaterWaveDir = float2(1,0);
				float cos278 = cos( radians( _WaterWaveRotate ) );
				float sin278 = sin( radians( _WaterWaveRotate ) );
				float2 rotator278 = mul( PositionWSxz481 - float2( 0.5,0.5 ) , float2x2( cos278 , -sin278 , sin278 , cos278 )) + float2( 0.5,0.5 );
				float2 panner259 = ( 1.0 * _Time.y * ( _MainWaterWaveDir * _WaterWaveMainSpeed ) + ( rotator278 * _WaterWaveMainTiling ));
				float3 unpack251 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner259 ), _WaterWaveMainStrength );
				unpack251.z = lerp( 1, unpack251.z, saturate(_WaterWaveMainStrength) );
				float2 panner269 = ( 1.0 * _Time.y * ( -_MainWaterWaveDir * _WaterWaveDetailSpeed ) + ( rotator278 * _WaterWaveDetailTiling ));
				float3 unpack275 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner269 ), _WaterWaveDetailStrength );
				unpack275.z = lerp( 1, unpack275.z, saturate(_WaterWaveDetailStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch972 = BlendNormal( unpack251 , unpack275 );
				#else
				float3 staticSwitch972 = float3(0,0,1);
				#endif
				float3 WaterWaveNormal306 = staticSwitch972;
				float3 temp_output_430_0 = ( positionWS36 * _FlowTiling );
				half3 Position160_g7376 = temp_output_430_0;
				float3 break170_g7376 = Position160_g7376;
				float2 appendResult171_g7376 = (float2(break170_g7376.z , break170_g7376.y));
				half3 Normal168_g7376 = vNormalWS39;
				float2 temp_output_180_0_g7376 = abs( (Normal168_g7376).xz );
				float2 temp_output_205_0_g7376 = ( temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 );
				float2 break183_g7376 = temp_output_205_0_g7376;
				float2 break185_g7376 = ( temp_output_205_0_g7376 / max( ( break183_g7376.x + break183_g7376.y ) , 1E-05 ) );
				float3 break186_g7376 = Position160_g7376;
				float2 appendResult191_g7376 = (float2(break186_g7376.x , break186_g7376.y));
				float4 break438 = ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7376 ) * break185_g7376.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7376 ) * break185_g7376.y ) );
				float2 appendResult437 = (float2(break438.b , break438.a));
				float2 normalMapRG1_g7378 = appendResult437;
				float4 localDecodeNormalRG1_g7378 = DecodeNormalRG( normalMapRG1_g7378 );
				float3 unpack4_g7378 = UnpackNormalScale( localDecodeNormalRG1_g7378, 0.25 );
				unpack4_g7378.z = lerp( 1, unpack4_g7378.z, saturate(0.25) );
				float3 normalizeResult449 = normalize( unpack4_g7378 );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch970 = normalizeResult449;
				#else
				float3 staticSwitch970 = float3(0,0,1);
				#endif
				float3 FlowNormal450 = staticSwitch970;
				half3 Position160_g7375 = ( temp_output_430_0 + ( _TimeParameters.x * float3(0,5,0) * 0.1 ) );
				float3 break170_g7375 = Position160_g7375;
				float2 appendResult171_g7375 = (float2(break170_g7375.z , break170_g7375.y));
				half3 Normal168_g7375 = vNormalWS39;
				float2 temp_output_180_0_g7375 = abs( (Normal168_g7375).xz );
				float2 temp_output_205_0_g7375 = ( temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 );
				float2 break183_g7375 = temp_output_205_0_g7375;
				float2 break185_g7375 = ( temp_output_205_0_g7375 / max( ( break183_g7375.x + break183_g7375.y ) , 1E-05 ) );
				float3 break186_g7375 = Position160_g7375;
				float2 appendResult191_g7375 = (float2(break186_g7375.x , break186_g7375.y));
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch968 = ( saturate( ( ( break438.r - saturate( ( ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g ) ) ) * 5.0 ) ) * GlobalMoist12401 );
				#else
				float staticSwitch968 = 0.0;
				#endif
				float FlowMask454 = ( staticSwitch968 * _FlowStrength );
				float2 UVs1_g7379 = ( appendResult18 + ( (( RipplesNormal294 + WaterWaveNormal306 )).xy * AccumulatedWaterMask286 * 0.02 ) + ( 0.02 * (FlowNormal450).xy * FlowMask454 ) );
				float vertexToFrag264_g7193 = IN.ase_texcoord2.x;
				float vertexToFrag337_g7193 = IN.ase_texcoord2.y;
				float vertexToFrag338_g7193 = IN.ase_texcoord2.z;
				float3 appendResult340_g7193 = (float3(vertexToFrag264_g7193 , vertexToFrag337_g7193 , vertexToFrag338_g7193));
				float3 normalizeResult451_g7193 = normalize( appendResult340_g7193 );
				float3 viewDirTS41 = normalizeResult451_g7193;
				float3 break6_g7379 = viewDirTS41;
				float2 appendResult5_g7379 = (float2(break6_g7379.x , break6_g7379.y));
				float2 plane1_g7379 = ( appendResult5_g7379 / break6_g7379.z );
				float refp1_g7379 = 1.0;
				float scale1_g7379 = ( _AccumulatedWaterParallaxStrength * 0.01 );
				float2 localIterativeParallaxLegacy1_g7379 = IterativeParallaxLegacy1_g7379( height1_g7379 , UVs1_g7379 , plane1_g7379 , refp1_g7379 , scale1_g7379 );
				#ifdef _ACCUMULATEDWATER_ON
				float2 staticSwitch974 = localIterativeParallaxLegacy1_g7379;
				#else
				float2 staticSwitch974 = appendResult18;
				#endif
				float2 DistortionUV298 = staticSwitch974;
				float4 tex2DNode110 = SAMPLE_TEXTURE2D( _BaseColorMap, LinearRepeat2_g7220, DistortionUV298 );
				float Alpha149 = tex2DNode110.a;
				#ifdef _ALPHATEST_ON
				float staticSwitch156 = ( Alpha149 - _AlphaClipOffset );
				#else
				float staticSwitch156 = 1.0;
				#endif
				

				float Alpha = staticSwitch156;
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
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define ASE_SRP_VERSION 140011
			#define ASE_USING_SAMPLING_MACROS 1

			
			CBUFFER_START(UnityPerMaterial)
			float4 _SpecColor;
			float4 _BaseColorMap_ST;
			float4 _BaseColor;
			float _OnAccumulatedWaterLightingAtten;
			float _OnAccumulatedWaterEnvEeflectAtten;
			float _MoistDiffuseCoeff;
			float _MoistRoughnessCoeff;
			float _Roughness1;
			float _Float6;
			float _AccumulatedwaterReflectStrength;
			float _Metallic01;
			float _NormalStrength;
			float _OcclusionBaked;
			float _MoistAccumulatedwaterCoeff;
			float _Cull;
			float _EmissionStrength0;
			float _AlphaClipOffset;
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

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#include "Packages/com.worldsystem/Assets/Plugins/AmplifyShaderEditorExtend/ShaderLibrary/BaseFunctionLibrary.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile_fragment __ _ACCUMULATEDWATER_ON
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D(_BaseColorMap);
			TEXTURE2D(_AccumulatedWaterMask);
			float _AccumulatedWaterMaskTiling;
			float _GlobalMoist;
			float _AccumulatedWaterContrast;
			float _AccumulatedWaterSteepHillExtinction;
			TEXTURE2D(_RipplesNormalAtlas);
			float _RipplesMainTiling;
			float4 _XColumnsYRowsZSpeedWStrartFrame;
			float _RipplesMainStrength;
			TEXTURE2D(_WaterWaveNormal);
			float _WaterWaveMainSpeed;
			float _WaterWaveRotate;
			float _WaterWaveMainTiling;
			float _WaterWaveMainStrength;
			float _WaterWaveDetailSpeed;
			float _WaterWaveDetailTiling;
			float _WaterWaveDetailStrength;
			TEXTURE2D(_FlowMap);
			float3 _FlowTiling;
			float _FlowStrength;
			float _AccumulatedWaterParallaxStrength;
			TEXTURE2D(Smoothness);


			float4 MetaVertexPosition_Ref( float4 positionOS, float2 texcoord1, float2 texcoord2, float4 LightmapST, float4 DynamicLightmapST )
			{
				return MetaVertexPosition( positionOS, texcoord1, texcoord2, LightmapST, DynamicLightmapST );
			}
			
			float3 TransformObjectToWorldNormal_Ref33_g7196( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			
			float3 TransformWorldToTangentDir_Ref133_g7194( float3 directionWS, float3x3 TBN )
			{
				return TransformWorldToTangentDir(directionWS, TBN);
			}
			
			float2 IterativeParallaxLegacy1_g7379( float height, float2 UVs, float2 plane, float refp, float scale )
			{
				UVs += plane * scale * refp;
				UVs += (height - 1) * plane * scale;
				return UVs;
			}
			

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 positionOS414_g7193 = v.positionOS;
				float2 texcoord1414_g7193 = v.texcoord1.xy;
				float2 texcoord2414_g7193 = v.texcoord2.xy;
				float4 LightmapST414_g7193 = unity_LightmapST;
				float4 DynamicLightmapST414_g7193 = unity_DynamicLightmapST;
				float4 localMetaVertexPosition_Ref414_g7193 = MetaVertexPosition_Ref( positionOS414_g7193 , texcoord1414_g7193 , texcoord2414_g7193 , LightmapST414_g7193 , DynamicLightmapST414_g7193 );
				
				float2 break15 = ( ( v.ase_texcoord.xy * _BaseColorMap_ST.xy ) + _BaseColorMap_ST.zw );
				float vertexToFrag17 = break15.x;
				o.ase_texcoord.x = vertexToFrag17;
				float vertexToFrag16 = break15.y;
				o.ase_texcoord.y = vertexToFrag16;
				float localPosition1_g7205 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g7204 = ( 0.0 );
				float3 temp_output_14_0_g7197 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g7204 = temp_output_14_0_g7197;
				Position position1_g7204 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g7204 , position1_g7204 );
				Position position1_g7205 =(Position)position1_g7204;
				float3 OS1_g7205 = float3( 0,0,0 );
				float3 WS1_g7205 = float3( 0,0,0 );
				float3 VS1_g7205 = float3( 0,0,0 );
				float4 CS1_g7205 = float4( 0,0,0,0 );
				float4 NDC1_g7205 = float4( 0,0,0,0 );
				float3 SS1_g7205 = float3( 0,0,0 );
				float4 DS1_g7205 = float4( 0,0,0,0 );
				float3 LS1_g7205 = float3( 0,0,0 );
				float4 SC1_g7205 = float4( 0,0,0,0 );
				Position_float( position1_g7205 , OS1_g7205 , WS1_g7205 , VS1_g7205 , CS1_g7205 , NDC1_g7205 , SS1_g7205 , DS1_g7205 , LS1_g7205 , SC1_g7205 );
				float3 temp_output_345_7_g7193 = WS1_g7205;
				float3 vertexPositionWS386_g7193 = temp_output_345_7_g7193;
				float3 break310_g7193 = vertexPositionWS386_g7193;
				float vertexToFrag320_g7193 = break310_g7193.x;
				o.ase_texcoord.z = vertexToFrag320_g7193;
				float vertexToFrag321_g7193 = break310_g7193.y;
				o.ase_texcoord.w = vertexToFrag321_g7193;
				float vertexToFrag322_g7193 = break310_g7193.z;
				o.ase_texcoord1.x = vertexToFrag322_g7193;
				float3 temp_output_31_0_g7196 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g7196 = temp_output_31_0_g7196;
				float3 localTransformObjectToWorldNormal_Ref33_g7196 = TransformObjectToWorldNormal_Ref33_g7196( normalOS33_g7196 );
				float3 normalizeResult140_g7196 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g7196 );
				float3 temp_output_515_34_g7193 = normalizeResult140_g7196;
				float3 VertexNormalWS314_g7193 = temp_output_515_34_g7193;
				float3 break138_g7193 = VertexNormalWS314_g7193;
				float vertexToFrag323_g7193 = break138_g7193.x;
				o.ase_texcoord1.y = vertexToFrag323_g7193;
				float vertexToFrag324_g7193 = break138_g7193.y;
				o.ase_texcoord1.z = vertexToFrag324_g7193;
				float vertexToFrag325_g7193 = break138_g7193.z;
				o.ase_texcoord1.w = vertexToFrag325_g7193;
				float3 normalizeResult129_g7193 = ASESafeNormalize( ( _WorldSpaceCameraPos - vertexPositionWS386_g7193 ) );
				float3 temp_output_43_0_g7194 = normalizeResult129_g7193;
				float3 directionWS133_g7194 = temp_output_43_0_g7194;
				float3 temp_output_43_0_g7195 = ( v.ase_tangent.xyz + float3( 0,0,0 ) );
				float3 objToWorldDir42_g7195 = mul( GetObjectToWorldMatrix(), float4( temp_output_43_0_g7195, 0 ) ).xyz;
				float3 normalizeResult128_g7195 = ASESafeNormalize( objToWorldDir42_g7195 );
				float3 VertexTangentlWS474_g7193 = normalizeResult128_g7195;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 normalizeResult473_g7193 = ASESafeNormalize( ( cross( VertexNormalWS314_g7193 , VertexTangentlWS474_g7193 ) * ase_vertexTangentSign ) );
				float3 VertexBitangentWS476_g7193 = normalizeResult473_g7193;
				float3x3 temp_output_103_0_g7194 = float3x3(VertexTangentlWS474_g7193, VertexBitangentWS476_g7193, VertexNormalWS314_g7193);
				float3x3 TBN133_g7194 = temp_output_103_0_g7194;
				float3 localTransformWorldToTangentDir_Ref133_g7194 = TransformWorldToTangentDir_Ref133_g7194( directionWS133_g7194 , TBN133_g7194 );
				float3 normalizeResult132_g7194 = ASESafeNormalize( localTransformWorldToTangentDir_Ref133_g7194 );
				float3 break336_g7193 = normalizeResult132_g7194;
				float vertexToFrag264_g7193 = break336_g7193.x;
				o.ase_texcoord2.x = vertexToFrag264_g7193;
				float vertexToFrag337_g7193 = break336_g7193.y;
				o.ase_texcoord2.y = vertexToFrag337_g7193;
				float vertexToFrag338_g7193 = break336_g7193.z;
				o.ase_texcoord2.z = vertexToFrag338_g7193;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;

				float4 defaultPositionCS = MetaVertexPosition( v.positionOS, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
				o.positionCS = localMetaVertexPosition_Ref414_g7193;

				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float vertexToFrag17 = IN.ase_texcoord.x;
				float vertexToFrag16 = IN.ase_texcoord.y;
				float2 appendResult18 = (float2(vertexToFrag17 , vertexToFrag16));
				float vertexToFrag320_g7193 = IN.ase_texcoord.z;
				float vertexToFrag321_g7193 = IN.ase_texcoord.w;
				float vertexToFrag322_g7193 = IN.ase_texcoord1.x;
				float3 appendResult311_g7193 = (float3(vertexToFrag320_g7193 , vertexToFrag321_g7193 , vertexToFrag322_g7193));
				float3 positionWS402_g7193 = appendResult311_g7193;
				float3 positionWS36 = positionWS402_g7193;
				float3 break479 = positionWS36;
				float2 appendResult480 = (float2(break479.x , break479.z));
				float2 PositionWSxz481 = appendResult480;
				float localGlobalSampler2_g7220 = ( 0.0 );
				SamplerState PointClamp2_g7220 = sampler_PointClamp;
				SamplerState LinearClamp2_g7220 = sampler_LinearClamp;
				SamplerState PointRepeat2_g7220 = sampler_PointRepeat;
				SamplerState LinearRepeat2_g7220 = sampler_LinearRepeat;
				{
				PointClamp2_g7220 = sampler_PointClamp;
				LinearClamp2_g7220 = sampler_LinearClamp;
				PointRepeat2_g7220 = sampler_PointRepeat;
				LinearRepeat2_g7220 = sampler_LinearRepeat;
				}
				float GlobalMoist12401 = saturate( ( _GlobalMoist - 1.0 ) );
				float lerpResult519 = lerp( 0.5 , ( SAMPLE_TEXTURE2D( _AccumulatedWaterMask, LinearRepeat2_g7220, ( PositionWSxz481 * _AccumulatedWaterMaskTiling ) ).r + (GlobalMoist12401*2.0 + -1.0) ) , _AccumulatedWaterContrast);
				float vertexToFrag323_g7193 = IN.ase_texcoord1.y;
				float vertexToFrag324_g7193 = IN.ase_texcoord1.z;
				float vertexToFrag325_g7193 = IN.ase_texcoord1.w;
				float3 appendResult142_g7193 = (float3(vertexToFrag323_g7193 , vertexToFrag324_g7193 , vertexToFrag325_g7193));
				float3 normalizeResult459_g7193 = normalize( appendResult142_g7193 );
				float3 NormalWS388_g7193 = normalizeResult459_g7193;
				float3 vNormalWS39 = NormalWS388_g7193;
				float dotResult408 = dot( vNormalWS39 , float3(0,1,0) );
				float smoothstepResult413 = smoothstep( _AccumulatedWaterSteepHillExtinction , 1.0 , dotResult408);
				float FlatArea522 = smoothstepResult413;
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch962 = ( saturate( (lerpResult519).x ) * FlatArea522 * _MoistAccumulatedwaterCoeff );
				#else
				float staticSwitch962 = 0.0;
				#endif
				float AccumulatedWaterMask286 = staticSwitch962;
				float height1_g7379 = AccumulatedWaterMask286;
				float2 break135_g7377 = ( PositionWSxz481 * _RipplesMainTiling );
				float2 appendResult206_g7377 = (float2(frac( break135_g7377.x ) , frac( break135_g7377.y )));
				float temp_output_4_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.x;
				float temp_output_5_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.y;
				float2 appendResult116_g7377 = (float2(temp_output_4_0_g7377 , temp_output_5_0_g7377));
				float temp_output_122_0_g7377 = ( temp_output_4_0_g7377 * temp_output_5_0_g7377 );
				float2 appendResult175_g7377 = (float2(temp_output_122_0_g7377 , temp_output_5_0_g7377));
				float Columns213_g7377 = temp_output_4_0_g7377;
				float Rows212_g7377 = temp_output_5_0_g7377;
				float temp_output_133_0_g7377 = ( fmod( _TimeParameters.x , ( Columns213_g7377 * Rows212_g7377 ) ) * _XColumnsYRowsZSpeedWStrartFrame.z );
				float clampResult129_g7377 = clamp( _XColumnsYRowsZSpeedWStrartFrame.w , 1E-05 , ( temp_output_122_0_g7377 - 1.0 ) );
				float temp_output_185_0_g7377 = frac( ( ( temp_output_133_0_g7377 + ( clampResult129_g7377 + 1E-05 ) ) / temp_output_122_0_g7377 ) );
				float2 appendResult186_g7377 = (float2(temp_output_185_0_g7377 , ( 1.0 - temp_output_185_0_g7377 )));
				float2 temp_output_203_0_g7377 = ( ( appendResult206_g7377 / appendResult116_g7377 ) + ( floor( ( appendResult175_g7377 * appendResult186_g7377 ) ) / appendResult116_g7377 ) );
				float3 unpack233 = UnpackNormalScale( SAMPLE_TEXTURE2D( _RipplesNormalAtlas, LinearRepeat2_g7220, temp_output_203_0_g7377 ), _RipplesMainStrength );
				unpack233.z = lerp( 1, unpack233.z, saturate(_RipplesMainStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch964 = unpack233;
				#else
				float3 staticSwitch964 = float3(0,0,1);
				#endif
				float3 RipplesNormal294 = staticSwitch964;
				float2 _MainWaterWaveDir = float2(1,0);
				float cos278 = cos( radians( _WaterWaveRotate ) );
				float sin278 = sin( radians( _WaterWaveRotate ) );
				float2 rotator278 = mul( PositionWSxz481 - float2( 0.5,0.5 ) , float2x2( cos278 , -sin278 , sin278 , cos278 )) + float2( 0.5,0.5 );
				float2 panner259 = ( 1.0 * _Time.y * ( _MainWaterWaveDir * _WaterWaveMainSpeed ) + ( rotator278 * _WaterWaveMainTiling ));
				float3 unpack251 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner259 ), _WaterWaveMainStrength );
				unpack251.z = lerp( 1, unpack251.z, saturate(_WaterWaveMainStrength) );
				float2 panner269 = ( 1.0 * _Time.y * ( -_MainWaterWaveDir * _WaterWaveDetailSpeed ) + ( rotator278 * _WaterWaveDetailTiling ));
				float3 unpack275 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner269 ), _WaterWaveDetailStrength );
				unpack275.z = lerp( 1, unpack275.z, saturate(_WaterWaveDetailStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch972 = BlendNormal( unpack251 , unpack275 );
				#else
				float3 staticSwitch972 = float3(0,0,1);
				#endif
				float3 WaterWaveNormal306 = staticSwitch972;
				float3 temp_output_430_0 = ( positionWS36 * _FlowTiling );
				half3 Position160_g7376 = temp_output_430_0;
				float3 break170_g7376 = Position160_g7376;
				float2 appendResult171_g7376 = (float2(break170_g7376.z , break170_g7376.y));
				half3 Normal168_g7376 = vNormalWS39;
				float2 temp_output_180_0_g7376 = abs( (Normal168_g7376).xz );
				float2 temp_output_205_0_g7376 = ( temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 );
				float2 break183_g7376 = temp_output_205_0_g7376;
				float2 break185_g7376 = ( temp_output_205_0_g7376 / max( ( break183_g7376.x + break183_g7376.y ) , 1E-05 ) );
				float3 break186_g7376 = Position160_g7376;
				float2 appendResult191_g7376 = (float2(break186_g7376.x , break186_g7376.y));
				float4 break438 = ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7376 ) * break185_g7376.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7376 ) * break185_g7376.y ) );
				float2 appendResult437 = (float2(break438.b , break438.a));
				float2 normalMapRG1_g7378 = appendResult437;
				float4 localDecodeNormalRG1_g7378 = DecodeNormalRG( normalMapRG1_g7378 );
				float3 unpack4_g7378 = UnpackNormalScale( localDecodeNormalRG1_g7378, 0.25 );
				unpack4_g7378.z = lerp( 1, unpack4_g7378.z, saturate(0.25) );
				float3 normalizeResult449 = normalize( unpack4_g7378 );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch970 = normalizeResult449;
				#else
				float3 staticSwitch970 = float3(0,0,1);
				#endif
				float3 FlowNormal450 = staticSwitch970;
				half3 Position160_g7375 = ( temp_output_430_0 + ( _TimeParameters.x * float3(0,5,0) * 0.1 ) );
				float3 break170_g7375 = Position160_g7375;
				float2 appendResult171_g7375 = (float2(break170_g7375.z , break170_g7375.y));
				half3 Normal168_g7375 = vNormalWS39;
				float2 temp_output_180_0_g7375 = abs( (Normal168_g7375).xz );
				float2 temp_output_205_0_g7375 = ( temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 );
				float2 break183_g7375 = temp_output_205_0_g7375;
				float2 break185_g7375 = ( temp_output_205_0_g7375 / max( ( break183_g7375.x + break183_g7375.y ) , 1E-05 ) );
				float3 break186_g7375 = Position160_g7375;
				float2 appendResult191_g7375 = (float2(break186_g7375.x , break186_g7375.y));
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch968 = ( saturate( ( ( break438.r - saturate( ( ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g ) ) ) * 5.0 ) ) * GlobalMoist12401 );
				#else
				float staticSwitch968 = 0.0;
				#endif
				float FlowMask454 = ( staticSwitch968 * _FlowStrength );
				float2 UVs1_g7379 = ( appendResult18 + ( (( RipplesNormal294 + WaterWaveNormal306 )).xy * AccumulatedWaterMask286 * 0.02 ) + ( 0.02 * (FlowNormal450).xy * FlowMask454 ) );
				float vertexToFrag264_g7193 = IN.ase_texcoord2.x;
				float vertexToFrag337_g7193 = IN.ase_texcoord2.y;
				float vertexToFrag338_g7193 = IN.ase_texcoord2.z;
				float3 appendResult340_g7193 = (float3(vertexToFrag264_g7193 , vertexToFrag337_g7193 , vertexToFrag338_g7193));
				float3 normalizeResult451_g7193 = normalize( appendResult340_g7193 );
				float3 viewDirTS41 = normalizeResult451_g7193;
				float3 break6_g7379 = viewDirTS41;
				float2 appendResult5_g7379 = (float2(break6_g7379.x , break6_g7379.y));
				float2 plane1_g7379 = ( appendResult5_g7379 / break6_g7379.z );
				float refp1_g7379 = 1.0;
				float scale1_g7379 = ( _AccumulatedWaterParallaxStrength * 0.01 );
				float2 localIterativeParallaxLegacy1_g7379 = IterativeParallaxLegacy1_g7379( height1_g7379 , UVs1_g7379 , plane1_g7379 , refp1_g7379 , scale1_g7379 );
				#ifdef _ACCUMULATEDWATER_ON
				float2 staticSwitch974 = localIterativeParallaxLegacy1_g7379;
				#else
				float2 staticSwitch974 = appendResult18;
				#endif
				float2 DistortionUV298 = staticSwitch974;
				float4 tex2DNode110 = SAMPLE_TEXTURE2D( _BaseColorMap, LinearRepeat2_g7220, DistortionUV298 );
				float Alpha149 = tex2DNode110.a;
				#ifdef _ALPHATEST_ON
				float staticSwitch156 = ( Alpha149 - _AlphaClipOffset );
				#else
				float staticSwitch156 = 1.0;
				#endif
				
				float4 Albedo57 = tex2DNode110;
				
				float4 tex2DNode112 = SAMPLE_TEXTURE2D( Smoothness, LinearRepeat2_g7220, DistortionUV298 );
				float3 temp_cast_1 = (0.1).xxx;
				float3 Emission572 = ( saturate( ( (tex2DNode112).rgb - temp_cast_1 ) ) * 1.111111 * _EmissionStrength0 );
				

				float Alpha = staticSwitch156;
				float3 BakedAlbedo = Albedo57.rgb;
				float3 BakedEmission = Emission572;

				#ifdef _ALPHATEST_ON
					clip(Alpha - 0.5);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BakedAlbedo;
				metaInput.Emission = BakedEmission;

				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 140011
			#define ASE_USING_SAMPLING_MACROS 1


			CBUFFER_START(UnityPerMaterial)
			float4 _SpecColor;
			float4 _BaseColorMap_ST;
			float4 _BaseColor;
			float _OnAccumulatedWaterLightingAtten;
			float _OnAccumulatedWaterEnvEeflectAtten;
			float _MoistDiffuseCoeff;
			float _MoistRoughnessCoeff;
			float _Roughness1;
			float _Float6;
			float _AccumulatedwaterReflectStrength;
			float _Metallic01;
			float _NormalStrength;
			float _OcclusionBaked;
			float _MoistAccumulatedwaterCoeff;
			float _Cull;
			float _EmissionStrength0;
			float _AlphaClipOffset;
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
			#define ASE_NEEDS_VERT_NORMAL
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma multi_compile_fragment __ _ACCUMULATEDWATER_ON
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;

				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D(_BaseColorMap);
			TEXTURE2D(_AccumulatedWaterMask);
			float _AccumulatedWaterMaskTiling;
			float _GlobalMoist;
			float _AccumulatedWaterContrast;
			float _AccumulatedWaterSteepHillExtinction;
			TEXTURE2D(_RipplesNormalAtlas);
			float _RipplesMainTiling;
			float4 _XColumnsYRowsZSpeedWStrartFrame;
			float _RipplesMainStrength;
			TEXTURE2D(_WaterWaveNormal);
			float _WaterWaveMainSpeed;
			float _WaterWaveRotate;
			float _WaterWaveMainTiling;
			float _WaterWaveMainStrength;
			float _WaterWaveDetailSpeed;
			float _WaterWaveDetailTiling;
			float _WaterWaveDetailStrength;
			TEXTURE2D(_FlowMap);
			float3 _FlowTiling;
			float _FlowStrength;
			float _AccumulatedWaterParallaxStrength;
			TEXTURE2D(_NormalMap);


			float3 TransformObjectToWorldNormal_Ref33_g7196( float3 normalOS )
			{
				return TransformObjectToWorldNormal(normalOS,false);
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			
			float3 TransformWorldToTangentDir_Ref133_g7194( float3 directionWS, float3x3 TBN )
			{
				return TransformWorldToTangentDir(directionWS, TBN);
			}
			
			float2 IterativeParallaxLegacy1_g7379( float height, float2 UVs, float2 plane, float refp, float scale )
			{
				UVs += plane * scale * refp;
				UVs += (height - 1) * plane * scale;
				return UVs;
			}
			

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
				
				float localPosition1_g7205 = ( 0.0 );
				float localGetPositionTransformSpaceFromObject1_g7204 = ( 0.0 );
				float3 temp_output_14_0_g7197 = ( v.positionOS.xyz + float3( 0,0,0 ) );
				float3 positionOS1_g7204 = temp_output_14_0_g7197;
				Position position1_g7204 =(Position)0;
				GetPositionTransformSpaceFromObject_float( positionOS1_g7204 , position1_g7204 );
				Position position1_g7205 =(Position)position1_g7204;
				float3 OS1_g7205 = float3( 0,0,0 );
				float3 WS1_g7205 = float3( 0,0,0 );
				float3 VS1_g7205 = float3( 0,0,0 );
				float4 CS1_g7205 = float4( 0,0,0,0 );
				float4 NDC1_g7205 = float4( 0,0,0,0 );
				float3 SS1_g7205 = float3( 0,0,0 );
				float4 DS1_g7205 = float4( 0,0,0,0 );
				float3 LS1_g7205 = float3( 0,0,0 );
				float4 SC1_g7205 = float4( 0,0,0,0 );
				Position_float( position1_g7205 , OS1_g7205 , WS1_g7205 , VS1_g7205 , CS1_g7205 , NDC1_g7205 , SS1_g7205 , DS1_g7205 , LS1_g7205 , SC1_g7205 );
				float4 vertexPositionCS382_g7193 = CS1_g7205;
				float4 temp_output_21_313 = vertexPositionCS382_g7193;
				
				float2 break15 = ( ( v.ase_texcoord.xy * _BaseColorMap_ST.xy ) + _BaseColorMap_ST.zw );
				float vertexToFrag17 = break15.x;
				o.ase_texcoord1.x = vertexToFrag17;
				float vertexToFrag16 = break15.y;
				o.ase_texcoord1.y = vertexToFrag16;
				float3 temp_output_345_7_g7193 = WS1_g7205;
				float3 vertexPositionWS386_g7193 = temp_output_345_7_g7193;
				float3 break310_g7193 = vertexPositionWS386_g7193;
				float vertexToFrag320_g7193 = break310_g7193.x;
				o.ase_texcoord1.z = vertexToFrag320_g7193;
				float vertexToFrag321_g7193 = break310_g7193.y;
				o.ase_texcoord1.w = vertexToFrag321_g7193;
				float vertexToFrag322_g7193 = break310_g7193.z;
				o.ase_texcoord2.x = vertexToFrag322_g7193;
				float3 temp_output_31_0_g7196 = ( v.normalOS + float3( 0,0,0 ) );
				float3 normalOS33_g7196 = temp_output_31_0_g7196;
				float3 localTransformObjectToWorldNormal_Ref33_g7196 = TransformObjectToWorldNormal_Ref33_g7196( normalOS33_g7196 );
				float3 normalizeResult140_g7196 = ASESafeNormalize( localTransformObjectToWorldNormal_Ref33_g7196 );
				float3 temp_output_515_34_g7193 = normalizeResult140_g7196;
				float3 VertexNormalWS314_g7193 = temp_output_515_34_g7193;
				float3 break138_g7193 = VertexNormalWS314_g7193;
				float vertexToFrag323_g7193 = break138_g7193.x;
				o.ase_texcoord2.y = vertexToFrag323_g7193;
				float vertexToFrag324_g7193 = break138_g7193.y;
				o.ase_texcoord2.z = vertexToFrag324_g7193;
				float vertexToFrag325_g7193 = break138_g7193.z;
				o.ase_texcoord2.w = vertexToFrag325_g7193;
				float3 normalizeResult129_g7193 = ASESafeNormalize( ( _WorldSpaceCameraPos - vertexPositionWS386_g7193 ) );
				float3 temp_output_43_0_g7194 = normalizeResult129_g7193;
				float3 directionWS133_g7194 = temp_output_43_0_g7194;
				float3 temp_output_43_0_g7195 = ( v.ase_tangent.xyz + float3( 0,0,0 ) );
				float3 objToWorldDir42_g7195 = mul( GetObjectToWorldMatrix(), float4( temp_output_43_0_g7195, 0 ) ).xyz;
				float3 normalizeResult128_g7195 = ASESafeNormalize( objToWorldDir42_g7195 );
				float3 VertexTangentlWS474_g7193 = normalizeResult128_g7195;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 normalizeResult473_g7193 = ASESafeNormalize( ( cross( VertexNormalWS314_g7193 , VertexTangentlWS474_g7193 ) * ase_vertexTangentSign ) );
				float3 VertexBitangentWS476_g7193 = normalizeResult473_g7193;
				float3x3 temp_output_103_0_g7194 = float3x3(VertexTangentlWS474_g7193, VertexBitangentWS476_g7193, VertexNormalWS314_g7193);
				float3x3 TBN133_g7194 = temp_output_103_0_g7194;
				float3 localTransformWorldToTangentDir_Ref133_g7194 = TransformWorldToTangentDir_Ref133_g7194( directionWS133_g7194 , TBN133_g7194 );
				float3 normalizeResult132_g7194 = ASESafeNormalize( localTransformWorldToTangentDir_Ref133_g7194 );
				float3 break336_g7193 = normalizeResult132_g7194;
				float vertexToFrag264_g7193 = break336_g7193.x;
				o.ase_texcoord3.x = vertexToFrag264_g7193;
				float vertexToFrag337_g7193 = break336_g7193.y;
				o.ase_texcoord3.y = vertexToFrag337_g7193;
				float vertexToFrag338_g7193 = break336_g7193.z;
				o.ase_texcoord3.z = vertexToFrag338_g7193;
				
				float3 break141_g7193 = VertexTangentlWS474_g7193;
				float vertexToFrag326_g7193 = break141_g7193.x;
				o.ase_texcoord3.w = vertexToFrag326_g7193;
				float vertexToFrag327_g7193 = break141_g7193.y;
				o.ase_texcoord4.x = vertexToFrag327_g7193;
				float vertexToFrag328_g7193 = break141_g7193.z;
				o.ase_texcoord4.y = vertexToFrag328_g7193;
				float3 break148_g7193 = VertexBitangentWS476_g7193;
				float vertexToFrag329_g7193 = break148_g7193.x;
				o.ase_texcoord4.z = vertexToFrag329_g7193;
				float vertexToFrag330_g7193 = break148_g7193.y;
				o.ase_texcoord4.w = vertexToFrag330_g7193;
				float vertexToFrag331_g7193 = break148_g7193.z;
				o.ase_texcoord5.x = vertexToFrag331_g7193;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord5.yzw = 0;

				o.positionCS = temp_output_21_313;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;

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
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
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

				float vertexToFrag17 = IN.ase_texcoord1.x;
				float vertexToFrag16 = IN.ase_texcoord1.y;
				float2 appendResult18 = (float2(vertexToFrag17 , vertexToFrag16));
				float vertexToFrag320_g7193 = IN.ase_texcoord1.z;
				float vertexToFrag321_g7193 = IN.ase_texcoord1.w;
				float vertexToFrag322_g7193 = IN.ase_texcoord2.x;
				float3 appendResult311_g7193 = (float3(vertexToFrag320_g7193 , vertexToFrag321_g7193 , vertexToFrag322_g7193));
				float3 positionWS402_g7193 = appendResult311_g7193;
				float3 positionWS36 = positionWS402_g7193;
				float3 break479 = positionWS36;
				float2 appendResult480 = (float2(break479.x , break479.z));
				float2 PositionWSxz481 = appendResult480;
				float localGlobalSampler2_g7220 = ( 0.0 );
				SamplerState PointClamp2_g7220 = sampler_PointClamp;
				SamplerState LinearClamp2_g7220 = sampler_LinearClamp;
				SamplerState PointRepeat2_g7220 = sampler_PointRepeat;
				SamplerState LinearRepeat2_g7220 = sampler_LinearRepeat;
				{
				PointClamp2_g7220 = sampler_PointClamp;
				LinearClamp2_g7220 = sampler_LinearClamp;
				PointRepeat2_g7220 = sampler_PointRepeat;
				LinearRepeat2_g7220 = sampler_LinearRepeat;
				}
				float GlobalMoist12401 = saturate( ( _GlobalMoist - 1.0 ) );
				float lerpResult519 = lerp( 0.5 , ( SAMPLE_TEXTURE2D( _AccumulatedWaterMask, LinearRepeat2_g7220, ( PositionWSxz481 * _AccumulatedWaterMaskTiling ) ).r + (GlobalMoist12401*2.0 + -1.0) ) , _AccumulatedWaterContrast);
				float vertexToFrag323_g7193 = IN.ase_texcoord2.y;
				float vertexToFrag324_g7193 = IN.ase_texcoord2.z;
				float vertexToFrag325_g7193 = IN.ase_texcoord2.w;
				float3 appendResult142_g7193 = (float3(vertexToFrag323_g7193 , vertexToFrag324_g7193 , vertexToFrag325_g7193));
				float3 normalizeResult459_g7193 = normalize( appendResult142_g7193 );
				float3 NormalWS388_g7193 = normalizeResult459_g7193;
				float3 vNormalWS39 = NormalWS388_g7193;
				float dotResult408 = dot( vNormalWS39 , float3(0,1,0) );
				float smoothstepResult413 = smoothstep( _AccumulatedWaterSteepHillExtinction , 1.0 , dotResult408);
				float FlatArea522 = smoothstepResult413;
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch962 = ( saturate( (lerpResult519).x ) * FlatArea522 * _MoistAccumulatedwaterCoeff );
				#else
				float staticSwitch962 = 0.0;
				#endif
				float AccumulatedWaterMask286 = staticSwitch962;
				float height1_g7379 = AccumulatedWaterMask286;
				float2 break135_g7377 = ( PositionWSxz481 * _RipplesMainTiling );
				float2 appendResult206_g7377 = (float2(frac( break135_g7377.x ) , frac( break135_g7377.y )));
				float temp_output_4_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.x;
				float temp_output_5_0_g7377 = _XColumnsYRowsZSpeedWStrartFrame.y;
				float2 appendResult116_g7377 = (float2(temp_output_4_0_g7377 , temp_output_5_0_g7377));
				float temp_output_122_0_g7377 = ( temp_output_4_0_g7377 * temp_output_5_0_g7377 );
				float2 appendResult175_g7377 = (float2(temp_output_122_0_g7377 , temp_output_5_0_g7377));
				float Columns213_g7377 = temp_output_4_0_g7377;
				float Rows212_g7377 = temp_output_5_0_g7377;
				float temp_output_133_0_g7377 = ( fmod( _TimeParameters.x , ( Columns213_g7377 * Rows212_g7377 ) ) * _XColumnsYRowsZSpeedWStrartFrame.z );
				float clampResult129_g7377 = clamp( _XColumnsYRowsZSpeedWStrartFrame.w , 1E-05 , ( temp_output_122_0_g7377 - 1.0 ) );
				float temp_output_185_0_g7377 = frac( ( ( temp_output_133_0_g7377 + ( clampResult129_g7377 + 1E-05 ) ) / temp_output_122_0_g7377 ) );
				float2 appendResult186_g7377 = (float2(temp_output_185_0_g7377 , ( 1.0 - temp_output_185_0_g7377 )));
				float2 temp_output_203_0_g7377 = ( ( appendResult206_g7377 / appendResult116_g7377 ) + ( floor( ( appendResult175_g7377 * appendResult186_g7377 ) ) / appendResult116_g7377 ) );
				float3 unpack233 = UnpackNormalScale( SAMPLE_TEXTURE2D( _RipplesNormalAtlas, LinearRepeat2_g7220, temp_output_203_0_g7377 ), _RipplesMainStrength );
				unpack233.z = lerp( 1, unpack233.z, saturate(_RipplesMainStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch964 = unpack233;
				#else
				float3 staticSwitch964 = float3(0,0,1);
				#endif
				float3 RipplesNormal294 = staticSwitch964;
				float2 _MainWaterWaveDir = float2(1,0);
				float cos278 = cos( radians( _WaterWaveRotate ) );
				float sin278 = sin( radians( _WaterWaveRotate ) );
				float2 rotator278 = mul( PositionWSxz481 - float2( 0.5,0.5 ) , float2x2( cos278 , -sin278 , sin278 , cos278 )) + float2( 0.5,0.5 );
				float2 panner259 = ( 1.0 * _Time.y * ( _MainWaterWaveDir * _WaterWaveMainSpeed ) + ( rotator278 * _WaterWaveMainTiling ));
				float3 unpack251 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner259 ), _WaterWaveMainStrength );
				unpack251.z = lerp( 1, unpack251.z, saturate(_WaterWaveMainStrength) );
				float2 panner269 = ( 1.0 * _Time.y * ( -_MainWaterWaveDir * _WaterWaveDetailSpeed ) + ( rotator278 * _WaterWaveDetailTiling ));
				float3 unpack275 = UnpackNormalScale( SAMPLE_TEXTURE2D( _WaterWaveNormal, LinearRepeat2_g7220, panner269 ), _WaterWaveDetailStrength );
				unpack275.z = lerp( 1, unpack275.z, saturate(_WaterWaveDetailStrength) );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch972 = BlendNormal( unpack251 , unpack275 );
				#else
				float3 staticSwitch972 = float3(0,0,1);
				#endif
				float3 WaterWaveNormal306 = staticSwitch972;
				float3 temp_output_430_0 = ( positionWS36 * _FlowTiling );
				half3 Position160_g7376 = temp_output_430_0;
				float3 break170_g7376 = Position160_g7376;
				float2 appendResult171_g7376 = (float2(break170_g7376.z , break170_g7376.y));
				half3 Normal168_g7376 = vNormalWS39;
				float2 temp_output_180_0_g7376 = abs( (Normal168_g7376).xz );
				float2 temp_output_205_0_g7376 = ( temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 * temp_output_180_0_g7376 );
				float2 break183_g7376 = temp_output_205_0_g7376;
				float2 break185_g7376 = ( temp_output_205_0_g7376 / max( ( break183_g7376.x + break183_g7376.y ) , 1E-05 ) );
				float3 break186_g7376 = Position160_g7376;
				float2 appendResult191_g7376 = (float2(break186_g7376.x , break186_g7376.y));
				float4 break438 = ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7376 ) * break185_g7376.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7376 ) * break185_g7376.y ) );
				float2 appendResult437 = (float2(break438.b , break438.a));
				float2 normalMapRG1_g7378 = appendResult437;
				float4 localDecodeNormalRG1_g7378 = DecodeNormalRG( normalMapRG1_g7378 );
				float3 unpack4_g7378 = UnpackNormalScale( localDecodeNormalRG1_g7378, 0.25 );
				unpack4_g7378.z = lerp( 1, unpack4_g7378.z, saturate(0.25) );
				float3 normalizeResult449 = normalize( unpack4_g7378 );
				#ifdef _ACCUMULATEDWATER_ON
				float3 staticSwitch970 = normalizeResult449;
				#else
				float3 staticSwitch970 = float3(0,0,1);
				#endif
				float3 FlowNormal450 = staticSwitch970;
				half3 Position160_g7375 = ( temp_output_430_0 + ( _TimeParameters.x * float3(0,5,0) * 0.1 ) );
				float3 break170_g7375 = Position160_g7375;
				float2 appendResult171_g7375 = (float2(break170_g7375.z , break170_g7375.y));
				half3 Normal168_g7375 = vNormalWS39;
				float2 temp_output_180_0_g7375 = abs( (Normal168_g7375).xz );
				float2 temp_output_205_0_g7375 = ( temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 * temp_output_180_0_g7375 );
				float2 break183_g7375 = temp_output_205_0_g7375;
				float2 break185_g7375 = ( temp_output_205_0_g7375 / max( ( break183_g7375.x + break183_g7375.y ) , 1E-05 ) );
				float3 break186_g7375 = Position160_g7375;
				float2 appendResult191_g7375 = (float2(break186_g7375.x , break186_g7375.y));
				#ifdef _ACCUMULATEDWATER_ON
				float staticSwitch968 = ( saturate( ( ( break438.r - saturate( ( ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g * ( ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult171_g7375 ) * break185_g7375.x ) + ( SAMPLE_TEXTURE2D( _FlowMap, LinearRepeat2_g7220, appendResult191_g7375 ) * break185_g7375.y ) ).g ) ) ) * 5.0 ) ) * GlobalMoist12401 );
				#else
				float staticSwitch968 = 0.0;
				#endif
				float FlowMask454 = ( staticSwitch968 * _FlowStrength );
				float2 UVs1_g7379 = ( appendResult18 + ( (( RipplesNormal294 + WaterWaveNormal306 )).xy * AccumulatedWaterMask286 * 0.02 ) + ( 0.02 * (FlowNormal450).xy * FlowMask454 ) );
				float vertexToFrag264_g7193 = IN.ase_texcoord3.x;
				float vertexToFrag337_g7193 = IN.ase_texcoord3.y;
				float vertexToFrag338_g7193 = IN.ase_texcoord3.z;
				float3 appendResult340_g7193 = (float3(vertexToFrag264_g7193 , vertexToFrag337_g7193 , vertexToFrag338_g7193));
				float3 normalizeResult451_g7193 = normalize( appendResult340_g7193 );
				float3 viewDirTS41 = normalizeResult451_g7193;
				float3 break6_g7379 = viewDirTS41;
				float2 appendResult5_g7379 = (float2(break6_g7379.x , break6_g7379.y));
				float2 plane1_g7379 = ( appendResult5_g7379 / break6_g7379.z );
				float refp1_g7379 = 1.0;
				float scale1_g7379 = ( _AccumulatedWaterParallaxStrength * 0.01 );
				float2 localIterativeParallaxLegacy1_g7379 = IterativeParallaxLegacy1_g7379( height1_g7379 , UVs1_g7379 , plane1_g7379 , refp1_g7379 , scale1_g7379 );
				#ifdef _ACCUMULATEDWATER_ON
				float2 staticSwitch974 = localIterativeParallaxLegacy1_g7379;
				#else
				float2 staticSwitch974 = appendResult18;
				#endif
				float2 DistortionUV298 = staticSwitch974;
				float4 tex2DNode110 = SAMPLE_TEXTURE2D( _BaseColorMap, LinearRepeat2_g7220, DistortionUV298 );
				float Alpha149 = tex2DNode110.a;
				#ifdef _ALPHATEST_ON
				float staticSwitch156 = ( Alpha149 - _AlphaClipOffset );
				#else
				float staticSwitch156 = 1.0;
				#endif
				
				float4 tex2DNode116 = SAMPLE_TEXTURE2D( _NormalMap, LinearRepeat2_g7220, DistortionUV298 );
				float2 normalMapRG1_g7380 = (tex2DNode116).rg;
				float4 localDecodeNormalRG1_g7380 = DecodeNormalRG( normalMapRG1_g7380 );
				float3 unpack4_g7380 = UnpackNormalScale( localDecodeNormalRG1_g7380, _NormalStrength );
				unpack4_g7380.z = lerp( 1, unpack4_g7380.z, saturate(_NormalStrength) );
				float3 PrimitiveNormalTS878 = unpack4_g7380;
				float3 lerpResult284 = lerp( PrimitiveNormalTS878 , BlendNormal( RipplesNormal294 , WaterWaveNormal306 ) , AccumulatedWaterMask286);
				float3 lerpResult470 = lerp( lerpResult284 , FlowNormal450 , FlowMask454);
				float3 NormalTs25 = lerpResult470;
				float vertexToFrag326_g7193 = IN.ase_texcoord3.w;
				float vertexToFrag327_g7193 = IN.ase_texcoord4.x;
				float vertexToFrag328_g7193 = IN.ase_texcoord4.y;
				float3 appendResult134_g7193 = (float3(vertexToFrag326_g7193 , vertexToFrag327_g7193 , vertexToFrag328_g7193));
				float3 normalizeResult448_g7193 = normalize( appendResult134_g7193 );
				float3 TangentWS315_g7193 = normalizeResult448_g7193;
				float vertexToFrag329_g7193 = IN.ase_texcoord4.z;
				float vertexToFrag330_g7193 = IN.ase_texcoord4.w;
				float vertexToFrag331_g7193 = IN.ase_texcoord5.x;
				float3 appendResult144_g7193 = (float3(vertexToFrag329_g7193 , vertexToFrag330_g7193 , vertexToFrag331_g7193));
				float3 normalizeResult449_g7193 = normalize( appendResult144_g7193 );
				float3 BitangentWS316_g7193 = normalizeResult449_g7193;
				float3x3 TBN24 = float3x3(TangentWS315_g7193, BitangentWS316_g7193, NormalWS388_g7193);
				float3 normalizeResult29 = normalize( mul( NormalTs25, TBN24 ) );
				float3 normalWS58 = normalizeResult29;
				

				surfaceDescription.Alpha = staticSwitch156;
                float3 WorldNormal = normalWS58;

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
Node;AmplifyShaderEditor.CommentaryNode;892;-1788.718,382.1726;Inherit;False;2943.254;2069.947;Comment;30;889;902;105;888;44;844;104;103;49;48;32;33;34;35;37;38;40;42;47;46;45;887;901;930;24;41;39;36;21;929;获取必要数据;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;323;-5921.911,-212.6481;Inherit;False;2641.651;970.0292;Comment;31;298;421;12;326;419;424;422;485;292;418;296;875;18;297;299;295;876;877;17;16;874;315;15;14;316;293;13;11;490;974;975;UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;21;-1509.341,432.1724;Inherit;False;Model;-1;;7193;db2a50576abc55b4182a85385bad6f66;8,430,0,511,0,512,0,514,0,513,0,505,0,431,0,552,0;7;307;FLOAT3;0,0,0;False;432;FLOAT3;0,0,0;False;377;FLOAT3;0,0,0;False;433;FLOAT3;0,0,0;False;554;FLOAT3;0,0,0;False;555;FLOAT3;0,0,0;False;556;FLOAT3;0,0,0;False;39;FLOAT2;543;FLOAT3;546;FLOAT;547;FLOAT4;347;FLOAT4;313;FLOAT4;413;FLOAT4;415;FLOAT2;273;FLOAT2;275;FLOAT3;277;FLOAT;410;FLOAT3;285;FLOAT4;243;FLOAT4;178;FLOAT3;0;FLOAT3;4;FLOAT3;5;FLOAT3x3;6;FLOAT3;312;FLOAT3;156;FLOAT3;271;FLOAT3;229;FLOAT3;7;FLOAT3;185;FLOAT3;194;FLOAT3;197;FLOAT3;11;FLOAT;21;FLOAT;540;FLOAT;541;FLOAT;542;FLOAT;22;INT;202;FLOAT4;224;FLOAT4;225;FLOAT3;220;INT;216;INT;217;INT;219
Node;AmplifyShaderEditor.CommentaryNode;490;-5774.135,386.6058;Inherit;False;761.2529;211;Comment;4;478;479;480;481;位置XZ;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;36;-924.4949,975.557;Inherit;False;positionWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;883;-6096.675,836.2208;Inherit;False;1636.943;378.7227;Comment;11;979;155;57;149;110;789;521;325;8;771;801;采样基础贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;483;-5915.265,6641.534;Inherit;False;2967.436;927.8618;Comment;35;454;996;450;968;970;560;969;449;556;453;971;440;451;437;439;452;465;438;464;784;463;783;460;436;435;785;430;457;432;429;459;458;456;999;1000;斜面水流;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;529;-5936.737,3810.849;Inherit;False;946.7977;280.6467;Comment;6;343;399;400;401;344;393;总控;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;478;-5724.135,436.6058;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;801;-5861.675,1020.315;Inherit;False;GlobalSampler;-1;;7220;ec6bc411ea2ff22459f0cf948a6f315a;0;0;4;SAMPLERSTATE;0;SAMPLERSTATE;3;SAMPLERSTATE;4;SAMPLERSTATE;5
Node;AmplifyShaderEditor.SimpleTimeNode;456;-5838.795,7216.217;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;458;-5856.795,7290.217;Inherit;False;Constant;_Vector5;Vector 5;44;0;Create;True;0;0;0;False;0;False;0,5,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;459;-5704.796,7354.218;Inherit;False;Constant;_Float19;Float 19;44;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;429;-5763.408,6805.384;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;432;-5791.636,6882.837;Inherit;False;Global;_FlowTiling;_FlowTiling;39;0;Create;False;0;0;0;False;0;False;1.5,8,1.5;1.5,4,1.5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;343;-5886.737,3861.255;Inherit;False;Global;_GlobalMoist;_GlobalMoist;13;0;Create;False;0;0;0;False;0;False;0;1.5;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;479;-5541.135,436.6058;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;771;-5638.06,1092.944;Inherit;False;MainSampler0;-1;True;1;0;SAMPLERSTATE;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-925.4793,847.0569;Inherit;False;vNormalWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;457;-5507.792,7211.217;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;430;-5544.636,6804.838;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;281;-4098.942,5600.012;Inherit;False;2290.78;1003.608;Comment;26;972;306;277;275;251;264;780;272;269;259;265;270;266;262;253;278;263;271;276;268;254;260;252;520;280;973;水波;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;399;-5560.938,3954.496;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;480;-5418.136,452.607;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;785;-5461.915,7344.658;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.TexturePropertyNode;435;-5797.636,7030.836;Inherit;True;Global;_FlowMap;_FlowMap;23;0;Create;False;0;0;0;False;0;False;1b28c8de5a0e75c42a955ff734a916b3;1b28c8de5a0e75c42a955ff734a916b3;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;436;-5565.636,6915.836;Inherit;False;39;vNormalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;460;-5358.796,7186.217;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;227;-6318.191,4845.756;Inherit;False;2230.42;759.946;Comment;24;286;962;223;224;522;595;226;413;519;408;415;416;220;410;409;222;219;403;402;778;338;339;482;963;积水;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;280;-4053.525,5875.299;Inherit;False;Global;_WaterWaveRotate;_WaterWaveRotate;25;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;400;-5382.938,3955.496;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;481;-5252.88,451.4125;Inherit;False;PositionWSxz;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;783;-5141.394,7113.984;Inherit;False;TwoPlanar Mapping;-1;;7375;b4330d004d475fd47a05c39ec08fa115;0;4;116;FLOAT3;0,0,0;False;156;SAMPLER2D;0,0,0;False;167;FLOAT3;0,0,0;False;210;SAMPLERSTATE;0,0,0;False;1;COLOR;19
Node;AmplifyShaderEditor.RegisterLocalVarNode;401;-5231.938,3955.496;Inherit;False;GlobalMoist12;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;520;-3840.388,5875.023;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;252;-3867.738,5803.147;Inherit;False;481;PositionWSxz;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;260;-3872.742,6037.761;Inherit;False;Constant;_MainWaterWaveDir;主水波方向;35;0;Create;False;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;482;-6140.582,4923.604;Inherit;False;481;PositionWSxz;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;339;-6272.805,5001.736;Inherit;False;Global;_AccumulatedWaterMaskTiling;_AccumulatedWaterMaskTiling;16;0;Create;False;0;0;0;False;0;False;0.6;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;463;-4897.791,7116.217;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CommentaryNode;250;-5943.266,5695.935;Inherit;False;1757.039;724.5112;涟漪;12;964;294;233;779;236;230;228;231;234;235;232;965;涟漪;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;254;-3687.739,5946.147;Inherit;False;Global;_WaterWaveMainTiling;_WaterWaveMainTiling;19;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;268;-3636.864,6260.336;Inherit;False;Global;_WaterWaveDetailTiling;_WaterWaveDetailTiling;19;0;Create;False;0;0;0;False;0;False;2;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;276;-3645.546,6356.867;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;271;-3724.868,6435.949;Inherit;False;Global;_WaterWaveDetailSpeed;_WaterWaveDetailSpeed;20;0;Create;False;0;0;0;False;0;False;0.05;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;278;-3660.523,5809.3;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;338;-5936.927,4942.736;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;778;-5959.321,5045.589;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.GetLocalVarNode;402;-5965.707,5120.243;Inherit;False;401;GlobalMoist12;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;263;-3666.74,6096.76;Inherit;False;Global;_WaterWaveMainSpeed;_WaterWaveMainSpeed;20;0;Create;False;0;0;0;False;0;False;0.05;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;784;-5180.633,6878.837;Inherit;False;TwoPlanar Mapping;-1;;7376;b4330d004d475fd47a05c39ec08fa115;0;4;116;FLOAT3;0,0,0;False;156;SAMPLER2D;0,0,0;False;167;FLOAT3;0,0,0;False;210;SAMPLERSTATE;0,0,0;False;1;COLOR;19
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;464;-4757.41,7101.183;Inherit;False;8;8;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;253;-3363.738,5836.147;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;262;-3353.74,6037.761;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;232;-5794.795,5913.138;Inherit;False;481;PositionWSxz;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;235;-5834.745,6176.313;Inherit;False;Global;_RipplesMainTiling;_RipplesMainTiling;17;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;266;-3365.865,6211.335;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;270;-3433.866,6354.949;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;403;-5730.166,5120.526;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;219;-5768.261,4918.884;Inherit;True;Global;_AccumulatedWaterMask;_AccumulatedWaterMask;20;0;Create;False;0;0;0;False;0;False;-1;None;a2cf78be182d9e04885921107bcd90b8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;438;-4889.632,6786.838;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SaturateNode;465;-4605.363,7114.928;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-5604.991,886.2205;Inherit;True;Property;_BaseColorMap;基础贴图;3;0;Create;False;0;0;0;False;1;LogicalTex(_,true,RGB_A,_);False;None;02f7e5464e6c31e448724797e31103f5;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;265;-3231.567,5651.084;Inherit;True;Global;_WaterWaveNormal;_WaterWaveNormal;22;0;Create;False;0;0;0;False;0;False;None;5e34ad465893856488924104088e658b;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;259;-3165.112,5835.866;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;-5577.745,5949.313;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;231;-5916.404,5991.935;Inherit;False;Global;_XColumnsYRowsZSpeedWStrartFrame;_XColumnsYRowsZSpeedWStrartFrame;17;0;Create;False;0;0;0;False;0;False;8,8,12,0;8,8,12,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;269;-3159.439,6213.354;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;272;-3229.567,6362.149;Inherit;False;Global;_WaterWaveDetailStrength;_WaterWaveDetailStrength;19;0;Create;False;0;0;0;False;0;False;0.03;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;780;-3173.993,6124.182;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RangedFloatNode;264;-3299.739,5963.76;Inherit;False;Global;_WaterWaveMainStrength;_WaterWaveMainStrength;21;0;Create;False;0;0;0;False;0;False;0.05;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;222;-5786.084,5245.208;Inherit;False;Global;_AccumulatedWaterContrast;_AccumulatedWaterContrast;17;0;Create;False;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;409;-5778.549,5354.002;Inherit;False;Constant;_Vector4;Vector 4;39;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;410;-5626.901,5326.551;Inherit;False;39;vNormalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;220;-5455.589,5093.812;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;452;-4409.194,6902.179;Inherit;False;Constant;_Float16;Float 16;44;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;439;-4406.326,6787.217;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;996;-4422.77,7160.963;Inherit;False;Constant;_Float8;Float 8;27;0;Create;True;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;325;-5345.021,886.2537;Inherit;False;BaseMap;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;228;-5418.71,5934.331;Inherit;False;Flipbook;-1;;7377;53c2488c220f6564ca6c90721ee16673;3,68,0,217,0,244,0;11;51;SAMPLER2D;0.0;False;167;SAMPLERSTATE;0;False;13;FLOAT2;0,0;False;24;FLOAT;0;False;210;FLOAT;4;False;4;FLOAT;4;False;5;FLOAT;4;False;130;FLOAT;0;False;2;FLOAT;0;False;55;FLOAT;0;False;70;FLOAT;0;False;5;COLOR;53;FLOAT2;0;FLOAT;47;FLOAT;48;FLOAT;218
Node;AmplifyShaderEditor.TexturePropertyNode;230;-5404.403,5745.936;Inherit;True;Global;_RipplesNormalAtlas;_RipplesNormalAtlas;21;0;Create;False;0;0;0;False;0;False;a6e49a766b3568e4eb66b720e93376e5;a6e49a766b3568e4eb66b720e93376e5;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;251;-2877.488,5811.385;Inherit;True;Property;_WaterWaveNormal0;WaterWaveNormal;33;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;275;-2879.315,6188.573;Inherit;True;Property;_WaterWaveNormal1;WaterWaveNormal;33;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;779;-5327.964,6250.968;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RangedFloatNode;416;-5462.876,5481.982;Inherit;False;Constant;_Float18;Float 18;41;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;415;-5620.412,5412.567;Inherit;False;Global;_AccumulatedWaterSteepHillExtinction;_AccumulatedWaterSteepHillExtinction;16;0;Create;False;0;0;0;False;0;False;0.97;0.97;0.9;0.999;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;408;-5425.07,5326.987;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;519;-5289.887,5190.808;Inherit;False;3;0;FLOAT;0.5;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;437;-4762.834,6847.464;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;236;-5393.028,6160.434;Inherit;False;Global;_RipplesMainStrength;_RipplesMainStrength;18;0;Create;False;0;0;0;False;0;False;0.4;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;451;-4228.643,6798.902;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;233;-5027.645,5935.145;Inherit;True;Property;_TextureSample2;Texture Sample 2;29;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;413;-5258.876,5325.982;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;226;-5137.922,5190.558;Inherit;False;FLOAT;0;1;2;3;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;965;-4897.346,5787.923;Inherit;False;Constant;_Vector9;Vector 9;28;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BlendNormalsNode;277;-2554.673,5995.36;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;973;-2500.877,5849.997;Inherit;False;Constant;_Vector11;Vector 11;31;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;326;-5843.58,-1.964325;Inherit;False;325;BaseMap;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;440;-4114.908,7081.362;Inherit;False;DecodeNormalRG;-1;;7378;369339b99dd84204b97be658d76839ca;0;2;2;FLOAT2;0,0;False;3;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;453;-4087.379,6800.135;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;556;-4234.974,6912.949;Inherit;False;401;GlobalMoist12;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;11;-5632.006,-121.4644;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;595;-5146.425,5466.269;Inherit;False;Property;_MoistAccumulatedwaterCoeff;积水系数;17;0;Create;False;0;0;0;False;0;False;1;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;522;-5100.317,5326.596;Inherit;False;FlatArea;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;224;-4988.644,5189.915;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;971;-3755.949,7183.979;Inherit;False;Constant;_Vector10;Vector 10;30;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;972;-2286.923,5971.891;Inherit;False;Property;_AccumulatedWater5;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureTransformNode;12;-5627.973,-2.845184;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.StaticSwitch;964;-4689.523,5908.947;Inherit;False;Property;_AccumulatedWater1;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;449;-3771.508,7085.121;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;969;-3946.827,6919.464;Inherit;False;Constant;_Float10;Float 10;30;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;560;-3937.476,6800.495;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-5400.59,-49.4399;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;-4820.734,5189.08;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;963;-4813.524,5099.588;Inherit;False;Constant;_Float5;Float 5;28;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;294;-4395.776,5908.887;Inherit;False;RipplesNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;970;-3544.949,7057.979;Inherit;False;Property;_AccumulatedWater4;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;306;-2021.714,5971.877;Inherit;False;WaterWaveNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;968;-3751.494,6776.868;Inherit;False;Property;_AccumulatedWater3;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1000;-3686.704,6886.274;Inherit;False;Global;_FlowStrength;_FlowStrength;27;0;Create;True;0;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;293;-5059.883,44.92712;Inherit;False;294;RipplesNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;316;-5061.935,114.52;Inherit;False;306;WaterWaveNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-5238.667,-3.061012;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;962;-4625.059,5164.177;Inherit;False;Property;_AccumulatedWater;_AccumulatedWater;27;0;Create;False;0;0;0;False;0;False;1;1;1;False;_ACCUMULATEDWATER_ON;Toggle;2;Key0;Key1;Create;False;False;Fragment;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;450;-3261.8,7058.015;Inherit;False;FlowNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;999;-3436.103,6777.075;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;15;-5072.415,-65.88619;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;315;-4824.417,48.05896;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;874;-4910.196,337.3041;Inherit;False;450;FlowNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;286;-4338.202,5163.21;Inherit;False;AccumulatedWaterMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;454;-3268.5,6776.535;Inherit;False;FlowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;16;-4907.563,-42.67428;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;17;-4911.337,-127.5093;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;877;-4722.781,336.1391;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;876;-4736.414,426.9105;Inherit;False;454;FlowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;295;-4725.928,261.1556;Inherit;False;Constant;_Float0;Float 0;37;0;Create;True;0;0;0;False;0;False;0.02;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;299;-4831.177,175.6078;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;297;-4701.938,47.02771;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;875;-4508.314,308.3051;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;296;-4515.927,49.15564;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;-922.2005,1106.059;Inherit;False;viewDirTS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;422;-4797.158,607.7697;Inherit;False;Global;_AccumulatedWaterParallaxStrength;_AccumulatedWaterParallaxStrength;16;0;Create;False;0;0;0;False;0;False;0.25;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-4679.825,-130.1714;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;292;-4268.03,19.54443;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;485;-4473.051,608.4259;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;424;-4477.387,522.8938;Inherit;False;Constant;_Float12;Float 12;41;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;421;-4505.422,437.7718;Inherit;False;41;viewDirTS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;418;-4384.535,-66.7729;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;419;-4098.384,-6.782681;Inherit;False;ParallaxMapping;-1;;7379;828cd39fcb68245479af3f7728c1427b;0;5;2;FLOAT;0;False;3;FLOAT2;0,0;False;8;FLOAT3;0,0,0;False;9;FLOAT;0.5;False;10;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;975;-3967.581,-99.59619;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;974;-3828.989,-31.09863;Inherit;False;Property;_AccumulatedWater6;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;884;-5919.454,1310.876;Inherit;False;1982.366;625.5806;Comment;19;116;563;186;663;658;662;564;830;135;125;786;301;790;123;122;113;878;985;994;采样NRA贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;298;-3565.922,-30.8086;Inherit;False;DistortionUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;786;-5867.538,1378.952;Inherit;True;Property;_NormalMap;NRA贴图;6;0;Create;False;0;0;0;False;1;LogicalTex(_,false,RG_B_A,_);False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;301;-5827.062,1558.095;Inherit;False;298;DistortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;790;-5869.454,1652.891;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.SamplerNode;116;-5578.291,1530.91;Inherit;True;Property;_NormalMap0;NRA贴图;6;0;Create;False;0;0;0;False;1;LogicalTex(_,false,RG_B_A,_);False;-1;None;c760f9ea423805b4f996ab9ab4790ed1;True;0;True;bump;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;186;-5019.47,1364.468;Inherit;False;Property;_NormalStrength;(N)法线强度;7;0;Create;False;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;563;-5199.079,1433.137;Inherit;False;FLOAT2;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;309;-3858.392,877.5803;Inherit;False;1744.068;506.4812;Comment;14;58;29;28;27;25;879;470;469;471;284;282;865;308;307;对Normal操作;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;564;-4463.878,1432.237;Inherit;False;DecodeNormalRG;-1;;7380;369339b99dd84204b97be658d76839ca;0;2;2;FLOAT2;0,0;False;3;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;307;-3829.392,1175.414;Inherit;False;306;WaterWaveNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;308;-3827.601,1104.927;Inherit;False;294;RipplesNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;878;-4179.089,1432.995;Inherit;False;PrimitiveNormalTS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;282;-3580.005,1108.359;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;865;-3581.5,1221.469;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;879;-3544.086,954.3896;Inherit;False;878;PrimitiveNormalTS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;284;-3220.114,957.8735;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;471;-3225.417,1096.626;Inherit;False;450;FlowNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;-3228.561,1172.885;Inherit;False;454;FlowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;470;-3032.342,1070.305;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;521;-5312.983,958.9448;Inherit;False;298;DistortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;789;-5336.94,1051.984;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-927.1688,911.3819;Inherit;False;TBN;-1;True;1;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SamplerNode;110;-5076.357,971.1606;Inherit;True;Property;_t_en_Landscape_01_02_d;t_en_Landscape_01_02_d;4;0;Create;True;0;0;0;False;0;False;-1;02f7e5464e6c31e448724797e31103f5;02f7e5464e6c31e448724797e31103f5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-2883.254,1071.139;Inherit;False;NormalTs;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;27;-2865.603,1168.722;Inherit;False;24;TBN;1;0;OBJECT;;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-4701.735,1065.324;Inherit;False;Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-2670.294,1087.878;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;855;-1773.296,3792.508;Inherit;False;2280.68;909.3556;Comment;8;942;640;917;643;729;728;944;940;近似实时GI;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;935;123.4601,2589.475;Inherit;False;2452.04;1093.138;Comment;10;768;936;908;934;932;933;893;900;976;977;混合近似实时反射与屏幕反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;930;-741.7568,1801.893;Inherit;False;1808.417;570.9078;Comment;11;61;920;918;919;614;65;64;63;62;927;928;光照贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;886;-5891.407,2698.441;Inherit;False;2819.873;752.8247;Comment;16;530;527;87;121;56;89;90;91;94;54;55;84;86;85;528;93;获取BRDF;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-1251.892,-61.27222;Inherit;False;149;Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-1347.257,8.897339;Inherit;False;Property;_AlphaClipOffset;Alpha剪辑偏移;2;0;Create;False;0;0;0;True;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;29;-2533.293,1087.208;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;960;-774.944,5729.173;Inherit;False;958.566;573.7283;Comment;7;81;956;664;953;949;948;914;合并光照;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;959;-1749.638,6403.449;Inherit;False;974;655.4608;Comment;10;72;68;120;69;70;67;74;66;73;71;获取遮蔽;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;958;-1749.798,5794.037;Inherit;False;966.4011;594.0369;Comment;9;76;75;77;78;79;83;82;80;671;附加灯光光照;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;957;-312.7986,4918.504;Inherit;False;1522.355;609.4159;Comment;18;95;97;98;99;96;477;829;831;832;828;853;950;854;850;827;834;952;955;直接光照;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;951;-1764.308,4887.967;Inherit;False;1409.412;726.1893;Comment;22;196;687;692;923;669;943;937;100;101;945;837;836;839;838;835;826;852;851;946;823;869;947;间接光照;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;940;-1750.442,3904.932;Inherit;False;1782.978;521.2755;Comment;20;617;615;616;924;650;938;654;677;649;921;922;678;676;653;665;666;682;690;686;691;混合环境颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;929;-519.5518,585.0235;Inherit;False;1068.989;373.906;Comment;7;108;905;107;106;903;109;982;反射探针;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;928;66.18819,1872.893;Inherit;False;467.3638;164.214;Comment;2;916;925;放弃光照贴图的色彩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;856;-1780.852,2669.079;Inherit;False;1824.154;813.3704;Comment;21;894;730;700;721;720;907;904;708;707;709;718;699;705;702;906;703;939;706;704;723;710;近似实时反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;927;310.22,2061.396;Inherit;False;712.1514;271.6152;Comment;4;931;724;725;726;提取光照贴图中的AO;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;901;-602.6448,1122.359;Inherit;False;1497.673;565.5826;Comment;11;898;763;762;754;755;756;758;759;757;760;761;屏幕空间反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;900;400.2025,2792.11;Inherit;False;998.0317;345.5103;Comment;5;767;848;847;899;895;混合;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;893;173.4601,3162.11;Inherit;False;1236.707;417.8783;Comment;10;766;802;849;846;845;843;842;890;841;980;SSR衰减;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;885;-5904.242,1999.11;Inherit;False;1581.124;547.5286;Comment;15;142;112;774;138;881;576;571;573;574;575;572;577;578;986;995;采样EM贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;866;-3922.103,4929.344;Inherit;False;1562.095;576.0402;Comment;14;867;966;967;555;549;857;861;811;809;817;813;814;812;810;水遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;859;-3946.326,4343.138;Inherit;False;875.7791;375.2147;Comment;4;310;961;597;598;湿润;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;531;-3802.722,1471.066;Inherit;False;1153.903;452.6983;Comment;8;52;314;863;391;312;880;592;586;对Roughness操作;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;530;-4375.054,3095.793;Inherit;False;658.0758;335.8212;Comment;4;335;327;330;329;对Diffuse操作;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;216;-5959.451,4210.782;Inherit;False;1992.378;579.8244;Comment;23;213;397;396;523;860;212;858;209;210;208;211;394;398;204;207;777;162;168;202;203;206;205;199;雨点;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;541;-3785.673,1995.533;Inherit;False;756.6915;346.8479;Comment;5;868;819;50;818;882;对Metallic操作;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;153;-1051.257,-15.10263;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-1041.892,-97.27219;Inherit;False;Constant;_Float4;Float 4;11;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-2334.518,1087.898;Inherit;False;normalWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;344;-5540.819,3861.419;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;393;-5365.986,3860.849;Inherit;False;GlobalMoist01;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;663;-4670.164,1360.876;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StickyNoteNode;30;94.09924,-426.9507;Inherit;False;968.9526;777.198;keyword;关键字;1,1,1,1;//主灯光全局关键字$multi_compile _ LIGHTMAP_SHADOW_MIXING$$multi_compile _ SHADOWS_SHADOWMASK$$multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN$$multi_compile_fragment _ _LIGHT_COOKIES $$multi_compile _ _LIGHT_LAYERS$$multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH$$//------------------------------------------------------------------------------------------------------$贴花$multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3$$//------------------------------------------------------------------------------------------------------$屏幕空间AO$multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION$$//------------------------------------------------------------------------------------------------------$烘焙GI全局关键字$$REQUIRE_BAKEDGI 1$$multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX$$multi_compile _ DIRLIGHTMAP_COMBINED$$multi_compile _ LIGHTMAP_ON$$multi_compile _ DYNAMICLIGHTMAP_ON$$//------------------------------------------------------------------------------------------------------$烘焙反射全局关键字$multi_compile_fragment _ _REFLECTION_PROBE_BLENDING$$multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION$$//------------------------------------------------------------------------------------------------------$附加灯光全局关键字$multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS$$multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS$$//------------------------------------------------------------------------------------------------------$渲染管线$multi_compile _ _FORWARD_PLUS$$$$;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;887;-1688.68,1645.777;Inherit;False;42;positionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-1735.417,1400.971;Inherit;False;37;shadowCoord;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;761;169.799,1241.029;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;760;-70.21406,1327.6;Inherit;False;Constant;_Vector7;Vector 4;0;0;Create;True;0;0;0;False;0;False;1,1,-1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;757;-550.6447,1249.359;Inherit;False;24;TBN;1;0;OBJECT;;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;759;-552.6447,1183.359;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;758;-358.6463,1183.359;Inherit;False;NormalTransformSpace;-1;;7477;49cf2c8d471f5a8469f03384352a304f;12,65,1,91,1,40,1,78,1,116,1,30,1,135,0,138,0,141,0,144,0,147,0,134,0;2;31;FLOAT3;0,0,0;False;52;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;6;FLOAT3;38;FLOAT3;34;FLOAT3;39;FLOAT3;36;FLOAT3;37;FLOAT3;35
Node;AmplifyShaderEditor.FunctionNode;756;-360.2137,1381.599;Inherit;False;PositionTransformSpace;-1;;7478;be470b7f25071594faeb0c9db5956ba5;1,13,1;1;14;FLOAT3;0,0,0;False;9;FLOAT3;0;FLOAT3;7;FLOAT3;8;FLOAT4;9;FLOAT3;10;FLOAT4;36;FLOAT4;11;FLOAT3;12;FLOAT4;35
Node;AmplifyShaderEditor.GetLocalVarNode;755;-544.213,1381.599;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;754;132.3527,1172.359;Inherit;False;42;positionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;762;182.7848,1432.599;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1013.719,1620.102;Inherit;False;shadowMask;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-1681.348,2146.276;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;104;-1687.712,2230.187;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;844;-1443.483,1669.374;Inherit;False;Camera;-1;;7487;1709bbd57a16e3043af768d1c0ea503a;0;2;28;FLOAT2;0,0;False;39;FLOAT4;0,0,0,0;False;18;FLOAT3;0;FLOAT3;3;FLOAT2;6;FLOAT;15;FLOAT;16;FLOAT;17;FLOAT;19;FLOAT;24;FLOAT3;25;FLOAT3;26;FLOAT;38;FLOAT;42;FLOAT;43;FLOAT;44;FLOAT;45;FLOAT;32;FLOAT;33;INT;35
Node;AmplifyShaderEditor.FunctionNode;44;-1454.042,1423.479;Inherit;False;MainLight;-1;;7488;8feda1e983ee2e6418994fa25e7d0c8f;0;3;29;FLOAT4;0,0,0,0;False;31;FLOAT3;0,0,0;False;30;FLOAT2;0,0;False;7;FLOAT3;0;FLOAT3;10;FLOAT;11;FLOAT;12;INT;13;OBJECT;17;FLOAT4;32
Node;AmplifyShaderEditor.GetLocalVarNode;888;-1694.407,1712.232;Inherit;False;38;positionDS;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;105;-1377.469,2203.484;Inherit;False;ReflectDir;-1;;7489;a73de9be7b6b71b4d92e3337acef4e55;0;2;5;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;902;-1086.914,2203.119;Inherit;False;ReflectionDirWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;763;411.3954,1381.405;Inherit;False; ;4;File;3;True;uv;FLOAT2;0,0;In;;Inherit;False;True;normalVS;FLOAT3;0,0,0;In;;Inherit;False;True;rayStart;FLOAT3;0,0,0;In;;Inherit;False;SSR_Pass;False;False;0;bb7bf17031706d94a86a6549b93d084b;False;3;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-658.6685,-133.5954;Inherit;False;57;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-663.7841,-201.3286;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;891;-651.0576,-40.20046;Inherit;False;572;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LuminanceNode;925;114.031,1921.95;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-652.228,2038.506;Inherit;False;32;staticLightMapUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-690.2276,2107.506;Inherit;False;33;dynamicLightMapUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-626.2277,2172.506;Inherit;False;34;vertexSH;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-623.2277,2242.506;Inherit;False;48;mainLight;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.FunctionNode;614;-365.2276,2067.506;Inherit;False;EnvLighting;-1;;7491;1c7b249abd298d448b85814cac86e2ab;0;5;17;FLOAT3;0,0,0;False;35;FLOAT2;0,0;False;36;FLOAT2;0,0;False;37;FLOAT3;0,0,0;False;21;OBJECT;;False;4;FLOAT3;0;FLOAT4;43;FLOAT4;45;FLOAT4;47
Node;AmplifyShaderEditor.RegisterLocalVarNode;919;103.0787,2157.835;Inherit;False;AmbientEquator;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;918;103.856,2085.835;Inherit;False;AmbientSky;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;920;101.9678,2227.391;Inherit;False;AmbientGround;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-622.2277,1965.506;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;916;289.3945,1920.736;Inherit;False;BakedGI;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;726;319.2707,2135.262;Inherit;False;Global;_ApproxRealtimeGI_AOMin;_ApproxRealtimeGI_AOMin;23;0;Create;True;0;0;0;False;0;False;-0.1;-0.002;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;725;320.2707,2220.262;Inherit;False;Global;_ApproxRealtimeGI_AOMax;_ApproxRealtimeGI_AOMax;23;0;Create;True;0;0;0;False;0;False;0.3;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;724;614.4216,2115.129;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;728;-83.41161,4459.963;Inherit;False;3;0;FLOAT;0.5;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;729;-410.9707,4510.873;Inherit;False;Global;_ApproxRealtimeGI_LightingMapContrast;_ApproxRealtimeGI_LightingMapContrast;23;0;Create;True;0;0;0;False;0;False;0.8;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;643;-209.0235,4592.781;Inherit;False;Global;_RealtimeGIStrength;_RealtimeGIStrength;23;0;Create;True;0;0;0;False;0;False;2;3.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;917;-286.52,4437.808;Inherit;False;916;BakedGI;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;640;135.3662,4436.739;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;156;-889.8795,-97.12039;Inherit;False;Property;ALPHATEST_ON;Alpha裁剪;1;0;Create;False;0;0;0;True;0;False;0;0;0;True;_ALPHATEST_ON;Toggle;2;Key0;Key1;Create;True;False;Fragment;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;843;588.1597,3297.275;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;845;734.2526,3301.646;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;847;740.5441,2950.027;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;848;944.5159,3000.651;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;766;924.9794,3208.421;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;802;1109.505,3207.384;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;710;-778.5679,3016.781;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;723;-1130.524,3043.651;Inherit;False;Global;_ApproxRealtimeGI_SkyColor;_ApproxRealtimeGI_SkyColor;23;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.227451,0.2980392,0.4941177,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RGBToHSVNode;704;-1484.403,2872.376;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;706;-1423.536,3014.907;Inherit;False;Constant;_Float3;Float 3;23;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;939;-1730.702,2870.642;Inherit;False;938;AmbientSkyMixEquator;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;703;-1400.156,2796.375;Inherit;False;Constant;_Float2;Float 2;23;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;906;-1429.994,2730.991;Inherit;False;905;BakedReflect;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;702;-1238.975,2732.164;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.HSVToRGBNode;705;-1255.236,2898.607;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;699;-994.9265,2875.168;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;718;-993.7283,3219.214;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;709;-1153.418,3251.604;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;707;-1256.085,3249.454;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;708;-1421.199,3307.655;Inherit;False;Constant;_Vector0;Vector 0;23;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;904;-1478.556,3236.06;Inherit;False;902;ReflectionDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;720;-573.7271,2989.957;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;730;-411.6597,2991.05;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;691;-1341.699,4220.881;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;686;-1491.025,4222.653;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;690;-1341.699,4046.883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;682;-1492.888,4048.632;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;666;-1151.709,4138.628;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;665;-1154.108,3974.93;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;653;-866.3931,3974.092;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;676;-845.9454,4110.923;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;678;-881.9454,4210.923;Inherit;False;Constant;_Float1;Float 1;23;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;922;-1710.022,4140.925;Inherit;False;919;AmbientEquator;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;921;-1704.376,3977.498;Inherit;False;918;AmbientSky;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;649;-124.7978,4084.456;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;944;56.05124,4205.26;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;677;-728.9453,4110.923;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;654;-568.1364,4074.94;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;938;-414.4312,4084.022;Inherit;False;AmbientSkyMixEquator;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;650;-373.8008,4186.456;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;924;-379.4351,4001.058;Inherit;False;920;AmbientGround;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DotProductOpNode;616;-506.9673,4193.358;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;615;-714.9643,4207.358;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;617;-680.2578,4273.377;Inherit;False;Constant;_Vector6;Vector 6;23;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;955;935.5587,5210.761;Inherit;False;MainDirectSpecular;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;687;-1208.449,4939.18;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;692;-1062.222,4940.409;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;923;-1408.553,4937.967;Inherit;False;920;AmbientGround;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DesaturateOpNode;669;-831.9929,5037.939;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;945;-881.7522,5132.836;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;946;-603.8937,5038.697;Inherit;False;IndirectDiffuse;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;95;-19.02956,5031.943;Inherit;False;UnityStandardPBRLighting_MainLight;-1;;7502;b41580952992c61469d041596eb13ef8;0;6;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;OBJECT;;False;15;OBJECT;;False;16;OBJECT;;False;17;FLOAT;0;False;2;FLOAT3;0;FLOAT3;18
Node;AmplifyShaderEditor.GetLocalVarNode;97;-261.7985,5039.503;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-259.7985,5106.503;Inherit;False;48;mainLight;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.GetLocalVarNode;99;-261.7985,5174.503;Inherit;False;93;brdfData;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.GetLocalVarNode;96;-262.7985,4968.504;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;477;412.2884,5086.416;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;2,2,2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;829;391.3992,5259.624;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;831;252.8245,5259.435;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;832;-33.17556,5315.435;Inherit;False;Property;_OnAccumulatedWaterLightingAtten;非积水区域直接光照衰减;25;0;Create;False;0;0;0;False;0;False;0;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;828;-1.601356,5229.623;Inherit;False;830;PrimitiveRoughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;853;604.7475,5388.083;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;950;375.4202,5341.426;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;854;405.7474,5414.083;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;850;162.4559,5414.919;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;827;598.2294,5231.787;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;952;406.3943,5020.222;Inherit;False;MainDirectDiffuse;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-1699.799,5844.036;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;75;-1699.799,5917.037;Inherit;False;42;positionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-1699.799,5988.036;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1695.799,6066.036;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-1688.472,6136.28;Inherit;False;49;shadowMask;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;83;-1685.007,6203.037;Inherit;False;93;brdfData;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-1685.509,6275.073;Inherit;False;35;vertexlight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;80;-1389.353,5994.404;Inherit;False;UnityStandardPBRLighting_AddLight;-1;;7513;82d97dd707e557a49be2444a391eab2e;0;9;9;FLOAT3;0,0,0;False;10;FLOAT2;0,0;False;11;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;1,1,1,1;False;15;OBJECT;;False;16;OBJECT;;False;17;FLOAT;0;False;21;FLOAT3;0,0,0;False;3;FLOAT3;0;FLOAT3;8;FLOAT3;23
Node;AmplifyShaderEditor.GetLocalVarNode;71;-1500.638,6653.91;Inherit;False;39;vNormalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-1501.638,6815.91;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-1501.638,6885.91;Inherit;False;52;Roughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-1499.638,6955.91;Inherit;False;57;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-1501.638,6585.911;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-1502.638,6517.911;Inherit;False;42;positionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-1501.108,6453.449;Inherit;False;113;BakedAOTex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1737.638,6723.91;Inherit;False;Property;_HorizonOcclusion;(A)地平线遮蔽(Horizon)强度;10;0;Create;False;0;0;0;True;0;False;0;1;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1436.638,6722.91;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;834;786.4547,5210.208;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;818;-3419.393,2070.476;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-3242.743,2070.581;Inherit;False;Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;819;-3756.95,2141.879;Inherit;False;Property;_AccumulatedwaterReflectStrength;积水环境反射强度;18;0;Create;False;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;868;-3696.46,2214.857;Inherit;False;867;ModifWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;586;-3442.357,1662.244;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;-3469.788,1776.204;Inherit;False;310;MoistMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;391;-3234.559,1580.446;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;863;-3249.831,1798.413;Inherit;False;861;WaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;314;-3041.896,1580.005;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.005;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;882;-3699.113,2047.654;Inherit;False;881;PrimitiveMetallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-4707.086,969.2046;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-5826.532,2301.176;Inherit;False;298;DistortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;112;-5620.935,2277.848;Inherit;True;Property;Smoothness;EM贴图;11;0;Create;False;0;0;0;False;1;LogicalTex(_,false,RGB_A,_);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;774;-5854.242,2382.472;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;576;-4765.781,2057.11;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;571;-5257.781,2050.11;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;573;-5103.781,2049.11;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;574;-5251.781,2122.109;Inherit;False;Constant;_Float11;Float 11;19;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;575;-4929.781,2050.11;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;572;-4596.781,2056.11;Inherit;False;Emission;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;577;-4983.781,2132.109;Inherit;False;Constant;_Float21;Float 21;19;0;Create;True;0;0;0;False;0;False;1.111111;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;578;-5069.01,2212.212;Inherit;False;Property;_EmissionStrength0;(E)自发光强度;12;0;Create;False;0;0;0;True;0;False;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;880;-3751.181,1582.12;Inherit;False;830;PrimitiveRoughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;592;-3767.689,1679.878;Inherit;False;Property;_MoistRoughnessCoeff;湿润粗糙度系数;16;0;Create;False;0;0;0;False;0;False;0.2;0.2;0.05;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;52;-2871.17,1581.172;Inherit;False;Roughness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;527;-4738.276,2764.779;Inherit;False;BrdfDataSwizzle;-1;;7518;b5fcd67913219ec4e914fcc8214ebc59;0;11;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;11;FLOAT;0;False;14;OBJECT;;False;11;FLOAT3;38;FLOAT3;39;FLOAT3;40;FLOAT;41;FLOAT;42;FLOAT;44;FLOAT;46;FLOAT;47;FLOAT;48;FLOAT;49;OBJECT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-4038.193,3236.645;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.6037736;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;56;-5247.129,2748.441;Inherit;False;SurfaceData;-1;;7519;e41fa51f5f7c55c4db0e5c93e9d28133;0;13;79;FLOAT3;0,0,0;False;80;FLOAT3;1,1,1;False;84;FLOAT3;1,1,1;False;70;FLOAT;0;False;68;FLOAT;0;False;128;FLOAT3;0,0,0;False;129;FLOAT3;0,0,0;False;130;FLOAT;0;False;85;FLOAT3;0,0,0;False;86;FLOAT3;0,0,0;False;131;FLOAT;0;False;132;FLOAT;0;False;133;FLOAT;0;False;19;FLOAT3;97;FLOAT;99;FLOAT3;94;FLOAT3;95;FLOAT;90;FLOAT;89;FLOAT;101;FLOAT;0;FLOAT;87;FLOAT;103;FLOAT;88;FLOAT;91;FLOAT;92;FLOAT;93;FLOAT;96;OBJECT;121;OBJECT;106;FLOAT;143;OBJECT;140
Node;AmplifyShaderEditor.RegisterLocalVarNode;94;-4713.525,3084.734;Inherit;False;fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;54;-5783.542,2995.665;Inherit;False;Property;_SpecColor;高光颜色;5;0;Create;False;0;0;0;True;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;55;-5841.407,2822.564;Inherit;False;Property;_BaseColor;基础颜色;4;0;Create;False;0;0;0;True;1;MainColor;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;84;-5790.633,2753.096;Inherit;False;57;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;330;-4321.911,3258.624;Inherit;False;Property;_MoistDiffuseCoeff;湿润漫反射系数;15;0;Create;False;0;0;0;False;0;False;0.3;0.2;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;327;-3887.479,3211.794;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;335;-4079.675,3338.266;Inherit;False;310;MoistMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;528;-3728.427,2764.298;Inherit;False;BrdfDataSwizzle;-1;;7523;b5fcd67913219ec4e914fcc8214ebc59;0;11;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;11;FLOAT;0;False;14;OBJECT;;False;11;FLOAT3;38;FLOAT3;39;FLOAT3;40;FLOAT;41;FLOAT;42;FLOAT;44;FLOAT;46;FLOAT;47;FLOAT;48;FLOAT;49;OBJECT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-3319.534,3005.103;Inherit;False;brdfData;-1;True;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-5566.033,2824.538;Inherit;False;50;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-5559.342,2882.633;Inherit;False;52;Roughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-5560.029,2947.37;Inherit;False;25;NormalTs;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-5549.44,3011.434;Inherit;False;113;BakedAOTex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-5551.346,3077.505;Inherit;False;58;normalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-5548.346,3148.505;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-5527.395,3213.88;Inherit;False;Constant;_Alpha;Alpha;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;42;-923.6937,1173.121;Inherit;False;positionSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-923.0545,1043.029;Inherit;False;viewDirWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;-925.2225,777.2253;Inherit;False;positionDS;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-923.5545,716.9818;Inherit;False;shadowCoord;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-924.4058,652.4683;Inherit;False;vertexlight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-926.0037,588.2913;Inherit;False;vertexSH;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-926.096,524.4536;Inherit;False;dynamicLightMapUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;32;-926.3804,459.0005;Inherit;False;staticLightMapUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VoronoiNode;199;-5677.837,4304.942;Inherit;False;0;0;1;0;2;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0.001;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.StepOpNode;205;-5460.34,4260.78;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-5332.339,4304.779;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;203;-5162.939,4311.179;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-5914.008,4383.779;Inherit;False;Global;_RaindropsTiling;_RaindropsTiling;15;0;Create;False;0;0;0;False;0;False;30;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;168;-5898.451,4303.408;Inherit;False;481;PositionWSxz;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;162;-4938.04,4341.737;Inherit;True;Global;_RaindropsGradientMap;_RaindropsGradientMap;19;0;Create;False;0;0;0;False;0;False;-1;e89949843abf7004e84edac7b109710b;e89949843abf7004e84edac7b109710b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;777;-5158.682,4478.563;Inherit;False;771;MainSampler0;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-4631.733,4388.205;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;204;-5487.34,4397.779;Inherit;False;Global;_RaindropsSplashSpeed;_RaindropsSplashSpeed;15;0;Create;False;0;0;0;False;0;False;0.3;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;398;-5389.898,4467.724;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;394;-5664.899,4465.724;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;211;-5452.971,4486.145;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;208;-4775.528,4528.276;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-4930.53,4580.276;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;209;-5216.528,4581.276;Inherit;False;Global;_RaindropsSize;_RaindropsSize;15;0;Create;False;0;0;0;False;0;False;0.4;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;858;-4647.299,4516.487;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;212;-4489.232,4387.821;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;860;-4182.582,4469.725;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;523;-4319.408,4616.139;Inherit;False;522;FlatArea;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;396;-4436.777,4516.507;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;397;-4291.377,4516.907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-4129.621,4492.934;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;598;-3925.643,4393.452;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;597;-3690.128,4470.542;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;310;-3305.94,4473.309;Inherit;False;MoistMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;934;1469.807,2828.474;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;767;1256.27,2853.213;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassSwitchNode;31;-683.2208,93.14508;Inherit;False;0;0;9;9;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;913;-790.6171,165.1235;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;911;-810.6171,141.1235;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;910;-835.6171,116.1235;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;909;-857.6171,94.12347;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-1703.717,1466.871;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-1735.717,1533.871;Inherit;False;32;staticLightMapUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;889;-1012.7,1693.778;Inherit;False;ForwardDirWS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-1014.632,1544.928;Inherit;False;mainLight;-1;True;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.DotProductOpNode;810;-3691.892,5180.57;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;812;-3572.894,5180.57;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;814;-3407.145,5178.875;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;813;-3206.862,5148.764;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;817;-3030.978,5163.666;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;809;-3879.992,5164.47;Inherit;False;39;vNormalWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;811;-3878.992,5233.869;Inherit;False;40;viewDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;861;-3252.222,5342.338;Inherit;False;WaterMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;857;-3387.721,5341.723;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;549;-3675.091,5335.982;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;555;-3623.031,5415.011;Inherit;False;454;FlowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;967;-3026.705,5066.532;Inherit;False;Constant;_Float9;Float 9;29;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;867;-2581.802,5141.284;Inherit;False;ModifWaterMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;966;-2864.914,5139.993;Inherit;False;Property;_AccumulatedWater2;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;898;630.0517,1383.195;Inherit;False;ReflectionSS;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;977;1803.395,2714.052;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;979;-6038.916,995.8173;Inherit;False;Property;_Float6;关键字列表;26;0;Create;False;0;0;0;True;1;LogicalKeywordList(_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;658;-5062.164,1490.875;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;662;-4832.164,1443.876;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;890;241.0805,3300.361;Inherit;False;889;ForwardDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;842;260.3094,3378.81;Inherit;False;Constant;_Vector8;Vector 8;25;0;Create;True;0;0;0;False;0;False;0,-1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;841;468.2636,3298.923;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;849;847.5347,3505.025;Inherit;False;286;AccumulatedWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;846;926.9672,3264.11;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;980;918.9049,3361.243;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;835;-951.8254,5311.667;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-1622.953,5176.14;Inherit;False;93;brdfData;1;0;OBJECT;;False;1;OBJECT;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;-1619.953,5107.746;Inherit;False;94;fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;721;-879.2279,3170.851;Inherit;False;Global;_ApproxRealtimeGI_MixCoeff;_ApproxRealtimeGI_MixCoeff;23;0;Create;True;0;0;0;False;0;False;0.2;0.005652905;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;700;-799.7231,3270.9;Inherit;False;Global;_ApproxRealtimeGI_ReflectionStrength;_ApproxRealtimeGI_ReflectionStrength;23;0;Create;True;0;0;0;False;0;False;0.2;0.999642;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;903;-467.8161,794.0099;Inherit;False;902;ReflectionDirWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-438.4136,720.1136;Inherit;False;42;positionSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-461.9439,874.019;Inherit;False;52;Roughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;982;-289.8901,875.3291;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;905;309.1733,678.2219;Inherit;False;BakedReflect;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;108;-175.2834,678.8268;Inherit;False;EnvReflect;-1;;7526;5ceebe00e6af51a45836843f20fb949d;2,45,0,44,0;7;16;FLOAT3;0,0,0;False;18;FLOAT2;0,0;False;15;FLOAT3;0,0,0;False;17;FLOAT;0;False;38;FLOAT;0;False;20;SAMPLERCUBE;0;False;25;FLOAT4;0,0,0,0;False;2;FLOAT3;14;FLOAT3;43
Node;AmplifyShaderEditor.GetLocalVarNode;107;-442.4136,654.113;Inherit;False;36;positionWS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;68;-1177.64,6454.911;Inherit;False;EnvOcclusion;-1;;7527;1f8d06f242dc65745b935d33a763c278;5,54,0,60,0,55,0,65,0,53,0;8;3;FLOAT;1;False;5;FLOAT2;0,0;False;10;FLOAT3;0,0,0;False;21;FLOAT3;0,0,0;False;22;FLOAT;1;False;11;FLOAT3;0,0,0;False;13;FLOAT;0;False;15;FLOAT3;0,0,0;False;8;FLOAT;0;FLOAT;18;FLOAT;6;FLOAT;7;FLOAT;33;FLOAT;36;FLOAT;24;FLOAT;29
Node;AmplifyShaderEditor.SimpleAddOpNode;125;-5033.196,1847.455;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-5319.183,1848.147;Inherit;False;Property;_OcclusionBaked;(A)环境光遮蔽;9;0;Create;False;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;122;-4897.183,1772.149;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-4727.559,1773.53;Inherit;False;BakedAOTex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-5322.925,2326.729;Inherit;False;Property;_Metallic01;(M)金属度;13;0;Create;False;0;0;0;True;0;False;1;-1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-5259.491,1573.914;Inherit;False;Property;_Roughness1;(R)粗糙度;8;0;Create;False;0;0;0;False;0;False;1;-0.252;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;986;-4965.378,2349.54;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;155;-6041.975,906.3425;Inherit;False;Property;_Cull;剔除模式;0;0;Create;False;0;0;0;True;1;BuiltinEnum(_, CullMode);False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;671;-973.3986,6017.934;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;2,2,2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;956;-724.944,5972.326;Inherit;False;955;MainDirectSpecular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;953;-693.3757,5907.238;Inherit;False;952;MainDirectDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;948;-692.3669,5779.173;Inherit;False;946;IndirectDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;949;-692.3669,5842.173;Inherit;False;947;IndirectSpecular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;664;-661.2478,6165.757;Inherit;False;572;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;839;-1553.426,5296.733;Inherit;False;830;PrimitiveRoughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;838;-1589.207,5365.296;Inherit;False;Property;_OnAccumulatedWaterEnvEeflectAtten;非积水区域环境反射衰减;24;0;Create;False;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;837;-1300.002,5335.546;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;836;-1161.426,5335.733;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;852;-1161.946,5501.156;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;851;-938.5383,5425.625;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;196;-1387.073,5039.883;Inherit;False;UnityStandardPBRLighting_Indirect;-1;;7529;4e256c588e7d3354f8b27082ef19ebfc;0;7;29;FLOAT3;0,0,0;False;23;FLOAT3;0,0,0;False;42;FLOAT3;0,0,0;False;20;FLOAT;0;False;31;OBJECT;;False;43;OBJECT;;False;44;FLOAT;0;False;2;FLOAT3;28;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;943;-1655.049,4972.47;Inherit;False;942;ApproxRealtimeGI;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;937;-1714.308,5042.161;Inherit;False;936;ApproxRealtimeFinalReflect;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;907;-797.9936,2883.991;Inherit;False;905;BakedReflect;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;894;-259.2086,2988.821;Inherit;False;ApproxRealtimeReflection;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;931;797.6925,2124.569;Inherit;False;BakedGItoAO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;899;483.2382,3024.506;Inherit;False;898;ReflectionSS;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;895;439.4359,2859.997;Inherit;False;894;ApproxRealtimeReflection;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;932;1244.807,2717.475;Inherit;False;931;BakedGItoAO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;908;1152.202,2636.6;Inherit;False;894;ApproxRealtimeReflection;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;933;1465.806,2640.475;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;768;1650.495,2804.71;Inherit;False;Property;_ScreenReflection;屏幕空间实时反射;14;0;Create;False;0;0;0;False;0;False;1;1;1;True;_SCREENREFLECTION_ON;Toggle;2;Key0;Key1;Create;True;False;Fragment;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;976;1933.598,2777.822;Inherit;False;Property;_AccumulatedWater7;_AccumulatedWater;27;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Reference;962;False;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;936;2197.188,2777.334;Inherit;False;ApproxRealtimeFinalReflect;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;823;-1183.622,5424.789;Inherit;False;393;GlobalMoist01;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;869;-1369.601,5500.848;Inherit;False;867;ModifWaterMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;826;-768.8166,5286.608;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;942;285.6964,4439.312;Inherit;False;ApproxRealtimeGI;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;947;-609.3044,5288.131;Inherit;False;IndirectSpecular;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;993;-520.7704,-491.1638;Inherit;False;5;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;991;-795.277,-617.3242;Inherit;False;947;IndirectSpecular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;989;-791.2858,-537.2593;Inherit;False;952;MainDirectDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;988;-822.8541,-472.1709;Inherit;False;955;MainDirectSpecular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;992;-757.158,-404.7402;Inherit;False;572;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;990;-790.277,-677.3242;Inherit;False;946;IndirectDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;985;-4954.378,1595.54;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;830;-4607.958,1594.548;Inherit;False;PrimitiveRoughness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;994;-4766.587,1594.393;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;995;-4774.384,2351.545;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;881;-4591.118,2361.172;Inherit;False;PrimitiveMetallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;961;-3563.311,4392.369;Inherit;False;Property;_Raindrops;_Raindrops;27;0;Create;False;0;0;0;False;0;False;1;0;0;False;_RAINDROPS_ON;Toggle;2;Key0;Key1;Create;False;False;Fragment;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;915;-663.3584,-279.8755;Inherit;False;914;FinalColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;81;-384.3118,5901.902;Inherit;False;CalculateFinalColor;-1;;7531;4a3e69251c0a99c4aac425c4fd3784d1;0;13;13;FLOAT3;0,0,0;False;14;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;18;FLOAT;1;False;19;FLOAT;1;False;21;FLOAT;1;False;20;FLOAT;1;False;6;FLOAT3;0,0,0;False;8;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;914;-58.37669,5901.232;Inherit;False;FinalColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;822;-293.7713,317.8709;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;SmoothnessMetallic;0;8;SmoothnessMetallic;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;503;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;504;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;505;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;506;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;SceneSelectionPass;0;5;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;507;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ScenePickingPass;0;6;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;508;-404.1997,-120.7273;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthNormals;0;7;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;501;-384.7713,-107.1291;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ExtraPrePass;0;0;ExtraPrePass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;502;-354.4515,-124.8767;Float;False;True;-1;2;LogicalSGUI.LogicalSGUI;0;14;ScenesPBR;fa16f3565aea0f0448ed74c1d4f5407b;True;Forward;0;1;Forward;13;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;0;True;_Cull;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;10;Include;;False;;Native;False;0;0;;Pragma;multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN;False;;Custom;True;0;0;Forward;Pragma;multi_compile _ LIGHTMAP_ON;False;;Custom;True;0;0;Forward;Pragma;multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX;False;;Custom;True;0;0;Forward;Pragma;multi_compile_fragment _ _REFLECTION_PROBE_BLENDING;False;;Custom;True;0;0;Forward;Pragma;multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION;False;;Custom;True;0;0;Forward;Pragma;multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS;False;;Custom;True;0;0;Forward;Pragma;multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS;False;;Custom;True;0;0;Forward;Pragma;multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION;False;;Custom;True;0;0;Forward;Define;REQUIRE_BAKEDGI 1;False;;Custom;True;0;0;Forward;;0;0;Standard;20;Surface;0;0;  Blend;0;0;Forward Only;0;638584696063396300;Cast Shadows;1;638584696053842183;  Use Shadow Threshold;0;0;GPU Instancing;1;638584696040598906;LOD CrossFade;1;638584696032144661;Built-in Fog;0;638584696020544565;Meta Pass;1;638584695952511008;RoughnessMetallic Pass;0;638584696006530825;Extra Pre Pass;0;638584695998509573;Tessellation;0;638584695985920901;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;0;9;False;True;True;False;True;False;False;True;False;False;;True;0
WireConnection;36;0;21;312
WireConnection;479;0;478;0
WireConnection;771;0;801;5
WireConnection;39;0;21;0
WireConnection;457;0;456;0
WireConnection;457;1;458;0
WireConnection;457;2;459;0
WireConnection;430;0;429;0
WireConnection;430;1;432;0
WireConnection;399;0;343;0
WireConnection;480;0;479;0
WireConnection;480;1;479;2
WireConnection;460;0;430;0
WireConnection;460;1;457;0
WireConnection;400;0;399;0
WireConnection;481;0;480;0
WireConnection;783;116;460;0
WireConnection;783;156;435;0
WireConnection;783;167;436;0
WireConnection;783;210;785;0
WireConnection;401;0;400;0
WireConnection;520;0;280;0
WireConnection;463;0;783;19
WireConnection;276;0;260;0
WireConnection;278;0;252;0
WireConnection;278;2;520;0
WireConnection;338;0;482;0
WireConnection;338;1;339;0
WireConnection;784;116;430;0
WireConnection;784;156;435;0
WireConnection;784;167;436;0
WireConnection;784;210;785;0
WireConnection;464;0;463;1
WireConnection;464;1;463;1
WireConnection;464;2;463;1
WireConnection;464;3;463;1
WireConnection;464;4;463;1
WireConnection;464;5;463;1
WireConnection;464;6;463;1
WireConnection;464;7;463;1
WireConnection;253;0;278;0
WireConnection;253;1;254;0
WireConnection;262;0;260;0
WireConnection;262;1;263;0
WireConnection;266;0;278;0
WireConnection;266;1;268;0
WireConnection;270;0;276;0
WireConnection;270;1;271;0
WireConnection;403;0;402;0
WireConnection;219;1;338;0
WireConnection;219;7;778;0
WireConnection;438;0;784;19
WireConnection;465;0;464;0
WireConnection;259;0;253;0
WireConnection;259;2;262;0
WireConnection;234;0;232;0
WireConnection;234;1;235;0
WireConnection;269;0;266;0
WireConnection;269;2;270;0
WireConnection;220;0;219;1
WireConnection;220;1;403;0
WireConnection;439;0;438;0
WireConnection;439;1;465;0
WireConnection;325;0;8;0
WireConnection;228;13;234;0
WireConnection;228;24;231;4
WireConnection;228;4;231;1
WireConnection;228;5;231;2
WireConnection;228;130;231;3
WireConnection;251;0;265;0
WireConnection;251;1;259;0
WireConnection;251;5;264;0
WireConnection;251;7;780;0
WireConnection;275;0;265;0
WireConnection;275;1;269;0
WireConnection;275;5;272;0
WireConnection;275;7;780;0
WireConnection;408;0;410;0
WireConnection;408;1;409;0
WireConnection;519;1;220;0
WireConnection;519;2;222;0
WireConnection;437;0;438;2
WireConnection;437;1;438;3
WireConnection;451;0;439;0
WireConnection;451;1;452;0
WireConnection;233;0;230;0
WireConnection;233;1;228;0
WireConnection;233;5;236;0
WireConnection;233;7;779;0
WireConnection;413;0;408;0
WireConnection;413;1;415;0
WireConnection;413;2;416;0
WireConnection;226;0;519;0
WireConnection;277;0;251;0
WireConnection;277;1;275;0
WireConnection;440;2;437;0
WireConnection;440;3;996;0
WireConnection;453;0;451;0
WireConnection;522;0;413;0
WireConnection;224;0;226;0
WireConnection;972;1;973;0
WireConnection;972;0;277;0
WireConnection;12;0;326;0
WireConnection;964;1;965;0
WireConnection;964;0;233;0
WireConnection;449;0;440;0
WireConnection;560;0;453;0
WireConnection;560;1;556;0
WireConnection;13;0;11;0
WireConnection;13;1;12;0
WireConnection;223;0;224;0
WireConnection;223;1;522;0
WireConnection;223;2;595;0
WireConnection;294;0;964;0
WireConnection;970;1;971;0
WireConnection;970;0;449;0
WireConnection;306;0;972;0
WireConnection;968;1;969;0
WireConnection;968;0;560;0
WireConnection;14;0;13;0
WireConnection;14;1;12;1
WireConnection;962;1;963;0
WireConnection;962;0;223;0
WireConnection;450;0;970;0
WireConnection;999;0;968;0
WireConnection;999;1;1000;0
WireConnection;15;0;14;0
WireConnection;315;0;293;0
WireConnection;315;1;316;0
WireConnection;286;0;962;0
WireConnection;454;0;999;0
WireConnection;16;0;15;1
WireConnection;17;0;15;0
WireConnection;877;0;874;0
WireConnection;297;0;315;0
WireConnection;875;0;295;0
WireConnection;875;1;877;0
WireConnection;875;2;876;0
WireConnection;296;0;297;0
WireConnection;296;1;299;0
WireConnection;296;2;295;0
WireConnection;41;0;21;271
WireConnection;18;0;17;0
WireConnection;18;1;16;0
WireConnection;292;0;18;0
WireConnection;292;1;296;0
WireConnection;292;2;875;0
WireConnection;485;0;422;0
WireConnection;419;2;418;0
WireConnection;419;3;292;0
WireConnection;419;8;421;0
WireConnection;419;9;424;0
WireConnection;419;10;485;0
WireConnection;975;0;18;0
WireConnection;974;1;975;0
WireConnection;974;0;419;0
WireConnection;298;0;974;0
WireConnection;116;0;786;0
WireConnection;116;1;301;0
WireConnection;116;7;790;0
WireConnection;563;0;116;0
WireConnection;564;2;563;0
WireConnection;564;3;186;0
WireConnection;878;0;564;0
WireConnection;282;0;308;0
WireConnection;282;1;307;0
WireConnection;284;0;879;0
WireConnection;284;1;282;0
WireConnection;284;2;865;0
WireConnection;470;0;284;0
WireConnection;470;1;471;0
WireConnection;470;2;469;0
WireConnection;24;0;21;6
WireConnection;110;0;325;0
WireConnection;110;1;521;0
WireConnection;110;7;789;0
WireConnection;25;0;470;0
WireConnection;149;0;110;4
WireConnection;28;0;25;0
WireConnection;28;1;27;0
WireConnection;29;0;28;0
WireConnection;153;0;150;0
WireConnection;153;1;152;0
WireConnection;58;0;29;0
WireConnection;344;0;343;0
WireConnection;393;0;344;0
WireConnection;663;0;186;0
WireConnection;663;1;662;0
WireConnection;761;0;758;39
WireConnection;761;1;760;0
WireConnection;758;31;759;0
WireConnection;758;52;757;0
WireConnection;756;14;755;0
WireConnection;762;0;756;8
WireConnection;762;1;760;0
WireConnection;49;0;44;32
WireConnection;844;28;887;0
WireConnection;844;39;888;0
WireConnection;44;29;45;0
WireConnection;44;31;46;0
WireConnection;44;30;47;0
WireConnection;105;5;103;0
WireConnection;105;8;104;0
WireConnection;902;0;105;0
WireConnection;763;0;754;0
WireConnection;763;1;761;0
WireConnection;763;2;762;0
WireConnection;925;0;614;0
WireConnection;614;17;61;0
WireConnection;614;35;62;0
WireConnection;614;36;63;0
WireConnection;614;37;64;0
WireConnection;614;21;65;0
WireConnection;919;0;614;45
WireConnection;918;0;614;43
WireConnection;920;0;614;47
WireConnection;916;0;925;0
WireConnection;724;0;916;0
WireConnection;724;1;726;0
WireConnection;724;2;725;0
WireConnection;728;1;917;0
WireConnection;728;2;729;0
WireConnection;640;0;944;0
WireConnection;640;1;728;0
WireConnection;640;2;643;0
WireConnection;156;1;151;0
WireConnection;156;0;153;0
WireConnection;843;0;841;0
WireConnection;845;0;843;0
WireConnection;847;0;895;0
WireConnection;847;1;899;0
WireConnection;848;0;847;0
WireConnection;848;1;899;0
WireConnection;766;0;899;0
WireConnection;802;0;766;0
WireConnection;802;1;849;0
WireConnection;802;2;980;0
WireConnection;710;0;699;0
WireConnection;710;1;723;0
WireConnection;710;2;718;0
WireConnection;704;0;939;0
WireConnection;702;0;906;0
WireConnection;702;1;703;0
WireConnection;705;0;704;1
WireConnection;705;1;704;2
WireConnection;705;2;706;0
WireConnection;699;0;702;0
WireConnection;699;1;705;0
WireConnection;718;0;709;0
WireConnection;718;1;709;0
WireConnection;718;2;709;0
WireConnection;718;3;709;0
WireConnection;709;0;707;0
WireConnection;707;0;904;0
WireConnection;707;1;708;0
WireConnection;720;0;907;0
WireConnection;720;1;710;0
WireConnection;720;2;721;0
WireConnection;730;0;720;0
WireConnection;730;1;700;0
WireConnection;691;0;686;0
WireConnection;686;0;922;0
WireConnection;690;0;682;0
WireConnection;682;0;921;0
WireConnection;666;0;922;0
WireConnection;666;1;691;0
WireConnection;665;0;921;0
WireConnection;665;1;690;0
WireConnection;653;0;665;0
WireConnection;653;1;666;0
WireConnection;676;0;665;0
WireConnection;676;1;666;0
WireConnection;649;0;924;0
WireConnection;649;1;938;0
WireConnection;649;2;650;0
WireConnection;944;0;649;0
WireConnection;677;0;676;0
WireConnection;677;1;678;0
WireConnection;654;0;653;0
WireConnection;654;1;677;0
WireConnection;938;0;654;0
WireConnection;650;0;616;0
WireConnection;616;0;615;0
WireConnection;616;1;617;0
WireConnection;955;0;834;0
WireConnection;687;0;923;0
WireConnection;692;0;687;0
WireConnection;669;0;196;28
WireConnection;669;1;692;0
WireConnection;945;0;196;0
WireConnection;946;0;669;0
WireConnection;95;12;96;0
WireConnection;95;13;97;0
WireConnection;95;14;98;0
WireConnection;95;15;99;0
WireConnection;477;0;95;18
WireConnection;829;0;831;0
WireConnection;831;0;828;0
WireConnection;831;1;832;0
WireConnection;853;0;950;0
WireConnection;853;1;854;0
WireConnection;854;0;850;0
WireConnection;827;0;477;0
WireConnection;827;1;829;0
WireConnection;952;0;95;0
WireConnection;80;9;76;0
WireConnection;80;10;75;0
WireConnection;80;11;77;0
WireConnection;80;13;78;0
WireConnection;80;14;79;0
WireConnection;80;15;83;0
WireConnection;80;21;82;0
WireConnection;73;0;72;0
WireConnection;834;0;477;0
WireConnection;834;1;827;0
WireConnection;834;2;853;0
WireConnection;818;0;882;0
WireConnection;818;1;819;0
WireConnection;818;2;868;0
WireConnection;50;0;818;0
WireConnection;586;0;880;0
WireConnection;586;1;592;0
WireConnection;391;0;880;0
WireConnection;391;1;586;0
WireConnection;391;2;312;0
WireConnection;314;0;391;0
WireConnection;314;2;863;0
WireConnection;57;0;110;0
WireConnection;112;1;142;0
WireConnection;112;7;774;0
WireConnection;576;0;575;0
WireConnection;576;1;577;0
WireConnection;576;2;578;0
WireConnection;571;0;112;0
WireConnection;573;0;571;0
WireConnection;573;1;574;0
WireConnection;575;0;573;0
WireConnection;572;0;576;0
WireConnection;52;0;314;0
WireConnection;527;14;56;106
WireConnection;329;0;527;39
WireConnection;329;1;330;0
WireConnection;56;79;84;0
WireConnection;56;80;55;0
WireConnection;56;84;54;0
WireConnection;56;70;85;0
WireConnection;56;68;86;0
WireConnection;56;128;87;0
WireConnection;56;130;121;0
WireConnection;56;85;89;0
WireConnection;56;86;90;0
WireConnection;56;131;91;0
WireConnection;94;0;56;96
WireConnection;327;0;527;39
WireConnection;327;1;329;0
WireConnection;327;2;335;0
WireConnection;528;2;527;38
WireConnection;528;3;327;0
WireConnection;528;4;527;40
WireConnection;528;5;527;41
WireConnection;528;6;527;42
WireConnection;528;7;527;44
WireConnection;528;8;527;46
WireConnection;528;9;527;47
WireConnection;528;10;527;48
WireConnection;528;11;527;49
WireConnection;93;0;528;0
WireConnection;42;0;21;7
WireConnection;40;0;21;156
WireConnection;38;0;21;178
WireConnection;37;0;21;243
WireConnection;35;0;21;285
WireConnection;34;0;21;277
WireConnection;33;0;21;275
WireConnection;32;0;21;273
WireConnection;199;0;168;0
WireConnection;199;2;202;0
WireConnection;205;0;199;0
WireConnection;206;0;205;0
WireConnection;206;1;199;1
WireConnection;203;0;206;0
WireConnection;203;2;204;0
WireConnection;203;1;398;0
WireConnection;162;1;203;0
WireConnection;162;7;777;0
WireConnection;207;0;162;1
WireConnection;207;1;208;0
WireConnection;398;0;394;0
WireConnection;211;0;199;0
WireConnection;208;0;211;0
WireConnection;208;1;210;0
WireConnection;210;0;209;0
WireConnection;212;0;207;0
WireConnection;860;0;212;0
WireConnection;396;0;858;0
WireConnection;397;0;396;0
WireConnection;213;0;860;0
WireConnection;213;1;397;0
WireConnection;213;2;523;0
WireConnection;597;0;598;0
WireConnection;597;1;213;0
WireConnection;310;0;961;0
WireConnection;934;0;932;0
WireConnection;934;1;767;0
WireConnection;767;0;895;0
WireConnection;767;1;848;0
WireConnection;767;2;802;0
WireConnection;31;0;21;313
WireConnection;31;1;21;313
WireConnection;31;2;21;413
WireConnection;31;3;21;313
WireConnection;31;4;21;415
WireConnection;31;5;21;313
WireConnection;31;6;21;313
WireConnection;31;7;21;313
WireConnection;31;8;21;313
WireConnection;913;0;21;347
WireConnection;911;0;21;547
WireConnection;910;0;21;546
WireConnection;909;0;21;543
WireConnection;889;0;844;3
WireConnection;48;0;44;17
WireConnection;810;0;809;0
WireConnection;810;1;811;0
WireConnection;812;0;810;0
WireConnection;814;0;812;0
WireConnection;813;0;814;0
WireConnection;813;1;814;0
WireConnection;813;2;814;0
WireConnection;813;3;814;0
WireConnection;817;0;813;0
WireConnection;817;1;861;0
WireConnection;861;0;857;0
WireConnection;857;0;549;0
WireConnection;857;1;555;0
WireConnection;867;0;966;0
WireConnection;966;1;967;0
WireConnection;966;0;817;0
WireConnection;898;0;763;0
WireConnection;977;0;933;0
WireConnection;662;2;658;0
WireConnection;841;0;890;0
WireConnection;841;1;842;0
WireConnection;846;0;845;0
WireConnection;846;1;845;0
WireConnection;980;0;845;0
WireConnection;835;0;196;0
WireConnection;835;1;836;0
WireConnection;982;0;109;0
WireConnection;905;0;108;14
WireConnection;108;16;107;0
WireConnection;108;18;106;0
WireConnection;108;15;903;0
WireConnection;108;17;982;0
WireConnection;68;3;120;0
WireConnection;68;5;69;0
WireConnection;68;10;70;0
WireConnection;68;21;71;0
WireConnection;68;22;73;0
WireConnection;68;11;66;0
WireConnection;125;0;123;0
WireConnection;122;1;116;4
WireConnection;122;2;125;0
WireConnection;113;0;122;0
WireConnection;986;0;138;0
WireConnection;986;1;112;4
WireConnection;671;0;80;8
WireConnection;837;0;839;0
WireConnection;837;1;838;0
WireConnection;836;0;837;0
WireConnection;852;0;869;0
WireConnection;851;0;823;0
WireConnection;851;1;852;0
WireConnection;196;29;943;0
WireConnection;196;23;937;0
WireConnection;196;20;100;0
WireConnection;196;31;101;0
WireConnection;894;0;730;0
WireConnection;931;0;724;0
WireConnection;933;0;908;0
WireConnection;933;1;932;0
WireConnection;768;1;933;0
WireConnection;768;0;934;0
WireConnection;976;1;977;0
WireConnection;976;0;768;0
WireConnection;936;0;976;0
WireConnection;826;0;945;0
WireConnection;826;1;835;0
WireConnection;826;2;851;0
WireConnection;942;0;640;0
WireConnection;947;0;826;0
WireConnection;993;0;990;0
WireConnection;993;1;991;0
WireConnection;993;2;989;0
WireConnection;993;3;988;0
WireConnection;993;4;992;0
WireConnection;985;0;135;0
WireConnection;985;1;116;3
WireConnection;830;0;994;0
WireConnection;994;0;985;0
WireConnection;995;0;986;0
WireConnection;881;0;995;0
WireConnection;961;1;598;0
WireConnection;961;0;597;0
WireConnection;81;13;948;0
WireConnection;81;14;949;0
WireConnection;81;12;953;0
WireConnection;81;11;956;0
WireConnection;81;9;80;0
WireConnection;81;10;671;0
WireConnection;81;5;80;23
WireConnection;81;18;68;33
WireConnection;81;19;68;36
WireConnection;81;21;68;24
WireConnection;81;20;68;29
WireConnection;81;6;664;0
WireConnection;914;0;81;0
WireConnection;502;0;915;0
WireConnection;502;1;156;0
WireConnection;502;4;111;0
WireConnection;502;5;158;0
WireConnection;502;6;891;0
WireConnection;502;7;909;0
WireConnection;502;8;910;0
WireConnection;502;9;911;0
WireConnection;502;12;913;0
WireConnection;502;13;31;0
ASEEND*/
//CHKSM=04B7BF52908654BD4E7FD1035081C8C35C776876