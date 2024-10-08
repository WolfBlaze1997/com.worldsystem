


using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	public enum NodeAvailability
	{
		SurfaceShader = 1 << 0,
		ShaderFunction = 1 << 1,
		CustomLighting = 1 << 2,
		TemplateShader	= 1 << 3
	}


	[AttributeUsage( AttributeTargets.Class )]
	public class NodeAttributes : Attribute
	{

		public string Name;
		public string Description;
		public string Category;
		public KeyCode ShortcutKey;
		public bool Available;
		public System.Type[] CastType; 
		public bool Deprecated;
		public string DeprecatedAlternative;
		public System.Type DeprecatedAlternativeType;
		public bool FromCommunity;
		public string CustomCategoryColor; 
		public int SortOrderPriority; 
		public int NodeAvailabilityFlags;
		private string m_nodeUrl;
		public string Community;
		public string Tags;
		public NodeAttributes( string name, string category, string description, System.Type castType = null, KeyCode shortcutKey = KeyCode.None, bool available = true, bool deprecated = false, string deprecatedAlternative = null, System.Type deprecatedAlternativeType = null, string community = null, string customCategoryColor = null, int sortOrderPriority = -1, int nodeAvailabilityFlags = int.MaxValue, string tags = null )
		{
			Name = name;
			Description = description;
			Category = category;
			if( castType != null )
				CastType = new System.Type[] { castType };

			ShortcutKey = shortcutKey;
			Available = available;
			Deprecated = deprecated;
			DeprecatedAlternative = deprecatedAlternative;
			Community = community;
			if( string.IsNullOrEmpty( Community ) )
				Community = string.Empty;
			else
				FromCommunity = true;

			if( !string.IsNullOrEmpty( customCategoryColor ) )
				CustomCategoryColor = customCategoryColor;

			DeprecatedAlternativeType = deprecatedAlternativeType;
			SortOrderPriority = sortOrderPriority;
			NodeAvailabilityFlags = nodeAvailabilityFlags;
			Tags = tags;
			if( string.IsNullOrEmpty( tags ) )
				Tags = string.Empty;
			
		}

		public NodeAttributes( string name, string category, string description, KeyCode shortcutKey, bool available, int sortOrderPriority, int nodeAvailabilityFlags, params System.Type[] castTypes )
		{
			Name = name;
			Description = description;
			Category = category;
			if( castTypes != null && castTypes.Length > 0 )
			{
				CastType = castTypes;
			}

			ShortcutKey = shortcutKey;
			Available = available;
			SortOrderPriority = sortOrderPriority;
			NodeAvailabilityFlags = nodeAvailabilityFlags;
			
		}

		public string NodeUrl
		{
			get
			{
				if( string.IsNullOrEmpty( m_nodeUrl ) )
				{
					m_nodeUrl = ( FromCommunity ? Constants.CommunityNodeCommonUrl : Constants.NodeCommonUrl ) + UIUtils.UrlReplaceInvalidStrings( Name );
				}
				return m_nodeUrl;
			}
		}
	}
}
