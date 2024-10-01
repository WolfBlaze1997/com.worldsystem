


using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
	public class Preferences
	{
		public enum ShowOption
		{
			Always = 0,
			OnNewVersion = 1,
			Never = 2
		}

		private readonly static GUIContent StartUp = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Show start screen on Unity launch"
#else
"Unity启动时显示开始屏幕"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"You can set if you want to see the start screen everytime Unity launchs, only just when there's a new version available or never."
#else
"您可以设置是否希望每次Unity启动时都能看到开始屏幕，只有在有新版本可用时才能看到，或者永远不会看到。"
#endif
);
		public readonly static string PrefStartUp = "ASELastSession";
		public static ShowOption GlobalStartUp { get; private set; } = 0;

		private readonly static GUIContent AlwaysSnapToGrid = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Always Snap to Grid"
#else
"始终捕捉到网格"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Always snap to grid when dragging nodes around, instead of using control."
#else
"拖动节点时，始终捕捉到网格，而不是使用控件。"
#endif
);
		public readonly static string PrefAlwaysSnapToGrid = "ASEAlwaysSnapToGrid";
		public static bool GlobalAlwaysSnapToGrid { get; private set; } = false;

		private readonly static GUIContent EnableUndo = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Enable Undo (unstable)"
#else
"启用撤消（不稳定）"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Enables undo for actions within the shader graph canvas. Currently unstable, use with caution."
#else
"启用着色器图形画布中操作的撤消。目前不稳定，请谨慎使用。"
#endif
);
		public readonly static string PrefEnableUndo = "ASEEnableUndo";
		public static bool GlobalEnableUndo { get; private set; } = false;

		private readonly static GUIContent AutoSRP = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Auto import SRP shader templates"
#else
"自动导入SRP着色器模板"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"By default Amplify Shader Editor checks for your SRP version and automatically imports the correct corresponding shader templates.\nTurn this OFF if you prefer to import them manually."
#else
"默认情况下，Amplify Shader Editor会检查SRP版本，并自动导入正确的相应着色器模板。\n如果您希望手动导入，请关闭此选项。"
#endif
);
		public readonly static string PrefAutoSRP = "ASEAutoSRP";
		public static bool GlobalAutoSRP { get; private set; } = true;

		private readonly static GUIContent DefineSymbol = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Add Amplify Shader Editor define symbol"
#else
"添加Amplify着色器编辑器定义符号"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Turning it OFF will disable the automatic insertion of the define symbol and remove it from the list while turning it ON will do the opposite.\nThis is used for compatibility with other plugins, if you are not sure if you need this leave it ON."
#else
"将其关闭将禁用定义符号的自动插入，并将其从列表中删除，而将其打开将执行相反的操作。\n这是为了与其他插件兼容，如果您不确定是否需要，请将其打开。"
#endif
);
		public readonly static string PrefDefineSymbol = "ASEDefineSymbol";
		public static bool GlobalDefineSymbol { get; private set; } = true;

		private readonly static GUIContent ClearLog = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Clear Log on Update"
#else
"清除登录更新"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Clears the previously generated log each time the Update button is pressed"
#else
"每次按下“更新”按钮时，清除之前生成的日志"
#endif
);
		public readonly static string PrefClearLog = "ASEClearLog";
		public static bool GlobalClearLog { get; private set; } = true;

		private readonly static GUIContent LogShaderCompile = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Log Shader Compile"
#else
"日志着色器编译"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Log message to console when a shader compilation is finished"
#else
"着色器编译完成时向控制台记录消息"
#endif
);
		public readonly static string PrefLogShaderCompile = "ASELogShaderCompile";
		public static bool GlobalLogShaderCompile { get; private set; } = false;

		private readonly static GUIContent LogBatchCompile = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Log Batch Compile"
#else
"日志批量编译"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Log message to console when a batch compilation is finished"
#else
"批编译完成时向控制台记录消息"
#endif
);
		public readonly static string PrefLogBatchCompile = "ASELogBatchCompile";
		public static bool GlobalLogBatchCompile { get; private set; } = false;

		private readonly static GUIContent UpdateOnSceneSave = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Update on Scene save (Ctrl+S)"
#else
"场景保存更新（Ctrl+S）"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"ASE is aware of Ctrl+S and will use it to save shader"
#else
"ASE知道Ctrl+S，并将使用它来保存着色器"
#endif
);
		public readonly static string PrefUpdateOnSceneSave = "ASEUpdateOnSceneSave";
		public static bool GlobalUpdateOnSceneSave { get; private set; } = true;

		private readonly static GUIContent DisablePreviews = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Disable Node Previews"
#else
"禁用节点预览"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Disable preview on nodes from being updated to boost up performance on large graphs"
#else
"禁止更新节点预览以提高大型图形的性能"
#endif
);
		public readonly static string PrefDisablePreviews = "ASEActivatePreviews";
		public static bool GlobalDisablePreviews { get; private set; } = false;

		private readonly static GUIContent ForceTemplateMinShaderModel = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Force Template Min. Shader Model"
#else
"力模板最小着色器模型"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"If active, when loading a shader its shader model will be replaced by the one specified in template if what is loaded is below the one set over the template."
#else
"如果处于活动状态，则加载着色器时，如果加载的着色器模型低于模板上设置的着色器模型，则其着色器模型将被模板中指定的着色器模型替换。"
#endif
);
		public readonly static string PrefForceTemplateMinShaderModel = "ASEForceTemplateMinShaderModel";
		public static bool GlobalForceTemplateMinShaderModel { get; private set; } = true;

		private readonly static GUIContent ForceTemplateInlineProperties = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Force Template Inline Properties"
#else
"强制模板内联属性"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"If active, defaults all inline properties to template values."
#else
"如果处于活动状态，则将所有内联属性默认为模板值。"
#endif
);
		public readonly static string PrefForceTemplateInlineProperties = "ASEForceTemplateInlineProperties";
		public static bool GlobalForceTemplateInlineProperties { get; private set; } = false;


		[SettingsProvider]
		public static SettingsProvider ImpostorsSettings()
		{
			var provider = new SettingsProvider( "Preferences/Amplify Shader Editor", SettingsScope.User )
			{
				guiHandler = ( string searchContext ) =>
				{
					PreferencesGUI();
				},

				keywords = new HashSet<string>( new[] { "start", "screen", "import", "shader", "templates", "macros", "macros", "define", "symbol" } ),

			};
			return provider;
		}

		private static void ResetSettings()
		{
			IOUtils.SetAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
			UIUtils.ActivatePreviews( true );

			EditorPrefs.DeleteKey( PrefStartUp );
			EditorPrefs.DeleteKey( PrefAlwaysSnapToGrid );
			EditorPrefs.DeleteKey( PrefEnableUndo );
			EditorPrefs.DeleteKey( PrefAutoSRP );
			EditorPrefs.DeleteKey( PrefDefineSymbol );
			EditorPrefs.DeleteKey( PrefClearLog );
			EditorPrefs.DeleteKey( PrefLogShaderCompile );
			EditorPrefs.DeleteKey( PrefLogBatchCompile );
			EditorPrefs.DeleteKey( PrefUpdateOnSceneSave );
			EditorPrefs.DeleteKey( PrefDisablePreviews );
			EditorPrefs.DeleteKey( PrefForceTemplateMinShaderModel );
			EditorPrefs.DeleteKey( PrefForceTemplateInlineProperties );

			LoadSettings();
		}

		private static void LoadSettings()
		{
			GlobalStartUp = ( ShowOption )EditorPrefs.GetInt( PrefStartUp, 0 );
			GlobalAlwaysSnapToGrid = EditorPrefs.GetBool( PrefAlwaysSnapToGrid, false );
			GlobalEnableUndo = EditorPrefs.GetBool( PrefEnableUndo, false );
			GlobalAutoSRP = EditorPrefs.GetBool( PrefAutoSRP, true );
			GlobalDefineSymbol = EditorPrefs.GetBool( PrefDefineSymbol, true );
			GlobalClearLog = EditorPrefs.GetBool( PrefClearLog, true );
			GlobalLogShaderCompile = EditorPrefs.GetBool( PrefLogShaderCompile, false );
			GlobalLogBatchCompile = EditorPrefs.GetBool( PrefLogBatchCompile, false );
			GlobalUpdateOnSceneSave = EditorPrefs.GetBool( PrefUpdateOnSceneSave, true );
			GlobalDisablePreviews = EditorPrefs.GetBool( PrefDisablePreviews, false );
			GlobalForceTemplateMinShaderModel = EditorPrefs.GetBool( PrefForceTemplateMinShaderModel, true );
			GlobalForceTemplateInlineProperties = EditorPrefs.GetBool( PrefForceTemplateInlineProperties, false );
		}

		private static void SaveSettings()
		{
			bool prevDefineSymbol = EditorPrefs.GetBool( PrefDefineSymbol, true );
			bool prevDisablePreviews = EditorPrefs.GetBool( PrefDisablePreviews, false );

			if ( GlobalDefineSymbol != prevDefineSymbol )
			{
				if ( GlobalDefineSymbol )
				{
					IOUtils.SetAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
				}
				else
				{
					IOUtils.RemoveAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
				}
			}

			if ( GlobalDisablePreviews != prevDisablePreviews )
			{
				UIUtils.ActivatePreviews( !GlobalDisablePreviews );
			}

			EditorPrefs.SetInt( PrefStartUp, ( int )GlobalStartUp );
			EditorPrefs.SetBool( PrefAlwaysSnapToGrid, GlobalAlwaysSnapToGrid );
			EditorPrefs.SetBool( PrefEnableUndo, GlobalEnableUndo );
			EditorPrefs.SetBool( PrefAutoSRP, GlobalAutoSRP );
			EditorPrefs.SetBool( PrefDefineSymbol, GlobalDefineSymbol );
			EditorPrefs.SetBool( PrefClearLog, GlobalClearLog );
			EditorPrefs.SetBool( PrefLogShaderCompile, GlobalLogShaderCompile );
			EditorPrefs.SetBool( PrefLogBatchCompile, GlobalLogBatchCompile );
			EditorPrefs.SetBool( PrefUpdateOnSceneSave, GlobalUpdateOnSceneSave );
			EditorPrefs.SetBool( PrefDisablePreviews, GlobalDisablePreviews );
			EditorPrefs.SetBool( PrefForceTemplateMinShaderModel, GlobalForceTemplateMinShaderModel );
			EditorPrefs.SetBool( PrefForceTemplateInlineProperties, GlobalForceTemplateInlineProperties );
		}

		[InitializeOnLoadMethod]
		public static void Initialize()
		{
			LoadSettings();
		}

		public static void PreferencesGUI()
		{
			var cache = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 250;

			EditorGUI.BeginChangeCheck();
			{
				GlobalStartUp = ( ShowOption )EditorGUILayout.EnumPopup( StartUp, GlobalStartUp );
				GlobalAlwaysSnapToGrid = EditorGUILayout.Toggle( AlwaysSnapToGrid, GlobalAlwaysSnapToGrid );
				GlobalEnableUndo = EditorGUILayout.Toggle( EnableUndo, GlobalEnableUndo );
				GlobalAutoSRP = EditorGUILayout.Toggle( AutoSRP, GlobalAutoSRP );
				GlobalDefineSymbol = EditorGUILayout.Toggle( DefineSymbol, GlobalDefineSymbol );
				GlobalClearLog = EditorGUILayout.Toggle( ClearLog, GlobalClearLog );
				GlobalLogShaderCompile = EditorGUILayout.Toggle( LogShaderCompile, GlobalLogShaderCompile );
				GlobalLogBatchCompile = EditorGUILayout.Toggle( LogBatchCompile, GlobalLogBatchCompile );
				GlobalUpdateOnSceneSave = EditorGUILayout.Toggle( UpdateOnSceneSave, GlobalUpdateOnSceneSave );
				GlobalDisablePreviews = EditorGUILayout.Toggle( DisablePreviews, GlobalDisablePreviews );
				GlobalForceTemplateMinShaderModel = EditorGUILayout.Toggle( ForceTemplateMinShaderModel, GlobalForceTemplateMinShaderModel );
				GlobalForceTemplateInlineProperties = EditorGUILayout.Toggle( ForceTemplateInlineProperties, GlobalForceTemplateInlineProperties );
			}
			if ( EditorGUI.EndChangeCheck() )
			{
				SaveSettings();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Reset and Forget All"
#else
"重置并全部忘记"
#endif
) )
			{
				ResetSettings();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = cache;
		}
	}
}
