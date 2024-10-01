


using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	
	
	
	
	
	
	
	
	

	[Serializable]
	public class VertexDataNode : ParentNode
	{
		[SerializeField]
		protected string m_currentVertexData;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentVertexData = "vertex";


			
			
			
			
			
			
			
			
			
			
			AddOutputVectorPorts( WirePortDataType.FLOAT4, "Out" );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalVar );
			return GetOutputVectorItem( 0, outputId, Constants.VertexShaderInputStr + "." + m_currentVertexData );
		}
	}
}
