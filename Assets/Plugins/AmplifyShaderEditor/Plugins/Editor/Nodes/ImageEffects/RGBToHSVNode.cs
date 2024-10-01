using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"RGB to HSV"
#else
"RGB转换为HSV"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Converts from RGB to HSV color space"
#else
"从RGB转换到HSV颜色空间"
#endif
)]
	public sealed class RGBToHSVNode : ParentNode
	{
		public readonly static string RGBToHSVHeader = "RGBToHSV( {0} )";
		public readonly static string[] RGBToHSVFunction = {	"{0}3 RGBToHSV({0}3 c)\n",
																"{\n",
																"\t{0}4 K = {0}4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);\n",
																"\t{0}4 p = lerp( {0}4( c.bg, K.wz ), {0}4( c.gb, K.xy ), step( c.b, c.g ) );\n",
																"\t{0}4 q = lerp( {0}4( p.xyw, c.r ), {0}4( c.r, p.yzx ), step( p.x, c.r ) );\n",
																"\t{0} d = q.x - min( q.w, q.y );\n",
																"\t{0} e = 1.0e-10;\n",
																"\treturn {0}3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);\n",
																"}"
															};

		public readonly static bool[] RGBToHSVFlags = {	true,
														false,
														true,
														true,
														true,
														true,
														true,
														true,
														false};

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, "RGB" );
			AddOutputPort( WirePortDataType.FLOAT3, "HSV" );
			AddOutputPort( WirePortDataType.FLOAT, "Hue" );
			AddOutputPort( WirePortDataType.FLOAT, "Saturation" );
			AddOutputPort( WirePortDataType.FLOAT, "Value" );
			m_previewShaderGUID = "0f2f09b49bf4954428aafa2dfe1a9a09";
			m_useInternalPortData = true;
			m_autoWrapProperties = true;
			m_customPrecision = true;
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			DrawPrecisionProperty();
		}

		public static void AddRGBToHSVFunction( ref MasterNodeDataCollector dataCollector, string precisionString )
		{
			if( !dataCollector.HasFunction( RGBToHSVHeader ) )
			{
				
				int currIndent = UIUtils.ShaderIndentLevel;
				if( dataCollector.MasterNodeCategory == AvailableShaderTypes.Template )
				{
					UIUtils.ShaderIndentLevel = 0;
				}
				else
				{
					UIUtils.ShaderIndentLevel = 1;
					UIUtils.ShaderIndentLevel++;
				}

				string finalFunction = string.Empty;
				for( int i = 0; i < RGBToHSVFunction.Length; i++ )
				{
					finalFunction += UIUtils.ShaderIndentTabs + ( RGBToHSVFlags[ i ] ? string.Format( RGBToHSVFunction[ i ], precisionString ) : RGBToHSVFunction[ i ] );
				}
				UIUtils.ShaderIndentLevel--;
				UIUtils.ShaderIndentLevel = currIndent;

				dataCollector.AddFunction( RGBToHSVHeader, finalFunction );
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );

			string precisionString = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, WirePortDataType.FLOAT );
			AddRGBToHSVFunction( ref dataCollector, precisionString );

			string rgbValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );

			RegisterLocalVariable( 0, string.Format( RGBToHSVHeader, rgbValue ), ref dataCollector, "hsvTorgb" + OutputId );
			return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );
		}
	}
}
