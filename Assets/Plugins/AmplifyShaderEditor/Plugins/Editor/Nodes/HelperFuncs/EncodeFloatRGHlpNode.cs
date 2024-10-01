


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Encode Float RG "
#else
"对浮点RG进行编码"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Encodes [0..1] range float into a float2"
#else
"将[0..1]范围浮点数编码为float2"
#endif
)]
	public sealed class EncodeFloatRGHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "EncodeFloatRG ";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_outputPorts[ 0 ].Name = "RG";
			m_previewShaderGUID = "a44b520baa5c39e41bc69a22ea46f24d";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "encodeFloatRG" + OutputId;
		}
	}
}
