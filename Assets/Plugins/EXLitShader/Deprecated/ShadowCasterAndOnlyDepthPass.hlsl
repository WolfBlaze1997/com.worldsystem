#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "../FunctionLibrary/SpeedTreeWind.hlsl"

#define Varyings debugVaryings
#include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
#undef Varyings

#if defined(_LIGHTINGMODEL_PLANT)
    #if defined(_WINDQUALITY_FASTEST) || defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
        #define ENABLE_WIND
    #endif
#endif

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
#define GEOM_TYPE_BRANCH 0
#define GEOM_TYPE_FROND 1
#define GEOM_TYPE_LEAF 2
#define GEOM_TYPE_FACINGLEAF 3
float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 texcoord : TEXCOORD0;
    #if defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT)
        float4 texcoord1 : TEXCOORD1;
        float4 texcoord2 : TEXCOORD2;
        float4 texcoord3 : TEXCOORD3;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uv : TEXCOORD0;
    // half3 viewDirWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

//支持shadow偏移
float4 GetShadowPositionHClipSupportShadowBias(float3 positionOS, half3 normalOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float3 normalWS = TransformObjectToWorldNormal(normalOS);

    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif

    return positionCS;
}

void PlantWindAnimator(Attributes input, inout float3 positionOS, inout half3 normalOS)
{
    #if defined(ENABLE_WIND)
        if (_WindEnabled > 0)
        {
            float3 rotatedWindVector = mul(_ST_WindVector.xyz, (float3x3)unity_ObjectToWorld);
            float windLength = length(rotatedWindVector);
            if (windLength < 1e-5)
            {
                // sanity check that wind data is available
                return;
            }
            rotatedWindVector /= windLength;

            float3 treePos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
            float3 windyPosition = positionOS;

            #ifndef EFFECT_BILLBOARD
                // geometry type
                float geometryType = (int) (input.texcoord3.w + 0.25);
                bool leafTwo = false;
                if (geometryType > GEOM_TYPE_FACINGLEAF)
                {
                    geometryType -= 2;
                    leafTwo = true;
                }

                // leaves
                if (geometryType > GEOM_TYPE_FROND)
                {
                    // remove anchor position
                    float3 anchor = float3(input.texcoord1.zw, input.texcoord2.w);//
                    windyPosition -= anchor;

                    if (geometryType == GEOM_TYPE_FACINGLEAF)
                    {
                        // face camera-facing leaf to camera
                        float offsetLen = length(windyPosition);
                        windyPosition = mul(windyPosition.xyz, (float3x3)UNITY_MATRIX_IT_MV); // inv(MV) * windyPosition
                        windyPosition = normalize(windyPosition) * offsetLen; // make sure the offset vector is still scaled

                    }

                    // leaf wind
                    #if defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST)
                        #ifdef _WINDQUALITY_BEST
                            bool bBestWind = true;
                        #else
                            bool bBestWind = false;
                        #endif
                        float leafWindTrigOffset = anchor.x + anchor.y;
                        windyPosition = LeafWind(bBestWind, leafTwo, windyPosition, normalOS, input.texcoord3.x, float3(0, 0, 0), input.texcoord3.y, input.texcoord3.z, leafWindTrigOffset, rotatedWindVector);
                    #endif

                    // move back out to anchor
                    windyPosition += anchor;
                }

                // frond wind
                bool bPalmWind = false;
                #ifdef _WINDQUALITY_PALM
                    bPalmWind = true;
                    if (geometryType == GEOM_TYPE_FROND)
                    {
                        windyPosition = RippleFrond(windyPosition, normalOS, input.texcoord.x, input.texcoord.y, input.texcoord3.x, input.texcoord3.y, input.texcoord3.z);
                    }
                #endif

                // branch wind (applies to all 3D geometry)
                #if defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
                    float3 rotatedBranchAnchor = normalize(mul(_ST_WindBranchAnchor.xyz, (float3x3)unity_ObjectToWorld)) * _ST_WindBranchAnchor.w;
                    windyPosition = BranchWind(bPalmWind, windyPosition, treePos, float4(input.texcoord.zw, 0, 0), rotatedWindVector, rotatedBranchAnchor);
                #endif

            #endif // !EFFECT_BILLBOARD

            // global wind
            float globalWindTime = _ST_WindGlobal.x;
            #if defined(EFFECT_BILLBOARD) && defined(UNITY_INSTANCING_ENABLED)
                #if defined(DISABLE_SRP_BATCHING)
                    globalWindTime += UNITY_ACCESS_INSTANCED_PROP(STWind, _GlobalWindTime);
                #else
                    globalWindTime += _GlobalWindTime;
                #endif
            #endif

            windyPosition = GlobalWind(windyPosition, treePos, true, rotatedWindVector, globalWindTime);
            // input.vertex.xyz = windyPosition;
            positionOS = windyPosition;
        }
    #endif
}

Varyings ShadowPassAndOnlyDepthVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.uv.xy = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);

    #if defined(ENABLE_WIND) && defined(_LIGHTINGMODEL_PLANT)
        PlantWindAnimator(input, input.positionOS.xyz, input.normalOS);
    #endif

    //公告牌
    #if defined(EFFECT_BILLBOARD)
        BillboardLod(input.texcoord.z, input.normalOS, input.tangentOS, input.positionOS.xyz, output.uv.z);
    #endif

    // output.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));

    #if defined(SHADOWCASTER_PASS)
        output.positionCS = GetShadowPositionHClipSupportShadowBias(input.positionOS.xyz, input.normalOS);
    #else
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    #endif

    return output;
}

half4 ShadowPassAndOnlyDepthFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    UnityTexture2D BaseMap = UnityBuildTexture2DStruct(_BaseMap);

    //alphaTest
    #if defined(_ALPHATEST_ON)
        #if defined(EFFECT_BILLBOARD)
            clip(BaseMap.SampleSupportNoTileing(input.uv.xy, _ScaleOrRotate).a * _BaseColor.a * input.uv.z - 0.3333);
        #else
            clip(BaseMap.SampleSupportNoTileing(input.uv.xy, _ScaleOrRotate).a * _BaseColor.a - _Cutoff);
        #endif
        //我们只在开启alphaTest时,启用Lod淡入淡出,不要给普通的不透明物体使用LODDithering,以免造成性能损失
        //支持Lod交叉淡入淡出,需要统一LODGroup的淡入淡出设置,以免出现不同的着色器变体,破坏SRP批处理
        // #if !defined(SHADER_QUALITY_LOW)
        //     #if defined(LOD_FADE_CROSSFADE)
        //         LODDitheringTransition(ComputeFadeMaskSeed(input.viewDirWS, GetNormalizedScreenSpaceUV(input.positionCS.xy)), unity_LODFade.x);
        //     #endif
        // #endif
    #endif


    return 0;
}

#endif
