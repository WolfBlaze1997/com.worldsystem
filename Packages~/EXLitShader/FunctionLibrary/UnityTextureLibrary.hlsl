#ifndef UNITY_TEXTURE_INCLUDED
#define UNITY_TEXTURE_INCLUDED
//这个与URP内置的"texture.hlsl"相同,我这里做了外置进行了扩展
// #include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"

// 纹理相关函数
void TriangleGrid(out half w1, out half w2, out half w3, out int2 vertex1, out int2 vertex2, out int2 vertex3, half2 uv)
{
    // 缩放uv
    uv *= 2 * sqrt(3);
    // 变形
    const half2x2 gridToSkewedGrid = half2x2(1.0, -0.57735027, 0.0, 1.15470054);
    half2 skewedCoord = mul(gridToSkewedGrid, uv);
    int2 baseId = int2(floor(skewedCoord));
    half3 temp = half3(frac(skewedCoord), 0);
    temp.z = 1.0 - temp.x - temp.y;
    half s = step(0.0, -temp.z);
    half s2 = 2 * s - 1;
    w1 = -temp.z * s2;
    w2 = s - temp.y * s2;
    w3 = s - temp.x * s2;
    vertex1 = baseId + int2(s, s);
    vertex2 = baseId + int2(s, 1 - s);
    vertex3 = baseId + int2(1 - s, s);
}
half2x2 LoadRot2x2(int2 idx, half rotStrength)
{
    half angle = abs(idx.x * idx.y) + abs(idx.x + idx.y) + PI;
    angle = fmod(angle, 2 * PI);
    if (angle < 0) angle += 2 * PI;
    if (angle > PI) angle -= 2 * PI;
    angle *= rotStrength;
    half cs = cos(angle), si = sin(angle);
    return half2x2(cs, -si, si, cs);
}
half2 MakeCenST(int2 Vertex)
{
    half2x2 invSkewMat = half2x2(1.0, 0.5, 0.0, 1.0 / 1.15470054);
    return mul(invSkewMat, Vertex) / (2 * sqrt(3));
}
half2 hash(half2 p)
{
    half2 r = mul(half2x2(127.1, 311.7, 269.5, 183.3), p);
    
    return frac(sin(r) * 43758.5453);
}
half3 Pow_7(half3 x)
{
    return x * x * x * x * x * x * x;
}
half3 Gain3(half3 x, half r)
{
    half k = log(1 - r) / log(0.5);
    half3 s = 2 * step(0.5, x);
    half3 m = 2 * (1 - s);
    half3 res = 0.5 * s + 0.25 * m * pow(max(0.0, s + x * m), k);
    return res.xyz / (res.x + res.y + res.z);
}

//单采样去除重复感
float sum(float3 v)
{
    return v.x + v.y + v.z;
}
float4 hash4(float2 p)
{
    return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
    2.0 + dot(p, float2(11.0, 47.0)),
    3.0 + dot(p, float2(41.0, 29.0)),
    4.0 + dot(p, float2(23.0, 31.0)))) * 103.0);
}
float4 SampleTextureNoTiling_OneSample(TEXTURE2D(tex), SAMPLER(s), float2 uv, float2 screenUV)
{

    float DITHER_THRESHOLDS[16] = {
        1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
    };

    // float2 uv = TRANSFORM_TEX(UV, _MainTex);
    float2 suv = screenUV.xy * _ScreenParams.xy;
    int2 iuv = floor(uv);
    float2 fuv = frac(uv);

    // generate per-tile transform
    float4 ofa = hash4(iuv + int2(0, 0));
    float4 ofb = hash4(iuv + int2(1, 0));
    float4 ofc = hash4(iuv + int2(0, 1));
    float4 ofd = hash4(iuv + int2(1, 1));
    float2 ddxV = ddx(uv);
    float2 ddyV = ddy(uv);

    // transform per-tile uvs
    ofa.zw = sign(ofa.zw - 0.5);
    ofb.zw = sign(ofb.zw - 0.5);
    ofc.zw = sign(ofc.zw - 0.5);
    ofd.zw = sign(ofd.zw - 0.5);
    float2 b = smoothstep(0.25, 0.75, fuv);

    int index = (int(suv.x) % 4) * 4 + int(suv.y) % 4;
    float r = DITHER_THRESHOLDS[index];
    b = clamp(sign(b - r), 0.0, 1.0);
    float4 ofDither = lerp(lerp(ofa, ofb, b.x), lerp(ofc, ofd, b.x), b.y);
    ddxV *= ofDither.zw;
    ddyV *= ofDither.zw;
    uv = uv * ofDither.zw + ofDither.xy;
    float4 color = PLATFORM_SAMPLE_TEXTURE2D_GRAD(tex, s, uv, ddxV, ddyV);
    return color;//float4(_ScreenParams.x,_ScreenParams.y,0,1);

}

//------------------------------------------------------

#ifdef SHADER_API_GLES
    #define UNITY_BARE_SAMPLER(n) GLES2UnsupportedSamplerState n
#else
    #define UNITY_BARE_SAMPLER(n) SAMPLER(n)
#endif

struct GLES2UnsupportedSamplerState { };

UNITY_BARE_SAMPLER(default_sampler_Linear_Repeat);

struct UnitySamplerState
{
    UNITY_BARE_SAMPLER(samplerstate);
};

#ifdef SHADER_API_GLES
    #define UnityBuildSamplerStateStruct(n) UnityBuildSamplerStateStructInternal()
#else
    #define UnityBuildSamplerStateStruct(n) UnityBuildSamplerStateStructInternal(n)
#endif

UnitySamplerState UnityBuildSamplerStateStructInternal(SAMPLER(samplerstate))
{
    UnitySamplerState result;
    #ifndef SHADER_API_GLES
        result.samplerstate = samplerstate;
    #endif
    return result;
}

struct UnityTexture2D
{
    TEXTURE2D(tex);
    UNITY_BARE_SAMPLER(samplerstate);
    float4 texelSize;
    float4 scaleTranslate;

    // these functions allows users to convert code using Texture2D to UnityTexture2D by simply changing the type of the variable
    // the existing texture macros will call these functions, which will forward the call to the texture appropriately
    float4 Sample(UnitySamplerState s, float2 uv)
    {
        return SAMPLE_TEXTURE2D(tex, s.samplerstate, uv);
    }
    float4 SampleLevel(UnitySamplerState s, float2 uv, float lod)
    {
        return SAMPLE_TEXTURE2D_LOD(tex, s.samplerstate, uv, lod);
    }
    float4 SampleBias(UnitySamplerState s, float2 uv, float bias)
    {
        return SAMPLE_TEXTURE2D_BIAS(tex, s.samplerstate, uv, bias);
    }
    float4 SampleGrad(UnitySamplerState s, float2 uv, float2 dpdx, float2 dpdy)
    {
        return SAMPLE_TEXTURE2D_GRAD(tex, s.samplerstate, uv, dpdx, dpdy);
    }

    float2 GetTransformedUV(float2 uv)
    {
        return uv * scaleTranslate.xy + scaleTranslate.zw;
    }

    //我在这里做了一些扩展
    float4 Sample(float2 uv)
    {
        return SAMPLE_TEXTURE2D(tex, samplerstate, uv);
    }
    float4 Sample(TEXTURE2D(tex), SAMPLER(s), float2 uv)
    {
        return SAMPLE_TEXTURE2D(tex, s, uv);
    }
    float4 SampleGrad(TEXTURE2D(tex), SAMPLER(s), float2 uv)
    {
        return SAMPLE_TEXTURE2D_GRAD(tex, s, uv, ddx(uv), ddy(uv));
    }
    float4 SampleGrad(TEXTURE2D(tex), SAMPLER(s), float2 uv, float2 ddx, float2 ddy)
    {
        return SAMPLE_TEXTURE2D_GRAD(tex, s, uv, ddx, ddy);
    }
    half3 SampleNormal(float2 uv, half scale = half(1.0))
    {
        half4 n = SAMPLE_TEXTURE2D(tex, samplerstate, uv);
        #if BUMP_SCALE_NOT_SUPPORTED
            return UnpackNormal(n);
        #else
            return UnpackNormalScale(n, scale);
        #endif
    }
    half3 SampleNormal(SAMPLER(s), float2 uv, half scale = half(1.0))
    {
        half4 n = SAMPLE_TEXTURE2D(tex, s, uv);
        #if BUMP_SCALE_NOT_SUPPORTED
            return UnpackNormal(n);
        #else
            return UnpackNormalScale(n, scale);
        #endif
    }
    half3 DecodeNormalRG(float2 normalMapRG, half scale = half(1.0))
    {
        #if defined(UNITY_ASTC_NORMALMAP_ENCODING)
            half4 normaltex = half4(1, normalMapRG.y, 0, normalMapRG.x);
        #elif defined(UNITY_NO_DXT5nm)
            //通用rbg,未使用DXT5nm格式时使用
            half4 normaltex = half4(normalMapRG.x, normalMapRG.y, 1.0, 1.0);
        #else
            //DXT5nm (1, y, 0, x) or BC5 (x, y, 0, 1)
            half4 normaltex = half4(normalMapRG.x, normalMapRG.y, 0.0, 1.0);
        #endif
        return UnpackNormalScale(normaltex, scale);
    }

    //------------------------------------------------------------------------
    //支持消除平铺重复感
    float4 SampleSupportNoTileing(SAMPLER(s), float2 uv, float2 positionDSxy, half scaleOrRotate, half noise = 0.0)
    {
        #if defined(_SAMPLE_NOISETILING)
            // TEX = SampleNoiseTiling_half(tex, s, uv, noise);
            half k = noise; // cheap (cache friendly) lookup 采样一个noise
            half2 duvdx = ddx(uv);
            half2 duvdy = ddy(uv);
            half l = k * 8.0;
            half f = frac(l);
            half ia = floor(l); // my method
            half ib = ia + 1.0;
            // half2 offa = sin(half2(3.0, 7.0) * ia); // can replace with any other hash
            // half2 offb = sin(half2(3.0, 7.0) * ib); // can replace with any other hash
            half2 offa = abs(frac(half2(PI, 2 * PI) * ia) * 2 - 1.0); // 将消耗较高的超越函数sin优化为abs/frac,
            half2 offb = abs(frac(half2(PI, 2 * PI) * ib) * 2 - 1.0); // 将消耗较高的超越函数sin优化为abs/frac
            half4 cola = SAMPLE_TEXTURE2D_GRAD(tex, s, uv + offa, duvdx, duvdy);
            half4 colb = SAMPLE_TEXTURE2D_GRAD(tex, s, uv + offb, duvdx, duvdy);
            half3 colaSubcolb = cola.xyz - colb.xyz;
            return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * (colaSubcolb.x + colaSubcolb.y + colaSubcolb.z)));
        #elif defined(_SAMPLE_ONESAMPLEONTILING)
            float DITHER_THRESHOLDS[16] = {
                1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
            };
            float2 suv = positionDSxy.xy;
            int2 iuv = floor(uv);
            float2 fuv = frac(uv);

            // generate per-tile transform
            float4 ofa = hash4(iuv + int2(0, 0));
            float4 ofb = hash4(iuv + int2(1, 0));
            float4 ofc = hash4(iuv + int2(0, 1));
            float4 ofd = hash4(iuv + int2(1, 1));
            float2 ddxV = ddx(uv);
            float2 ddyV = ddy(uv);

            // transform per-tile uvs
            ofa.zw = sign(ofa.zw - 0.5);
            ofb.zw = sign(ofb.zw - 0.5);
            ofc.zw = sign(ofc.zw - 0.5);
            ofd.zw = sign(ofd.zw - 0.5);
            float2 b = smoothstep(0.25, 0.75, fuv);

            int index = (int(suv.x) % 4) * 4 + int(suv.y) % 4;
            float r = DITHER_THRESHOLDS[index];
            b = clamp(sign(b - r), 0.0, 1.0);
            float4 ofDither = lerp(lerp(ofa, ofb, b.x), lerp(ofc, ofd, b.x), b.y);
            ddxV *= ofDither.zw;
            ddyV *= ofDither.zw;
            uv = uv * ofDither.zw + ofDither.xy;
            return SAMPLE_TEXTURE2D_GRAD(tex, s, uv, ddxV, ddyV);
        #elif defined(_SAMPLE_HEXAGONTILING)
            // TEX = SampleHexagonTiling_half(tex, s, uv, scaleOrRotate);
            half2 dUVdx = ddx(uv), dUVdy = ddy(uv);
            half w1, w2, w3;
            int2 vertex1, vertex2, vertex3;
            TriangleGrid(w1, w2, w3, vertex1, vertex2, vertex3, uv);
            half2x2 rot1 = LoadRot2x2(vertex1, scaleOrRotate);
            half2x2 rot2 = LoadRot2x2(vertex2, scaleOrRotate);
            half2x2 rot3 = LoadRot2x2(vertex3, scaleOrRotate);
            half2 cen1 = MakeCenST(vertex1);
            half2 cen2 = MakeCenST(vertex2);
            half2 cen3 = MakeCenST(vertex3);
            half2 uv1 = mul(uv - cen1, rot1) + cen1 + hash(vertex1);
            half2 uv2 = mul(uv - cen2, rot2) + cen2 + hash(vertex2);
            half2 uv3 = mul(uv - cen3, rot3) + cen3 + hash(vertex3);
            half4 c1 = SAMPLE_TEXTURE2D_GRAD(tex, s, uv1, mul(dUVdx, rot1), mul(dUVdy, rot1));
            half4 c2 = SAMPLE_TEXTURE2D_GRAD(tex, s, uv2, mul(dUVdx, rot2), mul(dUVdy, rot2));
            half4 c3 = SAMPLE_TEXTURE2D_GRAD(tex, s, uv3, mul(dUVdx, rot3), mul(dUVdy, rot3));
            half3 Lw = half3(0.299, 0.587, 0.114);
            half3 Dw = half3(dot(c1.xyz, Lw), dot(c2.xyz, Lw), dot(c3.xyz, Lw));
            Dw = lerp(1.0, Dw, 0.6);// 0.6
            half3 W = Dw * Pow_7(half3(w1, w2, w3));// 7
            W /= (W.x + W.y + W.z);
            return W.x * c1 + W.y * c2 + W.z * c3;
        #else
            return SAMPLE_TEXTURE2D(tex, s, uv);
        #endif
    }
    float4 SampleSupportNoTileing(float2 uv, float2 positionDSxy, half scaleOrRotate, half noise = 0.0)
    {
        return SampleSupportNoTileing(samplerstate, uv, positionDSxy, scaleOrRotate, noise);
    }

    half3 SampleNormalSupportNoTileing(SAMPLER(s), float2 uv, float2 positionDSxy, half scaleOrRotate, half scale, half noise = 0.0)
    {
        half4 n = SampleSupportNoTileing(s, uv, positionDSxy, scaleOrRotate, noise);
        #if BUMP_SCALE_NOT_SUPPORTED
            return UnpackNormal(n);
        #else
            return UnpackNormalScale(n, scale);
        #endif
    }
    half3 SampleNormalSupportNoTileing(float2 uv, float2 positionDSxy, half scaleOrRotate, half scale, half noise = 0.0)
    {
        return SampleNormalSupportNoTileing(samplerstate, uv, positionDSxy, scaleOrRotate, scale, noise);
    }
    //------------------------------------------------------------------------


    #ifndef SHADER_API_GLES
        float CalculateLevelOfDetail(UnitySamplerState s, float2 uv)
        {
            return CALCULATE_TEXTURE2D_LOD(tex, s.samplerstate, uv);
        }

        float4 Sample(SAMPLER(s), float2 uv)
        {
            return SAMPLE_TEXTURE2D(tex, s, uv);
        }
        float4 SampleLevel(SAMPLER(s), float2 uv, float lod)
        {
            return SAMPLE_TEXTURE2D_LOD(tex, s, uv, lod);
        }
        float4 SampleBias(SAMPLER(s), float2 uv, float bias)
        {
            return SAMPLE_TEXTURE2D_BIAS(tex, s, uv, bias);
        }
        float4 SampleGrad(SAMPLER(s), float2 uv, float2 dpdx, float2 dpdy)
        {
            return SAMPLE_TEXTURE2D_GRAD(tex, s, uv, dpdx, dpdy);
        }
        float4 SampleCmpLevelZero(SAMPLER_CMP(s), float2 uv, float cmp)
        {
            return SAMPLE_TEXTURE2D_SHADOW(tex, s, float3(uv, cmp));
        }
        float4 Load(int3 pixel)
        {
            return LOAD_TEXTURE2D_LOD(tex, pixel.xy, pixel.z);
        }
        float CalculateLevelOfDetail(SAMPLER(s), float2 uv)
        {
            return CALCULATE_TEXTURE2D_LOD(tex, s, uv);
        }
    #endif

    #ifdef PLATFORM_SUPPORT_GATHER
        float4 Gather(UnitySamplerState s, float2 uv)
        {
            return GATHER_TEXTURE2D(tex, s.samplerstate, uv);
        }
        float4 GatherRed(UnitySamplerState s, float2 uv)
        {
            return GATHER_RED_TEXTURE2D(tex, s.samplerstate, uv);
        }
        float4 GatherGreen(UnitySamplerState s, float2 uv)
        {
            return GATHER_GREEN_TEXTURE2D(tex, s.samplerstate, uv);
        }
        float4 GatherBlue(UnitySamplerState s, float2 uv)
        {
            return GATHER_BLUE_TEXTURE2D(tex, s.samplerstate, uv);
        }
        float4 GatherAlpha(UnitySamplerState s, float2 uv)
        {
            return GATHER_ALPHA_TEXTURE2D(tex, s.samplerstate, uv);
        }

        float4 Gather(SAMPLER(s), float2 uv)
        {
            return GATHER_TEXTURE2D(tex, s, uv);
        }
        float4 GatherRed(SAMPLER(s), float2 uv)
        {
            return GATHER_RED_TEXTURE2D(tex, s, uv);
        }
        float4 GatherGreen(SAMPLER(s), float2 uv)
        {
            return GATHER_GREEN_TEXTURE2D(tex, s, uv);
        }
        float4 GatherBlue(SAMPLER(s), float2 uv)
        {
            return GATHER_BLUE_TEXTURE2D(tex, s, uv);
        }
        float4 GatherAlpha(SAMPLER(s), float2 uv)
        {
            return GATHER_ALPHA_TEXTURE2D(tex, s, uv);
        }
    #endif
};

float4 tex2D(UnityTexture2D tex, float2 uv)
{
    return SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, uv);
}
float4 tex2Dlod(UnityTexture2D tex, float4 uv0l)
{
    return SAMPLE_TEXTURE2D_LOD(tex.tex, tex.samplerstate, uv0l.xy, uv0l.w);
}
float4 tex2Dbias(UnityTexture2D tex, float4 uv0b)
{
    return SAMPLE_TEXTURE2D_BIAS(tex.tex, tex.samplerstate, uv0b.xy, uv0b.w);
}

#define UnityBuildTexture2DStruct(n) UnityBuildTexture2DStructInternal(TEXTURE2D_ARGS(n, sampler##n), n##_TexelSize, n##_ST)
#define UnityBuildTexture2DStructNoScale(n) UnityBuildTexture2DStructInternal(TEXTURE2D_ARGS(n, sampler##n), n##_TexelSize, float4(1, 1, 0, 0))
#define UnityBuildTexture2DStructOtherSampler(tex, samplerstate) UnityBuildTexture2DStructInternal(TEXTURE2D_ARGS(tex, samplerstate), tex##_TexelSize, tex##_ST)
#define UnityBuildTexture2DStructOnlyTexture(tex, samplerstate) UnityBuildTexture2DStructInternal(TEXTURE2D_ARGS(tex, samplerstate), float4(0, 0, 0, 0), float4(1, 1, 0, 0))
UnityTexture2D UnityBuildTexture2DStructInternal(TEXTURE2D_PARAM(tex, samplerstate), float4 texelSize, float4 scaleTranslate)
{
    UnityTexture2D result;
    result.tex = tex;
    #ifndef SHADER_API_GLES
        result.samplerstate = samplerstate;
    #endif
    result.texelSize = texelSize;
    result.scaleTranslate = scaleTranslate;
    return result;
}

struct UnityTexture2DArray
{
    TEXTURE2D_ARRAY(tex);
    UNITY_BARE_SAMPLER(samplerstate);

    // these functions allows users to convert code using Texture2DArray to UnityTexture2DArray by simply changing the type of the variable
    // the existing texture macros will call these functions, which will forward the call to the texture appropriately
    #ifndef SHADER_API_GLES
        float4 Sample(UnitySamplerState s, float3 uv)
        {
            return SAMPLE_TEXTURE2D_ARRAY(tex, s.samplerstate, uv.xy, uv.z);
        }
        float4 SampleLevel(UnitySamplerState s, float3 uv, float lod)
        {
            return SAMPLE_TEXTURE2D_ARRAY_LOD(tex, s.samplerstate, uv.xy, uv.z, lod);
        }
        float4 SampleBias(UnitySamplerState s, float3 uv, float bias)
        {
            return SAMPLE_TEXTURE2D_ARRAY_BIAS(tex, s.samplerstate, uv.xy, uv.z, bias);
        }
        float4 SampleGrad(UnitySamplerState s, float3 uv, float2 dpdx, float2 dpdy)
        {
            return SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, s.samplerstate, uv.xy, uv.z, dpdx, dpdy);
        }

        float4 Sample(SAMPLER(s), float3 uv)
        {
            return SAMPLE_TEXTURE2D_ARRAY(tex, s, uv.xy, uv.z);
        }
        float4 SampleLevel(SAMPLER(s), float3 uv, float lod)
        {
            return SAMPLE_TEXTURE2D_ARRAY_LOD(tex, s, uv.xy, uv.z, lod);
        }
        float4 SampleBias(SAMPLER(s), float3 uv, float bias)
        {
            return SAMPLE_TEXTURE2D_ARRAY_BIAS(tex, s, uv.xy, uv.z, bias);
        }
        float4 SampleGrad(SAMPLER(s), float3 uv, float2 dpdx, float2 dpdy)
        {
            return SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, s, uv.xy, uv.z, dpdx, dpdy);
        }
        float4 SampleCmpLevelZero(SAMPLER_CMP(s), float3 uv, float cmp)
        {
            return SAMPLE_TEXTURE2D_ARRAY_SHADOW(tex, s, float3(uv.xy, cmp), uv.z);
        }
        float4 Load(int4 pixel)
        {
            return LOAD_TEXTURE2D_ARRAY(tex, pixel.xy, pixel.z);
        }
    #endif
};

#define UnityBuildTexture2DArrayStruct(n) UnityBuildTexture2DArrayStructInternal(TEXTURE2D_ARRAY_ARGS(n, sampler##n))
UnityTexture2DArray UnityBuildTexture2DArrayStructInternal(TEXTURE2D_ARRAY_PARAM(tex, samplerstate))
{
    UnityTexture2DArray result;
    result.tex = tex;
    #ifndef SHADER_API_GLES
        result.samplerstate = samplerstate;
    #endif
    return result;
}


struct UnityTextureCube
{
    TEXTURECUBE(tex);
    UNITY_BARE_SAMPLER(samplerstate);

    // these functions allows users to convert code using TextureCube to UnityTextureCube by simply changing the type of the variable
    // the existing texture macros will call these functions, which will forward the call to the texture appropriately
    float4 Sample(UnitySamplerState s, float3 dir)
    {
        return SAMPLE_TEXTURECUBE(tex, s.samplerstate, dir);
    }
    float4 SampleLevel(UnitySamplerState s, float3 dir, float lod)
    {
        return SAMPLE_TEXTURECUBE_LOD(tex, s.samplerstate, dir, lod);
    }
    float4 SampleBias(UnitySamplerState s, float3 dir, float bias)
    {
        return SAMPLE_TEXTURECUBE_BIAS(tex, s.samplerstate, dir, bias);
    }

    #ifndef SHADER_API_GLES
        float4 Sample(SAMPLER(s), float3 dir)
        {
            return SAMPLE_TEXTURECUBE(tex, s, dir);
        }
        float4 SampleLevel(SAMPLER(s), float3 dir, float lod)
        {
            return SAMPLE_TEXTURECUBE_LOD(tex, s, dir, lod);
        }
        float4 SampleBias(SAMPLER(s), float3 dir, float bias)
        {
            return SAMPLE_TEXTURECUBE_BIAS(tex, s, dir, bias);
        }
    #endif

    #ifdef PLATFORM_SUPPORT_GATHER
        float4 Gather(UnitySamplerState s, float3 dir)
        {
            return GATHER_TEXTURECUBE(tex, s.samplerstate, dir);
        }
        float4 Gather(SAMPLER(s), float3 dir)
        {
            return GATHER_TEXTURECUBE(tex, s, dir);
        }
    #endif
};

float4 texCUBE(UnityTextureCube tex, float3 dir)
{
    return SAMPLE_TEXTURECUBE(tex.tex, tex.samplerstate, dir);
}
float4 texCUBEbias(UnityTextureCube tex, float4 dirBias)
{
    return SAMPLE_TEXTURECUBE_BIAS(tex.tex, tex.samplerstate, dirBias.xyz, dirBias.w);
}

#define UnityBuildTextureCubeStruct(n) UnityBuildTextureCubeStructInternal(TEXTURECUBE_ARGS(n, sampler##n))
UnityTextureCube UnityBuildTextureCubeStructInternal(TEXTURECUBE_PARAM(tex, samplerstate))
{
    UnityTextureCube result;
    result.tex = tex;
    #ifndef SHADER_API_GLES
        result.samplerstate = samplerstate;
    #endif
    return result;
}


struct UnityTexture3D
{
    TEXTURE3D(tex);
    UNITY_BARE_SAMPLER(samplerstate);

    // these functions allows users to convert code using Texture3D to UnityTexture3D by simply changing the type of the variable
    // the existing texture macros will call these functions, which will forward the call to the texture appropriately
    float4 Sample(UnitySamplerState s, float3 uvw)
    {
        return SAMPLE_TEXTURE3D(tex, s.samplerstate, uvw);
    }

    #ifndef SHADER_API_GLES
        float4 SampleLevel(UnitySamplerState s, float3 uvw, float lod)
        {
            return SAMPLE_TEXTURE3D_LOD(tex, s.samplerstate, uvw, lod);
        }

        float4 Sample(SAMPLER(s), float3 uvw)
        {
            return SAMPLE_TEXTURE2D(tex, s, uvw);
        }
        float4 SampleLevel(SAMPLER(s), float3 uvw, float lod)
        {
            return SAMPLE_TEXTURE2D_LOD(tex, s, uvw, lod);
        }
        float4 Load(int4 pixel)
        {
            return LOAD_TEXTURE3D_LOD(tex, pixel.xyz, pixel.w);
        }
    #endif
};

float4 tex3D(UnityTexture3D tex, float3 uvw)
{
    return SAMPLE_TEXTURE3D(tex.tex, tex.samplerstate, uvw);
}

#define UnityBuildTexture3DStruct(n) UnityBuildTexture3DStructInternal(TEXTURE3D_ARGS(n, sampler##n))
UnityTexture3D UnityBuildTexture3DStructInternal(TEXTURE3D_PARAM(tex, samplerstate))
{
    UnityTexture3D result;
    result.tex = tex;
    #ifndef SHADER_API_GLES
        result.samplerstate = samplerstate;
    #endif
    return result;
}

//扩展
half4 SampleMultiFeature(UnityTexture2D map, SamplerState s, float2 uv, float params)
{
    half4 tex = map.Sample(s, uv);
    #ifdef TEXTURE2D_NOISETILING
        tex = map.SampleNoiseTiling(s, uv, NoiseGenerate_half(uv, params));
    #endif
    #ifdef TEXTURE2D_HEXAGONTILING
        tex = map.SampleHexagonTiling(s, uv, params);
    #endif
    return tex;
}

#endif // UNITY_TEXTURE_INCLUDED
