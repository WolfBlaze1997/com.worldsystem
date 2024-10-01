


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	public enum DisableBatchingTagValues
	{
		True,
		False,
		LODFading
	}

	[Serializable]
	public class RenderingOptionsOpHelper
	{
		private const string RenderingOptionsStr = 
#if !WB_LANGUAGE_CHINESE
" Rendering Options"
#else
"渲染选项"
#endif
;
		private readonly static GUIContent EmissionGIFlags = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Emission GI Flag"
#else
"排放GI标志"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Modifies Emission GI flags"
#else
"修改排放GI标志"
#endif
);
		private readonly static GUIContent LODCrossfadeContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" LOD Group Cross Fade"
#else
"LOD组交叉褪色"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Applies a dither crossfade to be used with LOD groups for smoother transitions. Uses one interpolator\nDefault: OFF"
#else
"应用与LOD组一起使用的抖动交叉效果，以实现更平滑的过渡。使用一个插值器\n默认值：OFF"
#endif
);
		private readonly static GUIContent DisableBatchingContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Disable Batching"
#else
"禁用批处理"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nDisables objects to be batched and used with DrawCallBatching Default: False"
#else
"\n禁用要批处理并与DrawCallBatching一起使用的对象默认值：False"
#endif
);
		private readonly static GUIContent IgnoreProjectorContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Ignore Projector"
#else
"忽略投影仪"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True then an object that uses this shader will not be affected by Projectors Default: False"
#else
"\n如果为True，则使用此着色器的对象将不受投影仪默认值的影响：False"
#endif
);
		private readonly static GUIContent UseDefaultCasterContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Use Default Shadow Caster"
#else
"使用默认阴影投射器"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True always use surface default shadow caster Default: False"
#else
"\n如果为True，则始终使用曲面默认阴影投射器默认值：False"
#endif
);
		private readonly static GUIContent ForceNoShadowCastingContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Force No Shadow Casting"
#else
"强制不投射阴影"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True then an object that is rendered using this subshader will never cast shadows Default: False"
#else
"\n如果为True，则使用此子着色器渲染的对象将永远不会投射阴影默认值：False"
#endif
);
		private readonly static GUIContent ForceEnableInstancingContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Force Enable Instancing"
#else
"强制启用实例化"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True forces instancing on shader independent of having instanced properties"
#else
"\n如果True强制在着色器上实例化，而与实例化属性无关"
#endif
);
		private readonly static GUIContent ForceDisableInstancingContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Force Disable Instancing"
#else
"强制禁用实例化"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True forces disable instancing on shader independent of having instanced properties"
#else
"\n如果True强制禁用着色器上的实例化，而与实例化属性无关"
#endif
);
		private readonly static GUIContent SpecularHightlightsContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Fwd Specular Highlights Toggle"
#else
"Fwd镜面反射高光切换"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True creates a material toggle to set Unity's internal specular highlight rendering keyword"
#else
"\n如果为True，则创建材质切换以设置Unity的内部镜面高光渲染关键字"
#endif
);
		private readonly static GUIContent ReflectionsContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Fwd Reflections Toggle"
#else
"Fwd反射切换"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"\nIf True creates a material toggle to set Unity's internal reflections rendering keyword"
#else
"\n如果为True，则创建材质切换以设置Unity的内部反射渲染关键字"
#endif
);

		[SerializeField]
		private bool m_forceEnableInstancing = false;

		[SerializeField]
		private bool m_forceDisableInstancing = false;

		[SerializeField]
		private bool m_specularHighlightToggle = false;

		[SerializeField]
		private bool m_reflectionsToggle = false;

		[SerializeField]
		private bool m_lodCrossfade = false;

		[SerializeField]
		private DisableBatchingTagValues m_disableBatching = DisableBatchingTagValues.False;

		[SerializeField]
		private bool m_ignoreProjector = false;

		[SerializeField]
		private bool m_useDefaultShadowCaster = false;

		[SerializeField]
		private bool m_forceNoShadowCasting = false;

		[SerializeField]
		private List<CodeGenerationData> m_codeGenerationDataList;
		
		public RenderingOptionsOpHelper()
		{
			m_codeGenerationDataList = new List<CodeGenerationData>();
			m_codeGenerationDataList.Add( new CodeGenerationData( " Exclude Deferred", "exclude_path:deferred" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Exclude Forward", "exclude_path:forward" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Exclude Legacy Deferred", "exclude_path:prepass" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Shadows", "noshadow" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Ambient Light", "noambient" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Per Vertex Light", "novertexlights" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Lightmaps", "nolightmap " ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Dynamic Global GI", "nodynlightmap" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Directional lightmaps", "nodirlightmap" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Built-in Fog", "nofog" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Meta Pass", "nometa" ) );
			m_codeGenerationDataList.Add( new CodeGenerationData( " Add Pass", "noforwardadd" ) );
		}

		public bool IsOptionActive( string option )
		{
			return !m_codeGenerationDataList.Find( x => x.Name.Equals( option ) ).IsActive;
		}

		public void Draw( StandardSurfaceOutputNode owner )
		{
			bool value = owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedRenderingOptions;
			NodeUtils.DrawPropertyGroup( ref value, RenderingOptionsStr, () =>
			{
				int codeGenCount = m_codeGenerationDataList.Count;
				
				for( int i = 4; i < codeGenCount; i++ )
				{
					m_codeGenerationDataList[ i ].IsActive = !owner.EditorGUILayoutToggleLeft( m_codeGenerationDataList[ i ].Name, !m_codeGenerationDataList[ i ].IsActive );
				}
				m_lodCrossfade = owner.EditorGUILayoutToggleLeft( LODCrossfadeContent, m_lodCrossfade );
				m_ignoreProjector = owner.EditorGUILayoutToggleLeft( IgnoreProjectorContent, m_ignoreProjector );
				EditorGUI.BeginDisabledGroup( !owner.CastShadows );
				m_useDefaultShadowCaster = owner.EditorGUILayoutToggleLeft( UseDefaultCasterContent, m_useDefaultShadowCaster );
				EditorGUI.EndDisabledGroup();
				m_forceNoShadowCasting = owner.EditorGUILayoutToggleLeft( ForceNoShadowCastingContent, m_forceNoShadowCasting );
				if( owner.ContainerGraph.IsInstancedShader )
				{
					GUI.enabled = false;
					owner.EditorGUILayoutToggleLeft( ForceEnableInstancingContent, true );
					GUI.enabled = true;
				}
				else
				{
					m_forceEnableInstancing = owner.EditorGUILayoutToggleLeft( ForceEnableInstancingContent, m_forceEnableInstancing );
				}

				m_forceDisableInstancing = owner.EditorGUILayoutToggleLeft( ForceDisableInstancingContent, m_forceDisableInstancing );
				m_specularHighlightToggle = owner.EditorGUILayoutToggleLeft( SpecularHightlightsContent, m_specularHighlightToggle );
				m_reflectionsToggle = owner.EditorGUILayoutToggleLeft( ReflectionsContent, m_reflectionsToggle );
				m_disableBatching = (DisableBatchingTagValues)owner.EditorGUILayoutEnumPopup( DisableBatchingContent, m_disableBatching );
				Material mat = owner.ContainerGraph.CurrentMaterial;
				if( mat != null )
				{
					mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)owner.EditorGUILayoutEnumPopup( EmissionGIFlags, mat.globalIlluminationFlags );
				}
			} );
			owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedRenderingOptions = value;
		}

		public void Build( ref string OptionalParameters )
		{
			int codeGenCount = m_codeGenerationDataList.Count;

			for( int i = 0; i < codeGenCount; i++ )
			{
				if( m_codeGenerationDataList[ i ].IsActive )
				{
					OptionalParameters += m_codeGenerationDataList[ i ].Value + Constants.OptionalParametersSep;
				}
			}

			if( m_lodCrossfade )
			{
				OptionalParameters += Constants.LodCrossFadeOption2017 + Constants.OptionalParametersSep;
			}
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			for( int i = 0; i < m_codeGenerationDataList.Count; i++ )
			{
				m_codeGenerationDataList[ i ].IsActive = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 10005 )
			{
				m_lodCrossfade = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 10007 )
			{
				m_disableBatching = (DisableBatchingTagValues)Enum.Parse( typeof( DisableBatchingTagValues ), nodeParams[ index++ ] );
				m_ignoreProjector = Convert.ToBoolean( nodeParams[ index++ ] );
				m_forceNoShadowCasting = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 11002 )
			{
				m_forceEnableInstancing = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 15205 )
			{
				m_forceDisableInstancing = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 14403 )
			{
				m_specularHighlightToggle = Convert.ToBoolean( nodeParams[ index++ ] );
				m_reflectionsToggle = Convert.ToBoolean( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 16307 )
			{
				m_useDefaultShadowCaster = Convert.ToBoolean( nodeParams[ index++ ] );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			for( int i = 0; i < m_codeGenerationDataList.Count; i++ )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_codeGenerationDataList[ i ].IsActive );
			}

			IOUtils.AddFieldValueToString( ref nodeInfo, m_lodCrossfade );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_disableBatching );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_ignoreProjector );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_forceNoShadowCasting );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_forceEnableInstancing );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_forceDisableInstancing );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_specularHighlightToggle );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_reflectionsToggle );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_useDefaultShadowCaster );
		}
		
		public void Destroy()
		{
			m_codeGenerationDataList.Clear();
			m_codeGenerationDataList = null;
		}
		public bool UseDefaultShadowCaster { get { return m_useDefaultShadowCaster; } }
		public bool ForceEnableInstancing { get { return m_forceEnableInstancing; } }
		public bool ForceDisableInstancing { get { return m_forceDisableInstancing; } }

		public bool LodCrossfade { get { return m_lodCrossfade; } }
		public bool IgnoreProjectorValue { get { return m_ignoreProjector; } set { m_ignoreProjector = value; } }
		public bool SpecularHighlightToggle { get { return m_specularHighlightToggle; } set { m_specularHighlightToggle = value; } }
		public bool ReflectionsToggle { get { return m_reflectionsToggle; } set { m_reflectionsToggle = value; } }

		public string DisableBatchingTag { get { return ( m_disableBatching != DisableBatchingTagValues.False ) ? string.Format( Constants.TagFormat, "DisableBatching", m_disableBatching ) : string.Empty; } }
		public string IgnoreProjectorTag { get { return ( m_ignoreProjector ) ? string.Format( Constants.TagFormat, "IgnoreProjector", "True" ) : string.Empty; } }
		public string ForceNoShadowCastingTag { get { return ( m_forceNoShadowCasting ) ? string.Format( Constants.TagFormat, "ForceNoShadowCasting", "True" ) : string.Empty; } }
	}
}
