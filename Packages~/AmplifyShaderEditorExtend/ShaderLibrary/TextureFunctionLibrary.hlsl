#ifndef TEXTURE_FUNCTION
#define TEXTURE_FUNCTION

//--------------------------------------------------------
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

float4 SampleSupportNoTileing_OneSample(float2 uv, sampler2D tex, float2 positionDSxy)
{
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

    uint index = (uint(suv.x) % 4) * 4 + uint(suv.y) % 4;
    float r = DITHER_THRESHOLDS[index];
    b = clamp(sign(b - r), 0.0, 1.0);
    float4 ofDither = lerp(lerp(ofa, ofb, b.x), lerp(ofc, ofd, b.x), b.y);
    ddxV *= ofDither.zw;
    ddyV *= ofDither.zw;
    uv = uv * ofDither.zw + ofDither.xy;
    return tex2Dgrad(tex, uv, ddxV, ddyV);
}
float4 SampleSupportNoTileing_twoSample(float2 uv, sampler2D tex, float noise)
{
    half k = noise; // cheap (cache friendly) lookup 采样一个noise
    half2 duvdx = ddx(uv);
    half2 duvdy = ddy(uv);
    half l = k * 8.0;
    half f = frac(l);
    half ia = floor(l); // my method
    half ib = ia + 1.0;
    half2 offa = abs(frac(half2(PI, 2 * PI) * ia) * 2 - 1.0); // 将消耗较高的超越函数sin优化为abs/frac,
    half2 offb = abs(frac(half2(PI, 2 * PI) * ib) * 2 - 1.0); // 将消耗较高的超越函数sin优化为abs/frac
    half4 cola = tex2Dgrad(tex, uv + offa, duvdx, duvdy);
    half4 colb = tex2Dgrad(tex, uv + offb, duvdx, duvdy);
    half3 colaSubcolb = cola.xyz - colb.xyz;
    return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * (colaSubcolb.x + colaSubcolb.y + colaSubcolb.z)));
}

float4 SampleSupportNoTileing_HexagonSample(float2 uv, sampler2D tex,float scaleOrRotate)
{
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
    half4 c1 = tex2Dgrad(tex, uv1, mul(dUVdx, rot1), mul(dUVdy, rot1));
    half4 c2 = tex2Dgrad(tex, uv2, mul(dUVdx, rot2), mul(dUVdy, rot2));
    half4 c3 = tex2Dgrad(tex, uv3, mul(dUVdx, rot3), mul(dUVdy, rot3));
    half3 Lw = half3(0.299, 0.587, 0.114);
    half3 Dw = half3(dot(c1.xyz, Lw), dot(c2.xyz, Lw), dot(c3.xyz, Lw));
    Dw = lerp(1.0, Dw, 0.6);// 0.6
    half3 W = Dw * Pow_7(half3(w1, w2, w3));// 7
    W /= (W.x + W.y + W.z);
    return W.x * c1 + W.y * c2 + W.z * c3;
}


#endif