


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Encode View Normal Stereo"
#else
"编码视图普通立体声"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Encodes view space normal into two numbers in [0..1] range"
#else
"将视图空间法线编码为[0..1]范围内的两个数字"
#endif
)]
	public sealed class EncodeViewNormalStereoHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "EncodeViewNormalStereo";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_inputPorts[ 0 ].Name = "XYZ";
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_previewShaderGUID = "3d0b3d482b7246c4cb60fa73e6ceac6c";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "encodeViewNormalStereo" + OutputId;
		}
	}
}
