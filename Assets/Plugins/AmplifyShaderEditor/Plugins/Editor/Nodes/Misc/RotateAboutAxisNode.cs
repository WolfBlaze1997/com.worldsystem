

using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
    [Serializable]
    [NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Rotate About Axis"
#else
"绕轴旋转"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vector Operators"
#else
"矢量运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Rotates a vector around a normalized axis"
#else
"围绕归一化轴旋转向量"
#endif
)]
    public class RotateAboutAxisNode : ParentNode
    {
        private const string FunctionHeader = "float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )";
        private const string FunctionCall = 
#if !WB_LANGUAGE_CHINESE
"RotateAroundAxis( {0}, {1}, {2}, {3} )"
#else
"RotateAroundAxis（｛0｝、｛1｝、｛2｝、｛3｝）"
#endif
;
        private readonly string[] FunctionBody =
        {
            "float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )\n",
            "{\n",
            "\toriginal -= center;\n",
            "\tfloat C = cos( angle );\n",
            "\tfloat S = sin( angle );\n",
            "\tfloat t = 1 - C;\n",
            "\tfloat m00 = t * u.x * u.x + C;\n",
            "\tfloat m01 = t * u.x * u.y - S * u.z;\n",
            "\tfloat m02 = t * u.x * u.z + S * u.y;\n",
            "\tfloat m10 = t * u.x * u.y + S * u.z;\n",
            "\tfloat m11 = t * u.y * u.y + C;\n",
            "\tfloat m12 = t * u.y * u.z - S * u.x;\n",
            "\tfloat m20 = t * u.x * u.z - S * u.y;\n",
            "\tfloat m21 = t * u.y * u.z + S * u.x;\n",
            "\tfloat m22 = t * u.z * u.z + C;\n",
            "\tfloat3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );\n",
            "\treturn mul( finalMatrix, original ) + center;\n",
            "}\n"
        };

        private const string NormalizeAxisLabel = 
#if !WB_LANGUAGE_CHINESE
"Rotation Axis"
#else
"旋转轴"
#endif
;
        private const string NonNormalizeAxisLabel = 
#if !WB_LANGUAGE_CHINESE
"Normalized Rotation Axis"
#else
"标准化旋转轴"
#endif
;
        private const string NormalizeAxisStr = 
#if !WB_LANGUAGE_CHINESE
"Normalize Axis"
#else
"规范化轴"
#endif
;

        [UnityEngine.SerializeField]
        private bool m_normalizeAxis = false;

        protected override void CommonInit( int uniqueId )
        {
            base.CommonInit( uniqueId );
            AddInputPort( WirePortDataType.FLOAT3, false, m_normalizeAxis? NormalizeAxisLabel: NonNormalizeAxisLabel );
            AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Rotation Angle"
#else
"旋转角度"
#endif
);
            AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Pivot Point"
#else
"枢轴点"
#endif
);
            AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Position"
#else
"职位"
#endif
);
            AddOutputPort( WirePortDataType.FLOAT3, Constants.EmptyPortValue );
            m_useInternalPortData = true;
			m_autoWrapProperties = true;
        }

        public override void DrawProperties()
        {
            base.DrawProperties();
            EditorGUI.BeginChangeCheck();
            m_normalizeAxis = EditorGUILayoutToggle( NormalizeAxisStr, m_normalizeAxis );
            if( EditorGUI.EndChangeCheck() )
            {
                m_inputPorts[ 0 ].Name = (m_normalizeAxis ? NormalizeAxisLabel : NonNormalizeAxisLabel);
            }
        }

        public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
        {
            if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
                return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

            string normalizeRotAxis = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
            if( m_normalizeAxis )
            {
                normalizeRotAxis = string.Format( "normalize( {0} )", normalizeRotAxis );
            }
            string rotationAngle = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
            string pivotPoint = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );
            string position = m_inputPorts[ 3 ].GeneratePortInstructions( ref dataCollector );
            dataCollector.AddFunction( FunctionHeader, FunctionBody, false );
            RegisterLocalVariable( 0, string.Format( FunctionCall, pivotPoint, position, normalizeRotAxis, rotationAngle ), ref dataCollector, "rotatedValue" + OutputId );
            return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
        }
        public override void ReadFromString( ref string[] nodeParams )
        {
            base.ReadFromString( ref nodeParams );
            m_normalizeAxis = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
        }

        public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
        {
            base.WriteToString( ref nodeInfo, ref connectionsInfo );
            IOUtils.AddFieldValueToString( ref nodeInfo, m_normalizeAxis );
        }
    }
}
