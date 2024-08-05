#ifndef SSR_BLENDS
#define SSR_BLENDS

	// Copyright 2021 Kronnect - All Rights Reserved.

    TEXTURE2D_X(_MainTex);
    float4 _MainTex_TexelSize;

	float4 _MaterialData;

    float4 _SSRSettings;
    #define THICKNESS _SSRSettings.x

    float4 _SSRSettings4;
    #define SEPARATION_POS _SSRSettings4.x
    float  _MinimumBlur;
    float  _MinimumThickness;
    #define MINIMUM_THICKNESS _MinimumThickness

    TEXTURE2D_X(_RayCastRT);
    TEXTURE2D_X(_BlurRTMip0);
    TEXTURE2D_X(_BlurRTMip1);
    TEXTURE2D_X(_BlurRTMip2);
    TEXTURE2D_X(_BlurRTMip3);
    TEXTURE2D_X(_BlurRTMip4);

    TEXTURE2D_X(_GBuffer2); // for debug normals

	struct AttributesFS {
		float4 positionHCS : POSITION;
		float2 uv          : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

 	struct VaryingsSSR {
    	float4 positionCS : SV_POSITION;
    	float2 uv  : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
	};


	VaryingsSSR VertSSR(AttributesFS input) {
	    VaryingsSSR output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = float4(input.positionHCS.xyz, 1.0);

		#if UNITY_UV_STARTS_AT_TOP
		output.positionCS.y *= -1;
		#endif

        output.uv = input.uv;
    	return output;
	}


	half4 FragCopy (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);
   		half4 pixel = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);
        return pixel;
	}

	half4 FragCopyExact (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);
   		half4 pixel = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, i.uv);
        pixel = max(pixel, 0.0);
        return pixel;
	}


	half4 FragCopyDepth (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv = UnityStereoTransformScreenSpaceTex(i.uv);
        float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, i.uv.xy).r;
        depth = LinearEyeDepth(depth, _ZBufferParams);
        #if SSR_BACK_FACES
            float backDepth = SAMPLE_TEXTURE2D_X(_DownscaledShinyBackDepthRT, sampler_PointClamp, i.uv.xy).r;
            backDepth = LinearEyeDepth(backDepth, _ZBufferParams);
            backDepth = clamp(backDepth, depth + MINIMUM_THICKNESS, depth + THICKNESS);
            return half4(depth, backDepth, 0, 1.0);
        #else
            return half4(depth.xxx, 1.0);
        #endif
	}

    half4 Combine(VaryingsSSR i) {

    	half4 mip0  = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);
        if (mip0.w <= 0) return half4(0,0,0,0);
		
        half4 mip1  = SAMPLE_TEXTURE2D_X(_BlurRTMip0, sampler_LinearClamp, i.uv);
        half4 mip2  = SAMPLE_TEXTURE2D_X(_BlurRTMip1, sampler_LinearClamp, i.uv);
        half4 mip3  = SAMPLE_TEXTURE2D_X(_BlurRTMip2, sampler_LinearClamp, i.uv);
        half4 mip4  = SAMPLE_TEXTURE2D_X(_BlurRTMip3, sampler_LinearClamp, i.uv);
        half4 mip5  = SAMPLE_TEXTURE2D_X(_BlurRTMip4, sampler_LinearClamp, i.uv);

        half r = mip5.a;
        half4 reflData = SAMPLE_TEXTURE2D_X(_RayCastRT, sampler_PointClamp, i.uv);
        if (reflData.z > 0) {
            r = min(reflData.z, r);
        }

        half roughness = clamp(r + _MinimumBlur, 0, 5);

        half w0 = max(0, 1.0 - roughness);
        half w1 = max(0, 1.0 - abs(roughness - 1.0));
        half w2 = max(0, 1.0 - abs(roughness - 2.0));
        half w3 = max(0, 1.0 - abs(roughness - 3.0));
        half w4 = max(0, 1.0 - abs(roughness - 4.0));
        half w5 = max(0, 1.0 - abs(roughness - 5.0));

        half4 refl = mip0 * w0 + mip1 * w1 + mip2 * w2 + mip3 * w3 + mip4 * w4 + mip5 * w5;
		// return refl;
		// return step(0.01,reflData.w).xxxx;
        return half4(refl.xyz,step(0.01,reflData.w) );
        return half4(refl.xyz,step(0.01,reflData.w));
	}

	half4 FragCombine (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);
        return Combine(i);
    }


	half4 FragCombineWithCompare (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);
        if (i.uv.x < SEPARATION_POS - _MainTex_TexelSize.x * 3) {
            return 0;
        } else if (i.uv.x < SEPARATION_POS + _MainTex_TexelSize.x * 3) {
            return 1.0;
        } else {
            return Combine(i);
        }
	}


	half4 FragDebugDepth (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);
        float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, i.uv.xy).r;
        depth = Linear01Depth(depth, _ZBufferParams);
        return half4(depth.xxx, 1.0);
    }


	half4 FragDebugNormals (VaryingsSSR i) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv     = SSRStereoTransformScreenSpaceTex(i.uv);

        float4 normals = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, sampler_PointClamp, i.uv, 0);
        #if defined(_GBUFFER_NORMALS_OCT)
            half2 remappedOctNormalWS = Unpack888ToFloat2(normals.xyz); // values between [ 0,  1]
            half2 octNormalWS = remappedOctNormalWS.xy * 2.0h - 1.0h;    // values between [-1, +1]
            float3 normalWS = UnpackNormalOctQuadEncode(octNormalWS);
        #else
            float3 normalWS = normals.xyz;
        #endif

        return half4(normalWS, 1.0);
    }


#endif // SSR_BLENDS