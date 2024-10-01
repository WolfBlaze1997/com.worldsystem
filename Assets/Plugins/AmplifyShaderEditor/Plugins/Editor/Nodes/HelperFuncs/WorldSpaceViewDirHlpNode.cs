


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Space View Dir"
#else
"世界空间视图目录"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"World space direction (not normalized) from given object space vertex position towards the camera"
#else
"从给定对象空间顶点位置朝向摄影机的世界空间方向（未归一化）"
#endif
)]
	public sealed class WorldSpaceViewDirHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "WorldSpaceViewDir";
			
			m_funcLWFormatOverride = "( _WorldSpaceCameraPos.xyz - mul(GetObjectToWorldMatrix(), {0} ).xyz )";
			m_funcHDFormatOverride = "( _WorldSpaceCameraPos.xyz - mul(GetObjectToWorldMatrix(), {0} ).xyz )";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 0 ].Vector4InternalData = new UnityEngine.Vector4( 0, 0, 0, 1 );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";
			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );
			m_previewShaderGUID = "fe0e09756a8a0ba408015b43e66cb8a6";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "worldSpaceViewDir" + OutputId;
		}
	}
}
