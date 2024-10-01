


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Color"
#else
"颜色"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Surface Data"
#else
"地表数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Interpolated per-vertex color"
#else
"逐顶点插值颜色"
#endif
, null, UnityEngine.KeyCode.None, true, true, 
#if !WB_LANGUAGE_CHINESE
"Vertex Color"
#else
"顶点颜色"
#endif
, typeof( VertexColorNode ) )]
	public sealed class ColorInputsNode : SurfaceShaderINParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentInput = SurfaceInputs.COLOR;
			InitialSetup();
		}
	}
}
