






using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	public enum BlendOps
	{
		ColorBurn,
		ColorDodge,
		Darken,
		Divide,
		Difference,
		Exclusion,
		SoftLight,
		HardLight,
		HardMix,
		Lighten,
		LinearBurn,
		LinearDodge,
		LinearLight,
		Multiply,
		Overlay,
		PinLight,
		Subtract,
		Screen,
		VividLight
	}
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Blend Operations"
#else
"混合操作"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Common layer blending modes"
#else
"常见图层混合模式"
#endif
)]
	public class BlendOpsNode : ParentNode
	{
		
		
		
		

		
		
		
		

		
		
		
		
		

		
		
		
		

		
		
		

		private const string ASEDarkerColorCall = "ASEDarkerColor{}({0},{1})";
		private const string ASEDarkerColorFunc = "inline float ASEDarkerColor{0}( float srcLocalVar, float dstLocalVar ){" +
		" return ({1} < {2}) ? s : d; }";

		private const string ASELighterColorCall = "ASELighterColor{}({0},{1})";
		private const string ASELighterColorFunc = "inline float ASELighterColor{0}( float srcLocalVar, float dstLocalVar ){" +
		" return ({1} > {2}) ? s : d; }";

		private const string BlendOpsModeStr = 
#if !WB_LANGUAGE_CHINESE
"Blend Op"
#else
"混合操作"
#endif
;
		private const string SaturateResultStr = 
#if !WB_LANGUAGE_CHINESE
"Saturate"
#else
"浸透"
#endif
;

		[SerializeField]
		private BlendOps m_currentBlendOp = BlendOps.ColorBurn;

		[SerializeField]
		private WirePortDataType m_mainDataType = WirePortDataType.COLOR;

		[SerializeField]
		private bool m_saturate = true;

		private UpperLeftWidgetHelper m_upperLeftWidget = new UpperLeftWidgetHelper();

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.COLOR, false, 
#if !WB_LANGUAGE_CHINESE
"Source"
#else
"来源"
#endif
);
			AddInputPort( WirePortDataType.COLOR, false, 
#if !WB_LANGUAGE_CHINESE
"Destiny"
#else
"命运"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Alpha"
#else
"阿尔法"
#endif
);
			m_inputPorts[ 2 ].FloatInternalData = 1;
			AddOutputPort( WirePortDataType.COLOR, Constants.EmptyPortValue );
			m_inputPorts[ 0 ].AddPortForbiddenTypes(	WirePortDataType.FLOAT3x3,
														WirePortDataType.FLOAT4x4,
														WirePortDataType.SAMPLER1D,
														WirePortDataType.SAMPLER2D,
														WirePortDataType.SAMPLER3D,
														WirePortDataType.SAMPLERCUBE,
														WirePortDataType.SAMPLER2DARRAY );
			m_inputPorts[ 1 ].AddPortForbiddenTypes(	WirePortDataType.FLOAT3x3,
														WirePortDataType.FLOAT4x4,
														WirePortDataType.SAMPLER1D,
														WirePortDataType.SAMPLER2D,
														WirePortDataType.SAMPLER3D,
														WirePortDataType.SAMPLERCUBE,
														WirePortDataType.SAMPLER2DARRAY );
			m_textLabelWidth = 75;
			m_autoWrapProperties = true;
			m_hasLeftDropdown = true;
			SetAdditonalTitleText( string.Format( Constants.SubTitleTypeFormatStr, m_currentBlendOp ) );
			m_useInternalPortData = true;
			m_previewShaderGUID = "6d6b3518705b3ba49acdc6e18e480257";
		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();

			m_previewMaterialPassId = (int)m_currentBlendOp;
			PreviewMaterial.SetInt( "_Sat", m_saturate ? 1 : 0 );
			int lerpMode = ( m_inputPorts[ 2 ].IsConnected || m_inputPorts[ 2 ].FloatInternalData < 1 ) ? 1 : 0;
			PreviewMaterial.SetInt( "_Lerp", lerpMode );
		}

		public override void AfterCommonInit()
		{
			base.AfterCommonInit();

			if( PaddingTitleLeft == 0 )
			{
				PaddingTitleLeft = Constants.PropertyPickerWidth + Constants.IconsLeftRightMargin;
				if( PaddingTitleRight == 0 )
					PaddingTitleRight = Constants.PropertyPickerWidth + Constants.IconsLeftRightMargin;
			}
		}

		public override void OnInputPortConnected( int portId, int otherNodeId, int otherPortId, bool activateNode = true )
		{
			base.OnInputPortConnected( portId, otherNodeId, otherPortId, activateNode );
			UpdateConnection( portId );
		}

		public override void OnConnectedOutputNodeChanges( int inputPortId, int otherNodeId, int otherPortId, string name, WirePortDataType type )
		{
			base.OnConnectedOutputNodeChanges( inputPortId, otherNodeId, otherPortId, name, type );
			UpdateConnection( inputPortId );
		}

		public override void OnInputPortDisconnected( int portId )
		{
			base.OnInputPortDisconnected( portId );
			UpdateDisconnection( portId );
		}

		void UpdateConnection( int portId )
		{
			if( portId == 2 )
				return;

			m_inputPorts[ portId ].MatchPortToConnection();
			int otherPortId = ( portId + 1 ) % 2;
			if( m_inputPorts[ otherPortId ].IsConnected )
			{
				m_mainDataType = UIUtils.GetPriority( m_inputPorts[ 0 ].DataType ) > UIUtils.GetPriority( m_inputPorts[ 1 ].DataType ) ? m_inputPorts[ 0 ].DataType : m_inputPorts[ 1 ].DataType;
			}
			else
			{
				m_mainDataType = m_inputPorts[ portId ].DataType;
				m_inputPorts[ otherPortId ].ChangeType( m_mainDataType, false );
			}
			m_outputPorts[ 0 ].ChangeType( m_mainDataType, false );
		}

		void UpdateDisconnection( int portId )
		{
			if( portId == 2 )
				return;

			int otherPortId = ( portId + 1 ) % 2;
			if( m_inputPorts[ otherPortId ].IsConnected )
			{
				m_mainDataType = m_inputPorts[ otherPortId ].DataType;
				m_inputPorts[ portId ].ChangeType( m_mainDataType, false );
				m_outputPorts[ 0 ].ChangeType( m_mainDataType, false );
			}
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			EditorGUI.BeginChangeCheck();
			m_currentBlendOp = (BlendOps)EditorGUILayoutEnumPopup( BlendOpsModeStr, m_currentBlendOp );
			if( EditorGUI.EndChangeCheck() )
			{
				SetAdditonalTitleText( string.Format( Constants.SubTitleTypeFormatStr, m_currentBlendOp ) );
			}
			m_saturate = EditorGUILayoutToggle( SaturateResultStr, m_saturate );
		}

		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );
			m_upperLeftWidget.DrawWidget<BlendOps>( ref m_currentBlendOp, this, OnWidgetUpdate );
		}

		private readonly Action<ParentNode> OnWidgetUpdate = ( x ) =>
		{
			x.SetAdditonalTitleText( string.Format( Constants.SubTitleTypeFormatStr, ( x as BlendOpsNode ).m_currentBlendOp ) );
		};

		private string CreateMultiChannel( ref MasterNodeDataCollector dataCollector, string function, string srcLocalVar, string dstLocalVar, string varName )
		{
			switch( m_outputPorts[ 0 ].DataType )
			{
				default:
				{
					return string.Format( function, srcLocalVar, dstLocalVar );
				}
				case WirePortDataType.FLOAT2:
				{
					string xChannelName = varName + OutputId + "X";
					string xChannelValue = string.Format( function, srcLocalVar + ".x", dstLocalVar + ".x" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, xChannelName, xChannelValue );

					string yChannelName = varName + OutputId + "Y";
					string yChannelValue = string.Format( function, srcLocalVar + ".y", dstLocalVar + ".y" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, yChannelName, yChannelValue );

					return string.Format( "float2({0},{1})", xChannelName, yChannelName );
				}
				case WirePortDataType.FLOAT3:
				{
					string xChannelName = varName + OutputId + "X";
					string xChannelValue = string.Format( function, srcLocalVar + ".x", dstLocalVar + ".x" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, xChannelName, xChannelValue );

					string yChannelName = varName + OutputId + "Y";
					string yChannelValue = string.Format( function, srcLocalVar + ".y", dstLocalVar + ".y" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, yChannelName, yChannelValue );

					string zChannelName = varName + OutputId + "Z";
					string zChannelValue = string.Format( function, srcLocalVar + ".z", dstLocalVar + ".z" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, zChannelName, zChannelValue );

					return string.Format( "float3({0},{1},{2})", xChannelName, yChannelName, zChannelName );
				}
				case WirePortDataType.FLOAT4:
				case WirePortDataType.COLOR:
				{
					string xChannelName = varName + OutputId + "X";
					string xChannelValue = string.Format( function, srcLocalVar + ".x", dstLocalVar + ".x" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, xChannelName, xChannelValue );

					string yChannelName = varName + OutputId + "Y";
					string yChannelValue = string.Format( function, srcLocalVar + ".y", dstLocalVar + ".y" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, yChannelName, yChannelValue );

					string zChannelName = varName + OutputId + "Z";
					string zChannelValue = string.Format( function, srcLocalVar + ".z", dstLocalVar + ".z" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, zChannelName, zChannelValue );

					string wChannelName = varName + OutputId + "W";
					string wChannelValue = string.Format( function, srcLocalVar + ".w", dstLocalVar + ".w" );
					dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, wChannelName, wChannelValue );

					return string.Format( "float4({0},{1},{2},{3})", xChannelName, yChannelName, zChannelName, wChannelName );
				}
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string src = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, m_mainDataType, false, true );
			string dst = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, m_mainDataType, false, true );

			string srcLocalVar = "blendOpSrc" + OutputId;
			string dstLocalVar = "blendOpDest" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_mainDataType ) + " " + srcLocalVar, src + ";" );
			dataCollector.AddLocalVariable( UniqueId, UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_mainDataType ) + " " + dstLocalVar, dst + ";" );

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

			string result = string.Empty;
			switch( m_currentBlendOp )
			{
				case BlendOps.ColorBurn:
				{
					result = string.Format( "( 1.0 - ( ( 1.0 - {0}) / max( {1}, 0.00001) ) )", dstLocalVar, srcLocalVar);
				}
				break;
				case BlendOps.ColorDodge:
				{
					result = string.Format(  "( {0}/ max( 1.0 - {1}, 0.00001 ) )", dstLocalVar, srcLocalVar );
				}
				break;
				case BlendOps.Darken:
				{
					result = "min( " + srcLocalVar + " , " + dstLocalVar + " )";
				}
				break;
				case BlendOps.Divide:
				{
					result = string.Format( "( {0} / max({1},0.00001) )", dstLocalVar, srcLocalVar );
				}
				break;
				case BlendOps.Difference:
				{
					result = "abs( " + srcLocalVar + " - " + dstLocalVar + " )";
				}
				break;
				case BlendOps.Exclusion:
				{
					result = "( 0.5 - 2.0 * ( " + srcLocalVar + " - 0.5 ) * ( " + dstLocalVar + " - 0.5 ) )";
				}
				break;
				case BlendOps.SoftLight:
				{
					result = string.Format( "2.0f*{0}*{1} + {0}*{0}*(1.0f - 2.0f*{1})", dstLocalVar, srcLocalVar );
				}
				break;
				case BlendOps.HardLight:
				{
					result = " (( " + srcLocalVar + " > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( " + srcLocalVar + " - 0.5 ) ) * ( 1.0 - " + dstLocalVar + " ) ) : ( 2.0 * " + srcLocalVar + " * " + dstLocalVar + " ) )";
					
					
				}
				break;
				case BlendOps.HardMix:
				{
					result = " round( 0.5 * ( " + srcLocalVar + " + " + dstLocalVar + " ) )";
				}
				break;
				case BlendOps.Lighten:
				{
					result = "	max( " + srcLocalVar + ", " + dstLocalVar + " )";
				}
				break;
				case BlendOps.LinearBurn:
				{
					result = "( " + srcLocalVar + " + " + dstLocalVar + " - 1.0 )";
				}
				break;
				case BlendOps.LinearDodge:
				{
					result = "( " + srcLocalVar + " + " + dstLocalVar + " )";
				}
				break;
				case BlendOps.LinearLight:
				{
					result = "(( " + srcLocalVar + " > 0.5 )? ( " + dstLocalVar + " + 2.0 * " + srcLocalVar + " - 1.0 ) : ( " + dstLocalVar + " + 2.0 * ( " + srcLocalVar + " - 0.5 ) ) )";
					
					
				}
				break;
				case BlendOps.Multiply:
				{
					result = "( " + srcLocalVar + " * " + dstLocalVar + " )";
				}
				break;
				case BlendOps.Overlay:
				{
					
					result = "(( " + dstLocalVar + " > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - " + dstLocalVar + " ) * ( 1.0 - " + srcLocalVar + " ) ) : ( 2.0 * " + dstLocalVar + " * " + srcLocalVar + " ) )";
					
					
				}
				break;
				case BlendOps.PinLight:
				{
					result = "(( " + srcLocalVar + " > 0.5 ) ? max( " + dstLocalVar + ", 2.0 * ( " + srcLocalVar + " - 0.5 ) ) : min( " + dstLocalVar + ", 2.0 * " + srcLocalVar + " ) )";
					
					
				}
				break;
				case BlendOps.Subtract:
				{
					result = "( " + dstLocalVar + " - " + srcLocalVar + " )";
				}
				break;
				case BlendOps.Screen:
				{
					result = "( 1.0 - ( 1.0 - " + srcLocalVar + " ) * ( 1.0 - " + dstLocalVar + " ) )";
				}
				break;
				case BlendOps.VividLight:
				{
					result = string.Format( "(( {0} > 0.5 ) ? ( {1} / max( ( 1.0 - {0} ) * 2.0 ,0.00001) ) : ( 1.0 - ( ( ( 1.0 - {1} ) * 0.5 ) / max( {0},0.00001) ) ) )", srcLocalVar, dstLocalVar);
					
					
				}
				break;
			}

			UIUtils.ShaderIndentLevel = currIndent;
			if( m_inputPorts[ 2 ].IsConnected || m_inputPorts[ 2 ].FloatInternalData < 1.0 )
			{
				string opacity = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
				string lerpVar = "lerpBlendMode" + OutputId;
				string lerpResult = string.Format( "lerp({0},{1},{2})", dstLocalVar, result, opacity );
				dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, m_outputPorts[ 0 ].DataType, lerpVar, lerpResult );
				result = lerpVar;				
			}

			if( m_saturate )
				result = "( saturate( " + result + " ))";

			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_currentBlendOp );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_saturate );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_currentBlendOp = (BlendOps)Enum.Parse( typeof( BlendOps ), GetCurrentParam( ref nodeParams ) );
			m_saturate = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			SetAdditonalTitleText( string.Format( Constants.SubTitleTypeFormatStr, m_currentBlendOp ) );
		}

		public override void Destroy()
		{
			base.Destroy();
			m_upperLeftWidget = null;
		}
	}
}
