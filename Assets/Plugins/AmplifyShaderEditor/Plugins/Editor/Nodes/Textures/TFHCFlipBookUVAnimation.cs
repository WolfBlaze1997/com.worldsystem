





using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{

	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Flipbook UV Animation"
#else
"动画书UV动画"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"UV Coordinates"
#else
"UV坐标"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Animate a Flipbook Texture Modifying UV Coordinates."
#else
"为动画书纹理设置动画修改UV坐标。"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
	public sealed class TFHCFlipBookUVAnimation : ParentNode

	{

		private const string TextureVerticalDirectionStr = 
#if !WB_LANGUAGE_CHINESE
"Texture Direction"
#else
"纹理方向"
#endif
;
		private const string NegativeSpeedBehaviorStr = 
#if !WB_LANGUAGE_CHINESE
"If Negative Speed"
#else
"如果速度为负"
#endif
;

		[SerializeField]
		private int m_selectedTextureVerticalDirection = 0;

		[SerializeField]
		private int m_negativeSpeedBehavior = 0;

		[SerializeField]
		private readonly string[] m_textureVerticalDirectionValues = { "Top To Bottom", "Bottom To Top" };

		[SerializeField]
		private readonly string[] m_negativeSpeedBehaviorValues = { "Switch to Positive", "Reverse Animation" };


		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT2, false, "UV" );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Columns"
#else
"柱"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Rows"
#else
"排"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Speed"
#else
"速度"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Start Frame"
#else
"开始帧"
#endif
);
            AddInputPort( WirePortDataType.FLOAT, false,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Time"
#else
"时间"
#endif
/*<C!>*/ );

            AddOutputVectorPorts( WirePortDataType.FLOAT2, "UV" );
			m_outputPorts[ 1 ].Name = "U";
			m_outputPorts[ 2 ].Name = "V";
			m_textLabelWidth = 125;
			m_useInternalPortData = true;
			m_autoWrapProperties = true;
			m_previewShaderGUID = "04fe24be792bfd5428b92132d7cf0f7d";
		}

        public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
        {
            base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
            if( portId == 5 )
            {
                m_previewMaterialPassId = 1;
            }
        }

        public override void OnInputPortDisconnected( int portId )
        {
            base.OnInputPortDisconnected( portId );
            if( portId == 5 )
            {
                m_previewMaterialPassId = 0;
            }
        }

        public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUILayout.BeginVertical();
			m_selectedTextureVerticalDirection = EditorGUILayoutPopup( TextureVerticalDirectionStr, m_selectedTextureVerticalDirection, m_textureVerticalDirectionValues );
			m_negativeSpeedBehavior = EditorGUILayoutPopup( NegativeSpeedBehaviorStr, m_negativeSpeedBehavior, m_negativeSpeedBehaviorValues );
			EditorGUILayout.EndVertical();
			EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Flipbook UV Animation:\n\n  - UV: Texture Coordinates to Flipbook.\n - Columns: number of Columns (X) of the Flipbook Texture.\n  - Rows: number of Rows (Y) of the Flipbook Textures.\n  - Speed: speed of the animation.\n - Texture Direction: set the vertical order of the texture tiles.\n - If Negative Speed: set the behavior when speed is negative.\n\n - Out: UV Coordinates."
#else
"动画书UV动画：\n\n-UV：与动画书的纹理坐标。\n-列数：动画书纹理的列数（X）。\n-行数：动画书纹理的行数（Y）。\n-速度：动画的速度。\n-纹理方向：设置纹理块的垂直顺序。\n-如果速度为负：设置速度为负时的行为。\n\n-输出：UV坐标。"
#endif
, MessageType.None );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_selectedTextureVerticalDirection = ( int ) int.Parse( GetCurrentParam( ref nodeParams ) );
			m_negativeSpeedBehavior = ( int ) int.Parse( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedTextureVerticalDirection );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_negativeSpeedBehavior );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			
			
			
			
			
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );

			string uv = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string columns = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			if ( !m_inputPorts[ 1 ].IsConnected )
				columns = ( float.Parse( columns ) == 0f ? "1" : columns );

			string rows = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
			if ( !m_inputPorts[ 2 ].IsConnected )
				rows = ( float.Parse( rows ) == 0f ? "1" : rows );

			string speed = m_inputPorts[ 3 ].GeneratePortInstructions( ref dataCollector );
			string startframe = m_inputPorts[ 4 ].GeneratePortInstructions( ref dataCollector );
            string timer = m_inputPorts[ 5 ].IsConnected ? m_inputPorts[ 5 ].GeneratePortInstructions( ref dataCollector ) : "_Time[ 1 ]";

			string vcomment1 = "// *** BEGIN Flipbook UV Animation vars ***";
			string vcomment2 = "// Total tiles of Flipbook Texture";
			string vtotaltiles = "float fbtotaltiles" + OutputId + " = " + columns + " * " + rows + ";";
			string vcomment3 = "// Offsets for cols and rows of Flipbook Texture";
			string vcolsoffset = "float fbcolsoffset" + OutputId + " = 1.0f / " + columns + ";";
			string vrowssoffset = "float fbrowsoffset" + OutputId + " = 1.0f / " + rows + ";";
			string vcomment4 = "// Speed of animation";

            string vspeed = string.Format(  "float fbspeed{0} = {1} * {2};", OutputId,timer,speed);
			string vcomment5 = "// UV Tiling (col and row offset)";
			string vtiling = "float2 fbtiling" + OutputId + " = float2(fbcolsoffset" + OutputId + ", fbrowsoffset" + OutputId + ");";
			string vcomment6 = "// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)";
			string vcomment7 = "// Calculate current tile linear index";
			
			string vcurrenttileindex = "float fbcurrenttileindex" + OutputId + " = round( fmod( fbspeed" + OutputId + " + " + startframe + ", fbtotaltiles" + OutputId + ") );";
			string  vcurrenttileindex1 = "fbcurrenttileindex" + OutputId + " += ( fbcurrenttileindex" + OutputId + " < 0) ? fbtotaltiles" + OutputId + " : 0;";
			
			
			string vcomment8 = "// Obtain Offset X coordinate from current tile linear index";

			
			
			string voffsetx1 = "float fblinearindextox" + OutputId + " = round ( fmod ( fbcurrenttileindex" + OutputId + ", " + columns + " ) );";
			string vcomment9 = String.Empty;
			string voffsetx2 = String.Empty;
			if ( m_negativeSpeedBehavior != 0 )
			{
				vcomment9 = "// Reverse X animation if speed is negative";
				voffsetx2 = "fblinearindextox" + OutputId + " = (" + speed + " > 0 ? fblinearindextox" + OutputId + " : (int)" + columns + " - fblinearindextox" + OutputId + ");";
			}
			string vcomment10 = "// Multiply Offset X by coloffset";
			string voffsetx3 = "float fboffsetx" + OutputId + " = fblinearindextox" + OutputId + " * fbcolsoffset" + OutputId + ";";
			string vcomment11 = "// Obtain Offset Y coordinate from current tile linear index";
			
			string voffsety1 = "float fblinearindextoy" + OutputId + " = round( fmod( ( fbcurrenttileindex" + OutputId + " - fblinearindextox" + OutputId + " ) / " + columns + ", " + rows + " ) );";
			
			
			
			string vcomment12 = String.Empty;
			string voffsety2 = String.Empty;
			if ( m_negativeSpeedBehavior == 0 )
			{
				if ( m_selectedTextureVerticalDirection == 0 )
				{
					vcomment12 = "// Reverse Y to get tiles from Top to Bottom";
					voffsety2 = "fblinearindextoy" + OutputId + " = (int)(" + rows + "-1) - fblinearindextoy" + OutputId + ";";
				}
			}
			else
			{
				string reverseanimationoperator = String.Empty;
				if ( m_selectedTextureVerticalDirection == 0 )
				{
					vcomment12 = "// Reverse Y to get tiles from Top to Bottom and Reverse Y animation if speed is negative";
					reverseanimationoperator = " < ";
				}
				else
				{
					vcomment12 = "// Reverse Y animation if speed is negative";
					reverseanimationoperator = " > ";
				}
				voffsety2 = "fblinearindextoy" + OutputId + " = (" + speed + reverseanimationoperator + " 0 ? fblinearindextoy" + OutputId + " : (int)" + rows + " - fblinearindextoy" + OutputId + ");";
			}
			string vcomment13 = "// Multiply Offset Y by rowoffset";
			string voffsety3 = "float fboffsety" + OutputId + " = fblinearindextoy" + OutputId + " * fbrowsoffset" + OutputId + ";";
			string vcomment14 = "// UV Offset";
			string voffset = "float2 fboffset" + OutputId + " = float2(fboffsetx" + OutputId + ", fboffsety" + OutputId + ");";
			
			string vcomment15 = "// Flipbook UV";
			string vfbuv = "half2 fbuv" + OutputId + " = " + uv + " * fbtiling" + OutputId + " + fboffset" + OutputId + ";";
			string vcomment16 = "// *** END Flipbook UV Animation vars ***";
			string result = "fbuv" + OutputId;

			dataCollector.AddLocalVariable( UniqueId, vcomment1 );
			dataCollector.AddLocalVariable( UniqueId, vcomment2 );
			dataCollector.AddLocalVariable( UniqueId, vtotaltiles );
			dataCollector.AddLocalVariable( UniqueId, vcomment3 );
			dataCollector.AddLocalVariable( UniqueId, vcolsoffset );
			dataCollector.AddLocalVariable( UniqueId, vrowssoffset );
			dataCollector.AddLocalVariable( UniqueId, vcomment4 );
			dataCollector.AddLocalVariable( UniqueId, vspeed );
			dataCollector.AddLocalVariable( UniqueId, vcomment5 );
			dataCollector.AddLocalVariable( UniqueId, vtiling );
			dataCollector.AddLocalVariable( UniqueId, vcomment6 );
			dataCollector.AddLocalVariable( UniqueId, vcomment7 );
			dataCollector.AddLocalVariable( UniqueId, vcurrenttileindex );
			dataCollector.AddLocalVariable( UniqueId, vcurrenttileindex1 );
			dataCollector.AddLocalVariable( UniqueId, vcomment8 );
			dataCollector.AddLocalVariable( UniqueId, voffsetx1 );
			if ( m_negativeSpeedBehavior != 0 )
			{
				dataCollector.AddLocalVariable( UniqueId, vcomment9 );
				dataCollector.AddLocalVariable( UniqueId, voffsetx2 );
			}
			dataCollector.AddLocalVariable( UniqueId, vcomment10 );
			dataCollector.AddLocalVariable( UniqueId, voffsetx3 );
			dataCollector.AddLocalVariable( UniqueId, vcomment11 );
			dataCollector.AddLocalVariable( UniqueId, voffsety1 );
			if ( m_selectedTextureVerticalDirection == 0 || m_negativeSpeedBehavior != 0 )
			{
				dataCollector.AddLocalVariable( UniqueId, vcomment12 );
				dataCollector.AddLocalVariable( UniqueId, voffsety2 );
			}
			dataCollector.AddLocalVariable( UniqueId, vcomment13 );
			dataCollector.AddLocalVariable( UniqueId, voffsety3 );
			dataCollector.AddLocalVariable( UniqueId, vcomment14 );
			dataCollector.AddLocalVariable( UniqueId, voffset );
			dataCollector.AddLocalVariable( UniqueId, vcomment15 );
			dataCollector.AddLocalVariable( UniqueId, vfbuv );
			dataCollector.AddLocalVariable( UniqueId, vcomment16 );

			m_outputPorts[ 0 ].SetLocalValue( result, dataCollector.PortCategory );

			return GetOutputVectorItem( 0, outputId, result );

		}
	}
}
