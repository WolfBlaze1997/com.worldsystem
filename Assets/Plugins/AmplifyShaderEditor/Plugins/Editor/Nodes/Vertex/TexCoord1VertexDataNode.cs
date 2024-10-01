


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"[VS] Vertex TexCoord1"
#else
"[VS]顶点TexCoord1"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vertex Data"
#else
"顶点数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Second set of vertex texture coordinates. Only works on Vertex Shaders ports ( p.e. Local Vertex Offset Port )."
#else
"第二组顶点纹理坐标。仅适用于顶点着色器端口（例如局部顶点偏移端口）。"
#endif
,null,UnityEngine.KeyCode.None,true,true, 
#if !WB_LANGUAGE_CHINESE
"[VS] Vertex TexCoord"
#else
"[VS]顶点TexCoord"
#endif
)]
	public sealed class TexCoord1VertexDataNode : VertexDataNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentVertexData = "texcoord1";
		}
	}
}
