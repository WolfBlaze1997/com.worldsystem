


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Transform Params"
#else
"世界变换参数"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"World Transform Params contains information about the transform, W is usually 1.0, or -1.0 for odd-negative scale transforms"
#else
"世界变换参数包含有关变换的信息，W通常为1.0，对于奇数负比例变换为-1.0"
#endif
)]
	public sealed class WorldTransformParams : ConstVecShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, "X" );
			ChangeOutputName( 2, "Y" );
			ChangeOutputName( 3, "Z" );
			ChangeOutputName( 4, "W" );
			m_value = "unity_WorldTransformParams";
			m_previewShaderGUID = "5a2642605f085da458d6e03ade47b87a";
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( !m_outputPorts[ 0 ].IsConnected )
			{
				m_outputPorts[ 0 ].Visible = false;
				m_sizeIsDirty = true;
			}
		}
	}
}
