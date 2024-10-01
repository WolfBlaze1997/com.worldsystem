


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Custom Add Node"
#else
"自定义添加节点"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Debug"
#else
"调试"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Custom Node Debug ( Only for debug purposes)"
#else
"自定义节点调试（仅用于调试目的）"
#endif
, null, UnityEngine.KeyCode.None, false )]
	public sealed class CustomAddNode : CustomNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputsFromString( "customOut0", "#IP2*(#IP0 + #IP1 / #IP2)" );
			AddOutputsFromString( "customOut1", "#IP3 + #IP0*#IP2 + #IP1 / #IP2" );
		}
	}
}
