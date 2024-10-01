


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "PI",            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Constants And Properties"
#else
"常数和属性"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"PI constant : 3.14159265359"
#else
"PI常数：3.14159265359"
#endif
)]
	public sealed class PiNode : ParentNode
	{
		public PiNode() : base() { }
		public PiNode( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, true, 
#if !WB_LANGUAGE_CHINESE
"Multiplier"
#else
"乘数"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT, Constants.EmptyPortValue );
			m_textLabelWidth = 70;
			InputPorts[ 0 ].FloatInternalData = 1;
			m_useInternalPortData = true;
			m_previewShaderGUID = "bf4a65726dab3d445a69fb1d0945c33e";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			string finalValue = string.Empty;
			string piString = dataCollector.IsSRP ? "PI" : "UNITY_PI";
			if( !InputPorts[ 0 ].IsConnected && InputPorts[ 0 ].FloatInternalData == 1 )
			{
				finalValue = piString;
			} else
			{
				string multiplier = InputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
				finalValue = "( " + multiplier + " * " + piString + " )";
			}


			if ( finalValue.Equals( string.Empty ) )
			{
				UIUtils.ShowMessage( UniqueId, "PINode generating empty code", MessageSeverity.Warning );
			}
			return finalValue;
		}

		
		
		

			
			
		

		
		
		
		

	}
}
