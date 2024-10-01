


using UnityEngine;
namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"[VS] Vertex Color"
#else
"[VS]顶点颜色"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vertex Data"
#else
"顶点数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Vertex color. Only works on Vertex Shaders ports ( p.e. Local Vertex Offset Port )."
#else
"顶点颜色。仅适用于顶点着色器端口（例如局部顶点偏移端口）。"
#endif
, null,KeyCode.None,true,true, 
#if !WB_LANGUAGE_CHINESE
"Vertex Color"
#else
"顶点颜色"
#endif
,typeof(VertexColorNode))]
	public sealed class ColorVertexDataNode : VertexDataNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentVertexData = "color";
			ConvertFromVectorToColorPorts();
		}
	}
}
