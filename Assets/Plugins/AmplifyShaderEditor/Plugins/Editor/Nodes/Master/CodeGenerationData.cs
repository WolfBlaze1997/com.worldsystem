


using UnityEngine;
namespace AmplifyShaderEditor
{
	[System.Serializable]
	public class CodeGenerationData
	{
		[SerializeField]
		public bool IsActive;
		[SerializeField]
		public string Name;
		[SerializeField]
		public string Value;

		public CodeGenerationData( string name, string value )
		{
			IsActive = false;
			Name = name;
			Value = value;
		}
	}
}
