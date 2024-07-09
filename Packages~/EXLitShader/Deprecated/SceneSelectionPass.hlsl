#ifndef SCENESELECTION_PASS
#define SCENESELECTION_PASS
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "../FunctionLibrary/SpeedTreeUtility.hlsl"
#include "../FunctionLibrary/SpeedTreeWind.hlsl"

struct SpeedTreeVertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float4 texcoord2 : TEXCOORD2;
    float4 texcoord3 : TEXCOORD3;
    // float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct SpeedTreeVertexDepthOutput
{
    half2 uv : TEXCOORD0;
    // half4 color : TEXCOORD1;
    // half3 viewDirWS : TEXCOORD2;
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeData(inout SpeedTreeVertexInput input, float lodValue)
{
    // #if defined(LOD_FADE_PERCENTAGE) && (!defined(LOD_FADE_CROSSFADE) && !defined(EFFECT_BILLBOARD))
    //     input.vertex.xyz = lerp(input.vertex.xyz, input.texcoord2.xyz, lodValue);
    // #endif

    // wind
    #if defined(ENABLE_WIND) && !defined(_WINDQUALITY_NONE)
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
            float3 windyPosition = input.vertex.xyz;

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
                    float3 anchor = float3(input.texcoord1.zw, input.texcoord2.w);
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
                        windyPosition = LeafWind(bBestWind, leafTwo, windyPosition, input.normal, input.texcoord3.x, float3(0, 0, 0), input.texcoord3.y, input.texcoord3.z, leafWindTrigOffset, rotatedWindVector);
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
                        windyPosition = RippleFrond(windyPosition, input.normal, input.texcoord.x, input.texcoord.y, input.texcoord3.x, input.texcoord3.y, input.texcoord3.z);
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
            input.vertex.xyz = windyPosition;
        }
    #endif

    #if defined(EFFECT_BILLBOARD)
        float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
        // crossfade faces
        bool topDown = (input.texcoord.z > 0.5);
        float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
        float3 cameraDir = normalize(mul((float3x3)UNITY_MATRIX_M, _WorldSpaceCameraPos - treePos));
        float viewDot = max(dot(viewDir, input.normal), dot(cameraDir, input.normal));
        viewDot *= viewDot;
        viewDot *= viewDot;
        viewDot += topDown ? 0.38 : 0.18; // different scales for horz and vert billboards to fix transition zone

        // if invisible, avoid overdraw
        if (viewDot < 0.3333)
        {
            input.vertex.xyz = float3(0, 0, 0);
        }

        // input.color = float4(1, 1, 1, clamp(viewDot, 0, 1));

        // adjust lighting on billboards to prevent seams between the different faces
        if (topDown)
        {
            input.normal += cameraDir;
        }
        else
        {
            half3 binormal = cross(input.normal, input.tangent.xyz) * input.tangent.w;
            float3 right = cross(cameraDir, binormal);
            input.normal = cross(binormal, right);
        }
        input.normal = normalize(input.normal);
    #endif
}

SpeedTreeVertexDepthOutput SpeedTree8VertDepth(SpeedTreeVertexInput input)
{
    SpeedTreeVertexDepthOutput output = (SpeedTreeVertexDepthOutput)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // handle speedtree wind and lod
    InitializeData(input, unity_LODFade.x);
    output.uv = input.texcoord.xy;
    output.color = input.color;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

    // output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);

    #ifdef SHADOW_CASTER
        half3 normalWS = TransformObjectToWorldNormal(input.normal);

        #if _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 lightDirectionWS = normalize(_LightPosition - vertexInput.positionWS);
        #else
            float3 lightDirectionWS = _LightDirection;
        #endif

        float4 positionCS = TransformWorldToHClip(ApplyShadowBias(vertexInput.positionWS, normalWS, lightDirectionWS));
        output.clipPos = positionCS;
    #else
        output.clipPos = vertexInput.positionCS;
    #endif

    return output;
}

half4 SpeedTree8FragDepth(SpeedTreeVertexDepthOutput input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // #if !defined(SHADER_QUALITY_LOW)
    //     #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
    //         LODDitheringTransition(ComputeFadeMaskSeed(input.viewDirWS, input.clipPos.xy), unity_LODFade.x);
    //     #endif
    // #endif
    // half2 uv = input.uv;
    // half4 diffuse = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * _BaseColor;
    // half alpha = diffuse.a * input.color.a;

    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return half4(_ObjectId, _PassValue, 1.0, 1.0);
}


#endif