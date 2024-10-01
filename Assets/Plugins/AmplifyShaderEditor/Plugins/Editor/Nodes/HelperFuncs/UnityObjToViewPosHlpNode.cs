


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Object To View Pos"
#else
"查看位置对象"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transforms a point from object space to view space"
#else
"将点从对象空间转换到视图空间"
#endif
)]
	public sealed class UnityObjToViewPosHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "UnityObjectToViewPos";
			
			m_funcLWFormatOverride = "TransformWorldToView( TransformObjectToWorld( {0}) )";
			m_funcHDFormatOverride = "TransformWorldToView( TransformObjectToWorld( {0}) )";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";
			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );
			m_previewShaderGUID = "b790bc1d468a51840a9facef372b4729";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "unityObjectToViewPos" + OutputId;
		}
	}
}
