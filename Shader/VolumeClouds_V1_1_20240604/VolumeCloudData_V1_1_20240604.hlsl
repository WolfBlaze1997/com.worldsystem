#ifndef ALTOS_TYPES_INCLUDED
#define ALTOS_TYPES_INCLUDED

struct AtmosphereData
{
    float atmosThickness;
    float atmosHeight;
    float cloudFadeDistance;
    float distantCoverageAmount;
    float distantCoverageDepth;
};

struct AtmosHitData
{
    bool didHit;
    bool doubleIntersection;
    float nearDist;
    float nearDist2;
    float farDist;
    float farDist2;
};


struct IntersectData
{
    bool hit;
    bool inside;
    float frontfaceDistance;
    float backfaceDistance;
};

struct StaticMaterialData
{
    float2 uv;
	
    float3 mainCameraOrigin;
    float3 rayOrigin;
    
    // float3 sunPos;
    // float3 sunColor;
    // float sunIntensity;
	
    bool renderLocal;
	
    float cloudiness;
    float alphaAccumulation;
    
    float3 extinction;
    // float3 adjustedExtinction;
    
    float HG;
	
    float ambientExposure;
    float3 ambientColor;
    float3 fogColor;
    float fogPower;
    
    Texture3D baseTexture;
    // float4 baseTexture_TexelSize;
    float3 baseScale;
    // float3 baseTimescale;
	
    int lightingDistance;
    int planetRadius;
	
    float heightDensityInfluence;
    float cloudinessDensityInfluence;
	
    Texture2D weathermapTex;
};

struct RayData
{
    float3 rayOrigin;
    float3 rayPosition;
    float3 rayDirection;
    float3 rayDirectionUnjittered;
    float relativeDepth;
    float rayDepth;
    float stepSize;
    float shortStepSize;
    float noiseAdjustment;
    float noiseIntensity;
};

#endif
