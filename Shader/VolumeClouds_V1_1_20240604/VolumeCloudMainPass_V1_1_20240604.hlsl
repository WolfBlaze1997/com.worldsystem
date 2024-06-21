#ifndef ALTOS_RENDER_CLOUDS_INCLUDED
#define ALTOS_RENDER_CLOUDS_INCLUDED

#include "../ShaderLibrary/TextureUtils.hlsl"
#include "VolumeCloudData_V1_1_20240604.hlsl"

Texture2DArray _Render_BlueNoiseArray;
int _Render_BlueNoiseArrayIndices;
#define BLUE_NOISE_ARRAY_TEXEL_SIZE float4(0.0625,0.0625,16,16)

static float EPSILON_LARGE = 0.01;
static float EPSILON = 0.0001;
static float EPSILON_SMALL = 0.000001;
static float _CIRRUS_CLOUD_HEIGHT = 18000.0;
float3 _Lighting_AlbedoColor;
static float _CONVERT_KM_TO_M = 1000.0;

// 全局变量定义
float3 _PLANET_CENTER;
Texture3D _Modeling_ShapeDetail_NoiseTexture3D;
float4 _CLOUD_BASE_TEX_TexelSize;

float3 _Lighting_LightColorFilter;

float3 _ZenithColor;
float3 _HorizonColor;

Texture2D _SkyTexture;
int _HasSkyTexture;

float3 _ShadowCasterCameraForward;
float3 _ShadowCasterCameraUp;
float3 _ShadowCasterCameraRight;

int _ShadowPass;

float4 _ShadowCasterCameraPosition;
float4 _CloudShadowOrthoParams; // xy = orthographic size, z = offset distance
float _CloudShadowStrength;

int _Lighting_CheapAmbient;

// 闪电数组
float4 altos_LightningArray[2];
int altos_LightningArraySize;

float _Lighting_ExtinctionCoeff;
float _Modeling_Amount_CloudAmount;
float _Modeling_Position_CloudHeight;
float _Modeling_Position_CloudThickness;
float _Render_MaxRenderDistance;
float3 _Modeling_ShapeDetail_Scale;
float _Render_BlueNoise;
float3 _CLOUD_BASE_TIMESCALE;
float _Lighting_AtmosphereVisibility;
float _Lighting_MaxLightingDistance;
float _Modeling_Position_PlanetRadius;

float _Lighting_AmbientExposure;
float _Modeling_Amount_OverlayStartDistance;
float _Modeling_Amount_OverlayCloudAmount;
float _Lighting_HeightDensityInfluence;
float _Lighting_DensityInfluence;

float _Lighting_HGEccentricityForward;
float _Lighting_HGEccentricityBackward;
float _Lighting_HGStrength;
Texture2D _CLOUD_WEATHERMAP_TEX;
float3 _MotionDetail_Position;

Texture2D CloudBaseTexRT;

float _Lighting_ShadingStrengthFalloff;

float _Render_CoarseSteps = 32.0;
float _Render_DetailSteps = 32.0;
float2 _Render_MipmapDistance = float2(4000, 8000);
float _LIGHTING_INSCATTERING;
float _Lighting_ScatterMultiplier;
float _Lighting_ScatterStrength;


#define _MAXIMUM_EMPTY_DETAIL_STEPS 0


float3 GetCameraPosition()
{
    return _WorldSpaceCameraPos.xyz;
}

float CalculateHorizonFalloff(float3 rayPosition, float3 lightDirection, float planetRadius)
{
    float h = max(rayPosition.y, 0);
    float r = planetRadius;
    float a = r + h;
    float b = r / a;
    float c = acos(b);
    float angle = lightDirection.y * 1.571;
    float d = angle - c;
	
    return smoothstep(radians(0.0), radians(5.0), d);
}

float GetHeight01(float3 rayPos, float atmosThickness, float planetRadius, float atmosHeight)
{
    float height01 = distance(rayPos, _PLANET_CENTER) - (planetRadius + atmosHeight);
    height01 /= atmosThickness;
    return saturate(height01);
}

bool IsInsideSDFSphere(float3 pointToCheck, float3 spherePosition, float sphereRadius)
{
    float dist = distance(pointToCheck, spherePosition);
	
    if (dist < sphereRadius)
        return true;
	
    return false;
}

bool SolveQuadratic(float a, float b, float c, out float x0, out float x1)
{
    float discr = b * b - 4 * a * c;
    if (discr < 0)
        return false;
    else if (discr == 0)
        x0 = x1 = -0.5 * b / a;
    else
    {
        float q = (b > 0) ?
            -0.5 * (b + sqrt(discr)) :
            -0.5 * (b - sqrt(discr));
        x0 = q / a;
        x1 = c / q;
    }
	
    float lT = min(x0, x1);
    float gT = max(x0, x1);
    x0 = lT;
    x1 = gT;
 
    return true;
}

bool IntersectSphere(float3 rayOrigin, float3 rayDir, float sphereRad, float3 spherePosition, out float nearHit, out float farHit)
{
    nearHit = 0.0;
    farHit = 0.0;
	
    float a = dot(rayDir, rayDir);
    float3 L = rayOrigin - spherePosition;
    float b = 2.0 * dot(rayDir, L);
    float c = dot(L, L) - (sphereRad * sphereRad);
    float t0, t1;
    if (!SolveQuadratic(a, b, c, t0, t1))
        return false;
	
    float lt = min(t0, t1);
    float gt = max(t0, t1);
    t0 = lt;
    t1 = gt;
	
    if (t0 < 0)
    {
        t0 = t1;
        if (t0 < 0)
            return false;
    }
    nearHit = max(t0, 0);
    farHit = max(t1, 0);
    return true;
}


AtmosHitData AtmosphereIntersection(float3 rayOrigin, float3 rayDir, float atmosHeight, float planetRadius, float atmosThickness, float maxDist)
{
    float3 sphereCenter = _PLANET_CENTER;
    float innerRad = planetRadius + atmosHeight;
    float outerRad = planetRadius + atmosHeight + atmosThickness;
	
    float innerNear, innerFar, outerNear, outerFar;
    bool hitInner = IntersectSphere(rayOrigin, rayDir, innerRad, sphereCenter, innerNear, innerFar);
    bool hitOuter = IntersectSphere(rayOrigin, rayDir, outerRad, sphereCenter, outerNear, outerFar);
	
    AtmosHitData hitData;
    hitData.didHit = false;
    hitData.doubleIntersection = false;
	
    bool insideInner = IsInsideSDFSphere(rayOrigin, sphereCenter, innerRad);
    bool insideOuter = IsInsideSDFSphere(rayOrigin, sphereCenter, outerRad);
	
    float nearIntersectDistance = 0.0;
    float farIntersectDistance = 0.0;
    float nearIntersectDistance2 = 0.0;
    float farIntersectDistance2 = 0.0;
	
	//Case 1 (低于云体积)
    if (insideInner && insideOuter)
    {
        nearIntersectDistance = innerNear;
        farIntersectDistance = min(outerNear, maxDist);
    }
	
	// Case 2 (内部云体积)
    if (!insideInner && insideOuter)
    {
        farIntersectDistance = min(outerNear, maxDist);
		
		// InnerData.frontFaceDistance > 当光线与内部球体相交时为0。
        if (innerNear > 0.0)
        {
            farIntersectDistance = min(innerNear, maxDist);
			
            if (innerFar < maxDist)
            {
                nearIntersectDistance2 = innerFar;
                farIntersectDistance2 = min(outerFar, maxDist);
            }
        }
    }
	
    bool lookingAboveClouds = false;
	// Case 3 (云体积之上)
    if (!insideInner && !insideOuter)
    {
        if (!hitInner && !hitOuter)
            lookingAboveClouds = true;
		
        nearIntersectDistance = outerNear;
        farIntersectDistance = min(outerFar, maxDist);
		
		// InnerData.frontFaceDistance > 当光线与内部球体相交时为0。
        if (innerNear > 0.0)
        {
            farIntersectDistance = min(innerNear, maxDist);
            if (innerFar < maxDist)
            {
                nearIntersectDistance2 = innerFar;
                farIntersectDistance2 = min(outerFar, maxDist);
            }
        }
    }
	
    hitData.nearDist = nearIntersectDistance;
    hitData.nearDist2 = nearIntersectDistance2;
    hitData.farDist = farIntersectDistance;
    hitData.farDist2 = farIntersectDistance2;
	
    if (hitData.nearDist < maxDist)
        hitData.didHit = true;
	
    if (hitData.nearDist2 > EPSILON)
        hitData.doubleIntersection = true;
	
    if (lookingAboveClouds)
        hitData.didHit = false;
	
    return hitData;
}


AtmosphereData New_AtmosphereData(float thickness, float height, float fadeDistance, float distantCoverageAmount, float distantCoverageDepth)
{
    AtmosphereData a;
    a.atmosThickness = thickness;
    a.atmosHeight = height;
    a.cloudFadeDistance = fadeDistance;
    a.distantCoverageAmount = distantCoverageAmount;
    a.distantCoverageDepth = distantCoverageDepth;
    return a;
}


float GetMipLevel(float2 uv, float2 texelSize, float bias)
{
    float2 unnormalizeduv = uv * texelSize;
    float2 uvDDX = ddx(unnormalizeduv);
    float2 uvDDY = ddy(unnormalizeduv);
    float d = max(dot(uvDDX, uvDDX), dot(uvDDY, uvDDY));
    float mipLevel = 0.5 * log2(d);
    return max(mipLevel + bias, 0);
}

float GetMipLevel3D(float3 uv, float3 texelSize, float bias)
{
    float3 unnormalizeduv = uv * texelSize;
    float3 uvDDX = ddx(unnormalizeduv);
    float3 uvDDY = ddy(unnormalizeduv);
    float d = max(dot(uvDDX, uvDDX), dot(uvDDY, uvDDY));
    float mipLevel = 0.5 * log2(d);
    return max(mipLevel + bias, 0);
}


#define USE_FLOATING_ORIGIN 0
float2 GetWeathermapUV(float3 rayPosition, float maxRenderDistance)
{
    float2 UV = rayPosition.xz - _WorldSpaceCameraPos.xz;
    
	UV *= rcp(maxRenderDistance); // 需要为1.0/最大距离
    UV *= 0.5;
    UV += 0.5;
    return saturate(UV);
}


// #define POINT_SAMPLING
#ifdef POINT_SAMPLING
#define cloudSampler altos_point_repeat_sampler
#else
#define cloudSampler altos_linear_repeat_sampler
#endif

float3 GetWeathermap(RayData rayData, float maxRenderDistance)
{
    float2 weathermapUV = GetWeathermapUV(rayData.rayPosition, maxRenderDistance);
    float3 weathermapSample = CloudBaseTexRT.SampleLevel(cloudSampler, weathermapUV, 0).rgb;
    
    return weathermapSample;
}

float GetCloudDensityByHeight(float height01, float3 weather)
{
    float x = weather.b; 
    
    float result;

    // 这些是我们的云形状预设。
    // 我们不会一直走到0或1，这样我们就有一些发挥的空间
    // 低空云层。。。
    float a = 1.0 - abs(os_Remap(0.1, 0.3, -1, 1, height01));
    a = pow(a, 0.5);

    // 中层云层。。。
    float b = 1.0 - abs(os_Remap(0.15, 0.6, -1, 1, height01));
    b = pow(b, 0.5);

    // 超级高的云！
    float c = 1.0 - abs(os_Remap(0.05, 0.95, -1, 1, height01));
    c = pow(c, 0.5);

    //为了插值，我们从[0,1]到[0,2]，勒普底部范围
    //然后将[1，2]移动到[0，1]并lerp顶部范围
    x *= 2.0;
    float ab = lerp(a, b, saturate(x));
    x -= 1.0;
    result = lerp(ab, c, saturate(x));
    return result;
    
}

float GetCloudShapeVolumetric(StaticMaterialData m, RayData rayData, float2 weathermap, float densityAtHeight, float height01, int mip)
{
    float coverage = weathermap.r * densityAtHeight;
    
    float3 uvw = rayData.rayPosition * 0.0001;
	float cloudAdjFactor = weathermap.r * 0.7;
    
	// 采样基础
    // float3 heightOffset = (height01 * height01 * m.baseTimescale);//偏移云的顶部，使其朝风向倾斜（即，使其看起来云顶部的风更强）

    float3 baseUVW = uvw * m.baseScale - _MotionDetail_Position;
    // baseUVW += heightOffset;
	
    float baseVal = m.baseTexture.SampleLevel(cloudSampler, baseUVW, mip).r;
    baseVal = saturate(baseVal);
    
    baseVal = lerp(baseVal, 1.0, cloudAdjFactor);
    float value = os_Map01(1.0 - baseVal, 1.0, coverage);
	
    if (value < EPSILON_LARGE)
        return 0;
    
    value *= lerp(1.0, height01, m.heightDensityInfluence);
    value *= lerp(1.0, weathermap.r, m.cloudinessDensityInfluence);
    
    value *= (1.0 + weathermap.g);

    return saturate(value);
}

float BeerLambert(float absorptionCoefficient, float stepSize, float density)
{
    return exp(-absorptionCoefficient * stepSize * density);
}

float HenyeyGreenstein(float cos_angle, float eccentricity)
{
    float e2 = eccentricity * eccentricity;
    float f = abs((1.0 + e2 - 2.0 * eccentricity * cos_angle));
    float n = 1.0 - e2;
    float d = pow(f, 1.5);
    return (n / d) * 0.7854; // 相当于以下
    //return ((1.0 - e2) / pow(f, 1.5)) / 4.0 * 3.1416;
}

struct OSLightingData
{
    float3 baseLighting;
    float3 outScatterLighting;
    float additionalLighting;
};

#define RAIN_DENSITY_MODIFIER 4.0
float GetRainDensity(float p)
{
    return 1.0 + (p * RAIN_DENSITY_MODIFIER); // 1 + [0,1] * 4 = [1,5].
}

#define RAIN_LIGHTING_STRENGTH 0.8
float GetRainLightingReduction(float p)
{
    return 1.0 - p * p * RAIN_LIGHTING_STRENGTH;
}

OSLightingData GetLightingDataVolumetric(StaticMaterialData materialData, RayData rayData, AtmosphereData atmosData, int mip)
{
    OSLightingData data;
	
    float3 cachedRayOrigin = rayData.rayPosition;
	/* REMOVED -- 当直接抬头看时，这会产生“涡流”效应。
	//float r = GetHaltonFromTexture(rayData.rayDepth + _FrameId).g;
	//r = lerp(0, r, rayData.noiseIntensity);
	//r = Remap(0.0, 1.0, 2.0, 3.0, r);
	*/
    
	/* REMOVED - 性能提升，轻微视觉差异
	float t0, t1;
	IntersectSphere(rayData.rayPosition, materialData.sunPos, materialData.planetRadius + atmosData.atmosHeight + atmosData.atmosThickness, _PLANET_CENTER, t0, t1);
	float lightingDistanceToSample = min(t0, materialData.lightingDistance);
	*/
	
    float lightingDistanceToSample = materialData.lightingDistance;

    float totalDensity = 0.0;
    float3 extinction;

    // 我们可以使用这些特性来调整云降水照明吗？
    float sAmp = 1.0;
    float3 densityAdj = 0;

    
    float samplePositions[] = { 0.02, 0.06, 0.14, 0.25, 0.4, 0.7, 1.0 };
    #define LIGHTING_SAMPLE_COUNT 3
    [unroll]
    float prevDistance = 0;
    for (int i = 0; i < LIGHTING_SAMPLE_COUNT; i++)
    {
        float totalDistance = samplePositions[i] * lightingDistanceToSample;
        
        rayData.rayPosition = cachedRayOrigin + (_MainLightPosition * totalDistance);
        
        float3 weather = GetWeathermap(rayData, atmosData.cloudFadeDistance);
		  
        [branch]
        if (weather.r > EPSILON_LARGE)
        {
            float height01 = GetHeight01(rayData.rayPosition, atmosData.atmosThickness, materialData.planetRadius, atmosData.atmosHeight);
            float densityAtHeight = GetCloudDensityByHeight(height01, weather);
            float cloudDensity = GetCloudShapeVolumetric(materialData, rayData, weather, densityAtHeight, height01, mip);
            totalDensity += cloudDensity * (totalDistance - prevDistance) * GetRainDensity(weather.g);
			 
            extinction = materialData.extinction * sAmp;
            sAmp *= _Lighting_ShadingStrengthFalloff;
            densityAdj += extinction * totalDensity;
        }
        prevDistance = totalDistance;
    }
    

    
    // 计算基础照明
    data.baseLighting = exp(-densityAdj);
    
    data.outScatterLighting = 0.0;
    
    /////////////////////////////////////////////
    // Lightning                               //
    /////////////////////////////////////////////
    data.additionalLighting = 0;
    for(int lightningCount = 0; lightningCount < altos_LightningArraySize; lightningCount++)
    {
        float d = distance(cachedRayOrigin, altos_LightningArray[lightningCount].xyz);
        float lightningIntensity = (altos_LightningArray[lightningCount].w * 0.1) / (d * d);
        data.additionalLighting += lightningIntensity;
    }

    return data;
}

#define AMBIENT_STEP_LENGTH 400

float GetAmbientDensity(StaticMaterialData materialData, RayData rayData, AtmosphereData atmosData, float3 weather, int mip, int
                        cheapAmbientLighting)
{
    float height01 = GetHeight01(rayData.rayPosition, atmosData.atmosThickness, materialData.planetRadius, atmosData.atmosHeight);
	
    if (cheapAmbientLighting)
    {
        return height01 * height01;
    }
	
    float step = AMBIENT_STEP_LENGTH;
	
    rayData.rayPosition += float3(0, 1, 0) * step;
	
    float density = GetCloudDensityByHeight(height01, weather);
    float d = GetCloudShapeVolumetric(materialData, rayData, weather, density, height01, mip);
	
    float ambientDensity = exp(-d * materialData.extinction.r * step * 0.3 * GetRainDensity(weather.g));
    return ambientDensity;
}

void ApplyLighting(StaticMaterialData materialData, float atmosphereDensity, float accumulatedDepth, float3 lightEnergy, float ambientEnergy, float baseEnergy, inout float3 cloudColor)
{
    float atmosphericAttenuation = 1.0 - rcp(exp(accumulatedDepth * atmosphereDensity));
    cloudColor += materialData.fogColor * atmosphericAttenuation * baseEnergy;
    cloudColor += _MainLightColor * _Lighting_LightColorFilter * (1.0 - atmosphericAttenuation) * lightEnergy;
    cloudColor += materialData.ambientColor * materialData.ambientExposure * (1.0 - atmosphericAttenuation) * ambientEnergy;
}

float3 CalculateScattering(float3 scattering, float3 inverseExtinction, float transmittance, float priorAlpha)
{
    return (scattering - (scattering * transmittance)) * inverseExtinction * priorAlpha;
}

void EvaluateExtinction(float3 materialExtinction, float valueAtPoint, float stepSize, out float3 sampleExtinction, out float3 inverseSampleExtinction, out float transmittance, inout float alpha)
{
    sampleExtinction = materialExtinction * valueAtPoint;
    transmittance = exp(-sampleExtinction.r * stepSize);
    alpha *= transmittance;
    inverseSampleExtinction = rcp(sampleExtinction);
}

struct FragmentOutput
{
    float4 color;
};

StaticMaterialData Setup(in float2 UV)
{
    StaticMaterialData materialData;

    materialData.ambientColor = lerp(_ZenithColor, _HorizonColor, 0.5);
    materialData.ambientExposure = _Lighting_AmbientExposure + 1e-4;
    materialData.cloudiness = _Modeling_Amount_CloudAmount;
    materialData.alphaAccumulation = _Lighting_ExtinctionCoeff * 0.01;
    materialData.baseTexture = _Modeling_ShapeDetail_NoiseTexture3D;
    materialData.baseScale = _Modeling_ShapeDetail_Scale;
    // materialData.baseTimescale = _CLOUD_BASE_TIMESCALE * 0.0001;
    materialData.lightingDistance = _Lighting_MaxLightingDistance;
    materialData.planetRadius = _Modeling_Position_PlanetRadius * _CONVERT_KM_TO_M;
    materialData.heightDensityInfluence = _Lighting_HeightDensityInfluence;
    materialData.cloudinessDensityInfluence = _Lighting_DensityInfluence;
    materialData.weathermapTex = _CLOUD_WEATHERMAP_TEX;
    materialData.renderLocal = true;
    materialData.fogPower = _Lighting_AtmosphereVisibility;
    materialData.uv = UV;
    materialData.fogColor = _HorizonColor;
    materialData.rayOrigin = _WorldSpaceCameraPos;
    materialData.mainCameraOrigin = _WorldSpaceCameraPos;
    materialData.extinction = 0;
    materialData.HG = 0;
    return materialData;
}




FragmentOutput SampleClouds(float2 UV, int useDownscaledDepth, int useReprojection, int useDepth)
{
    
    float alpha = 1.0;
    float3 cloudColor = float3(0, 0, 0);
    float4 cloudData = float4(cloudColor, alpha);
	FragmentOutput o;
    o.color = cloudData;

    
	// 材料数据设置
    if (useReprojection == 1 && _ShadowPass == 0)
    {
        int2 uvIndex = UV * _RenderTextureDimensions.zw * 2.0;
        int2 uvIndexOffset[4] = { int2(0,0), int2(1,0), int2(0,1), int2(1,1) }; 
        // uvIndex += uvIndexOffset[_FrameId % 4];
        uvIndex += uvIndexOffset[_FrameId];
        UV = float2(uvIndex) * _RenderTextureDimensions.xy * 0.5;
    }
    
    
    StaticMaterialData materialData = Setup(UV);
    
    if (_HasSkyTexture && _ShadowPass == 0)
    {
        materialData.fogColor = _SkyTexture.SampleLevel(altos_linear_clamp_sampler, materialData.uv, 0).rgb;
    }
    
    if (_ShadowPass == 1 && dot(_MainLightPosition, float3(0, 1, 0)) < 0)
    {
        o.color = float4(_INF, 0, 0, 0);
        return o;
    }
	
	// Other properties...
    float accDepthSamples = 0;
    float baseEnergy = 0.0;
    float valueAtPoint = 0;
    int mip = 0;
	
	
	
	// 深度、光线和UV设置
    RayData rayData;
    rayData.rayOrigin = materialData.rayOrigin;;
    float viewLength, viewLengthUnjittered;
    GetWSRayDirectionFromUV(UV, rayData.rayDirectionUnjittered, viewLengthUnjittered);
    viewLength = viewLengthUnjittered;
    rayData.rayDirection = rayData.rayDirectionUnjittered;
    
	// 设置深度属性
    float depthRaw, depth01, depthEye, realDepthEye;
    depthRaw = 0;
    depth01 = _INF;
    depthEye = _INF;
    realDepthEye = _INF;
    [branch]
    if (useDepth == 1 && _ShadowPass == 0)
    {
        if (useDownscaledDepth == 0)
        {
            depthRaw = SampleSceneDepth(UV);
        }
        else
        {
            depthRaw = _DitheredDepthTexture.SampleLevel(altos_point_clamp_sampler, UV, 0).r;
        }
        
        depth01 = Linear01Depth(depthRaw, _ZBufferParams);
        if (depth01 < 1.0)
        {
            depthEye = LinearEyeDepth(depthRaw, _ZBufferParams);
            realDepthEye = depthEye * viewLength;
        }
    }
	
	[branch]
    if (_ShadowPass == 1)
    {
        float2 orthoSize = _CloudShadowOrthoParams.xy;
        orthoSize = (UV - 0.5) * orthoSize;
		
        rayData.rayDirection = _ShadowCasterCameraForward;
        rayData.rayDirectionUnjittered = _ShadowCasterCameraForward;
        materialData.rayOrigin = _ShadowCasterCameraPosition.xyz + _ShadowCasterCameraRight * orthoSize.x + _ShadowCasterCameraUp * orthoSize.y;
        rayData.rayOrigin = materialData.rayOrigin;
		
        materialData.renderLocal = false;
    }
    
    materialData.extinction = materialData.alphaAccumulation;
	
    AtmosphereData atmosData = New_AtmosphereData(_Modeling_Position_CloudThickness, _Modeling_Position_CloudHeight, _Render_MaxRenderDistance, _Modeling_Amount_OverlayCloudAmount, _Modeling_Amount_OverlayStartDistance);
	
    float maxRenderDistance = atmosData.cloudFadeDistance;
	
    _PLANET_CENTER = float3(materialData.mainCameraOrigin.x, 0.0 - materialData.planetRadius, materialData.mainCameraOrigin.z);
    if (_ShadowPass == 1)
    {
        maxRenderDistance = _CloudShadowOrthoParams.z * 2.0;
    }
	
    bool sampleLowAltitudeClouds = false;
    AtmosHitData hitData = (AtmosHitData)0;
    hitData.nearDist = 0;
    hitData.didHit = false;
    
    [branch]
    if (materialData.extinction.r > EPSILON)
    {
        hitData = AtmosphereIntersection(rayData.rayOrigin, rayData.rayDirection, atmosData.atmosHeight, materialData.planetRadius, atmosData.atmosThickness, maxRenderDistance);
		
        if (hitData.didHit)
            sampleLowAltitudeClouds = true;
	
        if (materialData.renderLocal && hitData.nearDist > realDepthEye && depth01 < 1.0)
            sampleLowAltitudeClouds = false;
    }
    
    bool doSampleHighAlt = false;
    
    if (_ShadowPass == 1)
    {
        doSampleHighAlt = false;
    }
    
    [branch]
    if (doSampleHighAlt || sampleLowAltitudeClouds)
    {
        #define HG_INTERPOLATION_FACTOR 0.5
        float cos_angle = dot(_MainLightPosition, rayData.rayDirection);
        float HGForward = HenyeyGreenstein(cos_angle, _Lighting_HGEccentricityForward);
        float HGBack = HenyeyGreenstein(cos_angle, _Lighting_HGEccentricityBackward);
        float HG = lerp(HGForward, HGBack, HG_INTERPOLATION_FACTOR);
        HG = lerp(1.0, HG, saturate(_Lighting_HGStrength));
        materialData.HG = HG;
    }
    
    float sampleDepth = 0;
    float maxDepth = 0;
    float volumeThickness = 0;
	
    int r = 0;
    int g = 0;
	
    float frontDepth = -1;
    float sumExtinction = 0;
    int extinctionCounter = 0;
	
	[branch]
    if (sampleLowAltitudeClouds)
    {
        hitData.nearDist = min(hitData.nearDist, realDepthEye);
        hitData.nearDist2 = min(hitData.nearDist2, realDepthEye);
        hitData.farDist = min(hitData.farDist, realDepthEye);
        hitData.farDist2 = min(hitData.farDist2, realDepthEye);
		
        sampleDepth = hitData.nearDist;
        maxDepth = max(hitData.farDist, hitData.farDist2);
        maxDepth = min(maxDepth, maxRenderDistance);
        volumeThickness = (hitData.farDist2 - hitData.nearDist2) + (hitData.farDist - hitData.nearDist);
		
        bool accountedForDoubleIntersect = true;
		
        if (hitData.doubleIntersection)
        {
            accountedForDoubleIntersect = false;
        }
        
        float coarseStepSize = volumeThickness * rcp(_Render_CoarseSteps);
        float detailStepSize = volumeThickness * rcp(_Render_DetailSteps);
        
        float2 repeat = ceil(_RenderTextureDimensions.zw * BLUE_NOISE_ARRAY_TEXEL_SIZE.xy);
        float blueNoise = SAMPLE_TEXTURE2D_ARRAY(_Render_BlueNoiseArray,altos_point_repeat_sampler,UV * repeat,_Render_BlueNoiseArrayIndices).r;
        
        rayData.noiseIntensity = _Render_BlueNoise;
        rayData.noiseAdjustment = coarseStepSize * blueNoise * rayData.noiseIntensity;
		
        sampleDepth += rayData.noiseAdjustment;
		
        rayData.rayDepth = sampleDepth;
		
        bool isFirstSample = true;
        int shortStepCounter = 0;
        
        // 指令
        bool sampleDirectLighting = true;
        bool sampleAmbientLighting = true;
        
        if (_ShadowPass == 1)
        {
            sampleAmbientLighting = false;
            sampleDirectLighting = false;
        }
		
        
        for (int i = 1; i <= int(_Render_DetailSteps); i++)
        {
            if (rayData.rayDepth > maxDepth)
                break;
			
            r++;
			
            rayData.rayPosition = rayData.rayOrigin + (rayData.rayDirection * rayData.rayDepth);
            float d = distance(materialData.mainCameraOrigin, rayData.rayPosition);
            
            mip = (d > _Render_MipmapDistance.y) ? 2 : (d > _Render_MipmapDistance.x) ? 1 : 0;
            
            valueAtPoint = 0;
			
            float height01 = 0;
            float density = 0;
            
            float3 weather = GetWeathermap(rayData, atmosData.cloudFadeDistance);
			
            [branch]
            if (weather.r > EPSILON_LARGE)
            {
                height01 = GetHeight01(rayData.rayPosition, atmosData.atmosThickness, materialData.planetRadius, atmosData.atmosHeight);
                density = GetCloudDensityByHeight(height01, weather);
				
                [branch]
                if(isFirstSample)
                {
                    valueAtPoint = GetCloudShapeVolumetric(materialData, rayData, weather, density, height01, mip);
                }
                else
                {
                    valueAtPoint = GetCloudShapeVolumetric(materialData, rayData, weather, density, height01, mip);
                }
                
            }
			
            if (isFirstSample && valueAtPoint > EPSILON_LARGE && _ShadowPass == 0)
            {
                i -= 1;
                rayData.rayDepth -= coarseStepSize;
                isFirstSample = false;
                shortStepCounter = _MAXIMUM_EMPTY_DETAIL_STEPS;
                continue;
            }
			
			
            float stepSize = coarseStepSize;
            if (shortStepCounter > 0)
            {
                stepSize = detailStepSize;
            }
			
			// 如果此时存在云，请对消光和照明进行采样
            [branch]
            if (valueAtPoint > EPSILON_LARGE)
            {
                
                g++;
                
                if (frontDepth < 0)
                {
                    frontDepth = rayData.rayDepth;
                }
                
                float priorAlpha = alpha;
                float3 sampleExtinction;
                float3 sampleExtinctionInverse;
                float transmittance;

                EvaluateExtinction(materialData.extinction, valueAtPoint, stepSize, sampleExtinction, sampleExtinctionInverse, transmittance, alpha);

                // 对于阴影
                sumExtinction += sampleExtinction.r * stepSize;
                extinctionCounter++;
                
                
                // 评估深度
                if (accDepthSamples < EPSILON)
                {
                    accDepthSamples = rayData.rayDepth;
                }
                else
                {
                    float energData = (sampleExtinction.r - (sampleExtinction.r * transmittance)) * sampleExtinctionInverse.r;
                    baseEnergy += energData * priorAlpha;
                    accDepthSamples += (rayData.rayDepth - accDepthSamples) * energData * priorAlpha;
                }

                
                float3 col = 0;
                
                // 直接照明
                [branch]
                if (sampleDirectLighting)
                {
                    #define _IN_SCATTERING_MIP_OFFSET 2
                    
				    // 在散射中
                    float inScattering = GetCloudShapeVolumetric(materialData, rayData, weather, density, height01, mip + _IN_SCATTERING_MIP_OFFSET);
                    inScattering *= inScattering;
                    inScattering *= _Lighting_ScatterMultiplier;
                    inScattering = saturate(inScattering);
                    inScattering = lerp(1.0, inScattering, _Lighting_ScatterStrength);
					
                    // 要做的事：考虑用内/外/散射来着色反照率，使糖看起来蓬松，呵呵
                    OSLightingData lightingData = GetLightingDataVolumetric(materialData, rayData, atmosData, mip);
                    float hg = materialData.HG * (1.0 - weather.g);
                    float3 finalLighting = _MainLightColor * _Lighting_LightColorFilter * ((lightingData.baseLighting * hg) + lightingData.outScatterLighting);
                    finalLighting *= GetRainLightingReduction(weather.g);
                    float3 lightData = (finalLighting + lightingData.additionalLighting) * sampleExtinction * _Lighting_AlbedoColor * inScattering;
                    float3 intScatter = (lightData - (lightData * transmittance)) * sampleExtinctionInverse;
                    col += intScatter * priorAlpha;
                }
				
				//环境照明
                [branch]
                if (sampleAmbientLighting)
                {
                    #define AMBIENT_HEIGHT_FLOOR 0.1
                    #define AMBIENT_HEIGHT_CEILING 0.7
                    #define AMBIENT_HEIGHT_MIN 0.6
                    #define AMBIENT_HEIGHT_MAX 1.0
                    
                    float ambHeight = os_Remap(AMBIENT_HEIGHT_FLOOR, AMBIENT_HEIGHT_CEILING, AMBIENT_HEIGHT_MIN, AMBIENT_HEIGHT_MAX, height01);
                    float ambDensity = GetAmbientDensity(materialData, rayData, atmosData, weather, mip, _Lighting_CheapAmbient);
					
                    float3 ambientLighting = materialData.ambientColor * materialData.ambientExposure * (ambDensity + ambHeight) * 0.5;
                    ambientLighting *= GetRainLightingReduction(weather.g);
                    float3 ambientData = sampleExtinction * ambientLighting * _Lighting_AlbedoColor;
                    float3 intAmb = (ambientData - (ambientData * transmittance)) * sampleExtinctionInverse;
                    col += intAmb * priorAlpha;
                }
                
                // float4 buto = 0;
                
                // 大气雾
                float3 fogData = materialData.fogColor * (sampleExtinction - (sampleExtinction * transmittance)) * sampleExtinctionInverse;
                float3 fog = fogData * priorAlpha;
                float atmosphericAttenuation = 1.0 - rcp(exp(rayData.rayDepth * materialData.fogPower));
                col = lerp(col, fog, atmosphericAttenuation);

                cloudColor += col;
            }
			
			#define MINIMUM_IMAGE_ALPHA_FOR_BREAK 1e-5
            if (alpha < MINIMUM_IMAGE_ALPHA_FOR_BREAK)
            {
                alpha = 0.0;
                break;
            }
			
			
            if (shortStepCounter > 0)
            {
                rayData.rayDepth += detailStepSize;
                shortStepCounter -= 1;
            }
            else
            {
                rayData.rayDepth += coarseStepSize;
            }
			
			
			// 如果需要，请处理“双相交”。
            if (hitData.doubleIntersection && !accountedForDoubleIntersect && rayData.rayDepth > hitData.farDist)
            {
                rayData.rayDepth = hitData.nearDist2;
                accountedForDoubleIntersect = true;
            }
        }
    }
    
    
    
    if (frontDepth < 0)
    {
        frontDepth = _INF;
    }
    
    if (_ShadowPass == 1)
    {
        float x = 0;
        if (extinctionCounter > 0)
        {
            x = sumExtinction / float(extinctionCounter);
			x *= 0.0002;
            x *= _CloudShadowStrength;
        }
        cloudColor = float3(frontDepth, x, accDepthSamples);
        alpha = 0;
    }


    cloudData = float4(cloudColor, 1.0 - alpha);
    
    FragmentOutput output;
    output.color = cloudData;
    return output;
}

#endif
