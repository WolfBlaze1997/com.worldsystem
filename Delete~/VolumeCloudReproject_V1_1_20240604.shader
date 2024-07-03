Shader "Hidden/WorldSystem/ReprojectOptimize_V1_1_20240604"
{
//    Properties
//    {
//        _Render_BlueNoiseArray ("_Render_BlueNoiseArray", 2DArray) = "" {}
//    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Reproject"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "../ShaderLibrary/GetCameraMotionVectors.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TextureUtils.hlsl"
            #include "Packages/com.worldsystem//Shader/ShaderLibrary/TemporalAA.hlsl"
            #include "VolumeCloudMainPass_V1_1_20240604.hlsl"

            Texture2D _CurrentFrame;
            Texture2D _PreviousFrame;

            int _IsFirstFrame;
            // int _UseDepth;
            float _CloudTextureRenderScale;

            /////////////////////////////
            // Explanation             //
            /////////////////////////////

            // Cases: 
            // (A) We sampled this pixel, so we don't need to reproject.
            // (B) We didn't sample this pixel this frame, so we must reproject from a previous frame.
            // (C) We didn't sample this pixel, but we don't have a previous frame to sample from.

            // Case A:
            // Sample the pixel from current frame
            // end.
            
            // Case B:
            // Sample the pixels from current frame
            // Sample the pixel from previous frame
            // Clamp the previous frame to the current bounding box
            // end.

            // Case C:
            // Render the clouds at this pixel (TO DO: Optimize this).

            float4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 UV = input.texcoord;
                
                // We sample the full screen in the first frame so that screenshots and other instant effects work OK.
                if(_IsFirstFrame == 1)
                {
                    return SampleClouds(input.texcoord, 0, 0, 0).color;
                }


                //////////////////
                // Case A       //
                //////////////////

                uint2 pixelCoord = UV * _ScreenParams.xy * _RenderScale.x;
                uint uvIndex = (pixelCoord.x % 2) + (pixelCoord.y % 2) * 2.0;
    
                // if(uvIndex == _FrameId % 4)
                if(uvIndex == _FrameId)
                {
                    return _CurrentFrame.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0);
                }


                //////////////////
                // Case B       //
                //////////////////


                float4 newFrame = _CurrentFrame.SampleLevel(altos_point_clamp_sampler, input.texcoord, 0);



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
	
                float2 motionVectors = GetMotionVector(input.texcoord);
                
                float2 HistUV = UV + motionVectors;
                bool isValidHistUV = IsUVInRange01(HistUV);
                
                // Get the texel size of the input cloud texture.
                float2 texelSize = GetTexCoordSize(_CloudTextureRenderScale); 

                if (isValidHistUV)
                {
                    float4 HistSample = _PreviousFrame.SampleLevel(altos_point_clamp_sampler, HistUV, 0);
        
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
                        v[i] = _CurrentFrame.SampleLevel(altos_point_clamp_sampler, UV + texelSize * offsets[i], 0);
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
		
                    float4 clampedHist = clamp(HistSample, minResult, maxResult);
                    return clampedHist;
                }

                // If we don't have a valid historical UV, sample the clouds again.
                // If we don't have a good depth reference, sample the clouds again.
                return SampleClouds(input.texcoord, 0, 0, 0).color;
            }
            ENDHLSL
        }
    }
}