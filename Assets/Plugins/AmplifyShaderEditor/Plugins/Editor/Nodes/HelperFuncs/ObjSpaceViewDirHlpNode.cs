


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Object Space View Dir"
#else
"对象空间视图目录"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Object space direction (not normalized) from given object space vertex position towards the camera"
#else
"从给定对象空间顶点位置朝向摄影机的对象空间方向（未归一化）"
#endif
)]
	public sealed class ObjSpaceViewDirHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ObjSpaceViewDir";
			
			m_funcLWFormatOverride = "( mul(GetWorldToObjectMatrix(), float4(_WorldSpaceCameraPos.xyz, 1)).xyz - {0}.xyz )";
			m_funcHDFormatOverride = "( mul(GetWorldToObjectMatrix(), float4(_WorldSpaceCameraPos.xyz, 1)).xyz - {0}.xyz )";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_inputPorts[ 0 ].Vector4InternalData = new UnityEngine.Vector4( 0, 0, 0, 1 );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";
			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );
			m_previewShaderGUID = "c7852de24cec4a744b5358921e23feee";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "objectSpaceViewDir" + OutputId;
		}
	}
}
