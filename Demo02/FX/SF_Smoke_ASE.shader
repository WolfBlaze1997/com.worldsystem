// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SF_Smoke_ASE"
{
	Properties
	{

		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_T_Smoke_01("T_Smoke_01", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}


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

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }

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

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140011


			CBUFFER_START(UnityPerMaterial)
			float4 _T_Smoke_01_ST;
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
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
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

			sampler2D _T_Smoke_01;


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
			
			float4 ShadowMask28_g1219( float2 StaticLightMapUV )
			{
				half4 shadowMask =half4(1, 1, 1, 1); 
				shadowMask = SAMPLE_SHADOWMASK(StaticLightMapUV);
				return shadowMask;
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
				
				float4 temp_output_9_313 = vertexPositionCS382_g1;
				
				float4 break535_g1 = v.ase_color;
				float vertexToFrag536_g1 = break535_g1.r;
				o.ase_texcoord2.z = vertexToFrag536_g1;
				float vertexToFrag537_g1 = break535_g1.g;
				o.ase_texcoord2.w = vertexToFrag537_g1;
				float vertexToFrag538_g1 = break535_g1.b;
				o.ase_texcoord3.x = vertexToFrag538_g1;
				float vertexToFrag539_g1 = break535_g1.a;
				o.ase_texcoord3.y = vertexToFrag539_g1;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
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
			
				o.positionCS = temp_output_9_313;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float localGetMainLight_Ref1_g1219 = ( 0.0 );
				float4 shadowCoord1_g1219 = float4( 0,0,0,0 );
				float3 positionWS1_g1219 = float3( 0,0,0 );
				float2 StaticLightMapUV28_g1219 = float2( 0,0 );
				float4 localShadowMask28_g1219 = ShadowMask28_g1219( StaticLightMapUV28_g1219 );
				float4 shadowMask1_g1219 = localShadowMask28_g1219;
				float3 Direction1_g1219 = float3( 0,0,0 );
				float3 Color1_g1219 = float3( 0,0,0 );
				float DistanceAttenuation1_g1219 = 0;
				float ShadowAttenuation1_g1219 = 0;
				int LayerMask1_g1219 = 0;
				Light light1_g1219 = (Light)0;
				{
				Light mainlight= (Light)0;
				mainlight = GetMainLight(shadowCoord1_g1219, positionWS1_g1219, shadowMask1_g1219);
				Direction1_g1219 = mainlight.direction;
				Color1_g1219 = mainlight.color;
				DistanceAttenuation1_g1219 = mainlight.distanceAttenuation;
				ShadowAttenuation1_g1219 = mainlight.shadowAttenuation;
				LayerMask1_g1219 = mainlight.layerMask;
				light1_g1219 = mainlight;
				}
				float2 uv_T_Smoke_01 = IN.ase_texcoord2.xy * _T_Smoke_01_ST.xy + _T_Smoke_01_ST.zw;
				float vertexToFrag536_g1 = IN.ase_texcoord2.z;
				float vertexToFrag537_g1 = IN.ase_texcoord2.w;
				float vertexToFrag538_g1 = IN.ase_texcoord3.x;
				float vertexToFrag539_g1 = IN.ase_texcoord3.y;
				float4 appendResult13 = (float4(vertexToFrag536_g1 , vertexToFrag537_g1 , vertexToFrag538_g1 , vertexToFrag539_g1));
				float4 VertexColor12 = appendResult13;
				float4 temp_output_14_0 = ( tex2D( _T_Smoke_01, uv_T_Smoke_01 ) * VertexColor12 );
				float dotResult22 = dot( float3(0,1,0) , Direction1_g1219 );
				

				float3 Color = ( float4( saturate( Color1_g1219 ) , 0.0 ) * temp_output_14_0 * ( saturate( dotResult22 ) * 0.5 ) ).rgb;
				float Alpha = ( temp_output_14_0.a * 0.5 );
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 WorldNormal = float3(0, 0, 1);
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
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

	
	}
	
	CustomEditor "LogicalSGUI.LogicalSGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.FunctionNode;9;-518.9803,-66.60291;Inherit;False;Model;-1;;1;db2a50576abc55b4182a85385bad6f66;8,430,0,511,0,512,0,514,0,513,0,505,0,431,0,552,0;7;307;FLOAT3;0,0,0;False;432;FLOAT3;0,0,0;False;377;FLOAT3;0,0,0;False;433;FLOAT3;0,0,0;False;554;FLOAT3;0,0,0;False;555;FLOAT3;0,0,0;False;556;FLOAT3;0,0,0;False;39;FLOAT2;543;FLOAT3;546;FLOAT;547;FLOAT4;347;FLOAT4;313;FLOAT4;413;FLOAT4;415;FLOAT2;273;FLOAT2;275;FLOAT3;277;FLOAT;410;FLOAT3;285;FLOAT4;243;FLOAT4;178;FLOAT3;0;FLOAT3;4;FLOAT3;5;FLOAT3x3;6;FLOAT3;312;FLOAT3;156;FLOAT3;271;FLOAT3;229;FLOAT3;7;FLOAT3;185;FLOAT3;194;FLOAT3;197;FLOAT3;11;FLOAT;21;FLOAT;540;FLOAT;541;FLOAT;542;FLOAT;22;INT;202;FLOAT4;224;FLOAT4;225;FLOAT3;220;INT;216;INT;217;INT;219
Node;AmplifyShaderEditor.DynamicAppendNode;13;26.59351,558.5062;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;20;-593.9819,-665.9551;Inherit;False;MainLight;-1;;1219;8feda1e983ee2e6418994fa25e7d0c8f;0;3;29;FLOAT4;0,0,0,0;False;31;FLOAT3;0,0,0;False;30;FLOAT2;0,0;False;7;FLOAT3;0;FLOAT3;10;FLOAT;11;FLOAT;12;INT;13;OBJECT;17;FLOAT4;32
Node;AmplifyShaderEditor.Vector3Node;21;-232.6434,-818.1755;Inherit;False;Constant;_Vector4;Vector 4;1;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;220.9463,557.6693;Inherit;False;VertexColor;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DotProductOpNode;22;-54.64343,-715.1755;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-876.6339,-310.2418;Inherit;False;12;VertexColor;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;8;-980.6738,-546.2532;Inherit;True;Property;_T_Smoke_01;T_Smoke_01;0;0;Create;True;0;0;0;False;0;False;-1;89b6842441074d74f808caca1009d8be;89b6842441074d74f808caca1009d8be;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;27;74.44385,-713.1102;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-588.634,-416.2422;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;29;-176.1134,-540.7535;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;18;-263.4041,-288.8777;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;224.4438,-671.1102;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassSwitchNode;10;-11.84009,168.787;Inherit;False;0;0;8;8;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;451.0181,-463.9551;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;107.984,-252.3027;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;SceneSelectionPass;0;5;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ScenePickingPass;0;6;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;DepthNormals;0;7;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;320,-114;Float;False;False;-1;2;LogicalSGUI.LogicalSGUI;0;14;New Amplify Shader;fa16f3565aea0f0448ed74c1d4f5407b;True;ExtraPrePass;0;0;ExtraPrePass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;675,-197;Float;False;True;-1;2;LogicalSGUI.LogicalSGUI;0;14;SF_Smoke_ASE;fa16f3565aea0f0448ed74c1d4f5407b;True;Forward;0;1;Forward;12;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;19;Surface;1;638567377584373878;  Blend;0;638567386743983867;Forward Only;0;0;Cast Shadows;0;638567376916262638;  Use Shadow Threshold;0;0;GPU Instancing;1;0;LOD CrossFade;0;638567376965275842;Built-in Fog;1;638567377010365327;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;0;8;False;True;False;False;False;False;False;False;False;;False;0
WireConnection;13;0;9;21
WireConnection;13;1;9;540
WireConnection;13;2;9;541
WireConnection;13;3;9;542
WireConnection;12;0;13;0
WireConnection;22;0;21;0
WireConnection;22;1;20;0
WireConnection;27;0;22;0
WireConnection;14;0;8;0
WireConnection;14;1;16;0
WireConnection;29;0;20;10
WireConnection;18;0;14;0
WireConnection;28;0;27;0
WireConnection;10;0;9;313
WireConnection;10;1;9;313
WireConnection;10;2;9;413
WireConnection;10;3;9;313
WireConnection;10;4;9;415
WireConnection;10;5;9;313
WireConnection;10;6;9;313
WireConnection;10;7;9;313
WireConnection;19;0;29;0
WireConnection;19;1;14;0
WireConnection;19;2;28;0
WireConnection;30;0;18;3
WireConnection;1;0;19;0
WireConnection;1;1;30;0
WireConnection;1;7;9;543
WireConnection;1;8;9;546
WireConnection;1;9;9;547
WireConnection;1;12;9;347
WireConnection;1;13;10;0
ASEEND*/
//CHKSM=9058FCBEB439CA95086FCB5AFE41CB9ACC4DAB19