


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Object To Clip Pos"
#else
"要剪切位置的对象"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transforms a point from object space to the camera’s clip space in homogeneous coordinates"
#else
"在齐次坐标系中将点从对象空间变换到摄影机的剪辑空间"
#endif
)]
	public sealed class UnityObjToClipPosHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "UnityObjectToClipPos";
			
			m_funcLWFormatOverride = "TransformWorldToHClip(TransformObjectToWorld({0}))";
			m_funcHDFormatOverride = "TransformWorldToHClip(TransformObjectToWorld({0}))";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].Name = "XYZW";
			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );
			AddOutputPort( WirePortDataType.FLOAT, "W" );
			m_previewShaderGUID = "14ec765a147a53340877b489e73f1c9f";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "unityObjectToClipPos" + OutputId;
		}
	}
}
