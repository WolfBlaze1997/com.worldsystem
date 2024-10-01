





using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Pixelate UV"
#else
"紫外像素"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"UV Coordinates"
#else
"UV坐标"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Pixelate Texture Modifying UV."
#else
"像素纹理修改UV。"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"The Four Headed Cat - @fourheadedcat"
#else
"四头猫-@fourheaddcat"
#endif
)]
	public sealed class TFHCPixelate : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT2, true, "UV" );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Pixels X"
#else
"像素X"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Pixels Y"
#else
"像素Y"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT2, "Out" );
			m_useInternalPortData = true;
			m_previewShaderGUID = "e2f7e3c513ed18340868b8cbd0d85cfb";
		}

		public override void DrawProperties()
		{
			base.DrawProperties ();
			EditorGUILayout.HelpBox ( 
#if !WB_LANGUAGE_CHINESE
"Pixelate UV.\n\n  - UV is the Texture Coordinates to pixelate.\n  - Pixels X is the number of horizontal pixels\n  - Pixels Y is the number of vertical pixels."
#else
"Pixelate UV。\n\n-UV是像素化的纹理坐标。\n-像素X是水平像素数\n-像素Y是垂直像素数。"
#endif
, MessageType.None);

		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string uv = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string PixelCount_X = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );
			string PixelCount_Y = m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector );

			string pixelWidth = "float pixelWidth" + OutputId + " =  1.0f / " + PixelCount_X + ";";
			string pixelHeight = "float pixelHeight" + OutputId + " = 1.0f / " + PixelCount_Y + ";";
			string pixelatedUV = "half2 pixelateduv" + OutputId + " = half2((int)(" + uv + ".x / pixelWidth" + OutputId + ") * pixelWidth" + OutputId + ", (int)(" + uv + ".y / pixelHeight" + OutputId + ") * pixelHeight" + OutputId + ");";
			string result = "pixelateduv" + OutputId;

			dataCollector.AddLocalVariable( UniqueId, pixelWidth );
			dataCollector.AddLocalVariable( UniqueId, pixelHeight );
			dataCollector.AddLocalVariable( UniqueId, pixelatedUV );

			return GetOutputVectorItem( 0, outputId, result);

		}
	}
}
