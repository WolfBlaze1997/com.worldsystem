#ifndef ALTOS_TEMPORAL_AA_INCLUDED
#define ALTOS_TEMPORAL_AA_INCLUDED

#define _DEBUG_MOTION_VECTORS 0

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#include "TextureUtils.hlsl"


float4 TemporalAA(Texture2D HistoricData, Texture2D NewFrameData, float2 UV, float BlendFactor, float2 MotionVector)
{
    float2 texelSize = _ScreenParams.zw - 1.0;

    float4 newFrame = NewFrameData.SampleLevel(altos_point_clamp_sampler, UV, 0);

    static float2 offsets[8] =
    {
        float2(0, -1),
		float2(0, 1),
		float2(1, 0),
		float2(-1, 0),
		float2(-1, -1),
		float2(1, -1),
		float2(-1, 1),
		float2(1, 1)
    };

    float2 HistUV = UV + MotionVector;
    bool isValidHistUV = IsUVInRange01(HistUV);
	
    if (isValidHistUV)
    {
        float4 HistSample = HistoricData.SampleLevel(altos_linear_clamp_sampler, HistUV, 0);
		
        float4 minResults[2] =
        {
            newFrame,
			newFrame
        };
        float4 maxResults[2] =
        {
            newFrame,
			newFrame
        };
		
        float4 v[8];
		
        for (int i = 0; i < 8; i++)
        {
            v[i] = NewFrameData.SampleLevel(altos_linear_clamp_sampler, UV + texelSize * offsets[i], 0);
        }

		
		// cross sample
        for (int j = 0; j < 4; j++)
        {
            minResults[0] = min(v[j], minResults[0]);
            maxResults[0] = max(v[j], maxResults[0]);
        }
		
        minResults[1] = minResults[0];
        maxResults[1] = maxResults[0];
		
		// box sample 
		// (leverages the results of the cross sample to avoid rework).
        for (int k = 4; k < 8; k++)
        {
            minResults[1] = min(v[k], minResults[1]);
            maxResults[1] = max(v[k], maxResults[1]);
        }
		
        
		// average the results of the cross and box samples
        float4 minResult, maxResult;
        minResult = (minResults[0] + minResults[1]) * 0.5;
        maxResult = (maxResults[0] + maxResults[1]) * 0.5;
		
        
        // Consider using a TAA AABB clip instead of a clamp...
        float4 clampedHist = clamp(HistSample, minResult, maxResult);
        newFrame = lerp(clampedHist, newFrame, BlendFactor);
    }
    
    #if _DEBUG_MOTION_VECTORS == 1
	    newFrame = float4(1.0, 1.0, 1.0, 0.0);
	
	    if (isValidHistUV)
	    {
		    newFrame = float4(MotionVector.x, MotionVector.y, 0, 0) * 10.0;
	    }
    #endif
	
    return newFrame;
}

void TAA_float(Texture2D HistoricData, Texture2D NewFrameData, float2 UV, float BlendFactor, float2 MotionVector, out float4 MergedData, out float3 MergedDataRGB, out float MergedDataA)
{
	#ifdef SHADERGRAPH_PREVIEW
	MergedData = 0;
	MergedDataRGB = 0;
	MergedDataA = 0;
	#endif
	
    MergedData = TemporalAA(HistoricData, NewFrameData, UV, BlendFactor, MotionVector);
	MergedDataRGB = MergedData.rgb;
	MergedDataA = MergedData.a;
}
#endif