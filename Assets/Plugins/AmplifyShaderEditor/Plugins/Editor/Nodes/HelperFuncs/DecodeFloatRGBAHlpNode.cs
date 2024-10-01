


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Decode Float RGBA"
#else
"解码浮点RGBA"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Decodes RGBA color into a float"
#else
"将RGBA颜色解码为浮点数"
#endif
)]
	public sealed class DecodeFloatRGBAHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "DecodeFloatRGBA";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 0 ].Name = "RGBA";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT, false );
			m_previewShaderGUID = "f71b31b15ff3f2042bafbed40acd29f4";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "decodeFloatRGBA" + OutputId;
		}
	}
}
