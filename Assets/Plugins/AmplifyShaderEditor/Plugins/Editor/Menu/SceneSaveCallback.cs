


namespace AmplifyShaderEditor
{
	
	public class SceneSaveCallback : UnityEditor.AssetModificationProcessor
	{
		private const string UnityStr = ".unity";

		static string[] OnWillSaveAssets( string[] paths )
		{
			if( !Preferences.GlobalUpdateOnSceneSave )
				return paths;

			bool canSave = false;

			if ( paths.Length == 0 )
			{
				canSave = true;
			}
			else
			{
				for ( int i = 0; i < paths.Length; i++ )
				{
					
					if ( !string.IsNullOrEmpty( paths[ i ] ) && paths[ i ].Contains( UnityStr ) )
					{
						canSave = true;
						break;
					}
				}
			}
			if ( canSave && UIUtils.CurrentWindow )
				UIUtils.CurrentWindow.SetCtrlSCallback( false );

			return paths;
		}
	}
}
