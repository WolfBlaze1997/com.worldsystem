


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Decode View Normal Stereo"
#else
"解码视图普通立体声"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Decodes view space normal from enc4.xy"
#else
"从enc4.xy解码正常视图空间"
#endif
)]
	public sealed class DecodeViewNormalStereoHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "DecodeViewNormalStereo";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";
			m_previewShaderGUID = "e996db1cc4510c84185cb9f933f916bb";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "decodeViewNormalStereo" + OutputId;
		}
	}
}
