





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Tau"
#else
"陶"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Constants And Properties"
#else
"常数和属性"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Tau constant (2*PI): 6.28318530718"
#else
"Tau常数（2*PI）：6.28318530718"
#endif
, null, KeyCode.None, true, false, null,null, 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
	public sealed class TauNode : ParentNode
	{
		private readonly string Tau = ( 2.0 * Mathf.PI ).ToString();
		public TauNode() : base() { }
		public TauNode( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_previewShaderGUID = "701bc295c0d75d8429eabcf45e8e008d";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			return dataCollector.IsSRP? "TWO_PI": Tau;
		}
	}
}
