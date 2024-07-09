shader "Universal Render Pipeline/EXLitShader"
{
    properties
    {
        [EnumGroup(RenderStateGroup2, _SENIOR, SurfaceOptions, _SurfaceOptions, Cull, _Cull, Blend, _Blend, Depth, _Depth, Stencil, _Stencil, Other, _Other)] RenderStateGroup2 ("表面选项/剔除/混合/深度/蒙版/其他/渲染状态", Vector) = (0, 0, 0, 0)
        [SubToggle(RenderStateGroup2_SurfaceOptions, _RECEIVE_SHADOWS_OFF)] ReceiveShadows_Off ("不接收阴影", float) = 0
        [SubToggle(RenderStateGroup2_SurfaceOptions, _SPECULARHIGHLIGHTS_OFF)] SpecularHighLights_Off ("关闭镜面反射高光", float) = 0
        [SubToggle(RenderStateGroup2_SurfaceOptions, _ENVIRONMENTREFLECTIONS_OFF)] EnvironmentReflections_Off ("关闭环境反射", float) = 0
        [SubToggle(RenderStateGroup2_SurfaceOptions, _SURFACE_TYPE_TRANSPARENT)] SurfaceTypeTransparent_On ("标记为透明", float) = 0
        [SubToggle(RenderStateGroup2_SurfaceOptions, _ALPHATEST_ON)] Alpha_On ("Alpha测试", float) = 0
        [LogicalSub(RenderStateGroup2_SurfaceOptions and RenderStateGroup2_ALPHATEST_ON indent)] _Cutoff ("裁剪偏移", Range(0.0, 1.0)) = 0.5

        // _Cutoff ("裁剪偏移", Range(0.0, 1.0)) = 0.5
        [SubKeywordEnum(RenderStateGroup2_SurfaceOptions, _off, EFFECT_BILLBOARD)] Billboard_Enum ("禁用/LOD公告牌/公告牌(Billboard)", float) = 0
        [SubKeywordEnum(RenderStateGroup2_SurfaceOptions, _off, _SAMPLE_NOISETILING, _SAMPLE_HEXAGONTILING, _SAMPLE_ONESAMPLEONTILING)] Sample_Enum ("关闭/Noise平铺/六边形平铺/抖动采样/消除纹理平铺重复", float) = 0
        [LogicalSubKeywordEnum(RenderStateGroup2_SurfaceOptions and RenderStateGroup2_SAMPLE_NOISETILING indent, _off, _SAMPLE_NOISETILING_USE_NOISEMAP)] NoiseSource_Enum ("函数计算/Noise贴图/干扰源", float) = 0
        [LogicalChannel(RenderStateGroup2_SurfaceOptions and RenderStateGroup2_SAMPLE_NOISETILING and RenderStateGroup2_SAMPLE_NOISETILING_USE_MIXMAP indent)]_UseMixMapChannel_Noise ("使用混合贴图通道", Vector) = (0, 0, 0, 1)
        [LogicalLineTex(RenderStateGroup2_SurfaceOptions and RenderStateGroup2_SAMPLE_NOISETILING and RenderStateGroup2_SAMPLE_NOISETILING_USE_NOISEMAP indent)]_NoiseMap ("干扰贴图", 2D) = "white" { }//rgb:albedo a:alpha
        [LogicalSub(RenderStateGroup2_SAMPLE_NOISETILING or RenderStateGroup2_SAMPLE_HEXAGONTILING indent)]_ScaleOrRotate ("缩放/旋转", Range(-1.0, 1.0)) = 0.3
        
        // [EnumGroup(RenderStateGroup2, _SENIOR)] RenderStateGroup2 ("渲染状态", Vector) = (0, 0, 0, 0)
        // [BuiltinEnum(RenderStateGroup2, CullMode)] _Cull ("剔除模式", float) = 2
        // [SubToggle(RenderStateGroup2, _ALPHATEST_ON)] Alpha_On ("Alpha测试", float) = 0
        // [LogicalSub(RenderStateGroup2_ALPHATEST_ON indent)] _Cutoff ("裁剪偏移", Range(0.0, 1.0)) = 0.5
        // [BuiltinEnum(RenderStateGroup2, BlendMode)] _SrcBlend ("来源RGB混合", Float) = 1
        // [BuiltinEnum(RenderStateGroup2, BlendMode)] _DstBlend ("目标RGB混合", Float) = 0

        [BuiltinEnum(RenderStateGroup2_Cull, CullMode)] _Cull ("剔除模式", float) = 2
        [BuiltinEnum(RenderStateGroup2_Cull, ColorWriteMask)] _ColorMask ("输出颜色通道", Float) = 16
        [BuiltinEnum(RenderStateGroup2_Blend, BlendMode)] _SrcBlend ("来源RGB混合", Float) = 1
        [BuiltinEnum(RenderStateGroup2_Blend, BlendMode)] _DstBlend ("目标RGB混合", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Blend, BlendMode)] _SrcBlendAlpha ("来源Alpha混合", Float) = 1
        [BuiltinEnum(RenderStateGroup2_Blend, BlendMode)] _DstBlendAlpha ("目标Alpha混合", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Blend, BlendOp)]  _BlendOp ("RGB混合操作", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Blend, BlendOp)]  _BlendOpAlpha ("Alpha混合操作", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Depth, CompareFunction)] _ZTest ("深度测试模式", Float) = 4
        [BuiltinEnum(RenderStateGroup2_Depth, Toggle)] _ZWrite ("深度写入模式", float) = 1
        [BuiltinEnum(RenderStateGroup2_Depth, Toggle)] _ZClip ("深度剪辑模式", float) = 1
        [Sub(RenderStateGroup2_Depth)]_ZSlope ("ZSlope", Range(-1.0, 1.0)) = 0.0
        [Sub(RenderStateGroup2_Depth)]_ZBias ("ZBias", Range(-1.0, 1.0)) = 0.0
        [Sub(RenderStateGroup2_Stencil)] _Stencil ("蒙版参考值", Range(0, 255)) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, CompareFunction)] _StencilComp ("蒙版测试模式", Float) = 8
        [Sub(RenderStateGroup2_Stencil)] _StencilWriteMask ("写入Mask", Range(0, 255)) = 255
        [Sub(RenderStateGroup2_Stencil)] _StencilReadMask ("读取Mask", Range(0, 255)) = 255
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _StencilPass ("蒙版测试通过", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _StencilFail ("蒙版测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _StencilZFail ("蒙版测试通过,深度测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, CompareFunction)] _FrontStencilComp ("正面-蒙版测试模式", Float) = 8
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _FrontStencilPass ("正面-蒙版测试通过", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _FrontStencilFail ("正面-蒙版测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _FrontStencilZFail ("正面-蒙版测试通过,深度测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, CompareFunction)] _BackStencilComp ("背面-蒙版测试模式", Float) = 8
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _BackStencilPass ("背面-蒙版测试通过", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _BackStencilFail ("背面-蒙版测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Stencil, StencilOp)] _BackStencilZFail ("背面-蒙版测试通过,深度测试失败", Float) = 0
        [BuiltinEnum(RenderStateGroup2_Other, Toggle)] _AlphaToMask ("AlphaToMask", float) = 0
        [BuiltinEnum(RenderStateGroup2_Other, Toggle)] _Conservative ("保守光栅化", float) = 0

        [EnumGroup(ShaderModelGroup1, _SENIOR, PBR_senior disable_WindEnabled_Enum, _SHADER_PBR, PLANT_senior disable_DetailGroup0 disable_ClearCoatGroup disable_ParallaxGroup0, _SHADER_PLANT)] ShaderModelGroup1 ("通用PBR/场景-植物/着色器模型", Vector) = (0, 0, 0, 0)
        
        [Title(ShaderModelGroup1, Albedo Alpha)][MainTexture][LogicalTex(ShaderModelGroup1, true, RGB_A, _)]_BaseMap("基础贴图", 2D) = "white" {}
        
        [MainColor][LogicalSub(ShaderModelGroup1)]_BaseColor ("基础颜色", Color) = (1.0, 1.0, 1.0, 1.0)//rgb:albedo a:alpha
        [LogicalSub(ShaderModelGroup1)]_SpecColor ("高光颜色", Color) = (1.0, 1.0, 1.0, 1.0)

        [Title(ShaderModelGroup1, Normal Roughness Occlusion)]
        [LogicalTex(ShaderModelGroup1, false, RG_B_A, _)]_NRAMap ("NRA贴图#通用PBR:(法线, 粗糙度, AO)#场景-植物:(法线, 粗糙度, AO)", 2D) = "bump" { }
        [LogicalSubToggle(ShaderModelGroup1_senior, EFFECT_BACKSIDE_NORMALS)]FlipBackNormal_On ("翻转背面法线", float) = 0
        [LogicalSub(ShaderModelGroup1)]_BumpScale ("法线强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1)]_Roughness ("粗糙度", Range(0, 2)) = 1.0
        [LogicalSubToggle(ShaderModelGroup1, _customreflect)] _CustomReflect ("自定义反射", Float) = 0
        [LogicalLineTex(ShaderModelGroup1_customreflect indent)]_CustomReflectMap ("自定义反射贴图", Cube) = "black" { }

        [LogicalSubToggle(ShaderModelGroup1, _OCCLUSION)] Occlusion_On ("环境光遮蔽", Float) = 1
        [LogicalSubToggle(ShaderModelGroup1_senior indent, _AO_MULTI_BOUNCE)]AOMultiBounce_On ("环境光遮蔽多重反弹", float) = 0
        [LogicalSub(ShaderModelGroup1_OCCLUSION indent)]_OcclusionStrength ("环境光遮蔽贴图强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_OCCLUSION indent)]_HorizonOcclusion ("地平线遮蔽(Horizon)强度", Range(0, 10)) = 1.0

        [LogicalTitle(ShaderModelGroup1_SHADER_PBR, Emission Metallic)]
        [LogicalTitle(ShaderModelGroup1_SHADER_PLANT, Emission SubSurfaceWeight)]
        [LogicalTex(ShaderModelGroup1, false, RGB_A, _)]_EmissionMixMap ("自发光混合贴图#通用PBR:(自发光, 金属度)#场景-植物:(自发光, 次表面强度)", 2D) = "black" { }
        [LogicalSub(ShaderModelGroup1_SHADER_PLANT)]_SubsurfaceColor ("散射颜色", Color) = (1.0, 1.0, 1.0, 1.0)
        [LogicalSub(ShaderModelGroup1_SHADER_PLANT)]_SubsurfaceIndirect ("间接光散射", Range(0.0, 1.0)) = 0.25
        [LogicalSub(ShaderModelGroup1_SHADER_PBR or ShaderModelGroup1_SHADER_ALPHATEST)]_Metallic ("金属度", Range(0, 2)) = 1.0
        [HDR][LogicalSub(ShaderModelGroup1)] _EmissionColor ("自发光颜色", Color) = (1, 1, 1, 1)
        [LogicalEmission(ShaderModelGroup1_SENIOR)] EmissionGI_GUI ("自发光GI", Float) = 0

        [LogicalTitle(ShaderModelGroup1_ExtraMixMap_Display, Extra Mixed)]
        [LogicalSubToggle(ShaderModelGroup1, _ExtraMixMap_Display)] ExtraMixMap_Display ("显示额外混合贴图", Float) = 0
        [LogicalTex(ShaderModelGroup1_ExtraMixMap_Display, false, R_G_B_A, _)]_ExtraMixMap ("额外混合贴图", 2D) = "black" { }

        [LogicalSubKeywordEnum(ShaderModelGroup1_SHADER_PLANT, _WINDQUALITY_NONE, _WINDQUALITY_FASTEST, _WINDQUALITY_FAST, _WINDQUALITY_BETTER, _WINDQUALITY_BEST, _WINDQUALITY_PALM)] WindEnabled_Enum ("关闭/最快/快速/更好/最好/棕榈树/风力", Float) = 0

        [LogicalTitle(ShaderModelGroup1_SENIOR, LightingTerm Control)]
        [LogicalSub(ShaderModelGroup1_SENIOR)]_MainDirectDiffuseStrength ("主要灯光漫反射强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_SENIOR)]_MainDirectSpecularStrength ("主要灯光镜面反射强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_SENIOR)]_AddDirectDiffuseStrength ("附加灯光漫反射强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_SENIOR)]_AddDirectSpecularStrength ("附加灯光镜面反射强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_SENIOR)]_IndirectDiffuseStrength ("间接漫反射强度", Range(0, 2)) = 1.0
        [LogicalSub(ShaderModelGroup1_SENIOR)]_IndirectSpecularStrength ("间接镜面反射强度", Range(0, 2)) = 1.0

        
        //////////
        //houdini VAT soft
        [EnumGroup(HoudiniVATGroup0, _SENIOR, Off, _off, HoudiniVATSoft, _HOUDINI_VAT_SOFT)] HoudiniVATGroup0 ("禁用/软体动画/Houdini顶点动画贴图(VAT)", Vector) = (0, 0, 0, 0)
        [LogicalTex(HoudiniVATGroup0_HOUDINI_VAT_SOFT, false, RGB_A, _)]_PositionVATMap ("VAT位置贴图", 2D) = "white" { }
        [LogicalTex(HoudiniVATGroup0_HOUDINI_VAT_SOFT, false, RGB_A, _)]_RotateVATMap ("VAT旋转贴图", 2D) = "white" { }
        [LogicalSubToggle(HoudiniVATGroup0_HOUDINI_VAT_SOFT, _)]_IsPosTexHDR ("位置贴图使用HDR", float) = 1
        [LogicalSubToggle(HoudiniVATGroup0_HOUDINI_VAT_SOFT, _)]_AutoPlay ("自动播放", float) = 1
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_DisplayFrame ("显示帧", float) = 1.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_PlaySpeed ("播放速度", float) = 1.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_AnimatorStrength ("动画幅度", float) = 1.0
        [Title(HoudiniVATGroup0_HOUDINI_VAT_SOFT, Houdini VAT Data)]
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_HoudiniFPS ("Houdini FPS", float) = 24.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_FrameCount ("Frame Count", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMax_X ("Bound Max X", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMax_Y ("Bound Max Y", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMax_Z ("Bound Max Z", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMin_X ("Bound Min X", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMin_Y ("Bound Min Y", float) = 0.0
        [LogicalSub(HoudiniVATGroup0_HOUDINI_VAT_SOFT)]_BoundMin_Z ("Bound Min Z", float) = 0.0

        [EnumGroup(ParallaxGroup0, _SENIOR, Off disable_ParallaxSource_Enum, _off, ParallaxOffset, _PARALLAXMAP)] ParallaxGroup0 ("禁用/视差偏移/视差(UV)", Vector) = (0, 0, 0, 0)
        [LogicalSubKeywordEnum(ParallaxGroup0_PARALLAXMAP, _USE_PARALLAXMAP_PARALLAX, _USE_EXTRAMIXMAP_PARALLAX)] ParallaxSource_Enum ("高度贴图/额外贴图/高度数据源", float) = 0
        [LogicalTex(ParallaxGroup0_PARALLAXMAP and ParallaxGroup0_USE_PARALLAXMAP_PARALLAX, false, RGB_A, _)]_HeightMap ("视差贴图(高度)", 2D) = "Gray" { }
        [LogicalChannel(ParallaxGroup0_PARALLAXMAP and ParallaxGroup0_USE_EXTRAMIXMAP_PARALLAX indent)]_UseMixMapChannel_Parallax ("使用混合贴图通道", Vector) = (1, 0, 0, 0)
        [LogicalSub(ParallaxGroup0_PARALLAXMAP)]_Parallax ("视差强度", Range(0, 1)) = 0.1

        [EnumGroup(DetailGroup0, _SENIOR, Off, _off, Detail1Multi, _DETAIL, Detail2Multi, _DETAIL_2MULTI, Detail4Multi, _DETAIL_4MULTI)] DetailGroup0 ("禁用/一层细节/两层细节/四层细节/细节", Vector) = (0, 0, 0, 0)
        [LogicalTitle(DetailGroup0_DETAIL or DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI, Detail One Layer)]
        [LogicalTex(DetailGroup0_DETAIL or DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI, true, RG_B_A, _)] _DetailMap0 ("1-细节贴图", 2D) = "bump" { }
        [LogicalSub(DetailGroup0_DETAIL or DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI)] _DetailOcclusionStrength0 ("1-细节遮蔽强度", Range(0.0, 2.0)) = 1.0
        [LogicalSub(DetailGroup0_DETAIL or DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI)] _DetailNormalScale0 ("1-细节法线强度", Range(0.0, 2.0)) = 1.0
        [LogicalTitle(DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI, Detail Two Layer)]
        [LogicalTex(DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI, true, RG_B_A, _)] _DetailMap1 ("2-细节贴图", 2D) = "linearGrey" { }
        [LogicalSub(DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI)] _DetailOcclusionStrength1 ("2-细节遮蔽强度", Range(0.0, 2.0)) = 1.0
        [LogicalSub(DetailGroup0_DETAIL_2MULTI or DetailGroup0_DETAIL_4MULTI)] _DetailNormalScale1 ("2-细节法线强度", Range(0.0, 2.0)) = 1.0
        [LogicalTitle(DetailGroup0_DETAIL_4MULTI, Detail Three Layer)]
        [LogicalTex(DetailGroup0_DETAIL_4MULTI, true, RG_B_A, _)] _DetailMap2 ("3-细节贴图", 2D) = "linearGrey" { }
        [LogicalSub(DetailGroup0_DETAIL_4MULTI)] _DetailOcclusionStrength2 ("3-细节遮蔽强度", Range(0.0, 2.0)) = 1.0
        [LogicalSub(DetailGroup0_DETAIL_4MULTI)] _DetailNormalScale2 ("3-细节法线强度", Range(0.0, 2.0)) = 1.0
        [LogicalTitle(DetailGroup0_DETAIL_4MULTI, Detail Four Layer)]
        [LogicalTex(DetailGroup0_DETAIL_4MULTI, true, RG_B_A, _)] _DetailMap3 ("4-细节贴图", 2D) = "linearGrey" { }
        [LogicalSub(DetailGroup0_DETAIL_4MULTI)] _DetailOcclusionStrength3 ("4-细节遮蔽强度", Range(0.0, 2.0)) = 1.0
        [LogicalSub(DetailGroup0_DETAIL_4MULTI)] _DetailNormalScale3 ("4-细节法线强度", Range(0.0, 2.0)) = 1.0
        
        [EnumGroup(ClearCoatGroup0, _SENIOR, Off disable_ClearCoatSource_Enum, _off, On, _CLEARCOAT)] ClearCoatGroup0 ("禁用/启用/清漆", Vector) = (0, 0, 0, 0)
        [LogicalTex(ClearCoatGroup0_CLEARCOAT, false, R_G_B_A, _)]_ClearCoatMap ("清漆贴图", 2D) = "white" { }
        [LogicalSub(ClearCoatGroup0_CLEARCOAT)]_ClearCoatMask ("清漆遮罩", Range(0.0, 1.0)) = 1.0
        [LogicalSub(ClearCoatGroup0_CLEARCOAT)] _ClearCoatSmoothness ("清漆光滑度", Range(0.0, 1.0)) = 1.0


        [EnumGroup(HueVariationGroup0, _SENIOR, Off, _off, On, EFFECT_HUE_VARIATION)] HueVariationGroup0 ("禁用/开启/色调变体", Vector) = (0, 0, 0, 0)
        [LogicalSub(HueVariationGroup0EFFECT_HUE_VARIATION)]_HueVariationColor ("色调变体颜色", Color) = (1.0, 0.5, 0.0, 0.1)

        [EnumGroup(DeBugGroup2, _SENIOR, Display_senior, _display)] DeBugGroup2 ("正常/调试", Vector) = (0, 0, 0, 0)

        [LogicalSub(DeBugGroup2_SENIOR)]_debugVector01 ("_debugVector01", Vector) = (0, 0, 0, 0)
        [LogicalSub(DeBugGroup2_SENIOR)]_debugVector02 ("_debugVector02", Vector) = (0, 0, 0, 0)
        [LogicalSub(DeBugGroup2_SENIOR)]_debugFloat01 ("_debugFloat01", Range(0.0, 1.0)) = 0
        [LogicalSub(DeBugGroup2_SENIOR)]_debugFloat02 ("_debugFloat02", Range(0.0, 1.0)) = 1.0
        [LogicalSub(DeBugGroup2_SENIOR)]_debugFloat03 ("_debugFloat03", Float) = 0
        [LogicalSub(DeBugGroup2_SENIOR)]_debugFloat04 ("_debugFloat04", Float) = 0

        [LogicalKeywordList(_)]_debugFloat05 ("_debugFloat04", Float) = 0
        //SRP批处理程序,需要将内部使用的变量暴露到Properties代码块
        [HideInInspector]_ST_WindVector ("_ST_WindVector", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindGlobal ("_ST_WindGlobal", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindBranch ("_ST_WindBranch", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindBranchTwitch ("_ST_WindBranchTwitch", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindBranchWhip ("_ST_WindBranchWhip", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindBranchAnchor ("_ST_WindBranchAnchor", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindBranchAdherences ("_ST_WindBranchAdherences", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindTurbulences ("_ST_WindTurbulences", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf1Ripple ("_ST_WindLeaf1Ripple", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf1Tumble ("_ST_WindLeaf1Tumble", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf1Twitch ("_ST_WindLeaf1Twitch", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf2Ripple ("_ST_WindLeaf2Ripple", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf2Tumble ("_ST_WindLeaf2Tumble", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindLeaf2Twitch ("_ST_WindLeaf2Twitch", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindFrondRipple ("_ST_WindFrondRipple", Vector) = (0, 0, 0, 0)
        [HideInInspector]_ST_WindAnimation ("_ST_WindAnimation", Vector) = (0, 0, 0, 0)

        // [HideInInspector] _ST_WindVectorHistory ("_ST_WindVectorHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindGlobalHistory ("_ST_WindGlobalHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindBranchHistory ("_ST_WindBranchHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindBranchTwitchHistory ("_ST_WindBranchTwitchHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindBranchWhipHistory ("_ST_WindBranchWhipHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindBranchAnchorHistory ("_ST_WindBranchAnchorHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindBranchAdherencesHistory ("_ST_WindBranchAdherencesHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindTurbulencesHistory ("_ST_WindTurbulencesHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf1RippleHistory ("_ST_WindLeaf1RippleHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf1TumbleHistory ("_ST_WindLeaf1TumbleHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf1TwitchHistory ("_ST_WindLeaf1TwitchHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf2RippleHistory ("_ST_WindLeaf2RippleHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf2TumbleHistory ("_ST_WindLeaf2TumbleHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindLeaf2TwitchHistory ("_ST_WindLeaf2TwitchHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindFrondRippleHistory ("_ST_WindFrondRippleHistory", Vector) = (0, 0, 0, 0)
        // [HideInInspector] _ST_WindAnimationHistory ("_ST_WindAnimationHistory", Vector) = (0, 0, 0, 0)
    }
    // category
    // {
    SubShader
    {
        // PackageRequirements
        // {
        //     "unity" : "2021.3.11" "com.unity.render-pipelines.universal" : "[10.2.1]"
        // }
        Tags { "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
        LOD 300
        /*必须将子着色器按LOD降序排列.例如LOD值为200,100和500的子着色器,必须按照500,200,100排序. https://docs.unity3d.com/cn/current/Manual/SL-ShaderLOD.html */
        pass
        {
            // PackageRequirements
            // {
            //     "unity" : "2021.3.11" "com.unity.render-pipelines.universal" : "[10.2.1]"
            // }
            Name "MainPass"
            Tags { "LightMode" = "UniversalForward" }
            Blend 0 [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            /* Blend [渲染目标RT] [SrcRGB乘值] [DstRGB乘值] , [SrcRGB乘值] [DstRGB乘值]
            一般使用 "Blend [Src乘值] [Dst乘值]" 即可
            混合颜色 = Src*Src乘值 OP Dst*Dst乘值
            Blend SrcAlpha OneMinusSrcAlpha 传统透明度
            Blend One OneMinusSrcAlpha      预乘透明度
            Blend One One                   加法
            Blend OneMinusDstColor One      软加法
            Blend DstColor Zero             乘法
            Blend DstColor SrcColor         2X乘法
            https://docs.unity3d.com/cn/current/Manual/SL-Blend.html */
            BlendOP [_BlendOp], [_BlendOpAlpha] /*https://docs.unity3d.com/cn/current/Manual/SL-BlendOp.html*/
            Cull [_Cull]
            Ztest [_ZTest]
            ZWrite [_ZWrite]
            ZClip [_ZClip]
            /*设置深度剪辑模式
            True(剪辑)将丢弃近远裁面之外的片元.这是默认设置
            False(钳制)比近平面更近的片元正好在近平面,而比远平面更远的片元正好在远平面
            https://docs.unity3d.com/cn/current/Manual/SL-ZClip.html*/
            Offset [_ZSlope], [_ZBias]
            /*有效值:
            [_ZSlope斜率]浮点数,范围–1到1.缩放最大Z斜率,不平行于近剪裁平面和远剪裁平面的
            多边形具有Z斜率,调整此值以避免此类多边形上出现视觉瑕疵.
            [_ZBias偏移]浮点数,范围–1到1.产生恒定的深度偏移,负值意味着GPU将多边形绘制得更靠近摄像机,
            正值意味着GPU将多边形绘制得更远离摄像机
            https://docs.unity3d.com/cn/current/Manual/SL-Offset.html*/
            ColorMask [_ColorMask]
            /*ColorMask [RGBA任意组合] [渲染目标RT]
            https://docs.unity3d.com/cn/current/Manual/SL-ColorMask.html*/
            AlphaToMask [_AlphaToMask]
            /*可以减少将多样本抗锯齿MSAA与使用Alpha测试的着色器(如植被着色器)一起使用时出现的过度锯齿,旨在与MSAA一起使用
            https://docs.unity3d.com/cn/current/Manual/SL-AlphaToMask.html*/
            Conservative [_Conservative]
            /*启用或禁用保守光栅化,通常情况下GPU只对覆盖范围足够的三角形进行光栅化.保守光栅化是指无论范
            围如何都进行光栅化,这在需要确定性时很有用,例如在执行遮挡剔除,GPU上的碰撞检测或可见性检测时
            保守光栅化意味着GPU在三角形边上生成更多的片元,这会导致帧时间增加
            https://docs.unity3d.com/cn/current/Manual/SL-Conservative.html*/
            Stencil
            {
                Ref [_Stencil]             /*(参考值),0到255的整数,和Comp搭配使用*/
                ReadMask [_StencilWriteMask]   /*(读取遮罩)不常用,0到255的整数*/
                WriteMask [_StencilReadMask] /*(写入遮罩)不常用,0到255的整数*/
                Comp [_StencilComp]
                /*(比较)设置通过蒙版测试的规则.
                比较操作值:Never从不,Less小于,LEqual小于等于,Greater大于,
                GEqual大于等于,Equal等于,NotEqual不等于,Always总是.*/
                Pass [_StencilPass]
                /*(通过)设置蒙版测试通过后对蒙版缓冲区的操作.
                蒙版操作值:Keep(保持),Zero(零)将零写入缓冲区,Replace(替代)将参考值写入缓冲区
                IncrSat递增缓冲区中的当前值.如果该值已经是 255,则保持为 255.
                DecrSat递减缓冲区中的当前值.如果该值已经是 0,则保持为 0.
                Invert将缓冲区中当前值的所有位求反,255减去缓冲区的值.
                IncrWrap递增缓冲区中的当前值.如果该值已经是 255,则变为 0.
                DecrWrap递减缓冲区中的当前值.如果该值已经是 0,则变为 255.*/
                Fail [_StencilFail]
                ZFail [_StencilZFail]
                CompFront [_FrontStencilComp]
                PassFront [_FrontStencilPass]
                FailFront [_FrontStencilFail]
                ZFailFront [_FrontStencilZFail]
                CompBack [_BackStencilComp]
                PassBack [_BackStencilPass]
                FailBack [_BackStencilFail]
                ZFailBack [_BackStencilZFail]
                /*如果定义了Comp,Pass,Fail,ZFail,则该值会覆盖单独对Front/Back进行蒙版测试的值,请删除定义
                https://docs.unity3d.com/cn/current/Manual/SL-Stencil.html*/
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature_local _ _PARALLAXMAP
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _ _CLEARCOAT
            #pragma shader_feature_local_fragment _ _OCCLUSION
            #pragma shader_feature_local_fragment _ _AO_MULTI_BOUNCE
            #pragma shader_feature_local_fragment _ _SAMPLE_NOISETILING _SAMPLE_ONESAMPLEONTILING _SAMPLE_HEXAGONTILING
            #pragma shader_feature_local_fragment _ _SAMPLE_NOISETILING_USE_NOISEMAP
            #pragma shader_feature_local_fragment _ _DETAIL _DETAIL_2MULTI _DETAIL_4MULTI
            #pragma shader_feature_local _ EFFECT_BILLBOARD
            #pragma shader_feature_local _ EFFECT_HUE_VARIATION
            #pragma shader_feature_local_fragment _ EFFECT_BACKSIDE_NORMALS
            #pragma shader_feature_local_fragment _ _USE_EXTRAMIXMAP_PARALLAX
            #pragma shader_feature_local_vertex _ _HOUDINI_VAT_SOFT

            //用于植物Shader
            // #pragma shader_feature_local _ _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM

            // -------------------------------------
            /*Universal Pipeline keywords*/
            //ok
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //URP15新增
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            //ok
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            //URP15新增了软阴影的质量
            //ok
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            //ok
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            //ok
            #pragma multi_compile _ _LIGHT_LAYERS
            //支持Forward+渲染通道
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
            #pragma target 4.5 _WRITE_RENDERING_LAYERS
            //注意这两个关键字在URP15中已被删除.这里保留是为了在URP12中保持兼容
            //ok
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            //ok
            #pragma multi_compile _ _CLUSTERED_RENDERING

            // -------------------------------------
            /*Unity defined keywords*/
            //ok
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //ok
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            //URP15新增,用于支持体积探针
            #pragma multi_compile _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma target 4.5 PROBE_VOLUMES_L1
            #pragma target 4.5 PROBE_VOLUMES_L2

            // -------------------------------------
            /*GPU Instancing*/
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            //URP15修改,用于支持Dost
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif

            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _ _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM
            #pragma shader_feature_local _ EFFECT_BILLBOARD
            #pragma shader_feature_local_vertex _ _HOUDINI_VAT_SOFT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            //URP15新增
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif

            // -------------------------------------
            // Universal Pipeline keywords
            // 这在阴影贴图生成过程中用于区分定向和电光源灯光阴影，因为它们使用不同的公式来应用“法线偏移”
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW


            #define SHADOWCASTER_PASS
            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"
            ENDHLSL
        }


        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _ _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM
            #pragma shader_feature_local _ EFFECT_BILLBOARD
            #pragma shader_feature_local_vertex _ _HOUDINI_VAT_SOFT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            //URP15新增
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif // HAVE_VFX_MODIFICATION

            #define DEPTHONLY_PASS
            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"
            ENDHLSL
        }
        // // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature_local_fragment _ _ALPHATEST_ON
            // #pragma shader_feature_local _ _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM
            #pragma shader_feature_local _ EFFECT_BILLBOARD
            #pragma shader_feature_local _ _DETAIL _DETAIL_2MULTI _DETAIL_4MULTI
            #pragma shader_feature_local _ _PARALLAXMAP
            #pragma shader_feature_local_fragment _ _USE_EXTRAMIXMAP_PARALLAX
            #pragma shader_feature_local_fragment _ _SAMPLE_NOISETILING _SAMPLE_HEXAGONTILING
            #pragma shader_feature_local_fragment _ _SAMPLE_NOISETILING_USE_NOISEMAP
            #pragma shader_feature_local_fragment _ EFFECT_BACKSIDE_NORMALS
            #pragma shader_feature_local_vertex _ _HOUDINI_VAT_SOFT

            // -------------------------------------
            /*Universal Pipeline keywords*/
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            //URP15新增
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif

            #define DEPTHNORMAL_PASS
            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _DETAIL _DETAIL_2MULTI _DETAIL_4MULTI
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

            #define META_PASS
            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SHADER_PBR _SHADER_PLANT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _ _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM
            #pragma shader_feature_local _ EFFECT_BILLBOARD
            #pragma shader_feature_local_vertex _ _HOUDINI_VAT_SOFT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            //URP15新增
            #ifndef HAVE_VFX_MODIFICATION
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #if UNITY_PLATFORM_ANDROID || UNITY_PLATFORM_WEBGL || UNITY_PLATFORM_UWP
                    #pragma target 3.5 DOTS_INSTANCING_ON
                #else
                    #pragma target 4.5 DOTS_INSTANCING_ON
                #endif
            #endif

            #define SCENESELECTION_PASS
            #include "./Main/Input.hlsl"
            #include "./Main/MainPass.hlsl"
            ENDHLSL
        }

        // //DeBug使用
        // Pass
        // {
        //     Name "ShadowCaster"
        //     Tags { "LightMode" = "ShadowCaster" }
        //     ZWrite On
        //     ZTest LEqual
        //     ColorMask 0
        //     Cull[_Cull]
        //     HLSLPROGRAM
        //     #pragma target 2.0
        //     #pragma vertex ShadowPassVertex
        //     #pragma fragment ShadowPassFragment
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        //     #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
        //     struct Attributes
        //     {
        //         float4 positionOS : POSITION;
        //         float3 normalOS : NORMAL;
        //         float2 texcoord : TEXCOORD0;
        //         UNITY_VERTEX_INPUT_INSTANCE_ID
        //     };
        //     struct Varyings
        //     {
        //         float2 uv : TEXCOORD0;
        //         float4 positionCS : SV_POSITION;
        //     };
        //     // TEXTURE2D(_NRAMap); SAMPLER(sampler_NRAMap);
        //     // float4 _NRAMap_ST;float _Cutoff;
        //     TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        //     float4 _BaseMap_ST;float _Cutoff;
        //     Varyings ShadowPassVertex(Attributes input)
        //     {
        //         Varyings output;
        //         UNITY_SETUP_INSTANCE_ID(input);
        //         output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
        //         output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        //         return output;
        //     }
        //     half4 ShadowPassFragment(Varyings input) : SV_TARGET
        //     {
        //         #if defined(_ALPHATEST_ON)
        //             clip(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a - _Cutoff);
        //         #endif
        //         return 0;
        //     }
        //     ENDHLSL
        // }
        // Pass
        // {
        //     Name "ForwardLit"
        //     Tags { "LightMode" = "UniversalForward" }
        //     HLSLPROGRAM
        //     #pragma target 2.0
        //     #pragma vertex LitPassVertex
        //     #pragma fragment LitPassFragment
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        //     #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        //     #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
        //     struct Attributes
        //     {
        //         float4 positionOS : POSITION;
        //         float3 normalOS : NORMAL;
        //         float2 texcoord : TEXCOORD0;
        //         UNITY_VERTEX_INPUT_INSTANCE_ID
        //     };
        //     struct Varyings
        //     {
        //         float2 uv : TEXCOORD0;
        //         float4 positionCS : SV_POSITION;
        //     };
        //     TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        //     float4 _BaseMap_ST;float _Cutoff;
        //     Varyings LitPassVertex(Attributes input)
        //     {
        //         Varyings output;
        //         UNITY_SETUP_INSTANCE_ID(input);
        //         output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
        //         output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        //         return output;
        //     }
        //     half4 LitPassFragment(Varyings input) : SV_TARGET
        //     {
        //         // Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
        //         // #ifdef LOD_FADE_CROSSFADE
        //         //     LODFadeCrossFade(input.positionCS);
        //         // #endif
        //         #if defined(_ALPHATEST_ON)
        //             clip(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a - _Cutoff);
        //         #endif
        //         return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).aaaa;
        //     }
        //     ENDHLSL
        // }

    }
    CustomEditor "LogicalSGUI.LogicalSGUI"

    // }
    // FallBack "Hidden/Universal Render Pipeline/FallbackError"

}
