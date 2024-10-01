


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Luminance"
#else
"亮度"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts color to luminance (grayscale)"
#else
"将颜色转换为亮度（灰度）"
#endif
, Deprecated = true, DeprecatedAlternativeType = typeof( TFHCGrayscale ), DeprecatedAlternative = 
#if !WB_LANGUAGE_CHINESE
"Grayscale"
#else
"灰度"
#endif
)]
	public sealed class LuminanceHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "Luminance";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_inputPorts[ 0 ].Name = "RGB";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "luminance" + OutputId;
		}
	}
}
