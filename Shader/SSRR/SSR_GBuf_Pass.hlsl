#ifndef SSR_GBUF_PASS
#define SSR_GBUF_PASS

	// Copyright 2021 Kronnect - All Rights Reserved.

#if UNITY_VERSION < 202100
	#define kMaterialFlagSpecularSetup 0
    #define UnpackMaterialFlags(x) x
#else
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#endif

    TEXTURE2D(_NoiseTex);
    float4 _NoiseTex_TexelSize;

    float4 _MaterialData;
    #define SMOOTHNESS _MaterialData.x
    #define FRESNEL _MaterialData.y
    #define FUZZYNESS _MaterialData.z
    #define DECAY _MaterialData.w

    float4 _SSRSettings;
    #define THICKNESS _SSRSettings.x
    #define SAMPLES _SSRSettings.y
    #define BINARY_SEARCH_ITERATIONS _SSRSettings.z
    #define MAX_RAY_LENGTH _SSRSettings.w

    float3 _SSRSettings5;
    #define REFLECTIONS_THRESHOLD _SSRSettings5.y
    #define SKYBOX_INTENSITY _SSRSettings5.z

#if SSR_THICKNESS_FINE
    #define THICKNESS_FINE _SSRSettings5.x
#else
    #define THICKNESS_FINE THICKNESS
#endif

    float4 _SSRSettings2;
    #define JITTER _SSRSettings2.x
    #define CONTACT_HARDENING _SSRSettings2.y

    float4 _SSRSettings3;
    #define INPUT_SIZE _SSRSettings3.xy
    #define GOLDEN_RATIO_ACUM _SSRSettings3.z
    #define DEPTH_BIAS _SSRSettings3.w

    float4x4 _WorldToViewDir;
    float4x4 _ViewToWorldDir;

    float3 _SSRBoundsMin, _SSRBoundsSize;
    #define BOUNDS_MIN _SSRBoundsMin
    #define BOUNDS_SIZE _SSRBoundsSize
    
    TEXTURE2D_X(_GBuffer0);
    TEXTURE2D_X(_GBuffer1);
    TEXTURE2D_X(_GBuffer2);
    TEXTURE2D_X(_SmoothnessMetallicRT);
    TEXTURE2D(_MetallicGradientTex);
    TEXTURE2D(_SmoothnessGradientTex);

	struct AttributesFS {
		float4 positionHCS : POSITION;
		float4 uv          : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

 	struct VaryingsSSR {
    	float4 positionCS : SV_POSITION;
    	float4 uv  : TEXCOORD0;
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
        float4 projPos = output.positionCS * 0.5;
        projPos.xy = projPos.xy + projPos.w;
        output.uv.zw = projPos.xy;
    	return output;
	}

    float collision;

#if SSR_METALLIC_WORKFLOW
	float4 SSR_Pass(float2 uv, float3 normalVS, float3 rayStart, float3 viewDirVS, float roughness, float reflectivity) {
#else
	float4 SSR_Pass(float2 uv, float3 normalVS, float3 rayStart, float3 viewDirVS, float roughness) {
#endif

        float3 rayDir = reflect( viewDirVS, normalVS );

        // if ray is toward the camera, early exit (optional)
        //if (rayDir.z < 0) return 0.0.xxxx;

        float  rayLength = MAX_RAY_LENGTH;

        float3 rayEnd = rayStart + rayDir * rayLength;
        if (rayEnd.z < _ProjectionParams.y) {
            rayLength = (_ProjectionParams.y - rayStart.z) / rayDir.z;
            rayEnd = rayStart + rayDir * rayLength;
        }

        float4 sposStart = mul(unity_CameraProjection, float4(rayStart, 1.0));
        float4 sposEnd = mul(unity_CameraProjection, float4(rayEnd, 1.0));
        float k0 = rcp(sposStart.w);
        float q0 = rayStart.z * k0;
        float k1 = rcp(sposEnd.w);
        float q1 = rayEnd.z * k1;
        float4 p = float4(uv, q0, k0);

        // length in pixels
        float2 uv1 = (sposEnd.xy * rcp(rayEnd.z) + 1.0) * 0.5;
        float2 duv = uv1 - uv;
        float2 duvPixel = abs(duv * INPUT_SIZE);
        float pixelDistance = max(duvPixel.x, duvPixel.y);
        int sampleCount = (int)clamp(pixelDistance, 1, SAMPLES);
        float4 pincr = float4(duv, q1-q0, k1-k0) * rcp(sampleCount);

        #if SSR_JITTER
            float jitter = SAMPLE_TEXTURE2D(_NoiseTex, sampler_PointRepeat, uv * INPUT_SIZE * _NoiseTex_TexelSize.xy + GOLDEN_RATIO_ACUM).r;
            pincr *= 1.0 + jitter * JITTER;
            p += pincr * (jitter * JITTER);
        #endif

        float dist = 0;
        float zdist = 0;
        float sceneDepth, sceneBackDepth, depthDiff;

        UNITY_LOOP
        for (int k = 0; k < sampleCount; k++) {
            p += pincr;
            if (any(floor(p.xy)!=0)) return 0.0.xxxx; // exit if out of screen space
            float pz = p.z / p.w;

            #if SSR_BACK_FACES
                GetLinearDepths(p.xy, sceneDepth, sceneBackDepth);
                if (pz >= sceneDepth && pz <= sceneBackDepth) {
            #else
                sceneDepth = GetLinearDepth(p.xy);
                depthDiff = pz - sceneDepth;
                if (depthDiff > 0 && depthDiff < THICKNESS) {
            #endif
                float4 origPincr = pincr;
                p -= pincr;
                float reduction = 1.0;
                UNITY_LOOP
                for (int j = 0; j < BINARY_SEARCH_ITERATIONS; j++) {
                    reduction *= 0.5;
                    p += pincr * reduction;
                    pz = p.z / p.w;
                    sceneDepth = GetLinearDepth(p.xy);
                    depthDiff = sceneDepth - pz;
                    pincr = sign(depthDiff) * origPincr;
                }
#if SSR_THICKNESS_FINE
                if (abs(depthDiff) < THICKNESS_FINE)
#endif
                {
                    float hitAccuracy = 1.0 - abs(depthDiff) / THICKNESS_FINE;
                zdist = (pz - rayStart.z) / (0.0001 + rayEnd.z - rayStart.z);
                float rayFade = 1.0 - saturate(zdist);
                collision = hitAccuracy * rayFade;
                break;
                }
                pincr = origPincr;
                p += pincr;
            }
        }

        if (collision > 0) {

            // intersection found

            #if SSR_METALLIC_WORKFLOW
                float reflectionIntensity = reflectivity;
            #else
                float reflectionIntensity = (1.0 - roughness);
            #endif
            reflectionIntensity *= pow(collision, DECAY);

            // compute fresnel
            float fresnel = 1.0 - FRESNEL * abs(dot(normalVS, viewDirVS));
            float reflectionAmount = reflectionIntensity * fresnel; 

            // compute blur amount
            float wdist = rayLength * zdist;
            float blurAmount = max(0, wdist - CONTACT_HARDENING) * FUZZYNESS * roughness;
            
            // return hit pixel
            return float4(p.xy, blurAmount + 0.001, reflectionAmount);
        }

        return float4(0,0,0,0);
	}


	float4 FragSSR (VaryingsSSR input) : SV_Target {

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, input.uv.xy).r;
        #if UNITY_REVERSED_Z
            depth = 1.0 - depth;
        #endif
        if (depth >= 1.0) return float4(0,0,0,0);

        depth = 2.0 * depth - 1.0;
        float2 zw = SSRStereoTransformScreenSpaceTex(input.uv.zw);
        float3 positionVS = ComputeViewSpacePosition(zw, depth, unity_CameraInvProjection);

        #if SSR_LIMIT_BOUNDS
            float3 positionWS = mul(_ViewToWorldDir , float4(positionVS.x, positionVS.y, -positionVS.z, 1.0)).xyz;
            float3 inside = (positionWS - BOUNDS_MIN) / BOUNDS_SIZE;
            if (any(floor(inside)!=0)) return 0;
        #endif

        float2 uv = SSRStereoTransformScreenSpaceTex(input.uv.xy);

        float3 normalWS;

        #if SSR_CUSTOM_SMOOTHNESS_METALLIC_PASS
            float3 normalVS = SampleSceneNormals(uv);
            #if SSR_SKYBOX
                normalWS = mul((float3x3)_ViewToWorldDir, normalVS);
            #endif
        #else
            float4 normals = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, sampler_PointClamp, uv, 0);
            #if defined(_GBUFFER_NORMALS_OCT)
                half2 remappedOctNormalWS = Unpack888ToFloat2(normals.xyz); // values between [ 0,  1]
                half2 octNormalWS = remappedOctNormalWS.xy * 2.0h - 1.0h;    // values between [-1, +1]
                normalWS = UnpackNormalOctQuadEncode(octNormalWS);
            #else
                normalWS = normals.xyz;
            #endif
            float3 normalVS = mul((float3x3)_WorldToViewDir, normalWS);
            normalVS.z *= -1.0;
        #endif

        collision = -1;
        float3 viewDirVS = normalize(positionVS);

        // get physically attributes
        #if SSR_METALLIC_WORKFLOW
            #if SSR_CUSTOM_SMOOTHNESS_METALLIC_PASS
                float2 smoothnessMetallic = SAMPLE_TEXTURE2D_X(_SmoothnessMetallicRT, sampler_PointClamp, uv).rg;
                float smoothness = smoothnessMetallic.r;
                float reflectivity = smoothnessMetallic.g;
            #else
                float4 gbuffer0 = SAMPLE_TEXTURE2D_X(_GBuffer0, sampler_PointClamp, uv);
                float4 gbuffer1 = SAMPLE_TEXTURE2D_X(_GBuffer1, sampler_PointClamp, uv);
                float reflectivity, smoothness = normals.w;
                uint materialFlags = UnpackMaterialFlags(gbuffer0.a);
                if (materialFlags & kMaterialFlagSpecularSetup) {
                    reflectivity = max(gbuffer1.r, max(gbuffer1.g, gbuffer1.b));
                } else {
                    reflectivity = gbuffer1.r;
                }
            #endif

            reflectivity = SAMPLE_TEXTURE2D_LOD(_MetallicGradientTex, sampler_LinearClamp, float2(reflectivity, 0), 0).r;
            if (reflectivity <= 0) return 0;

            float roughness = SAMPLE_TEXTURE2D_LOD(_SmoothnessGradientTex, sampler_LinearClamp, float2(1.0 - smoothness, 0), 0).r;
       		float4 reflection = SSR_Pass(input.uv.xy, normalVS, positionVS, viewDirVS, roughness, reflectivity);
        #else
            #if SSR_CUSTOM_SMOOTHNESS_METALLIC_PASS
                float smoothness = SAMPLE_TEXTURE2D_X(_SmoothnessMetallicRT, sampler_PointClamp, uv).r;
            #else
                float smoothness = normals.w;
            #endif
            float roughness = 1.0 - max(0, smoothness - REFLECTIONS_THRESHOLD);
            if (roughness >= 1.0) return 0;

   		    float4 reflection = SSR_Pass(input.uv.xy, normalVS, positionVS, viewDirVS, roughness);
        #endif
        
        #if SSR_SKYBOX
        if (collision < 0) {
            viewDirVS.z *= -1;
            float3 viewDirWS = mul((float3x3)_ViewToWorldDir, viewDirVS);
            float3 reflDir = reflect(viewDirWS, normalWS);
            reflection.xyz = reflDir;
            float sm = 1.0 - roughness;
            reflection.w = -sm;
        }
        #endif

        return reflection;
	}


#endif // SSR_GBUF_PASS