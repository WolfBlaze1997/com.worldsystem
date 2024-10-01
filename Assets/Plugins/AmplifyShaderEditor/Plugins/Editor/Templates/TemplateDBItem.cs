


using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TemplateDBItem
	{
		public string GUID;
	}

	[Serializable]
	public class TemplateDB : ScriptableObject
	{
		public List<TemplateDBItem> Items = new List<TemplateDBItem>();
	}
}
