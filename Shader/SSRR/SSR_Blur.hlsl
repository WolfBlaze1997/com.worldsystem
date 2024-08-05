#ifndef SSR_BLUR
#define SSR_BLUR

	// Copyright 2021 Kronnect - All Rights Reserved.
    TEXTURE2D_X(_MainTex);
    float4 _MainTex_TexelSize;
    TEXTURE2D_X(_RayCastRT);
    float4 _SSRSettings2;
    #define BLUR_MULTIPLIER _SSRSettings2.y
    float4 _SSRSettings4;
    #define DENOISE_POWER _SSRSettings4.w
    float2 _SSRBlurStrength;
    #define BLUR_STRENGTH_HORIZ _SSRBlurStrength.x
    #define BLUR_STRENGTH_VERT _SSRBlurStrength.y

#if defined(UNITY_SINGLE_PASS_STEREO)
    #define SSR_VERTEX_CROSS_DATA
    #define SSR_VERTEX_OUTPUT_GAUSSIAN_UV(o)
    #if defined(SSR_BLUR_HORIZ)
        #define SSR_FRAG_SETUP_GAUSSIAN_UV(i) float2 offset1 = float2(_MainTex_TexelSize.x * 1.3846153846 * BLUR_STRENGTH_HORIZ, 0); float2 offset2 = float2(_MainTex_TexelSize.x * 3.2307692308 * BLUR_STRENGTH_HORIZ, 0);
    #else
        #define SSR_FRAG_SETUP_GAUSSIAN_UV(i) float2 offset1 = float2(0, _MainTex_TexelSize.y * 1.3846153846 * BLUR_STRENGTH_VERT); float2 offset2 = float2(0, _MainTex_TexelSize.y * 3.2307692308 * BLUR_STRENGTH_VERT);
    #endif

#else
    #define SSR_VERTEX_CROSS_DATA float2 offset1 : TEXCOORD1; float2 offset2 : TEXCOORD2;
    #if defined(SSR_BLUR_HORIZ)
        #define SSR_VERTEX_OUTPUT_GAUSSIAN_UV(o) o.offset1 = float2(_MainTex_TexelSize.x * 1.3846153846 * BLUR_STRENGTH_HORIZ, 0); o.offset2 = float2(_MainTex_TexelSize.x * 3.2307692308 * BLUR_STRENGTH_HORIZ, 0);
    #else
        #define SSR_VERTEX_OUTPUT_GAUSSIAN_UV(o) o.offset1 = float2(0, _MainTex_TexelSize.y * 1.3846153846 * BLUR_STRENGTH_VERT); o.offset2 = float2(0, _MainTex_TexelSize.y * 3.2307692308 * BLUR_STRENGTH_VERT);
    #endif
    #define SSR_FRAG_SETUP_GAUSSIAN_UV(i) float2 offset1 = i.offset1; float2 offset2 = i.offset2;

#endif

	struct AttributesFS {
		float4 positionHCS : POSITION;
		float2 uv          : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

 	struct VaryingsCross {
    	float4 positionCS : SV_POSITION;
    	float2 uv  : TEXCOORD0;
        SSR_VERTEX_CROSS_DATA
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
	};


	VaryingsCross VertBlur(AttributesFS input) {
    	VaryingsCross output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.positionCS = float4(input.positionHCS.xyz, 1.0);

		#if UNITY_UV_STARTS_AT_TOP
		    output.positionCS.y *= -1;
		#endif

    	output.uv = input.uv;

        SSR_VERTEX_OUTPUT_GAUSSIAN_UV(output)

    	return output;
	}


	half4 FragBlur (VaryingsCross input): SV_Target {

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        input.uv = SSRStereoTransformScreenSpaceTex(input.uv);
        SSR_FRAG_SETUP_GAUSSIAN_UV(input)

        float2 uv = input.uv;
        
	    half4 c0 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv);
        half4 c1 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + offset1);
        half4 c2 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - offset1);
        half4 c3 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + offset2);
        half4 c4 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - offset2);

        #if SSR_DENOISE
            half l0 = abs(getLuma(c0.rgb));
            half l1 = abs(getLuma(c1.rgb));
            half l2 = abs(getLuma(c2.rgb));
            half l3 = abs(getLuma(c3.rgb));
            half l4 = abs(getLuma(c4.rgb));

            half ml = (l0+l1+l2+l3+l4) * 0.2;
            c0.rgb *= pow( (1.0 + min(ml, l0)) / (1.0 + l0) , DENOISE_POWER);
            c1.rgb *= pow( (1.0 + min(ml, l1)) / (1.0 + l1) , DENOISE_POWER);
            c2.rgb *= pow( (1.0 + min(ml, l2)) / (1.0 + l2) , DENOISE_POWER);
            c3.rgb *= pow( (1.0 + min(ml, l3)) / (1.0 + l3) , DENOISE_POWER);
            c4.rgb *= pow( (1.0 + min(ml, l4)) / (1.0 + l4) , DENOISE_POWER);
        #endif

        half4 blurred = c0 * 0.2270270270 + (c1 + c2) * 0.3162162162 + (c3 + c4) * 0.0702702703;
   	    return blurred;
	}

#endif // SSR_BLUR