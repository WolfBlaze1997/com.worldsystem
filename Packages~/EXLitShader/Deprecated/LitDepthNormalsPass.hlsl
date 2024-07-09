// #ifndef UNIVERSAL_FORWARD_LIT_DEPTH_NORMALS_PASS_INCLUDED
// #define UNIVERSAL_FORWARD_LIT_DEPTH_NORMALS_PASS_INCLUDED

// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// #define Varyings debugVaryings
// #include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
// #undef Varyings

// #if defined(_LIGHTINGMODEL_PLANT)
//     #if defined(_WINDQUALITY_FASTEST) || defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
//         #define ENABLE_WIND
//     #endif
// #endif

// // GLES2 has limited amount of interpolators
// #if defined(_PARALLAXMAP)
//     #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
// #endif

// struct Attributes
// {
//     float4 positionOS : POSITION;
//     float4 tangentOS : TANGENT;
//     float4 texcoord : TEXCOORD0;
//     #if defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT)
//         float4 texcoord1 : TEXCOORD1;
//         float4 texcoord2 : TEXCOORD2;
//         float4 texcoord3 : TEXCOORD3;
//     #endif
//     float3 normal : NORMAL;
//     UNITY_VERTEX_INPUT_INSTANCE_ID
// };

// struct Varyings
// {
//     float2 uv : TEXCOORD1;
//     half4 normalWS : TEXCOORD2;
//     half4 tangentWS : TEXCOORD3;    // xyz: tangent, w: sign
//     half4 bitangentWS : TEXCOORD4;    // xyz: tangent, w: sign

//     // half3 viewDirWS : TEXCOORD5;

//     #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
//         half3 viewDirTS : TEXCOORD8;
//     #endif

//     float4 positionCS : SV_POSITION;
//     UNITY_VERTEX_INPUT_INSTANCE_ID
//     UNITY_VERTEX_OUTPUT_STEREO
// };



// void PlantWindAnimator(Attributes input, inout float3 positionOS, inout half3 normalOS)
// {
//     #if defined(ENABLE_WIND)
//         if (_WindEnabled > 0)
//         {
//             float3 rotatedWindVector = mul(_ST_WindVector.xyz, (float3x3)unity_ObjectToWorld);
//             float windLength = length(rotatedWindVector);
//             if (windLength < 1e-5)
//             {
//                 // sanity check that wind data is available
//                 return;
//             }
//             rotatedWindVector /= windLength;

//             float3 treePos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
//             float3 windyPosition = positionOS;

//             #ifndef EFFECT_BILLBOARD
//                 // geometry type
//                 float geometryType = (int) (input.texcoord3.w + 0.25);
//                 bool leafTwo = false;
//                 if (geometryType > GEOM_TYPE_FACINGLEAF)
//                 {
//                     geometryType -= 2;
//                     leafTwo = true;
//                 }

//                 // leaves
//                 if (geometryType > GEOM_TYPE_FROND)
//                 {
//                     // remove anchor position
//                     float3 anchor = float3(input.texcoord1.zw, input.texcoord2.w);//
//                     windyPosition -= anchor;

//                     if (geometryType == GEOM_TYPE_FACINGLEAF)
//                     {
//                         // face camera-facing leaf to camera
//                         float offsetLen = length(windyPosition);
//                         windyPosition = mul(windyPosition.xyz, (float3x3)UNITY_MATRIX_IT_MV); // inv(MV) * windyPosition
//                         windyPosition = normalize(windyPosition) * offsetLen; // make sure the offset vector is still scaled

//                     }

//                     // leaf wind
//                     #if defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST)
//                         #ifdef _WINDQUALITY_BEST
//                             bool bBestWind = true;
//                         #else
//                             bool bBestWind = false;
//                         #endif
//                         float leafWindTrigOffset = anchor.x + anchor.y;
//                         windyPosition = LeafWind(bBestWind, leafTwo, windyPosition, normalOS, input.texcoord3.x, float3(0, 0, 0), input.texcoord3.y, input.texcoord3.z, leafWindTrigOffset, rotatedWindVector);
//                     #endif

//                     // move back out to anchor
//                     windyPosition += anchor;
//                 }

//                 // frond wind
//                 bool bPalmWind = false;
//                 #ifdef _WINDQUALITY_PALM
//                     bPalmWind = true;
//                     if (geometryType == GEOM_TYPE_FROND)
//                     {
//                         windyPosition = RippleFrond(windyPosition, normalOS, input.texcoord.x, input.texcoord.y, input.texcoord3.x, input.texcoord3.y, input.texcoord3.z);
//                     }
//                 #endif

//                 // branch wind (applies to all 3D geometry)
//                 #if defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
//                     float3 rotatedBranchAnchor = normalize(mul(_ST_WindBranchAnchor.xyz, (float3x3)unity_ObjectToWorld)) * _ST_WindBranchAnchor.w;
//                     windyPosition = BranchWind(bPalmWind, windyPosition, treePos, float4(input.texcoord.zw, 0, 0), rotatedWindVector, rotatedBranchAnchor);
//                 #endif

//             #endif // !EFFECT_BILLBOARD

//             // global wind
//             float globalWindTime = _ST_WindGlobal.x;
//             #if defined(EFFECT_BILLBOARD) && defined(UNITY_INSTANCING_ENABLED)
//                 #if defined(DISABLE_SRP_BATCHING)
//                     globalWindTime += UNITY_ACCESS_INSTANCED_PROP(STWind, _GlobalWindTime);
//                 #else
//                     globalWindTime += _GlobalWindTime;
//                 #endif
//             #endif

//             windyPosition = GlobalWind(windyPosition, treePos, true, rotatedWindVector, globalWindTime);
//             // input.vertex.xyz = windyPosition;
//             positionOS = windyPosition;
//         }
//     #endif
// }


// Varyings DepthNormalsVertex(Attributes input)
// {
//     Varyings output = (Varyings)0;
//     UNITY_SETUP_INSTANCE_ID(input);
//     UNITY_TRANSFER_INSTANCE_ID(input, output);
//     UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//     #if defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT)
//         PlantWindAnimator(input, input.positionOS.xyz, input.normalOS);
//     #endif

//     output.uv = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);
//     output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

//     VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
//     VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);

//     half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
//     output.normalWS.xyz = half3(normalInput.normalWS);
//     #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
//         float sign = input.tangentOS.w * float(GetOddNegativeScale());
//         half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
//     #endif

//     #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
//         output.tangentWS = tangentWS;
//     #endif

//     #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
//         half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
//         output.viewDirTS = viewDirTS;
//     #endif

//     return output;
// }

// half4 DepthNormalsFragment(Varyings input, bool vFace : SV_IsFrontFace) : SV_TARGET
// {
//     UNITY_SETUP_INSTANCE_ID(input);
//     UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
//     UnityTexture2D DetailAlbedoMap = UnityBuildTexture2DStructOtherSampler(_DetailAlbedoMap, sampler_DetailAlbedoMap);
//     UnityTexture2D DetailNormalMap = UnityBuildTexture2DStructOtherSampler(_DetailNormalMap, sampler_DetailNormalMap);
//     UnityTexture2D DetailAlbedoMap2 = UnityBuildTexture2DStructOtherSampler(_DetailAlbedoMap2, sampler_DetailAlbedoMap2);
//     UnityTexture2D DetailNormalMap2 = UnityBuildTexture2DStructOtherSampler(_DetailNormalMap2, sampler_DetailNormalMap2);

//     UnityTexture2D bumpMap = UnityBuildTexture2DStruct(_BumpMap);
//     UnityTexture2D BaseMap = UnityBuildTexture2DStruct(_BaseMap);
//     UnityTexture2D HeightMap = UnityBuildTexture2DStruct(_HeightMap);

//     //alphaTest
//     #if defined(_ALPHATEST_ON)
//         #if defined(EFFECT_BILLBOARD)
//             clip(BaseMap.SampleSupportNoTileing(input.uv.xy, _ScaleOrRotate).a * _BaseColor.a * input.uv.z - 0.3333);
//         #else
//             clip(BaseMap.SampleSupportNoTileing(input.uv.xy, _ScaleOrRotate).a * _BaseColor.a - _Cutoff);
//         #endif
//         //我们只在开启alphaTest时,启用Lod淡入淡出,不要给普通的不透明物体使用LODDithering,以免造成性能损失
//         //支持Lod交叉淡入淡出,需要统一LODGroup的淡入淡出设置,以免出现不同的着色器变体,破坏SRP批处理
//         // #if !defined(SHADER_QUALITY_LOW)
//         //     #if defined(LOD_FADE_CROSSFADE)
//         //         LODDitheringTransition(ComputeFadeMaskSeed(input.viewDirWS, GetNormalizedScreenSpaceUV(input.positionCS.xy)), unity_LODFade.x);
//         //     #endif
//         // #endif
//     #endif

//     //UV,视差
//     float2 baseUV = input.uv.xy;
//     #if defined(_PARALLAXMAP)
//         #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
//             baseUV += ParallaxOffset1Step(HeightMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).r, _Parallax, input.viewDirTS);
//         #else
//             half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
//             half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
//             baseUV += ParallaxOffset1Step(HeightMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).r, _Parallax, viewDirTS);
//         #endif
//     #endif

//     float sgn = input.tangentWS.w;      // should be either +1 or -1
//     float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
//     float3 normalTS = bumpMap.SampleNormalSupportNoTileing(baseUV, _ScaleOrRotate, _BumpScale);

//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         half DetailMask = DetailAlbedoMap.Sample(uv).a;
//         float2 detailUV = DetailAlbedoMap.GetTransformedUV(uv);
//         half3 DetailNormalTS = DetailNormalMap.SampleNormal(detailUV, _DetailNormalMapScale);
//         normalTS = ApplyDetailNormal_modif (DetailNormalTS, normalTS, DetailMask);
//         #if defined(_DETAIL_2MULTI)
//             half DetailMask2 = DetailAlbedoMap2.Sample(uv).a;
//             float2 detailUV2 = DetailAlbedoMap2.GetTransformedUV(uv);
//             half3 DetailNormalTS2 = DetailNormalMap2.SampleNormal(detailUV2, _DetailNormalMapScale2);
//             normalTS = ApplyDetailNormal_modif (DetailNormalTS2, normalTS, DetailMask2);
//         #endif
//     #endif

//     //背面翻转法线
//     #ifdef EFFECT_BACKSIDE_NORMALS
//         normalTS.z = IS_FRONT_VFACE(vFace, normalTS.z, -normalTS.z);
//     #endif

//     float3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));

//     //贴花
//     // #ifdef _DBUFFER
//     //     half3 albedo = 0;half3 specularColor = 0;half metallic = 0;half occlusion = 1.0;half perceptualSmoothness = 0.0;
//     //     ApplyDecal(input.positionCS, albedo, specularColor, normalWS,
//     //     metallic, occlusion, perceptualSmoothness);
//     // #endif

//     return half4(NormalizeNormalPerPixel(normalWS), 0.0);
// }



// //-------------------------------------------------
// struct SpeedTreeVertexInput
// {
//     float4 vertex : POSITION;
//     float3 normal : NORMAL;
//     float4 tangent : TANGENT;
//     float4 texcoord : TEXCOORD0;
//     float4 texcoord1 : TEXCOORD1;
//     float4 texcoord2 : TEXCOORD2;
//     float4 texcoord3 : TEXCOORD3;
//     // float4 color : COLOR;

//     UNITY_VERTEX_INPUT_INSTANCE_ID
// };
// struct SpeedTreeVertexDepthNormalOutput
// {
//     half2 uv : TEXCOORD0;
//     // half4 color : TEXCOORD1;

//     // #ifdef EFFECT_BUMP
//     half4 normalWS : TEXCOORD2;    // xyz: normal, w: viewDir.x
//     half4 tangentWS : TEXCOORD3;    // xyz: tangent, w: viewDir.y
//     half4 bitangentWS : TEXCOORD4;    // xyz: bitangent, w: viewDir.z
//     // #else
//     //     half3 normalWS : TEXCOORD2;
//     //     half3 viewDirWS : TEXCOORD3;
//     // #endif

//     float4 clipPos : SV_POSITION;
//     UNITY_VERTEX_INPUT_INSTANCE_ID
//     UNITY_VERTEX_OUTPUT_STEREO
// };
// struct SpeedTreeDepthNormalFragmentInput
// {
//     SpeedTreeVertexDepthNormalOutput interpolated;
//     #ifdef EFFECT_BACKSIDE_NORMALS
//         FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC;
//     #endif
// };

// void InitializeData(inout SpeedTreeVertexInput input, float lodValue)
// {
//     // wind
//     #if defined(ENABLE_WIND) && !defined(_WINDQUALITY_NONE)
//         if (_WindEnabled > 0)
//         {
//             float3 rotatedWindVector = mul(_ST_WindVector.xyz, (float3x3)unity_ObjectToWorld);
//             float windLength = length(rotatedWindVector);
//             if (windLength < 1e-5)
//             {
//                 // sanity check that wind data is available
//                 return;
//             }
//             rotatedWindVector /= windLength;

//             float3 treePos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
//             float3 windyPosition = input.vertex.xyz;

//             #ifndef EFFECT_BILLBOARD
//                 // geometry type
//                 float geometryType = (int) (input.texcoord3.w + 0.25);
//                 bool leafTwo = false;
//                 if (geometryType > GEOM_TYPE_FACINGLEAF)
//                 {
//                     geometryType -= 2;
//                     leafTwo = true;
//                 }

//                 // leaves
//                 if (geometryType > GEOM_TYPE_FROND)
//                 {
//                     // remove anchor position
//                     float3 anchor = float3(input.texcoord1.zw, input.texcoord2.w);
//                     windyPosition -= anchor;

//                     if (geometryType == GEOM_TYPE_FACINGLEAF)
//                     {
//                         // face camera-facing leaf to camera
//                         float offsetLen = length(windyPosition);
//                         windyPosition = mul(windyPosition.xyz, (float3x3)UNITY_MATRIX_IT_MV); // inv(MV) * windyPosition
//                         windyPosition = normalize(windyPosition) * offsetLen; // make sure the offset vector is still scaled

//                     }

//                     // leaf wind
//                     #if defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST)
//                         #ifdef _WINDQUALITY_BEST
//                             bool bBestWind = true;
//                         #else
//                             bool bBestWind = false;
//                         #endif
//                         float leafWindTrigOffset = anchor.x + anchor.y;
//                         windyPosition = LeafWind(bBestWind, leafTwo, windyPosition, input.normal, input.texcoord3.x, float3(0, 0, 0), input.texcoord3.y, input.texcoord3.z, leafWindTrigOffset, rotatedWindVector);
//                     #endif

//                     // move back out to anchor
//                     windyPosition += anchor;
//                 }

//                 // frond wind
//                 bool bPalmWind = false;
//                 #ifdef _WINDQUALITY_PALM
//                     bPalmWind = true;
//                     if (geometryType == GEOM_TYPE_FROND)
//                     {
//                         windyPosition = RippleFrond(windyPosition, input.normal, input.texcoord.x, input.texcoord.y, input.texcoord3.x, input.texcoord3.y, input.texcoord3.z);
//                     }
//                 #endif

//                 // branch wind (applies to all 3D geometry)
//                 #if defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
//                     float3 rotatedBranchAnchor = normalize(mul(_ST_WindBranchAnchor.xyz, (float3x3)unity_ObjectToWorld)) * _ST_WindBranchAnchor.w;
//                     windyPosition = BranchWind(bPalmWind, windyPosition, treePos, float4(input.texcoord.zw, 0, 0), rotatedWindVector, rotatedBranchAnchor);
//                 #endif

//             #endif // !EFFECT_BILLBOARD

//             // global wind
//             float globalWindTime = _ST_WindGlobal.x;
//             #if defined(EFFECT_BILLBOARD) && defined(UNITY_INSTANCING_ENABLED)
//                 #if defined(DISABLE_SRP_BATCHING)
//                     globalWindTime += UNITY_ACCESS_INSTANCED_PROP(STWind, _GlobalWindTime);
//                 #else
//                     globalWindTime += _GlobalWindTime;
//                 #endif
//             #endif

//             windyPosition = GlobalWind(windyPosition, treePos, true, rotatedWindVector, globalWindTime);
//             input.vertex.xyz = windyPosition;
//         }
//     #endif

//     #if defined(EFFECT_BILLBOARD)
//         float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
//         // crossfade faces
//         bool topDown = (input.texcoord.z > 0.5);
//         float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
//         float3 cameraDir = normalize(mul((float3x3)UNITY_MATRIX_M, _WorldSpaceCameraPos - treePos));
//         float viewDot = max(dot(viewDir, input.normal), dot(cameraDir, input.normal));
//         viewDot *= viewDot;
//         viewDot *= viewDot;
//         viewDot += topDown ? 0.38 : 0.18; // different scales for horz and vert billboards to fix transition zone

//         // if invisible, avoid overdraw
//         if (viewDot < 0.3333)
//         {
//             input.vertex.xyz = float3(0, 0, 0);
//         }

//         // input.color = float4(1, 1, 1, clamp(viewDot, 0, 1));

//         // adjust lighting on billboards to prevent seams between the different faces
//         if (topDown)
//         {
//             input.normal += cameraDir;
//         }
//         else
//         {
//             half3 binormal = cross(input.normal, input.tangent.xyz) * input.tangent.w;
//             float3 right = cross(cameraDir, binormal);
//             input.normal = cross(binormal, right);
//         }
//         input.normal = normalize(input.normal);
//     #endif
// }

// SpeedTreeVertexDepthNormalOutput SpeedTree8VertDepthNormal(SpeedTreeVertexInput input)
// {
//     SpeedTreeVertexDepthNormalOutput output = (SpeedTreeVertexDepthNormalOutput)0;
//     UNITY_SETUP_INSTANCE_ID(input);
//     UNITY_TRANSFER_INSTANCE_ID(input, output);
//     UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//     // handle speedtree wind and lod
//     InitializeData(input, unity_LODFade.x);

//     output.uv = input.texcoord.xy;
//     // output.color = input.color;


//     VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
//     half3 normalWS = TransformObjectToWorldNormal(input.normal);
//     half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
//     // #ifdef EFFECT_BUMP
//     real sign = input.tangent.w * GetOddNegativeScale();
//     output.normalWS.xyz = normalWS;
//     output.tangentWS.xyz = TransformObjectToWorldDir(input.tangent.xyz);
//     output.bitangentWS.xyz = cross(output.normalWS.xyz, output.tangentWS.xyz) * sign;

//     // View dir packed in w.
//     output.normalWS.w = viewDirWS.x;
//     output.tangentWS.w = viewDirWS.y;
//     output.bitangentWS.w = viewDirWS.z;
//     // #else
//     //     output.normalWS = normalWS;
//     //     output.viewDirWS = viewDirWS;
//     // #endif

//     output.clipPos = vertexInput.positionCS;
//     return output;
// }
// half4 SpeedTree8FragDepthNormal(SpeedTreeDepthNormalFragmentInput input) : SV_Target
// {
//     UNITY_SETUP_INSTANCE_ID(input.interpolated);
//     UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input.interpolated);

//     // #if !defined(SHADER_QUALITY_LOW)
//     //     #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
//     //         // #ifdef EFFECT_BUMP
//     //         half3 viewDirectionWS = half3(input.interpolated.normalWS.w, input.interpolated.tangentWS.w, input.interpolated.bitangentWS.w);
//     //         // #else
//     //         //     half3 viewDirectionWS = input.interpolated.viewDirWS;
//     //         // #endif
//     //         LODDitheringTransition(ComputeFadeMaskSeed(viewDirectionWS, input.interpolated.clipPos.xy), unity_LODFade.x);
//     //     #endif
//     // #endif

//     half2 uv = input.interpolated.uv;

//     // half4 diffuse = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * _Color;
//     // half alpha = diffuse.a * input.interpolated.color.a;
//     // AlphaDiscard(alpha, 0.3333);

//     Alpha(SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);


//     // normal
//     // #if defined(EFFECT_BUMP)
//     half3 normalTs = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
//     // #else
//     //     half3 normalTs = half3(0, 0, 1);
//     // #endif

//     // flip normal on backsides
//     // #ifdef EFFECT_BACKSIDE_NORMALS
//     //     if (input.facing < 0.5)
//     //     {
//     //         normalTs.z = -normalTs.z;
//     //     }
//     // #endif
//     #ifdef EFFECT_BACKSIDE_NORMALS
//         normalTs.z = input.facing ? normalTs.z : - normalTs.z;
//     #endif

//     // adjust billboard normals to improve GI and matching
//     #if defined(EFFECT_BILLBOARD)
//         normalTs.z *= 0.5;
//         normalTs = normalize(normalTs);
//     #endif

//     // #if defined(EFFECT_BUMP)
//     float3 normalWS = TransformTangentToWorld(normalTs, half3x3(input.interpolated.tangentWS.xyz, input.interpolated.bitangentWS.xyz, input.interpolated.normalWS.xyz));
//     return half4(NormalizeNormalPerPixel(normalWS), 0.0h);
//     // #else
//     //     return half4(NormalizeNormalPerPixel(input.interpolated.normalWS), 0.0h);
//     // #endif

// }


// #endif


//------------------------------------------------------------------------------------------------

#ifndef MAIN_PASS
#define MAIN_PASS

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 texcoord : TEXCOORD0;//UV0
    #if  defined(LIGHTMAP_ON) || (defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT))
        float4 texcoord1 : TEXCOORD1;//UV1
    #endif
    #if defined(DYNAMICLIGHTMAP_ON) || (defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT))
        float4 texcoord2 : TEXCOORD2;//UV2
    #endif

    #if defined(REQUIRE_TEXCOORD3) || (defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT))
        float4 texcoord3 : TEXCOORD3;//UV3
    #endif

    #ifdef REQUIRE_VERTEXCOLOR
        float4 vertexColor : COLOR;
    #endif
    #ifdef REQUIRE_VERTEXID
        uint vertexID : SV_VERTEXID;//顶点ID
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID//实例化相关宏,无脑添加

};
struct Varyings
{
    #ifdef REQUIRE_TEXCOORD3
        float4 texcoord : TEXCOORD0;
    #else
        float2 texcoord : TEXCOORD0;
    #endif
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    half4 tangentWS : TEXCOORD3;    // xyz: tangent, w: sign
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) || defined(_ADDITIONAL_LIGHTS_VERTEX)
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
        #else
            half fogFactor : TEXCOORD5;
        #endif
    #endif

    #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
        float4 shadowCoord : TEXCOORD6;
    #endif
    #if defined(REQUIRE_SH) || defined(LIGHTMAP_ON)
        DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
    #endif
    #ifdef DYNAMICLIGHTMAP_ON
        float2 dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
    #endif
    #ifdef REQUIRE_VERTEXCOLOR
        float4 vertexColor : TEXCOORD9;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirTS : TEXCOORD10;
    #endif

    #if defined(EFFECT_BILLBOARD) || defined(EFFECT_HUE_VARIATION)
        half4 interpolator : TEXCOORD11;//注意优化,建议完成后将其拆分放入positionWS,normalWS的w通道
    #endif

    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "../OpaqueObject/VertexAndFragmentFunction.hlsl"

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    //如若计算顶点动画,在此处对input.positionOS,input.normalOS,input.tangentOS计算修改即可
    //------------------------------------------
    VertexAnimator(input, input.positionOS.xyz, input.normalOS, input.tangentOS);
    //------------------------------------------

    //公告牌
    float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
    #if defined(EFFECT_BILLBOARD)
        // crossfade faces
        bool topDown = (input.texcoord.z > 0.5);
        float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
        float3 cameraDir = normalize(mul((float3x3)UNITY_MATRIX_M, _WorldSpaceCameraPos - treePos));
        float viewDot = max(dot(viewDir, input.normalOS), dot(cameraDir, input.normalOS));
        viewDot *= viewDot;
        viewDot *= viewDot;
        viewDot += topDown ? 0.38 : 0.18; // different scales for horz and vert billboards to fix transition zone

        // if invisible, avoid overdraw
        if (viewDot < 0.3333)
        {
            input.positionOS.xyz = float3(0, 0, 0);
        }
        output.interpolator.a = clamp(viewDot, 0, 1);
        // adjust lighting on billboards to prevent seams between the different faces
        if (topDown)
        {
            input.normalOS += cameraDir;
        }
        else
        {
            half3 binormal = cross(input.normalOS, input.tangentOS.xyz) * input.tangentOS.w;
            float3 right = cross(cameraDir, binormal);
            input.normalOS = cross(binormal, right);
        }
        input.normalOS = normalize(input.normalOS);
    #endif

    //色调变体相关
    #ifdef EFFECT_HUE_VARIATION
        float hueVariationAmount = frac(treePos.x + treePos.y + treePos.z);
        output.interpolator.g = saturate(hueVariationAmount * _HueVariationColor.a);
    #endif

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.positionWS = vertexInput.positionWS;
    output.normalWS = normalInput.normalWS;
    output.tangentWS = half4(normalInput.tangentWS.xyz, input.tangentOS.w * GetOddNegativeScale());
    output.texcoord.xy = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);
    #ifdef REQUIRE_TEXCOORD3
        output.texcoord.zw = input.texcoord3.xy;
    #endif
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) || defined(_ADDITIONAL_LIGHTS_VERTEX)
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            #else
                output.fogFactorAndVertexLight = half4(0, vertexLight);
            #endif
        #else
            output.fogFactor = fogFactor;
        #endif
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    #if defined(LIGHTMAP_ON) || defined(REQUIRE_SH)
        OUTPUT_LIGHTMAP_UV(input.texcoord1.xy, unity_LightmapST, output.staticLightmapUV);
        OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    #endif
    #ifdef DYNAMICLIGHTMAP_ON
        output.dynamicLightmapUV = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
        // output.dynamicLightmapUV = output.staticLightmapUV;
    #endif
    #ifdef REQUIRE_VERTEXCOLOR
        output.vertexColor = input.vertexColor;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
        half3 viewDirTS = GetViewDirectionTangentSpace(output.tangentWS, output.normalWS, viewDirWS);
        output.viewDirTS = viewDirTS;
    #endif
    output.positionCS = vertexInput.positionCS;
    return output;
}
//SAMPLE_GI
half4 frag(Varyings input, bool vFace : SV_IsFrontFace) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    //为避免干扰,顶点动画和片元着色器代码请在VertexAndFragmentFunction.hlsl文件中的VertexAnimator/FragmentShading函数中书写
    // return input.interpolator.aaaa;
    // return vFace.xxxx;
    return FragmentShading(input, vFace);
}



#endif