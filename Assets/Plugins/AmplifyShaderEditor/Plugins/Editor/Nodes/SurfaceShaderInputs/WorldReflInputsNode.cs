

using UnityEngine;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"[Deprecated] World Reflection"
#else
"[弃用]世界反思"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Surface Data"
#else
"地表数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"World reflection vector"
#else
"世界反射矢量"
#endif
, null, KeyCode.None, true, true, 
#if !WB_LANGUAGE_CHINESE
"World Reflection"
#else
"世界反思"
#endif
, typeof( WorldReflectionVector ) )]
	public sealed class WorldReflInputsNode : SurfaceShaderINParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentInput = SurfaceInputs.WORLD_REFL;
			InitialSetup();
		}
	}
}
