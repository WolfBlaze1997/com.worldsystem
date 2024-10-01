


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Encode Float RGBA"
#else
"对浮点数RGBA进行编码"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Encodes [0..1] range float into RGBA color, for storage in low precision render target"
#else
"将[0..1]范围浮点数编码为RGBA颜色，以存储在低精度渲染目标中"
#endif
)]
	public sealed class EncodeFloatRGBAHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "EncodeFloatRGBA";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].Name = "RGBA";
			m_previewShaderGUID = "c21569bf5b9371b4ca13c0c00abd5562";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "encodeFloatRGBA" + OutputId;
		}
	}
}
