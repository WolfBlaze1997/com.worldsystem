


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Camera Depth Fade"
#else
"相机深度衰减"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Outputs a 0 - 1 gradient representing the distance between the surface of this object and camera near plane"
#else
"输出一个0-1的渐变，表示该对象表面和相机近平面之间的距离"
#endif
)]
	public sealed class CameraDepthFade : ParentNode
	{
		
		
		
		private const string CameraDepthFadeFormat = "(( {0} -_ProjectionParams.y - {1} ) / {2})";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Vertex Position"
#else
"顶点位置"
#endif
, -1, MasterNodePortCategory.Fragment, 2 );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Length"
#else
"长度"
#endif
, -1, MasterNodePortCategory.Fragment, 0 );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Offset"
#else
"抵消"
#endif
, -1, MasterNodePortCategory.Fragment, 1 );
			GetInputPortByUniqueId( 0 ).FloatInternalData = 1;
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_useInternalPortData = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			InputPort vertexPort = GetInputPortByUniqueId( 2 );
			InputPort lengthPort = GetInputPortByUniqueId( 0 );
			InputPort offsetPort = GetInputPortByUniqueId( 1 );

			string distance = lengthPort.GeneratePortInstructions( ref dataCollector );
			string offset = offsetPort.GeneratePortInstructions( ref dataCollector );

			string value = string.Empty;
			string eyeDepth = string.Empty;

			if( dataCollector.IsTemplate )
			{
				if( vertexPort.IsConnected )
				{
					string varName = "customSurfaceDepth" + OutputId;
					GenerateInputInVertex( ref dataCollector, 2, varName, false );

					string formatStr = string.Empty;
					if( dataCollector.IsSRP )
						formatStr = "-TransformWorldToView(TransformObjectToWorld({0})).z";
					else
						formatStr = "-UnityObjectToViewPos({0}).z";

					string eyeInstruction = string.Format( formatStr, varName );
					eyeDepth = "customEye" + OutputId;
					dataCollector.TemplateDataCollectorInstance.RegisterCustomInterpolatedData( eyeDepth, WirePortDataType.FLOAT, CurrentPrecisionType, eyeInstruction );
				}
				else
				{
					eyeDepth = dataCollector.TemplateDataCollectorInstance.GetEyeDepth( CurrentPrecisionType );
				}

				value = string.Format( CameraDepthFadeFormat, eyeDepth, offset, distance );
				RegisterLocalVariable( 0, value, ref dataCollector, "cameraDepthFade" + OutputId );
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			}

			if( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				string vertexVarName = string.Empty;
				if( vertexPort.IsConnected )
				{
					vertexVarName = vertexPort.GeneratePortInstructions( ref dataCollector );
				}
				else
				{
					vertexVarName = Constants.VertexShaderInputStr + ".vertex.xyz";
				}

				
				value = string.Format( CameraDepthFadeFormat, "-UnityObjectToViewPos( " + vertexVarName + " ).z", offset, distance );
				RegisterLocalVariable( 0, value, ref dataCollector, "cameraDepthFade" + OutputId );
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			}

			dataCollector.AddToIncludes( UniqueId, Constants.UnityShaderVariables );

			if( dataCollector.TesselationActive )
			{
				if( vertexPort.IsConnected )
				{
					string vertexValue = vertexPort.GeneratePortInstructions( ref dataCollector );
					eyeDepth = "customSurfaceDepth" + OutputId;
					RegisterLocalVariable( 0, string.Format( "-UnityObjectToViewPos( {0} ).z", vertexValue ), ref dataCollector, eyeDepth );
				}
				else
				{
					eyeDepth = GeneratorUtils.GenerateScreenDepthOnFrag( ref dataCollector, UniqueId, CurrentPrecisionType );
				}
			}
			else
			{

				if( vertexPort.IsConnected )
				{
					string varName = "customSurfaceDepth" + OutputId;
					GenerateInputInVertex( ref dataCollector, 2, varName, false );
					dataCollector.AddToInput( UniqueId, varName, WirePortDataType.FLOAT );
					string vertexInstruction = "-UnityObjectToViewPos( " + varName + " ).z";
					dataCollector.AddToVertexLocalVariables( UniqueId, Constants.VertexShaderOutputStr + "." + varName + " = " + vertexInstruction + ";" );
					eyeDepth = Constants.InputVarStr + "." + varName;
				}
				else
				{
					dataCollector.AddToInput( UniqueId, "eyeDepth", WirePortDataType.FLOAT );
					string instruction = "-UnityObjectToViewPos( " + Constants.VertexShaderInputStr + ".vertex.xyz ).z";
					dataCollector.AddToVertexLocalVariables( UniqueId, Constants.VertexShaderOutputStr + ".eyeDepth = " + instruction + ";" );
					eyeDepth = Constants.InputVarStr + ".eyeDepth";
				}
			}

			value = string.Format( CameraDepthFadeFormat, eyeDepth, offset, distance );
			RegisterLocalVariable( 0, value, ref dataCollector, "cameraDepthFade" + OutputId );
			

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
