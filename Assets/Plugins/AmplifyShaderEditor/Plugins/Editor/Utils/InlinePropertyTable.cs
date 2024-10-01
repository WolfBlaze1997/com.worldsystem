using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{
	public class InlinePropertyTable
	{
		
		

		static List<InlineProperty> m_pool = new List<InlineProperty>( 32 );
		static List<InlineProperty> m_trackingTable = null;

		public static void Initialize()
		{
			m_trackingTable = m_pool; 
		}

		public static void Register( InlineProperty prop )
		{
			if ( m_trackingTable != null )
			{
				m_trackingTable.Add( prop );
			}
		}

		public static void ResolveDependencies()
		{
			if ( m_trackingTable != null )
			{
				foreach ( var prop in m_trackingTable )
				{
					prop.TryResolveDependency();
				}

				m_trackingTable.Clear();
				m_trackingTable = null;
			}
		}
	}
}
