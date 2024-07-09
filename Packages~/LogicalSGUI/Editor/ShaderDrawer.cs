// Copyright (c) 2022 Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace LogicalSGUI
{
	internal interface IBaseDrawer
	{
		void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps);
	}

	/// <summary>
	/// Create a Folding Group
	/// group：group name (Default: Property Name)
	/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// default Folding State: "on" or "off" (Default: off)
	/// default Toggle Displayed: "on" or "off" (Default: on)
	/// Target Property Type: FLoat, express Toggle value
	/// </summary>
	internal class MainDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;

		private bool _isFolding;
		private string _group;
		private string _keyword;
		private bool _defaultFoldingState;
		private bool _defaultToggleDisplayed;
		private static readonly float _height = 28f;

		public MainDrawer() : this(String.Empty) { }

		public MainDrawer(string group) : this(group, String.Empty) { }

		public MainDrawer(string group, string keyword) : this(group, keyword, "off") { }

		public MainDrawer(string group, string keyword, string defaultFoldingState) : this(group, keyword, defaultFoldingState, "on") { }

		public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed)
		{
			this._group = group;
			this._keyword = keyword;
			this._defaultFoldingState = defaultFoldingState == "on";
			this._defaultToggleDisplayed = defaultToggleDisplayed == "on";
		}

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterMainProp(inShader, inProp, _group);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue > 0 ? "On" : "Off");
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			LogicalSgui = Helper.GetLogicalSGUI(editor);
			props = LogicalSgui.props;
			shader = LogicalSgui.shader;

			var toggleValue = prop.floatValue > 0;
			string finalGroupName = (_group != String.Empty && _group != "_") ? _group : prop.name;
			bool isFirstFrame = !GroupStateHelper.ContainsGroup(editor.target, finalGroupName);
			_isFolding = isFirstFrame ? !_defaultFoldingState : GroupStateHelper.GetGroupFolding(editor.target, finalGroupName);

			EditorGUI.BeginChangeCheck();
			bool toggleResult = Helper.Foldout(position, ref _isFolding, toggleValue, _defaultToggleDisplayed, label);
			// EditorGUI.showMixedValue = false;

			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = toggleResult ? 1.0f : 0.0f;
				Helper.SetShaderKeyWord(editor.targets, Helper.GetKeyWord(_keyword, prop.name), toggleResult);
			}

			GroupStateHelper.SetGroupFolding(editor.target, finalGroupName, _isFolding);
		}

		// Call in custom shader gui
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return _height;
		}

		// Call when creating new material 
		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && (prop.type == MaterialProperty.PropType.Float
#if UNITY_2021_1_OR_NEWER
									 || prop.type == MaterialProperty.PropType.Int
#endif
										))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_keyword, prop.name), prop.floatValue > 0f);
		}
	}

	/// <summary>
	/// Draw a property with default style in the folding group
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Any
	/// </summary>
	internal class SubModifyDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected string group = String.Empty;
		protected string group1 = String.Empty;

		protected MaterialProperty prop;
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;

		public SubModifyDrawer() { }
		public SubModifyDrawer(string group) : this(group, String.Empty) { }

		public SubModifyDrawer(string group, string group1)
		{
			this.group = group;
			this.group1 = group1;
		}

		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.type == MaterialProperty.PropType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterSubProp(inShader, inProp, group);
			MetaDataHelper.RegisterSubProp(inShader, inProp, this.group1);

		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			this.prop = prop;
			LogicalSgui = Helper.GetLogicalSGUI(editor);
			props = LogicalSgui.props;
			shader = LogicalSgui.shader;

			bool isGroup1Empty;
			if (group1 == string.Empty)
			{
				isGroup1Empty = true;
			}
			else
			{
				isGroup1Empty = GroupStateHelper.IsSubVisible(editor.target, group1);
			}

			var rect = position;

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel++;

			if (GroupStateHelper.IsSubVisible(editor.target, group) && isGroup1Empty)
			{
				if (IsMatchPropType(prop))
				{
					RevertableHelper.SetRevertableGUIWidths();
					DrawProp(rect, prop, label, editor);
				}
				else
				{
					Debug.LogWarning($"Property:'{prop.name}' Type:'{prop.type}' mismatch!");
					editor.DefaultShaderProperty(rect, prop, label.text);
				}
			}

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel--;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GroupStateHelper.IsSubVisible(editor.target, group) ? GetVisibleHeight(prop) : 0;
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.DefaultShaderProperty(position, prop, label.text);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}
	}

	/// <summary>
	/// Draw a property with default style in the folding group
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Any
	/// </summary>
	internal class SubDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected string group = String.Empty;
		protected MaterialProperty prop;
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;
		protected bool isIndent;

		public SubDrawer() { }

		public SubDrawer(string group)
		{
			if (group.Contains(" indent"))
			{
				this.group = group.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.group = group;
				this.isIndent = false;
			}
		}

		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.type == MaterialProperty.PropType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterSubProp(inShader, inProp, group);
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			this.prop = prop;
			LogicalSgui = Helper.GetLogicalSGUI(editor);
			props = LogicalSgui.props;
			shader = LogicalSgui.shader;

			var rect = position;

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel++;

			if (this.isIndent)
				EditorGUI.indentLevel++;

			if (GroupStateHelper.IsSubVisible(editor.target, group))
			{
				if (IsMatchPropType(prop))
				{
					RevertableHelper.SetRevertableGUIWidths();
					DrawProp(rect, prop, label, editor);
				}
				else
				{
					Debug.LogWarning($"Property:'{prop.name}' Type:'{prop.type}' mismatch!");
					editor.DefaultShaderProperty(rect, prop, label.text);
				}
			}

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel--;

			if (this.isIndent)
				EditorGUI.indentLevel--;

		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GroupStateHelper.IsSubVisible(editor.target, group) ? GetVisibleHeight(prop) : 0;
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.DefaultShaderProperty(position, prop, label.text);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}
	}

	internal class EmissionDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected string group = String.Empty;
		protected MaterialProperty prop;
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;

		public EmissionDrawer() { }

		public EmissionDrawer(string group)
		{
			this.group = group;
		}

		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.type == MaterialProperty.PropType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterSubProp(inShader, inProp, group);
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			this.prop = prop;
			LogicalSgui = Helper.GetLogicalSGUI(editor);
			props = LogicalSgui.props;
			shader = LogicalSgui.shader;


			var rect = position;

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel++;

			if (GroupStateHelper.IsSubVisible(editor.target, group))
			{
				if (IsMatchPropType(prop))
				{
					RevertableHelper.SetRevertableGUIWidths();
					DrawProp(rect, prop, label, editor);
					// editor.LightmapEmissionFlagsProperty(0, true);
				}
				else
				{
					Debug.LogWarning($"Property:'{prop.name}' Type:'{prop.type}' mismatch!");
					editor.DefaultShaderProperty(rect, prop, label.text);
				}
			}

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel--;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GroupStateHelper.IsSubVisible(editor.target, group) ? GetVisibleHeight(prop) : 0;
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.LightmapEmissionFlagsProperty(0, true);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}

	}


	/// <summary>
	/// Similar to builtin Toggle()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// Target Property Type: FLoat
	/// </summary>
	internal class SubToggleDrawer : SubDrawer
	{
		private string _keyWord = String.Empty;

		public SubToggleDrawer() { }
		public SubToggleDrawer(string group) : this(group, String.Empty) { }

		public SubToggleDrawer(string group, string keyWord)
		{
			this.group = group;
			this._keyWord = keyWord;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue > 0 ? "On" : "Off");
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			EditorGUI.BeginChangeCheck();
			var rect = position;//EditorGUILayout.GetControlRect();
			var value = EditorGUI.Toggle(rect, label, prop.floatValue > 0.0f);
			string k = Helper.GetKeyWord(_keyWord, prop.name);
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = value ? 1.0f : 0.0f;
				Helper.SetShaderKeyWord(editor.targets, k, value);
			}

			GroupStateHelper.SetKeywordConditionalDisplay(editor.target, k, value);
			EditorGUI.showMixedValue = false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_keyWord, prop.name), prop.floatValue > 0f);
		}
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (prop.floatValue == 0f)
			{
				Helper.SetShaderKeyWord(editor.targets, Helper.GetKeyWord(_keyWord, prop.name), false);
			}
			return base.GetPropertyHeight(prop, label, editor);
		}
	}

	/// <summary>
	/// Similar to builtin PowerSlider()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// power: power of slider (Default: 1)
	/// Target Property Type: Range
	/// </summary>
	internal class SubPowerSliderDrawer : SubDrawer
	{
		private float _power = 1;

		public SubPowerSliderDrawer(float power) : this("_", power) { }

		public SubPowerSliderDrawer(string group, float power)
		{
			this.group = group;
			this._power = Mathf.Clamp(power, 0, float.MaxValue);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			editor.SetDefaultGUIWidths();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			Helper.PowerSlider(prop, _power, rect, label);
			EditorGUI.showMixedValue = false;
		}
	}


	/// <summary>
	/// Similar to builtin Enum() / KeywordEnum()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// n(s): display name
	/// k(s): keyword
	/// v(s): value
	/// Target Property Type: FLoat, express current keyword index
	/// </summary>
	internal class KWEnumDrawer : SubDrawer
	{
		private GUIContent[] _names;
		private GUIContent[] _newNames;
		private string[] _keyWords;
		private float[] _values;

		#region

		public KWEnumDrawer(string n1, string k1)
			: this("_", new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2)
			: this("_", new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3)
			: this("_", new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this("_", new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this("_", new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }

		public KWEnumDrawer(string group, string n1, string k1)
			: this(group, new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2)
			: this(group, new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3)
			: this(group, new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this(group, new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this(group, new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }
		#endregion

		public KWEnumDrawer(string group, string[] names, string[] keyWords = null, float[] values = null)
		{
			this.group = group;

			this._names = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
				this._names[index] = new GUIContent(names[index]);

			if (keyWords == null)
			{
				keyWords = new string[names.Length];
				for (int i = 0; i < names.Length; i++)
					keyWords[i] = String.Empty;
			}
			this._keyWords = keyWords;

			if (values == null)
			{
				values = new float[names.Length];
				for (int index = 0; index < names.Length; ++index)
					values[index] = index;
			}
			this._values = values;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < _names.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, _names[index].text);
		}

		private string[] GetKeywords(MaterialProperty property)
		{
			string[] keyWords = new string[_keyWords.Length];
			for (int i = 0; i < keyWords.Length; i++)
				keyWords[i] = GetKeywordName(property.name, _keyWords[i]);
			return keyWords;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position; //EditorGUILayout.GetControlRect();

			string[] keyWords = GetKeywords(prop);
			int index = Array.IndexOf(_values, prop.floatValue);
			if (index < 0)
			{
				index = 0;
				if (!prop.hasMixedValue)
				{
					Debug.LogError("Property: " + prop.displayName + " has unknown Enum Value: '" + prop.floatValue + "' !\n"
								 + "It will be set to: '" + _values[index] + "'!");
					prop.floatValue = _values[index];
					Helper.SetShaderKeyWord(editor.targets, keyWords, index);
				}
			}
			// if (prop.displayName)
			string[] CacheNames = prop.displayName.Split("/");
			this._newNames = new GUIContent[CacheNames.Length - 1];
			for (int j = 0; j < _newNames.Length; j++)
			{
				_newNames[j] = new GUIContent(CacheNames[j]);
			}

			// Helper.AdaptiveFieldWidth(EditorStyles.popup, _names[index], EditorStyles.popup.lineHeight);
			int newIndex;
			if (prop.displayName.Contains("/"))
			{
				GUIContent displayName = new GUIContent(CacheNames[CacheNames.Length - 1]);
				newIndex = EditorGUI.Popup(rect, displayName, index, _newNames);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
			}
			else
			{
				newIndex = EditorGUI.Popup(rect, label, index, _names);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
			}
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = _values[newIndex];
				Helper.SetShaderKeyWord(editor.targets, keyWords, newIndex);
			}

			// set keyword for conditional display
			for (int i = 0; i < keyWords.Length; i++)
			{
				GroupStateHelper.SetKeywordConditionalDisplay(editor.target, keyWords[i], newIndex == i);
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			string[] keyWords = GetKeywords(prop);
			if (prop.floatValue == 0f)
			{
				Helper.SetShaderKeyWord(editor.targets, keyWords, 0);
			}
			return base.GetPropertyHeight(prop, label, editor);
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, GetKeywords(prop), (int)prop.floatValue);
		}
	}

	internal class SubEnumDrawer : KWEnumDrawer
	{
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2)
			: base(group, new[] { n1, n2 }, null, new[] { v1, v2 }) { }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3)
			: base(group, new[] { n1, n2, n3 }, null, new[] { v1, v2, v3 }) { }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
			: base(group, new[] { n1, n2, n3, n4 }, null, new[] { v1, v2, v3, v4 }) { }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
			: base(group, new[] { n1, n2, n3, n4, n5 }, null, new[] { v1, v2, v3, v4, v5 }) { }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
			: base(group, new[] { n1, n2, n3, n4, n5, n6 }, null, new[] { v1, v2, v3, v4, v5, v6 }) { }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
			: base(group, new[] { n1, n2, n3, n4, n5, n6, n7 }, null, new[] { v1, v2, v3, v4, v5, v6, v7 }) { }

		protected override string GetKeywordName(string propName, string name) { return "_"; }
	}

	internal class SubKeywordEnumDrawer : KWEnumDrawer
	{
		public SubKeywordEnumDrawer(string group, string kw1, string kw2)
			: base(group, new[] { kw1, kw2 }, new[] { kw1, kw2 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3)
			: base(group, new[] { kw1, kw2, kw3 }, new[] { kw1, kw2, kw3 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4)
			: base(group, new[] { kw1, kw2, kw3, kw4 }, new[] { kw1, kw2, kw3, kw4 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5 }, new[] { kw1, kw2, kw3, kw4, kw5 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }
		protected override string GetKeywordName(string propName, string name) { return name.Replace(' ', '_').ToUpperInvariant(); }

	}




	/// <summary>
	/// Draw a Texture property in single line with a extra property
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// extraPropName: extra property name (Unity 2019.2+ only) (Default: none)
	/// Target Property Type: Texture
	/// Extra Property Type: Any, except Texture
	/// </summary>
	internal class TexDrawer : SubDrawer
	{
		private string _extraPropName = String.Empty;
		private ChannelDrawer _channelDrawer = new ChannelDrawer("_");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }

		public TexDrawer() { }

		public TexDrawer(string group) : this(group, String.Empty) { }

		public TexDrawer(string group, string extraPropName)
		{
			this.group = group;
			this._extraPropName = extraPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MaterialProperty extraProp = LogicalSGUI.FindProp(_extraPropName, inProps, true);
			MetaDataHelper.RegisterSubProp(inShader, inProp, group, extraProp == null ? null : new[] { extraProp });
			if (extraProp != null)
			{
				var text = string.Empty;
				if (extraProp.type == MaterialProperty.PropType.Vector)
					text = ChannelDrawer.GetChannelName(extraProp);
				else
					text = RevertableHelper.GetPropertyDefaultValueText(inShader, extraProp);

				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
																RevertableHelper.GetPropertyDefaultValueText(inShader, inProp) + ", " + text);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			var texLabel = label.text;

			MaterialProperty extraProp = LogicalSGUI.FindProp(_extraPropName, props, true);
			if (extraProp != null && extraProp.type != MaterialProperty.PropType.Texture)
			{
				var i = EditorGUI.indentLevel;
				Rect indentedRect, extraPropRect = new Rect(rect);
				switch (extraProp.type)
				{
#if UNITY_2021_1_OR_NEWER
					case MaterialProperty.PropType.Int:
#endif
					case MaterialProperty.PropType.Color:
					case MaterialProperty.PropType.Float:
					case MaterialProperty.PropType.Vector:
						texLabel = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						RevertableHelper.SetRevertableGUIWidths();
						EditorGUIUtility.labelWidth -= (indentedRect.xMin - extraPropRect.xMin) + 30f;
						extraPropRect = indentedRect;
						extraPropRect.xMin += 30f;
						EditorGUI.indentLevel = 0;
						break;
					case MaterialProperty.PropType.Range:
						label.text = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						editor.SetDefaultGUIWidths();
						EditorGUIUtility.fieldWidth += 1f;
						EditorGUIUtility.labelWidth = 0;
						EditorGUI.indentLevel = 0;
						extraPropRect = MaterialEditor.GetRectAfterLabelWidth(extraPropRect);
						extraPropRect.xMin += 2;
						break;
				}

				if (extraProp.type == MaterialProperty.PropType.Vector)
					_channelDrawer.DrawProp(extraPropRect, extraProp, label, editor);
				else
					editor.ShaderProperty(extraPropRect, extraProp, label);

				EditorGUI.indentLevel = i;

				var revertButtonRect = RevertableHelper.GetRevertButtonRect(extraProp, position, true);
				if (RevertableHelper.IsPropertyShouldRevert(editor.target, prop.name) ||
					RevertableHelper.DrawRevertableProperty(revertButtonRect, extraProp, editor, shader))
				{
					RevertableHelper.SetPropertyToDefault(shader, prop);
					RevertableHelper.SetPropertyToDefault(shader, extraProp);
					RevertableHelper.RemovePropertyShouldRevert(editor.targets, prop.name);
				}
			}

			editor.TexturePropertyMiniThumbnail(rect, prop, texLabel, label.tooltip);

			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
	/// Display up to 4 colors in a single line
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// color2-4: extra color property name (Unity 2019.2+ only)
	/// Target Property Type: Color
	/// </summary>
	internal class ColorDrawer : SubDrawer
	{
		private string[] _colorStrings = new string[3];

		public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3, string color4)
		{
			this.group = group;
			this._colorStrings[0] = color2;
			this._colorStrings[1] = color3;
			this._colorStrings[2] = color4;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Color; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			var extraColorProps = new List<MaterialProperty>();
			foreach (var extraColorProp in _colorStrings)
			{
				var p = LogicalSGUI.FindProp(extraColorProp, inProps);
				if (p != null && IsMatchPropType(p))
					extraColorProps.Add(p);
			}
			MetaDataHelper.RegisterSubProp(inShader, inProp, group, extraColorProps.ToArray());
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			Stack<MaterialProperty> cProps = new Stack<MaterialProperty>();
			for (int i = 0; i < 4; i++)
			{
				if (i == 0)
				{
					cProps.Push(prop);
					continue;
				}

				var p = LogicalSGUI.FindProp(_colorStrings[i - 1], props);
				if (p != null && IsMatchPropType(p))
					cProps.Push(p);
			}

			int count = cProps.Count;
			var colorArray = cProps.ToArray();
			var rect = position; //EditorGUILayout.GetControlRect();

			EditorGUI.PrefixLabel(rect, label);

			for (int i = 0; i < count; i++)
			{
				var cProp = colorArray[i];
				EditorGUI.showMixedValue = cProp.hasMixedValue;
				Rect r = new Rect(rect);
				var interval = 13 * i * (-0.25f + EditorGUI.indentLevel * 1.25f);
				float w = EditorGUIUtility.fieldWidth * (0.8f + EditorGUI.indentLevel * 0.2f);
				r.xMin += r.width - w * (i + 1) + interval;
				r.xMax -= w * i - interval;

				EditorGUI.BeginChangeCheck();
				Color src, dst;
				src = cProp.colorValue;
				var isHdr = (colorArray[i].flags & MaterialProperty.PropFlags.HDR) != MaterialProperty.PropFlags.None;
				dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, isHdr);
				if (EditorGUI.EndChangeCheck())
				{
					cProp.colorValue = dst;
				}
			}

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, position, true);
			bool[] shouldRevert = new bool[count];
			shouldRevert[count - 1] = RevertableHelper.IsPropertyShouldRevert(editor.target, prop.name);
			for (int i = 0; i < shouldRevert.Length - 1; i++)
			{
				shouldRevert[i] = RevertableHelper.DrawRevertableProperty(revertButtonRect, colorArray[i], editor, shader);
			}

			if (shouldRevert.Contains(true))
			{
				if (shouldRevert[count - 1])
					RevertableHelper.RemovePropertyShouldRevert(editor.targets, prop.name);
				for (int i = 0; i < count; i++)
				{
					RevertableHelper.SetPropertyToDefault(shader, colorArray[i]);
				}
			}

			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
	/// Draw a Ramp Map Editor (Defaulf Ramp Map Resolution: 512 * 2)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
	/// defaultWidth: default Ramp Width (Default: 512)
	/// Target Property Type: Texture2D
	/// </summary>
	internal class RampDrawer : SubDrawer
	{
		private string _defaultFileName;
		private float _defaultWidth;
		private float _defaultHeight = 2;
		private bool _isDirty;

		// used to read/write Gradient value in code
		private RampHelper.GradientObject _gradientObject;
		// used to modify Gradient value for users
		private SerializedObject _serializedObject;

		private static readonly GUIContent _iconMixImage = EditorGUIUtility.IconContent("darkviewbackground");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight * 2f; }

		public RampDrawer() : this(String.Empty) { }
		public RampDrawer(string group) : this(group, "RampMap") { }
		public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, 512) { }

		public RampDrawer(string group, string defaultFileName, float defaultWidth)
		{
			this.group = group;
			this._defaultFileName = defaultFileName;
			this._defaultWidth = Mathf.Max(2.0f, defaultWidth);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// TODO: cache these variables between different prop?
			_gradientObject = ScriptableObject.CreateInstance<RampHelper.GradientObject>();
			_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty);
			_serializedObject = new SerializedObject(_gradientObject);

			// Draw Label
			var labelRect = new Rect(position);//EditorGUILayout.GetControlRect();
			labelRect.yMax -= position.height * 0.5f;
			EditorGUI.PrefixLabel(labelRect, label);

			// Ramp buttons Rect
			var labelWidth = EditorGUIUtility.labelWidth;
			var indentLevel = EditorGUI.indentLevel;
			EditorGUIUtility.labelWidth = 0;
			EditorGUI.indentLevel = 0;
			var buttonRect = new Rect(position);//EditorGUILayout.GetControlRect();
			buttonRect.yMin += position.height * 0.5f;
			buttonRect = MaterialEditor.GetRectAfterLabelWidth(buttonRect);
			if (buttonRect.width < 50f) return;

			// Draw Ramp Editor
			bool hasChange, doSave, doDiscard;
			Texture newUserTexture, newCreatedTexture;
			hasChange = RampHelper.RampEditor(prop, buttonRect, _serializedObject.FindProperty("gradient"), _isDirty,
											  _defaultFileName, (int)_defaultWidth, (int)_defaultHeight,
											  out newCreatedTexture, out doSave, out doDiscard);

			if (hasChange || doSave)
			{
				// TODO: undo support
				// Undo.RecordObject(_gradientObject, "Edit Gradient");
				_serializedObject.ApplyModifiedProperties();
				RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, doSave);
				// EditorUtility.SetDirty(_gradientObject);
			}

			// Texture object field
			var textureRect = MaterialEditor.GetRectAfterLabelWidth(labelRect);
			newUserTexture = (Texture)EditorGUI.ObjectField(textureRect, prop.textureValue, typeof(Texture2D), false);

			// When tex has changed, update vars
			if (newUserTexture != prop.textureValue || newCreatedTexture != null || doDiscard)
			{
				if (newUserTexture != prop.textureValue)
					prop.textureValue = newUserTexture;
				if (newCreatedTexture != null)
					prop.textureValue = newCreatedTexture;
				_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty, doDiscard);
				_serializedObject.Update();
				if (doDiscard)
					RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, true);
			}

			// Preview texture override (larger preview, hides texture name)
			var previewRect = new Rect(textureRect.x + 1, textureRect.y + 1, textureRect.width - 19, textureRect.height - 2);
			if (prop.hasMixedValue)
			{
				EditorGUI.DrawPreviewTexture(previewRect, _iconMixImage.image);
				GUI.Label(new Rect(previewRect.x + previewRect.width * 0.5f - 10, previewRect.y, previewRect.width * 0.5f, previewRect.height), "―");
			}
			else if (prop.textureValue != null)
				EditorGUI.DrawPreviewTexture(previewRect, prop.textureValue);

			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}
	}

	/// <summary>
	/// Draw a min max slider (Unity 2019.2+ only)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// minPropName: Output Min Property Name
	/// maxPropName: Output Max Property Name
	/// Target Property Type: Range, range limits express the MinMaxSlider value range
	/// Output Min/Max Property Type: Range, it's value is limited by it's range
	/// </summary>
	internal class MinMaxSliderDrawer : SubDrawer
	{
		private string _minPropName;
		private string _maxPropName;

		public MinMaxSliderDrawer(string minPropName, string maxPropName) : this("_", minPropName, maxPropName) { }
		public MinMaxSliderDrawer(string group, string minPropName, string maxPropName)
		{
			this.group = group;
			this._minPropName = minPropName;
			this._maxPropName = maxPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			var minProp = LogicalSGUI.FindProp(_minPropName, inProps, true);
			var maxProp = LogicalSGUI.FindProp(_maxPropName, inProps, true);
			MetaDataHelper.RegisterSubProp(inShader, inProp, group, new[] { minProp, maxProp });
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, minProp).floatValue + " - " +
															RevertableHelper.GetDefaultProperty(inShader, maxProp).floatValue);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// read min max
			MaterialProperty min = LogicalSGUI.FindProp(_minPropName, props, true);
			MaterialProperty max = LogicalSGUI.FindProp(_maxPropName, props, true);
			if (min == null || max == null)
			{
				Debug.LogError("MinMaxSliderDrawer: minProp: " + (min == null ? "null" : min.name) + " or maxProp: " + (max == null ? "null" : max.name) + " not found!");
				return;
			}
			float minf = min.floatValue;
			float maxf = max.floatValue;

			// define draw area
			Rect controlRect = position; //EditorGUILayout.GetControlRect(); // this is the full length rect area
			var w = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;
			Rect inputRect = MaterialEditor.GetRectAfterLabelWidth(controlRect); // this is the remaining rect area after label's area
			EditorGUIUtility.labelWidth = w;

			// draw label
			EditorGUI.LabelField(controlRect, label);

			// draw min max slider
			Rect[] splittedRect = Helper.SplitRect(inputRect, 3);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = min.hasMixedValue;
			var newMinf = EditorGUI.FloatField(splittedRect[0], minf);
			if (EditorGUI.EndChangeCheck())
			{
				minf = Mathf.Clamp(newMinf, min.rangeLimits.x, min.rangeLimits.y);
				min.floatValue = minf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = max.hasMixedValue;
			var newMaxf = EditorGUI.FloatField(splittedRect[2], maxf);
			if (EditorGUI.EndChangeCheck())
			{
				maxf = Mathf.Clamp(newMaxf, max.rangeLimits.x, max.rangeLimits.y);
				max.floatValue = maxf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			if (splittedRect[1].width > 50f)
				EditorGUI.MinMaxSlider(splittedRect[1], ref minf, ref maxf, prop.rangeLimits.x, prop.rangeLimits.y);
			EditorGUI.showMixedValue = false;

			// write back min max if changed
			if (EditorGUI.EndChangeCheck())
			{
				min.floatValue = Mathf.Clamp(minf, min.rangeLimits.x, min.rangeLimits.y);
				max.floatValue = Mathf.Clamp(maxf, max.rangeLimits.x, max.rangeLimits.y);
			}

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, position, true);
			if (RevertableHelper.DrawRevertableProperty(revertButtonRect, min, editor, shader) ||
				RevertableHelper.DrawRevertableProperty(revertButtonRect, max, editor, shader))
			{
				RevertableHelper.SetPropertyToDefault(shader, min);
				RevertableHelper.SetPropertyToDefault(shader, max);
			}

		}
	}



	internal class LinkageDrawer : SubDrawer
	{
		private string _minPropName;
		private string _maxPropName;

		private float propFloatValue;

		private Color propColorValue;

		private int propIntValue;

		private Vector4 propVectorValue;

		private Texture propTextureValue;

		public LinkageDrawer(string minPropName) : this("_", minPropName) { }
		public LinkageDrawer(string group, string minPropName)
		{
			this.group = group;
			this._minPropName = minPropName;
		}

		// protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			var minProp = LogicalSGUI.FindProp(_minPropName, inProps, true);
			// var maxProp = LogicalSGUI.FindProp(_maxPropName, inProps, true);
			MetaDataHelper.RegisterSubProp(inShader, inProp, group, new[] { minProp });
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, minProp).floatValue.ToString());
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			MaterialProperty min = LogicalSGUI.FindProp(_minPropName, props, true);
			if (min == null)
			{
				Debug.LogError("MinMaxSliderDrawer: minProp: " + (min == null ? "null" : min.name));
				return;
			}
			switch (prop.type)
			{
				case MaterialProperty.PropType.Float:
					this.propFloatValue = min.floatValue;
					prop.floatValue = min.floatValue;
					EditorGUI.BeginChangeCheck();
					// this.propFloatValue = EditorGUI.FloatField(position,prop.displayName + " : " + min.displayName,propFloatValue);
					this.propFloatValue = editor.FloatProperty(position, prop, prop.displayName + " : " + min.displayName);
					if (EditorGUI.EndChangeCheck())
					{
						min.floatValue = this.propFloatValue;
					}
					var revertButtonRect0 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					if (RevertableHelper.DrawRevertableProperty(revertButtonRect0, min, editor, shader))
					{
						RevertableHelper.SetPropertyToDefault(shader, min);
					}
					break;

				case MaterialProperty.PropType.Color:
					this.propColorValue = min.colorValue;
					prop.colorValue = min.colorValue;
					EditorGUI.BeginChangeCheck();
					// this.propColorValue = EditorGUI.ColorField(position,prop.displayName + " : " + min.displayName,propColorValue);
					this.propColorValue = editor.ColorProperty(position, prop, prop.displayName + " : " + min.displayName);
					if (EditorGUI.EndChangeCheck())
					{
						min.colorValue = this.propColorValue;
					}
					var revertButtonRect1 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					if (RevertableHelper.DrawRevertableProperty(revertButtonRect1, min, editor, shader))
					{
						RevertableHelper.SetPropertyToDefault(shader, min);
					}
					break;

				case MaterialProperty.PropType.Int:
					this.propIntValue = min.intValue;
					prop.intValue = min.intValue;
					EditorGUI.BeginChangeCheck();
					// this.propIntValue = EditorGUI.IntField(position,prop.displayName + " : " + min.displayName,propIntValue);
					this.propIntValue = editor.IntegerProperty(position, prop, prop.displayName + " : " + min.displayName);
					if (EditorGUI.EndChangeCheck())
					{
						min.intValue = this.propIntValue;
					}
					var revertButtonRect2 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					if (RevertableHelper.DrawRevertableProperty(revertButtonRect2, min, editor, shader))
					{
						RevertableHelper.SetPropertyToDefault(shader, min);
					}
					break;

				case MaterialProperty.PropType.Range:
					this.propFloatValue = min.floatValue;
					prop.floatValue = min.floatValue;
					EditorGUI.BeginChangeCheck();
					this.propFloatValue = editor.RangeProperty(position, prop, prop.displayName + " : " + min.displayName);
					if (EditorGUI.EndChangeCheck())
					{
						min.floatValue = this.propFloatValue;
					}
					var revertButtonRect3 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					if (RevertableHelper.DrawRevertableProperty(revertButtonRect3, min, editor, shader))
					{
						RevertableHelper.SetPropertyToDefault(shader, min);
					}
					break;

				case MaterialProperty.PropType.Vector:
					this.propVectorValue = min.vectorValue;
					prop.vectorValue = min.vectorValue;
					EditorGUI.BeginChangeCheck();
					this.propVectorValue = editor.VectorProperty(position, prop, prop.displayName + " : " + min.displayName);
					if (EditorGUI.EndChangeCheck())
					{
						min.vectorValue = this.propVectorValue;
					}
					var revertButtonRect4 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					if (RevertableHelper.DrawRevertableProperty(revertButtonRect4, min, editor, shader))
					{
						RevertableHelper.SetPropertyToDefault(shader, min);
					}
					break;
					// case MaterialProperty.PropType.Texture :
					// 	this.propTextureValue = min.textureValue;
					// 	this.propVectorValue = min.textureScaleAndOffset;
					// 	prop.textureValue = min.textureValue;
					// 	prop.textureScaleAndOffset = min.textureScaleAndOffset;
					// 	EditorGUI.BeginChangeCheck();
					// 	this.propTextureValue = editor.TextureProperty(position,prop,prop.displayName + " : " + min.displayName,true);
					// 	// this.propVectorValue = MaterialEditor.TextureScaleOffsetProperty(position,propVectorValue);
					// 	if (EditorGUI.EndChangeCheck())
					// 	{
					// 		min.textureValue = this.propTextureValue;
					// 		min.textureScaleAndOffset = this.propVectorValue;
					// 	}
					// 	var revertButtonRect5 = RevertableHelper.GetRevertButtonRect(prop, position, true);
					// 	if (RevertableHelper.DrawRevertableProperty(revertButtonRect5, min, editor, shader))
					// 	{
					// 		RevertableHelper.SetPropertyToDefault(shader, min);
					// 	}
					// break;

			}

		}
	}

	/// <summary>
	/// Draw a R/G/B/A drop menu
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Vector, used to dot() with Texture Sample Value 
	/// </summary>
	internal class ChannelDrawer : SubDrawer
	{
		private static GUIContent[] _names = new[] { new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A"),
			new GUIContent("RGB Average"), new GUIContent("RGB Luminance") };
		private static int[] _intValues = new int[] { 0, 1, 2, 3, 4, 5 };
		private static Vector4[] _vector4Values = new[]
		{
			new Vector4(1, 0, 0, 0),
			new Vector4(0, 1, 0, 0),
			new Vector4(0, 0, 1, 0),
			new Vector4(0, 0, 0, 1),
			new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
			new Vector4(0.2126f, 0.7152f, 0.0722f, 0)
		};

		public ChannelDrawer() { }
		public ChannelDrawer(string group)
		{
			this.group = group;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Vector; }

		private static int GetChannelIndex(MaterialProperty prop)
		{
			int index;
			if (prop.vectorValue == _vector4Values[0])
				index = 0;
			else if (prop.vectorValue == _vector4Values[1])
				index = 1;
			else if (prop.vectorValue == _vector4Values[2])
				index = 2;
			else if (prop.vectorValue == _vector4Values[3])
				index = 3;
			else if (prop.vectorValue == _vector4Values[4])
				index = 4;
			else if (prop.vectorValue == _vector4Values[5])
				index = 5;
			else
			{
				Debug.LogError($"Channel Property:{prop.name} invalid vector found, reset to A");
				prop.vectorValue = _vector4Values[3];
				index = 3;
			}
			return index;
		}

		public static string GetChannelName(MaterialProperty prop)
		{
			return _names[GetChannelIndex(prop)].text;
		}

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, GetChannelName(RevertableHelper.GetDefaultProperty(inShader, inProp)));
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var rect = position; //EditorGUILayout.GetControlRect();
			var index = GetChannelIndex(prop);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			int num = EditorGUI.IntPopup(rect, label, index, _names, _intValues);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.vectorValue = _vector4Values[num];
			}
		}
	}

	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LogicalSGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// </summary>
	internal class PresetDrawer : SubDrawer
	{
		public string presetFileName;
		public PresetDrawer(string presetFileName) : this("_", presetFileName) { }
		public PresetDrawer(string group, string presetFileName)
		{
			this.group = group;
			this.presetFileName = presetFileName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null) return;

			var presetNames = preset.presets.Select(((inPreset) => (inPreset.presetName))).ToArray();
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, presetNames[index]);
			index = (int)inProp.floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyPreset(inShader, inProp, presetFileName, presetNames[index]);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position;

			int index = (int)Mathf.Max(0, prop.floatValue);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null || preset.presets.Count == 0)
			{
				var c = GUI.color;
				GUI.color = Color.red;
				label.text += $"  (Invalid Preset File: {presetFileName})";
				EditorGUI.LabelField(rect, label);
				GUI.color = c;
				return;
			}

			var presetNames = preset.presets.Select(((inPreset) => new GUIContent(inPreset.presetName))).ToArray();
			// Helper.AdaptiveFieldWidth(EditorStyles.popup, presetNames[index], EditorStyles.popup.lineHeight);
			int newIndex = EditorGUI.Popup(rect, label, index, presetNames);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = newIndex;
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
			}

			if (RevertableHelper.IsPropertyShouldRevert(prop.targets[0], prop.name))
			{
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
				RevertableHelper.RemovePropertyShouldRevert(prop.targets, prop.name);
			}
		}
	}

	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LogicalSGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// </summary>
	internal class RenderTypeDrawer : SubDrawer
	{
		public string presetFileName = "RenderType";

		public RenderTypeDrawer() : this("_", "RenderType") { }
		public RenderTypeDrawer(string presetFileName) : this("_", presetFileName) { }
		public RenderTypeDrawer(string group, string presetFileName)
		{
			this.group = group;
			this.presetFileName = presetFileName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null) return;

			var presetNames = preset.presets.Select(((inPreset) => (inPreset.presetName))).ToArray();
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, presetNames[index]);
			index = (int)inProp.floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyPreset(inShader, inProp, presetFileName, presetNames[index]);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position;



			int index = (int)Mathf.Max(0, prop.floatValue);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null || preset.presets.Count == 0)
			{
				var c = GUI.color;
				GUI.color = Color.red;
				label.text += $"  (Invalid Preset File: {presetFileName})";
				EditorGUI.LabelField(rect, label);
				GUI.color = c;
				return;
			}

			var presetNames = preset.presets.Select(((inPreset) => new GUIContent(inPreset.presetName))).ToArray();
			// Helper.AdaptiveFieldWidth(EditorStyles.popup, presetNames[index], EditorStyles.popup.lineHeight);
			int newIndex = EditorGUI.Popup(rect, label, index, presetNames);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = newIndex;
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();

				Material Material = (Material)prop.targets[0];
				MaterialUtils.SetupBlendMode(Material, newIndex);
			}

			if (RevertableHelper.IsPropertyShouldRevert(prop.targets[0], prop.name))
			{
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
				RevertableHelper.RemovePropertyShouldRevert(prop.targets, prop.name);
			}
		}
	}

	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LogicalSGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// </summary>
	internal class BuiltinEnumDrawer : SubDrawer
	{
		int newIndex;
		public string presetFileName;
		public BuiltinEnumDrawer(string presetFileName) : this("_", presetFileName) { }
		public BuiltinEnumDrawer(string group, string presetFileName)
		{
			this.group = group;
			this.presetFileName = presetFileName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null) return;

			var presetNames = preset.presets.Select(((inPreset) => (inPreset.presetName))).ToArray();
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, presetNames[index]);
			index = (int)inProp.floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyPreset(inShader, inProp, presetFileName, presetNames[index]);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position;

			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null || preset.presets.Count == 0)
			{
				var c = GUI.color;
				GUI.color = Color.red;
				label.text += $"  (Invalid Preset File: {presetFileName})";
				EditorGUI.LabelField(rect, label);
				GUI.color = c;
				return;
			}

			var presetNames = preset.presets.Select(((inPreset) => new GUIContent(inPreset.presetName))).ToArray();
			bool isNormal = true;
			for (int i = 0; i < presetNames.Length; i++)
			{
				if (preset.presets[i].propertyValues[0].floatValue > presetNames.Length)
				{
					isNormal = false;
					break;
				}
			}
			if (!isNormal)
			{
				this.newIndex = (int)Mathf.Log(prop.floatValue, 2);
			}
			else
			{
				this.newIndex = (int)prop.floatValue;
			}
			this.newIndex = EditorGUI.Popup(rect, label, newIndex, presetNames);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				// prop.floatValue = this.newIndex;
				if (!isNormal && preset.presets[this.newIndex].propertyValues[0].floatValue == 32)
				{
					prop.floatValue = 0;
				}
				else
				{
					prop.floatValue = preset.presets[this.newIndex].propertyValues[0].floatValue;
				}
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
			}

			if (RevertableHelper.IsPropertyShouldRevert(prop.targets[0], prop.name))
			{
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
				RevertableHelper.RemovePropertyShouldRevert(prop.targets, prop.name);
			}
		}
	}

	/// <summary>
	/// Similar to Header()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// tips: Modifying the Decorator parameters in Shader requires manually refreshing the GUI instance by throwing an exception
	/// </summary>
	internal class TitleDecorator : SubDrawer
	{
		private string _header;

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight + 6f; }

		public TitleDecorator(string header) : this("_", header) { }
		public TitleDecorator(string group, string header)
		{
			this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position.y += 2;
			position.x -= 12;
			position = EditorGUI.IndentedRect(position);
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			GUI.Label(position, _header, style);
		}
	}

	/// <summary>
	/// Similar to Header()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// tips: Modifying the Decorator parameters in Shader requires manually refreshing the GUI instance by throwing an exception
	/// </summary>
	internal class TitleDrawer : SubDrawer
	{
		private string _header;

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight + 6f; }

		public TitleDrawer(string header) : this("_", header) { }
		public TitleDrawer(string group, string header)
		{
			this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position.y += 2;
			position = EditorGUI.IndentedRect(position);
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			// GUI.Label(position, _header + prop.displayName, style);
			GUI.Label(position, _header, style);
		}
	}

	/// <summary>
	/// Tooltip, describes the details of the property. (Default: property.name and property default value)
	/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
	/// tooltip：a single-line string to display, support up to 4 ','. (Default: Newline)
	/// tips: Modifying Decorator parameters in Shader requires refreshing the cache by modifying the Property default value
	/// </summary>
	internal class TooltipDecorator : SubDrawer
	{
		private string _tooltip;


		#region 

		public TooltipDecorator() { }

		public TooltipDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public TooltipDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		public TooltipDecorator(string tooltip) { this._tooltip = tooltip; }
		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterPropertyTooltip(inShader, inProp, _tooltip);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}

	/// <summary>
	/// Display a Helpbox on the property
	/// You can also use "%Text" in DisplayName to add Helpbox that supports Multi-Language.
	/// message：a single-line string to display, support up to 4 ','. (Default: Newline)
	/// tips: Modifying Decorator parameters in Shader requires refreshing the cache by modifying the Property default value
	/// </summary>
	internal class HelpboxDecorator : TooltipDecorator
	{
		private string _message;


		#region 
		public HelpboxDecorator() { }

		public HelpboxDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public HelpboxDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public HelpboxDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		public HelpboxDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		public HelpboxDecorator(string message) { this._message = message; }
		#endregion


		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MetaDataHelper.RegisterPropertyHelpbox(inShader, inProp, _message);

			// To resolve such errors:
			// ArgumentException: Getting control 26's position in a group with only 26 controls when doing repaint
			{
				// When the Drawer draws in the Repaint stage but does not draw in the Init stage, an error will occur.
				// It is necessary to ensure that the same number of GUIs are drawn in different stages
				EditorGUI.HelpBox(EditorGUILayout.GetControlRect(), "", MessageType.None);
			}
		}
	}



	//-----------------------------------------------------------------------------------------------

	internal class EnumGroupDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;

		private bool _isFolding;
		private string _group;
		private string _togglekeyword;
		private bool _defaultFoldingState;
		private bool _defaultToggleDisplayed;
		private bool _isEnumDisplayed;
		private static Texture _logoMaterial = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("0cf5f76fdf41c4142975c3eefb1662aa"));
		private static Texture _logoLighting = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("30cbd2098b2af38499da12b1e3ba09e8"));
		private static Texture _logoSettings = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("bfbe4a04af25daf4889b56f7d11b143f"));
		private static Texture _logoData = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("eecb3ee690e42dc4f9226d9c7d6f8d8b"));
		private static Texture _logoPhysics = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("f7848739ef7d69847af596cee689db4b"));
		//枚举
		private GUIContent[] _names;
		private GUIContent[] _newNames;
		private string[] _keyWords;
		private float[] _values;//

		public EnumGroupDrawer(string group, string togglekeyword) :
		this(group, togglekeyword,/**/new string[1] { string.Empty }, new string[1] { string.Empty }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1/**/) :
		this(group, togglekeyword,/**/new string[1] { n1 }, new string[1] { k1 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2/**/) :
		this(group, togglekeyword,/**/new string[2] { n1, n2 }, new string[2] { k1, k2 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3/**/) :
		this(group, togglekeyword,/**/new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4/**/) :
		this(group, togglekeyword,/**/new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5/**/) :
		this(group, togglekeyword,/**/new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5, string n6, string k6/**/) :
		this(group, togglekeyword,/**/new string[6] { n1, n2, n3, n4, n5, n6 }, new string[6] { k1, k2, k3, k4, k5, k6 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5, string n6, string k6, string n7, string k7/**/) :
		this(group, togglekeyword,/**/new string[7] { n1, n2, n3, n4, n5, n6, n7 }, new string[7] { k1, k2, k3, k4, k5, k6, k7 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5, string n6, string k6, string n7, string k7, string n8, string k8/**/) :
		this(group, togglekeyword,/**/new string[8] { n1, n2, n3, n4, n5, n6, n7, n8 }, new string[8] { k1, k2, k3, k4, k5, k6, k7, k8 }/**/)
		{ }
		public EnumGroupDrawer(string group, string togglekeyword,/**/ string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5, string n6, string k6, string n7, string k7, string n8, string k8, string n9, string k9/**/) :
		this(group, togglekeyword,/**/new string[9] { n1, n2, n3, n4, n5, n6, n7, n8, n9 }, new string[9] { k1, k2, k3, k4, k5, k6, k7, k8, k9 }/**/)
		{ }

		public EnumGroupDrawer(string group, string togglekeyword,/**/ string[] names = null, string[] keyWords = null, float[] values = null/**/)
		{
			this._group = group;

			this._togglekeyword = togglekeyword;
			this._defaultFoldingState = false;
			this._defaultToggleDisplayed = false;
			this._isEnumDisplayed = names[0] == string.Empty ? false : true;

			//枚举
			this._names = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
				this._names[index] = new GUIContent(names[index]);

			if (keyWords == null)
			{
				keyWords = new string[names.Length];
				for (int i = 0; i < names.Length; i++)
					keyWords[i] = String.Empty;
			}
			this._keyWords = keyWords;

			if (values == null)
			{
				values = new float[names.Length];
				for (int index = 0; index < names.Length; ++index)
					values[index] = index;
			}
			this._values = values;//

		}

		//枚举
		protected bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Vector; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		private string[] GetKeywords(MaterialProperty property)
		{
			string[] keyWords = new string[_keyWords.Length];
			for (int i = 0; i < keyWords.Length; i++)
				keyWords[i] = GetKeywordName(property.name, _keyWords[i]);
			return keyWords;
		}//

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{

			MetaDataHelper.RegisterMainProp(inShader, inProp, _group);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue > 0 ? "On" : "Off");

			//枚举
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < _names.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, _names[index].text);//		
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (prop.vectorValue.w > 999)
			{
				//被禁用时,关闭功能
				string[] keyWords = GetKeywords(prop);
				EditorGUI.showMixedValue = false;
				Helper.SetShaderKeyWord(editor.targets, keyWords, 0);
			}
			else
			{
				string[] CacheNames = prop.displayName.Split("/");
				this._newNames = new GUIContent[CacheNames.Length - 1];
				for (int j = 0; j < _newNames.Length; j++)
				{
					_newNames[j] = new GUIContent(CacheNames[j]);
				}

				int index = Array.IndexOf(_values, prop.vectorValue.y);
				if (_isEnumDisplayed)
				{
					//枚举
					EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue = prop.hasMixedValue;

					Rect rect = position; //EditorGUILayout.GetControlRect();
					rect.x = position.x - 25;
					rect.y = position.y + 5;
					rect.width = position.width + 15;

					string[] keyWords = GetKeywords(prop);
					if (index < 0)
					{
						index = 0;
						if (!prop.hasMixedValue)
						{
							Debug.LogError("Property: " + prop.displayName + " has unknown Enum Value: '" + prop.vectorValue.y + "' !\n"
										+ "It will be set to: '" + _values[index] + "'!");
							prop.vectorValue = new Vector4(prop.vectorValue.x, _values[index], prop.vectorValue.z, prop.vectorValue.w);
							Helper.SetShaderKeyWord(editor.targets, keyWords, index);
						}
					}

					string[] Name01 = new string[_names.Length];
					for (int j = 0; j < _names.Length; j++)
					{
						Name01[j] = _names[j].text;
					}
					_defaultToggleDisplayed = Name01[index].Contains("_senior");

					int newIndex;
					if (prop.displayName.Contains("/"))
					{
						newIndex = EditorGUI.Popup(rect, new GUIContent(" "), index, _newNames);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
					}
					else
					{
						newIndex = EditorGUI.Popup(rect, new GUIContent(" "), index, _names);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
					}

					EditorGUI.showMixedValue = false;
					if (EditorGUI.EndChangeCheck())
					{
						// Debug.Log(newIndex + "  " + index);
						//------------------------------------------
						// 完成对其他组的禁用显示
						string[] strings01 = Name01[newIndex].Split(" disable_");
						string[] strings02 = new string[(int)(strings01.Length - 1.0)];
						for (int i = 0; i < strings02.Length; i++)
						{
							strings02[i] = strings01[i + 1];
						}
						if (Name01[newIndex].Contains(" disable_"))
						{
							for (int i = 0; i < strings02.Length; i++)
							{
								if (MaterialEditor.GetMaterialProperty(editor.targets, strings02[i]).type == MaterialProperty.PropType.Vector)
								{
									Vector4 vectorValue = MaterialEditor.GetMaterialProperty(editor.targets, strings02[i]).vectorValue;
									MaterialEditor.GetMaterialProperty(editor.targets, strings02[i]).vectorValue = new Vector4(vectorValue.x, 0, vectorValue.z, 9999);
								}
								if (MaterialEditor.GetMaterialProperty(editor.targets, strings02[i]).type == MaterialProperty.PropType.Float)
								{
									MaterialEditor.GetMaterialProperty(editor.targets, strings02[i]).floatValue = 0;
								}

							}
						}
						//当选其他选项时,将组的显示还原
						for (int i = 0; i < Name01.Length; i++)
						{
							if (!Name01[i].Contains(" disable_")) continue;
							if (Name01[i] == Name01[newIndex]) continue;

							string[] strings03 = Name01[i].Split(" disable_");
							string[] strings04 = new string[(int)(strings03.Length - 1.0)];
							for (int j = 0; j < strings04.Length; j++)
							{
								strings04[j] = strings03[j + 1];
								if (MaterialEditor.GetMaterialProperty(editor.targets, strings04[j]).type == MaterialProperty.PropType.Vector)
								{
									Vector4 vectorValue = MaterialEditor.GetMaterialProperty(editor.targets, strings04[j]).vectorValue;
									MaterialEditor.GetMaterialProperty(editor.targets, strings04[j]).vectorValue = new Vector4(vectorValue.x, vectorValue.y, vectorValue.z, 0);
								}
							}

						}
						//------------------------------------------

						prop.vectorValue = new Vector4(0, _values[newIndex], prop.vectorValue.z, prop.vectorValue.w);
						Helper.SetShaderKeyWord(editor.targets, keyWords, newIndex);
						_defaultToggleDisplayed = Name01[newIndex].Contains("_senior");

					}
					string[] displayKeyWords = new string[keyWords.Length];
					for (int i = 0; i < keyWords.Length; i++)
					{
						displayKeyWords[i] = keyWords[i].Replace(prop.name.ToUpperInvariant(), "");

					}
					// set keyword for conditional display
					for (int i = 0; i < keyWords.Length; i++)
					{
						GroupStateHelper.SetKeywordConditionalDisplay(editor.target, displayKeyWords[i], newIndex == i);
					}
					//枚举
				}

				EditorGUI.showMixedValue = prop.hasMixedValue;
				LogicalSgui = Helper.GetLogicalSGUI(editor);
				props = LogicalSgui.props;
				shader = LogicalSgui.shader;

				var toggleValue = prop.vectorValue.x > 0;
				string finalGroupName = (_group != String.Empty && _group != "_") ? _group : prop.name;
				bool isFirstFrame = !GroupStateHelper.ContainsGroup(editor.target, finalGroupName);
				_isFolding = isFirstFrame ? !_defaultFoldingState : GroupStateHelper.GetGroupFolding(editor.target, finalGroupName);

				EditorGUI.BeginChangeCheck();
				bool toggleResult = Helper.EnumFoldout(position, ref _isFolding, toggleValue, _defaultToggleDisplayed, new GUIContent(CacheNames[_newNames.Length]));
				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck())
				{
					prop.vectorValue = new(toggleResult ? 1.0f : 0.0f, prop.vectorValue.y, prop.vectorValue.z, prop.vectorValue.w);
					Helper.SetShaderKeyWord(editor.targets, prop.name + Helper.GetKeyWord(_togglekeyword, prop.name), toggleResult);
				}
				// set keyword for conditional display
				for (int i = 0; i < editor.targets.Length; i++)
				{
					GroupStateHelper.SetKeywordConditionalDisplay(editor.targets[i], Helper.GetKeyWord(_togglekeyword, prop.name), toggleResult);
				}

				GroupStateHelper.SetGroupFolding(editor.target, finalGroupName, _isFolding);

				EditorGUI.Foldout(position, !_isFolding, "");//绘制折叠三角形



				if (_isEnumDisplayed)
				{
					Rect rect0 = new Rect(position);
					rect0.x = position.x - 90 + position.width;
					rect0.y = position.y + 5;
					rect0.width = 80;
					if (prop.displayName.Contains("/"))
					{
						GUI.Box(rect0, this._newNames[index], EditorStyles.popup);//绘制枚举外观
					}
					else
					{
						if (_names[index].text.Contains("_senior"))
							_names[index].text = _names[index].text.Replace("_senior", " ");
						GUI.Box(rect0, _names[index], EditorStyles.popup);//绘制枚举外观
						_names[index].text = _names[index].text.Replace(" ", "_senior");
					}

				}

				if (_group.Contains("0"))
				{
					GUI.DrawTexture(new Rect(position.x + 8f, position.y + 4f, 18f, 18f), _logoMaterial);
				}
				if (_group.Contains("1"))
				{
					GUI.DrawTexture(new Rect(position.x + 8f, position.y + 4f, 18f, 18f), _logoLighting);
				}
				if (_group.Contains("2"))
				{
					GUI.DrawTexture(new Rect(position.x + 8f, position.y + 4f, 18f, 18f), _logoSettings);
				}
				if (_group.Contains("3"))
				{
					GUI.DrawTexture(new Rect(position.x + 8f, position.y + 4f, 18f, 18f), _logoData);
				}
				if (_group.Contains("4"))
				{
					GUI.DrawTexture(new Rect(position.x + 8f, position.y + 4f, 18f, 18f), _logoPhysics);
				}

			}
		}

		// Call in custom shader gui
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (prop.vectorValue.w > 999)
			{
				string[] keyWords = GetKeywords(prop);
				EditorGUI.showMixedValue = false;
				Helper.SetShaderKeyWord(editor.targets, keyWords, 0);
				return 0;
			}
			else
			{
				return 28f;
			}
		}

		// Call when creating new material 
		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && (prop.type == MaterialProperty.PropType.Vector
#if UNITY_2021_1_OR_NEWER
									 || prop.type == MaterialProperty.PropType.Float
#endif
										))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_togglekeyword, prop.name), prop.vectorValue.x > 0f);

			//枚举
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, GetKeywords(prop), (int)prop.vectorValue.y);//
		}
	}


	internal class LogicalSubDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		protected string[] _groups;
		protected bool _logical;
		// bool isDisplay;

		protected MaterialProperty prop;
		protected MaterialProperty[] props;
		protected LogicalSGUI LogicalSgui;
		protected Shader shader;
		protected bool isIndent;

		public LogicalSubDrawer() { }
		public LogicalSubDrawer(string logicalGroup)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

		}

		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.type == MaterialProperty.PropType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			foreach (string group in _groups)
			{
				MetaDataHelper.RegisterSubProp(inShader, inProp, group);
			}

		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			this.prop = prop;
			LogicalSgui = Helper.GetLogicalSGUI(editor);
			props = LogicalSgui.props;
			shader = LogicalSgui.shader;

			bool isDisplay;
			if (_logical)
			{
				isDisplay = false;
				for (int i = 0; i < _groups.Length; i++)
				{
					isDisplay = isDisplay || GroupStateHelper.IsSubVisible(editor.target, _groups[i]);
				}
			}
			else
			{
				isDisplay = true;
				for (int i = 0; i < _groups.Length; i++)
				{
					isDisplay = isDisplay && GroupStateHelper.IsSubVisible(editor.target, _groups[i]);
				}
			}

			var rect = position;
			EditorGUI.indentLevel++;

			if (this.isIndent)
				EditorGUI.indentLevel++;

			if (isDisplay)
			{
				if (IsMatchPropType(prop))
				{
					RevertableHelper.SetRevertableGUIWidths();
					DrawProp(rect, prop, label, editor);
				}
				else
				{
					Debug.LogWarning($"Property:'{prop.name}' Type:'{prop.type}' mismatch!");
					editor.DefaultShaderProperty(rect, prop, label.text);
				}
			}

			EditorGUI.indentLevel--;

			if (this.isIndent)
				EditorGUI.indentLevel--;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			bool isDisplay;
			if (_logical)
			{
				isDisplay = false;
				for (int i = 0; i < _groups.Length; i++)
				{
					isDisplay = isDisplay || GroupStateHelper.IsSubVisible(editor.target, _groups[i]);
				}
			}
			else
			{
				isDisplay = true;
				for (int i = 0; i < _groups.Length; i++)
				{
					isDisplay = isDisplay && GroupStateHelper.IsSubVisible(editor.target, _groups[i]);
				}
			}

			return isDisplay ? GetVisibleHeight(prop) : 0;
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.DefaultShaderProperty(position, prop, label.text);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}
	}


	/// <summary>
	/// Similar to builtin Toggle()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// Target Property Type: FLoat
	/// </summary>
	internal class LogicalSubToggleDrawer : LogicalSubDrawer
	{
		private string _keyWord = String.Empty;

		public LogicalSubToggleDrawer(string logicalGroup, string keyWord)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

			this._keyWord = keyWord;

		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue > 0 ? "On" : "Off");
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			EditorGUI.BeginChangeCheck();
			var rect = position;//EditorGUILayout.GetControlRect();
			var value = EditorGUI.Toggle(rect, label, prop.floatValue > 0.0f);
			string k = Helper.GetKeyWord(_keyWord, prop.name);
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = value ? 1.0f : 0.0f;
				Helper.SetShaderKeyWord(editor.targets, k, value);
			}

			GroupStateHelper.SetKeywordConditionalDisplay(editor.target, k, value);
			EditorGUI.showMixedValue = false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_keyWord, prop.name), prop.floatValue > 0f);
		}
	}


	/// <summary>
	/// Similar to builtin PowerSlider()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// power: power of slider (Default: 1)
	/// Target Property Type: Range
	/// </summary>
	internal class LogicalSubPowerSliderDrawer : LogicalSubDrawer
	{
		private float _power = 1;

		public LogicalSubPowerSliderDrawer(float power) : this("_", power) { }

		public LogicalSubPowerSliderDrawer(string logicalGroup, float power)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}
			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}
			// this.group = group;
			this._power = Mathf.Clamp(power, 0, float.MaxValue);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			editor.SetDefaultGUIWidths();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			Helper.PowerSlider(prop, _power, rect, label);
			EditorGUI.showMixedValue = false;
		}
	}


	/// <summary>
	/// Similar to builtin Enum() / KeywordEnum()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// n(s): display name
	/// k(s): keyword
	/// v(s): value
	/// Target Property Type: FLoat, express current keyword index
	/// </summary>
	internal class LogicalKWEnumDrawer : LogicalSubDrawer
	{
		private GUIContent[] _names;
		private GUIContent[] _newNames;
		private string[] _keyWords;
		private float[] _values;

		#region

		public LogicalKWEnumDrawer(string logicalGroup, string n1, string k1)
			: this(logicalGroup, new string[1] { n1 }, new string[1] { k1 }) { }

		public LogicalKWEnumDrawer(string logicalGroup, string n1, string k1, string n2, string k2)
			: this(logicalGroup, new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public LogicalKWEnumDrawer(string logicalGroup, string n1, string k1, string n2, string k2, string n3, string k3)
			: this(logicalGroup, new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public LogicalKWEnumDrawer(string logicalGroup, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this(logicalGroup, new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public LogicalKWEnumDrawer(string logicalGroup, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this(logicalGroup, new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }
		#endregion

		public LogicalKWEnumDrawer(string logicalGroup, string[] names, string[] keyWords = null, float[] values = null)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

			this._names = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
				this._names[index] = new GUIContent(names[index]);

			if (keyWords == null)
			{
				keyWords = new string[names.Length];
				for (int i = 0; i < names.Length; i++)
					keyWords[i] = String.Empty;
			}
			this._keyWords = keyWords;

			if (values == null)
			{
				values = new float[names.Length];
				for (int index = 0; index < names.Length; ++index)
					values[index] = index;
			}
			this._values = values;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			var index = (int)RevertableHelper.GetDefaultProperty(inShader, inProp).floatValue;
			if (index < _names.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, _names[index].text);
		}

		private string[] GetKeywords(MaterialProperty property)
		{
			string[] keyWords = new string[_keyWords.Length];
			for (int i = 0; i < keyWords.Length; i++)
				keyWords[i] = GetKeywordName(property.name, _keyWords[i]);
			return keyWords;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position; //EditorGUILayout.GetControlRect();

			string[] keyWords = GetKeywords(prop);
			int index = Array.IndexOf(_values, prop.floatValue);
			if (index < 0)
			{
				index = 0;
				if (!prop.hasMixedValue)
				{
					Debug.LogError("Property: " + prop.displayName + " has unknown Enum Value: '" + prop.floatValue + "' !\n"
								 + "It will be set to: '" + _values[index] + "'!");
					prop.floatValue = _values[index];
					Helper.SetShaderKeyWord(editor.targets, keyWords, index);
				}
			}
			// if (prop.displayName)
			string[] CacheNames = prop.displayName.Split("/");
			this._newNames = new GUIContent[CacheNames.Length - 1];
			for (int j = 0; j < _newNames.Length; j++)
			{
				_newNames[j] = new GUIContent(CacheNames[j]);
			}

			// Helper.AdaptiveFieldWidth(EditorStyles.popup, _names[index], EditorStyles.popup.lineHeight);
			int newIndex;
			if (prop.displayName.Contains("/"))
			{
				GUIContent displayName = new GUIContent(CacheNames[CacheNames.Length - 1]);
				newIndex = EditorGUI.Popup(rect, displayName, index, _newNames);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
			}
			else
			{
				newIndex = EditorGUI.Popup(rect, label, index, _names);//绘制枚举功能,被挡住后无法显示,最后处绘制枚举外观
			}
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = _values[newIndex];
				Helper.SetShaderKeyWord(editor.targets, keyWords, newIndex);
			}

			// set keyword for conditional display
			for (int i = 0; i < keyWords.Length; i++)
			{
				GroupStateHelper.SetKeywordConditionalDisplay(editor.target, keyWords[i], newIndex == i);
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			string[] keyWords = GetKeywords(prop);
			if (prop.floatValue == 0f)
			{
				Helper.SetShaderKeyWord(editor.targets, keyWords, 0);
			}
			return base.GetPropertyHeight(prop, label, editor);
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, GetKeywords(prop), (int)prop.floatValue);
		}
	}
	internal class LogicalSubEnumDrawer : LogicalKWEnumDrawer
	{
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2)
			: base(logicalGroup, new[] { n1, n2 }, null, new[] { v1, v2 }) { }
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2, string n3, float v3)
			: base(logicalGroup, new[] { n1, n2, n3 }, null, new[] { v1, v2, v3 }) { }
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
			: base(logicalGroup, new[] { n1, n2, n3, n4 }, null, new[] { v1, v2, v3, v4 }) { }
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
			: base(logicalGroup, new[] { n1, n2, n3, n4, n5 }, null, new[] { v1, v2, v3, v4, v5 }) { }
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
			: base(logicalGroup, new[] { n1, n2, n3, n4, n5, n6 }, null, new[] { v1, v2, v3, v4, v5, v6 }) { }
		public LogicalSubEnumDrawer(string logicalGroup, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
			: base(logicalGroup, new[] { n1, n2, n3, n4, n5, n6, n7 }, null, new[] { v1, v2, v3, v4, v5, v6, v7 }) { }

		protected override string GetKeywordName(string propName, string name) { return "_"; }
	}
	internal class LogicalSubKeywordEnumDrawer : LogicalKWEnumDrawer
	{
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2)
			: base(logicalGroup, new[] { kw1, kw2 }, new[] { kw1, kw2 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3)
			: base(logicalGroup, new[] { kw1, kw2, kw3 }, new[] { kw1, kw2, kw3 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4 }, new[] { kw1, kw2, kw3, kw4 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4, string kw5)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4, kw5 }, new[] { kw1, kw2, kw3, kw4, kw5 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }
		public LogicalSubKeywordEnumDrawer(string logicalGroup, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
			: base(logicalGroup, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }
		protected override string GetKeywordName(string propName, string name) { return name.Replace(' ', '_').ToUpperInvariant(); }

	}


	/// <summary>
	/// Draw a Ramp Map Editor (Defaulf Ramp Map Resolution: 512 * 2)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
	/// defaultWidth: default Ramp Width (Default: 512)
	/// Target Property Type: Texture2D
	/// </summary>
	internal class LogicalRampDrawer : LogicalSubDrawer
	{
		private string _defaultFileName;
		private float _defaultWidth;
		private float _defaultHeight = 2;
		private bool _isDirty;

		// used to read/write Gradient value in code
		private RampHelper.GradientObject _gradientObject;
		// used to modify Gradient value for users
		private SerializedObject _serializedObject;

		private static readonly GUIContent _iconMixImage = EditorGUIUtility.IconContent("darkviewbackground");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight * 2f; }

		public LogicalRampDrawer() : this(String.Empty) { }
		public LogicalRampDrawer(string logicalGroup) : this(logicalGroup, "RampMap") { }
		public LogicalRampDrawer(string logicalGroup, string defaultFileName) : this(logicalGroup, defaultFileName, 512) { }

		public LogicalRampDrawer(string logicalGroup, string defaultFileName, float defaultWidth)
		{
			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}
			// this.group = group;
			this._defaultFileName = defaultFileName;
			this._defaultWidth = Mathf.Max(2.0f, defaultWidth);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// TODO: cache these variables between different prop?
			_gradientObject = ScriptableObject.CreateInstance<RampHelper.GradientObject>();
			_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty);
			_serializedObject = new SerializedObject(_gradientObject);

			// Draw Label
			var labelRect = new Rect(position);//EditorGUILayout.GetControlRect();
			labelRect.yMax -= position.height * 0.5f;
			EditorGUI.PrefixLabel(labelRect, label);

			// Ramp buttons Rect
			var labelWidth = EditorGUIUtility.labelWidth;
			var indentLevel = EditorGUI.indentLevel;
			EditorGUIUtility.labelWidth = 0;
			EditorGUI.indentLevel = 0;
			var buttonRect = new Rect(position);//EditorGUILayout.GetControlRect();
			buttonRect.yMin += position.height * 0.5f;
			buttonRect = MaterialEditor.GetRectAfterLabelWidth(buttonRect);
			if (buttonRect.width < 50f) return;

			// Draw Ramp Editor
			bool hasChange, doSave, doDiscard;
			Texture newUserTexture, newCreatedTexture;
			hasChange = RampHelper.RampEditor(prop, buttonRect, _serializedObject.FindProperty("gradient"), _isDirty,
											  _defaultFileName, (int)_defaultWidth, (int)_defaultHeight,
											  out newCreatedTexture, out doSave, out doDiscard);

			if (hasChange || doSave)
			{
				// TODO: undo support
				// Undo.RecordObject(_gradientObject, "Edit Gradient");
				_serializedObject.ApplyModifiedProperties();
				RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, doSave);
				// EditorUtility.SetDirty(_gradientObject);
			}

			// Texture object field
			var textureRect = MaterialEditor.GetRectAfterLabelWidth(labelRect);
			newUserTexture = (Texture)EditorGUI.ObjectField(textureRect, prop.textureValue, typeof(Texture2D), false);

			// When tex has changed, update vars
			if (newUserTexture != prop.textureValue || newCreatedTexture != null || doDiscard)
			{
				if (newUserTexture != prop.textureValue)
					prop.textureValue = newUserTexture;
				if (newCreatedTexture != null)
					prop.textureValue = newCreatedTexture;
				_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty, doDiscard);
				_serializedObject.Update();
				if (doDiscard)
					RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, true);
			}

			// Preview texture override (larger preview, hides texture name)
			var previewRect = new Rect(textureRect.x + 1, textureRect.y + 1, textureRect.width - 19, textureRect.height - 2);
			if (prop.hasMixedValue)
			{
				EditorGUI.DrawPreviewTexture(previewRect, _iconMixImage.image);
				GUI.Label(new Rect(previewRect.x + previewRect.width * 0.5f - 10, previewRect.y, previewRect.width * 0.5f, previewRect.height), "―");
			}
			else if (prop.textureValue != null)
				EditorGUI.DrawPreviewTexture(previewRect, prop.textureValue);

			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}
	}

	/// <summary>
	/// Draw a min max slider (Unity 2019.2+ only)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// minPropName: Output Min Property Name
	/// maxPropName: Output Max Property Name
	/// Target Property Type: Range, range limits express the MinMaxSlider value range
	/// Output Min/Max Property Type: Range, it's value is limited by it's range
	/// </summary>
	internal class LogicalMinMaxSliderDrawer : LogicalSubDrawer
	{
		private string _minPropName;
		private string _maxPropName;

		public LogicalMinMaxSliderDrawer(string minPropName, string maxPropName) : this("_", minPropName, maxPropName) { }
		public LogicalMinMaxSliderDrawer(string logicalGroup, string minPropName, string maxPropName)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}
			// this.group = group;
			this._minPropName = minPropName;
			this._maxPropName = maxPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			var minProp = LogicalSGUI.FindProp(_minPropName, inProps, true);
			var maxProp = LogicalSGUI.FindProp(_maxPropName, inProps, true);
			MetaDataHelper.RegisterSubProp(inShader, inProp, _groups[0], new[] { minProp, maxProp });
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
															RevertableHelper.GetDefaultProperty(inShader, minProp).floatValue + " - " +
															RevertableHelper.GetDefaultProperty(inShader, maxProp).floatValue);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// read min max
			MaterialProperty min = LogicalSGUI.FindProp(_minPropName, props, true);
			MaterialProperty max = LogicalSGUI.FindProp(_maxPropName, props, true);
			if (min == null || max == null)
			{
				Debug.LogError("MinMaxSliderDrawer: minProp: " + (min == null ? "null" : min.name) + " or maxProp: " + (max == null ? "null" : max.name) + " not found!");
				return;
			}
			float minf = min.floatValue;
			float maxf = max.floatValue;

			// define draw area
			Rect controlRect = position; //EditorGUILayout.GetControlRect(); // this is the full length rect area
			var w = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;
			Rect inputRect = MaterialEditor.GetRectAfterLabelWidth(controlRect); // this is the remaining rect area after label's area
			EditorGUIUtility.labelWidth = w;

			// draw label
			EditorGUI.LabelField(controlRect, label);

			// draw min max slider
			Rect[] splittedRect = Helper.SplitRect(inputRect, 3);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = min.hasMixedValue;
			var newMinf = EditorGUI.FloatField(splittedRect[0], minf);
			if (EditorGUI.EndChangeCheck())
			{
				minf = Mathf.Clamp(newMinf, min.rangeLimits.x, min.rangeLimits.y);
				min.floatValue = minf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = max.hasMixedValue;
			var newMaxf = EditorGUI.FloatField(splittedRect[2], maxf);
			if (EditorGUI.EndChangeCheck())
			{
				maxf = Mathf.Clamp(newMaxf, max.rangeLimits.x, max.rangeLimits.y);
				max.floatValue = maxf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			if (splittedRect[1].width > 50f)
				EditorGUI.MinMaxSlider(splittedRect[1], ref minf, ref maxf, prop.rangeLimits.x, prop.rangeLimits.y);
			EditorGUI.showMixedValue = false;

			// write back min max if changed
			if (EditorGUI.EndChangeCheck())
			{
				min.floatValue = Mathf.Clamp(minf, min.rangeLimits.x, min.rangeLimits.y);
				max.floatValue = Mathf.Clamp(maxf, max.rangeLimits.x, max.rangeLimits.y);
			}

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, position, true);
			if (RevertableHelper.DrawRevertableProperty(revertButtonRect, min, editor, shader) ||
				RevertableHelper.DrawRevertableProperty(revertButtonRect, max, editor, shader))
			{
				RevertableHelper.SetPropertyToDefault(shader, min);
				RevertableHelper.SetPropertyToDefault(shader, max);
			}

		}
	}


	/// <summary>
	/// Draw a R/G/B/A drop menu
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Vector, used to dot() with Texture Sample Value 
	/// </summary>
	internal class LogicalChannelDrawer : LogicalSubDrawer
	{
		private static GUIContent[] _names = new[] { new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A"),
			new GUIContent("RGB Average"), new GUIContent("RGB Luminance") };
		private static int[] _intValues = new int[] { 0, 1, 2, 3, 4, 5 };
		private static Vector4[] _vector4Values = new[]
		{
			new Vector4(1, 0, 0, 0),
			new Vector4(0, 1, 0, 0),
			new Vector4(0, 0, 1, 0),
			new Vector4(0, 0, 0, 1),
			new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
			new Vector4(0.2126f, 0.7152f, 0.0722f, 0)
		};

		public LogicalChannelDrawer(string logicalGroup)
		{
			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Vector; }

		private static int GetChannelIndex(MaterialProperty prop)
		{
			int index;
			if (prop.vectorValue == _vector4Values[0])
				index = 0;
			else if (prop.vectorValue == _vector4Values[1])
				index = 1;
			else if (prop.vectorValue == _vector4Values[2])
				index = 2;
			else if (prop.vectorValue == _vector4Values[3])
				index = 3;
			else if (prop.vectorValue == _vector4Values[4])
				index = 4;
			else if (prop.vectorValue == _vector4Values[5])
				index = 5;
			else
			{
				Debug.LogError($"Channel Property:{prop.name} invalid vector found, reset to A");
				prop.vectorValue = _vector4Values[3];
				index = 3;
			}
			return index;
		}

		public static string GetChannelName(MaterialProperty prop)
		{
			return _names[GetChannelIndex(prop)].text;
		}

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			base.InitMetaData(inShader, inProp, inProps);
			MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp, GetChannelName(RevertableHelper.GetDefaultProperty(inShader, inProp)));
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var rect = position; //EditorGUILayout.GetControlRect();
			var index = GetChannelIndex(prop);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			int num = EditorGUI.IntPopup(rect, label, index, _names, _intValues);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.vectorValue = _vector4Values[num];
			}
		}
	}


	internal class LogicalTexDrawer : LogicalSubDrawer
	{
		private readonly bool _isDisplayST;
		private enum ChannelSplit
		{
			R_G_B_A = 0,
			RG_B_A = 1,
			RGB_A = 2
		}
		private ChannelSplit _channelSplit;
		private ChannelSplit _channelSplit0;

		private string[] shiftAs_RGB_A_Keywords;

		private static readonly Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("1c08516d56763d1448db75dc9a9b73be"));

		public LogicalTexDrawer() { }

		public LogicalTexDrawer(string logicalGroup, string isDisplayST, string channelSplit, string shiftAsRGB_A)
		{
			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

			this._isDisplayST = isDisplayST == "true";
			switch (channelSplit)
			{
				case "R_G_B_A":
					this._channelSplit = ChannelSplit.R_G_B_A;
					this._channelSplit0 = ChannelSplit.R_G_B_A;
					break;
				case "RG_B_A":
					this._channelSplit = ChannelSplit.RG_B_A;
					this._channelSplit0 = ChannelSplit.RG_B_A;
					break;
				case "RGB_A":
					this._channelSplit = ChannelSplit.RGB_A;
					this._channelSplit0 = ChannelSplit.RGB_A;
					break;
				default:
					this._channelSplit = ChannelSplit.R_G_B_A;
					this._channelSplit0 = ChannelSplit.R_G_B_A;
					if (!channelSplit.Contains("R_G_B_A") && !channelSplit.Contains("RG_B_A") && !channelSplit.Contains("RGB_A"))
					{
						Debug.LogError("第二个参数channelSplit =" + channelSplit + ",必须包含\"R_G_B_A\",\"RG_B_A\",\"RGB_A\"之一!");
					}
					break;
			}
			if (shiftAsRGB_A != null && shiftAsRGB_A != "_")
			{
				if (shiftAsRGB_A.Contains(" "))
				{
					this.shiftAs_RGB_A_Keywords = shiftAsRGB_A.Split(" ");
				}
				else
				{
					this.shiftAs_RGB_A_Keywords = new string[1];
					shiftAs_RGB_A_Keywords[0] = shiftAsRGB_A;
				}
			}

		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			Rect rect_h_1 = new Rect(position);
			Rect rect_st_0 = new Rect(position);
			Rect rect_info = new Rect(position);
			rect_h_1.y -= 3;
			rect_h_1.x += 4;
			rect_h_1.width += 0;
			EditorGUI.HelpBox(rect_h_1, "", MessageType.None);

			Texture2D texture01 = (Texture2D)editor.TextureProperty(position, prop, label.text, false);
			if (texture01 != null)
			{
				if (this.shiftAs_RGB_A_Keywords != null)
				{
					for (int i = 0; i < shiftAs_RGB_A_Keywords.Length; i++)
					{
						if ((editor.target as Material).IsKeywordEnabled(shiftAs_RGB_A_Keywords[i]))
							this._channelSplit = ChannelSplit.RGB_A;
						else
						{
							this._channelSplit = this._channelSplit0;
						}
					}
				}

				switch (_channelSplit)
				{
					case ChannelSplit.RGB_A:
						{
							Rect rect_r_A = new Rect(position);
							rect_r_A.x = rect_r_A.x + position.width - 130;
							rect_r_A.height = rect_r_A.height - 6;
							rect_r_A.width = rect_r_A.height;
							EditorGUI.DrawRect(rect_r_A, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_B = new Rect(rect_r_A);
							rect1_r_B.x -= 66;
							EditorGUI.DrawRect(rect1_r_B, new Color(0.1f, 0.1f, 0.1f, 1.0f));
							rect_info = rect1_r_B;

							Rect rect_t_A = new Rect(rect_r_A);
							rect_t_A.height -= 6;
							rect_t_A.width = rect_t_A.height;
							rect_t_A.x += 3;
							rect_t_A.y += 3;
							int mip = texture01.width / 256 / 2 + 1;
							EditorGUI.DrawPreviewTexture(rect_t_A, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Alpha, 0);

							Rect rect_t_B = new Rect(rect_t_A);
							rect_t_B.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_B, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.All, 0);

							GUIStyle gUIStyle = new GUIStyle(EditorStyles.boldLabel);
							//A
							Rect rect_l_A = new Rect(rect_t_A);
							rect_l_A.x += 8;
							rect_l_A.y += 25;
							gUIStyle.normal.textColor = Color.gray;
							EditorGUI.LabelField(rect_l_A, new GUIContent("A"), gUIStyle);

							//RGB (B槽)
							Rect rect_l_B = new Rect(rect_l_A);
							rect_l_B.x -= 64;
							Rect rect_l_RGB_R = new Rect(rect_l_B);
							rect_l_RGB_R.x -= 8;
							gUIStyle.normal.textColor = Color.red;
							EditorGUI.LabelField(rect_l_RGB_R, new GUIContent("R"), gUIStyle);
							Rect rect_l_RGB_G = new Rect(rect_l_B);
							rect_l_RGB_G.x -= 0;
							gUIStyle.normal.textColor = Color.green;
							EditorGUI.LabelField(rect_l_RGB_G, new GUIContent("G"), gUIStyle);
							Rect rect_l_RGB_B = new Rect(rect_l_B);
							rect_l_RGB_B.x += 8;
							gUIStyle.normal.textColor = Color.blue;
							EditorGUI.LabelField(rect_l_RGB_B, new GUIContent("B"), gUIStyle);
						}
						break;
					case ChannelSplit.RG_B_A:
						{
							Rect rect_r_A = new Rect(position);
							rect_r_A.x = rect_r_A.x + position.width - 130;
							rect_r_A.height = rect_r_A.height - 6;
							rect_r_A.width = rect_r_A.height;
							EditorGUI.DrawRect(rect_r_A, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_B = new Rect(rect_r_A);
							rect1_r_B.x -= 66;
							EditorGUI.DrawRect(rect1_r_B, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_G = new Rect(rect1_r_B);
							rect1_r_G.x -= 66;
							EditorGUI.DrawRect(rect1_r_G, new Color(0.1f, 0.1f, 0.1f, 1.0f));
							rect_info = rect1_r_G;

							Rect rect_t_A = new Rect(rect_r_A);
							rect_t_A.height -= 6;
							rect_t_A.width = rect_t_A.height;
							rect_t_A.x += 3;
							rect_t_A.y += 3;
							int mip = texture01.width / 256 / 2 + 1;
							EditorGUI.DrawPreviewTexture(rect_t_A, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Alpha, 0);

							Rect rect_t_B = new Rect(rect_t_A);
							rect_t_B.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_B, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Blue, 0);

							Rect rect_t_G = new Rect(rect_t_B);
							rect_t_G.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_G, texture01, material, ScaleMode.StretchToFill, 0, mip, 0, 0);

							GUIStyle gUIStyle = new GUIStyle(EditorStyles.boldLabel);
							//A
							Rect rect_l_A = new Rect(rect_t_A);
							rect_l_A.x += 8;
							rect_l_A.y += 25;
							gUIStyle.normal.textColor = Color.gray;
							EditorGUI.LabelField(rect_l_A, new GUIContent("A"), gUIStyle);

							//B
							Rect rect_l_B = new Rect(rect_l_A);
							rect_l_B.x -= 66;
							gUIStyle.normal.textColor = Color.blue;
							EditorGUI.LabelField(rect_l_B, new GUIContent("B"), gUIStyle);

							//G
							Rect rect_l_G = new Rect(rect_l_B);
							rect_l_G.x -= 66;
							Rect rect_l_RG_R = new Rect(rect_l_G);
							rect_l_RG_R.x -= 4;
							gUIStyle.normal.textColor = Color.red;
							EditorGUI.LabelField(rect_l_RG_R, new GUIContent("R"), gUIStyle);
							Rect rect_l_RG_G = new Rect(rect_l_G);
							rect_l_RG_G.x += 4;
							gUIStyle.normal.textColor = Color.green;
							EditorGUI.LabelField(rect_l_RG_G, new GUIContent("G"), gUIStyle);
							break;
						}
					case ChannelSplit.R_G_B_A:
						{
							Rect rect_r_A = new Rect(position);
							rect_r_A.x = rect_r_A.x + position.width - 130;
							rect_r_A.height = rect_r_A.height - 6;
							rect_r_A.width = rect_r_A.height;
							EditorGUI.DrawRect(rect_r_A, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_B = new Rect(rect_r_A);
							rect1_r_B.x -= 66;
							EditorGUI.DrawRect(rect1_r_B, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_G = new Rect(rect1_r_B);
							rect1_r_G.x -= 66;
							EditorGUI.DrawRect(rect1_r_G, new Color(0.1f, 0.1f, 0.1f, 1.0f));

							Rect rect1_r_R = new Rect(rect1_r_G);
							rect1_r_R.x -= 66;
							EditorGUI.DrawRect(rect1_r_R, new Color(0.1f, 0.1f, 0.1f, 1.0f));
							rect_info = rect1_r_R;

							Rect rect_t_A = new Rect(rect_r_A);
							rect_t_A.height -= 6;
							rect_t_A.width = rect_t_A.height;
							rect_t_A.x += 3;
							rect_t_A.y += 3;
							int mip = texture01.width / 256 / 2 + 1;
							EditorGUI.DrawPreviewTexture(rect_t_A, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Alpha, 0);

							Rect rect_t_B = new Rect(rect_t_A);
							rect_t_B.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_B, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Blue, 0);

							Rect rect_t_G = new Rect(rect_t_B);
							rect_t_G.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_G, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Green, 0);

							Rect rect_t_R = new Rect(rect_t_G);
							rect_t_R.x -= 66;
							EditorGUI.DrawPreviewTexture(rect_t_R, texture01, material, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.Red, 0);


							GUIStyle gUIStyle = new GUIStyle(EditorStyles.boldLabel);
							//A
							Rect rect_l_A = new Rect(rect_t_A);
							rect_l_A.x += 8;
							rect_l_A.y += 25;
							gUIStyle.normal.textColor = Color.gray;
							EditorGUI.LabelField(rect_l_A, new GUIContent("A"), gUIStyle);

							//B
							Rect rect_l_B = new Rect(rect_l_A);
							rect_l_B.x -= 66;
							gUIStyle.normal.textColor = Color.blue;
							EditorGUI.LabelField(rect_l_B, new GUIContent("B"), gUIStyle);

							//G
							Rect rect_l_G = new Rect(rect_l_B);
							rect_l_G.x -= 66;
							gUIStyle.normal.textColor = Color.green;
							EditorGUI.LabelField(rect_l_G, new GUIContent("G"), gUIStyle);

							//R
							Rect rect_l_R = new Rect(rect_l_G);
							rect_l_R.x -= 66;
							gUIStyle.normal.textColor = Color.red;
							EditorGUI.LabelField(rect_l_R, new GUIContent("R"), gUIStyle);
							break;
						}
					default:
						Debug.LogError("第二个参数channelSplit,必须输入\"R_G_B_A\",\"RG_B_A\",\"RGB_A\"之一!");
						break;
				}
				rect_info.x -= 100;
				rect_info.y -= 0;
				rect_info.width += 100;
				rect_info.height -= 0;
				string texInfo = new string("分辨率:    " + texture01.width + "X" + texture01.height + "\n" + "包装模式: " + texture01.wrapMode + "\n" +
				"过滤模式: " + texture01.filterMode + "\n" + "纹理格式: " + texture01.format + "\n" + "更新计数: " + texture01.updateCount);
				GUI.Label(rect_info, texInfo, EditorStyles.miniLabel);

				if (_isDisplayST)
				{
#if UNITY_2023_1_OR_NEWER
					rect_st_0.x -= position.width;
					rect_st_0.x += 70;
					rect_st_0.width += rect_info.x;
					rect_st_0.width -= 105;
					rect_st_0.y += 25;
#else
					rect_st_0.x -= position.width;
					rect_st_0.x += 70;
					rect_st_0.width += rect_info.x;
					rect_st_0.width -= 89;
					rect_st_0.y += 25;
#endif
					editor.TextureScaleOffsetProperty(rect_st_0, prop, false);
				}

			}


			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));


			string[] strings = prop.displayName.Split("#");
			string s1 = "";
			for (int i = 0; i < strings.Length; i++)
			{
				s1 += strings[i] + "\n";
			}
			// Debug.Log(_groups[0]);

			if(_groups[0] != "_" && _groups[0] != null)
			{
				Vector4 vector = (editor.target as Material).GetVector(_groups[0].Split("_")[0]);
				if (prop.displayName.Contains("#") && !this._isDisplayST && prop.textureValue != null)
				{
					string[] strings1 = prop.displayName.Split("#");
					string[] strings2 = new string[strings1.Length - 1];
					for (int i = 0; i < strings2.Length; i++)
					{
						strings2[i] = strings1[i + 1];
					}
					Rect rect = new Rect(rect_info);
	#if UNITY_2023_1_OR_NEWER
					rect.x = position.x;
					rect.width = rect_info.x;
					rect.x += 10;
					rect.width -= 45;
					rect.y += 16;
					rect.height -= 16;
	#else
					rect.x = position.x;
					rect.width = rect_info.x;
					rect.x += 10;
					rect.width -= 28;
					rect.y += 16;
					rect.height -= 16;
	#endif

					EditorGUI.HelpBox(rect, "当前使用“" + strings2[(int)vector.y].Split(":")[0] + "”" + "\n" + "通道信息 : " + strings2[(int)vector.y].Split(":")[1], MessageType.None);
				}
				else if (prop.displayName.Contains("#") && !this._isDisplayST && prop.textureValue == null)
				{
					string[] strings1 = prop.displayName.Split("#");
					string[] strings2 = new string[strings1.Length - 1];
					for (int i = 0; i < strings2.Length; i++)
					{
						strings2[i] = strings1[i + 1];
					}
					Rect rect = new Rect(position);
					rect.x += 10;
					rect.width -= 75;
					rect.y += 16;
					rect.height -= 23;

					EditorStyles.helpBox.fontSize += 3;
					EditorGUI.HelpBox(rect, "当前使用“" + strings2[(int)vector.y].Split(":")[0] + "”" + "\n" + "通道信息 : " + strings2[(int)vector.y].Split(":")[1], MessageType.Info);
					EditorStyles.helpBox.fontSize -= 3;

				}
				// Vector4 vector1 = editor.
			}


			//在logical运算符前添加一个参数,以区分,R_G_B_A,RG_B_A,RGB_A三种不同的拆分方式,再添加keyword判断在开启关键字是使用那种拆分方式,使用分隔符号 分隔
			//添加一个控制ST的属性
			//添加一个启用关键字显示表,用于debug 

		}
	}


	/// <summary>
	/// Draw a Texture property in single line with a extra property
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// extraPropName: extra property name (Unity 2019.2+ only) (Default: none)
	/// Target Property Type: Texture
	/// Extra Property Type: Any, except Texture
	/// </summary>
	internal class LogicalLineTexDrawer : LogicalSubDrawer
	{
		private string _extraPropName = String.Empty;
		private ChannelDrawer _channelDrawer = new ChannelDrawer("_");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }

		public LogicalLineTexDrawer() { }

		public LogicalLineTexDrawer(string logicalGroup) : this(logicalGroup, String.Empty) { }

		public LogicalLineTexDrawer(string logicalGroup, string extraPropName)
		{
			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

			// this.group = group;
			this._extraPropName = extraPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void InitMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps)
		{
			MaterialProperty extraProp = LogicalSGUI.FindProp(_extraPropName, inProps, true);
			MetaDataHelper.RegisterSubProp(inShader, inProp, _groups[0], extraProp == null ? null : new[] { extraProp });
			if (extraProp != null)
			{
				var text = string.Empty;
				if (extraProp.type == MaterialProperty.PropType.Vector)
					text = ChannelDrawer.GetChannelName(extraProp);
				else
					text = RevertableHelper.GetPropertyDefaultValueText(inShader, extraProp);

				MetaDataHelper.RegisterPropertyDefaultValueText(inShader, inProp,
																RevertableHelper.GetPropertyDefaultValueText(inShader, inProp) + ", " + text);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			var texLabel = label.text;

			MaterialProperty extraProp = LogicalSGUI.FindProp(_extraPropName, props, true);
			if (extraProp != null && extraProp.type != MaterialProperty.PropType.Texture)
			{
				var i = EditorGUI.indentLevel;
				Rect indentedRect, extraPropRect = new Rect(rect);
				switch (extraProp.type)
				{
#if UNITY_2021_1_OR_NEWER
					case MaterialProperty.PropType.Int:
#endif
					case MaterialProperty.PropType.Color:
					case MaterialProperty.PropType.Float:
					case MaterialProperty.PropType.Vector:
						texLabel = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						RevertableHelper.SetRevertableGUIWidths();
						EditorGUIUtility.labelWidth -= (indentedRect.xMin - extraPropRect.xMin) + 30f;
						extraPropRect = indentedRect;
						extraPropRect.xMin += 30f;
						EditorGUI.indentLevel = 0;
						break;
					case MaterialProperty.PropType.Range:
						label.text = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						editor.SetDefaultGUIWidths();
						EditorGUIUtility.fieldWidth += 1f;
						EditorGUIUtility.labelWidth = 0;
						EditorGUI.indentLevel = 0;
						extraPropRect = MaterialEditor.GetRectAfterLabelWidth(extraPropRect);
						extraPropRect.xMin += 2;
						break;
				}

				if (extraProp.type == MaterialProperty.PropType.Vector)
					_channelDrawer.DrawProp(extraPropRect, extraProp, label, editor);
				else
					editor.ShaderProperty(extraPropRect, extraProp, label);

				EditorGUI.indentLevel = i;

				var revertButtonRect = RevertableHelper.GetRevertButtonRect(extraProp, position, true);
				if (RevertableHelper.IsPropertyShouldRevert(editor.target, prop.name) ||
					RevertableHelper.DrawRevertableProperty(revertButtonRect, extraProp, editor, shader))
				{
					RevertableHelper.SetPropertyToDefault(shader, prop);
					RevertableHelper.SetPropertyToDefault(shader, extraProp);
					RevertableHelper.RemovePropertyShouldRevert(editor.targets, prop.name);
				}
			}

			editor.TexturePropertyMiniThumbnail(rect, prop, texLabel, label.tooltip);

			EditorGUI.showMixedValue = false;
		}
	}


	internal class LogicalKeywordListDrawer : LogicalSubDrawer
	{
		public LogicalKeywordListDrawer(string logicalGroup)
		{
			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

		}
		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			string enableLocalKeyword = "";
			UnityEngine.Rendering.LocalKeyword[] localKeywords = (editor.target as Material).enabledKeywords;
			for (int i = 0; i < localKeywords.Length; i++)
			{
				enableLocalKeyword += localKeywords[i].name + "\n";
			}
			Rect rect = new Rect(position);
			rect.width /= 2;
			rect.width -= 15;
			EditorGUI.TextArea(rect, "启用的本地关键字: " + "\n" + enableLocalKeyword);

			string enableGlobalKeyword = "";
			UnityEngine.Rendering.GlobalKeyword[] globalKeywords = Shader.enabledGlobalKeywords;
			for (int i = 0; i < globalKeywords.Length; i++)
			{
				enableGlobalKeyword += globalKeywords[i].name + "\n";
			}
			Rect rect0 = new Rect(position);
			rect0.width /= 2;
			rect0.x += rect.width;
			rect0.width += 25;
			rect0.x -= 10;
			EditorGUI.TextArea(rect0, "启用的全局关键字: " + "\n" + enableGlobalKeyword);
		}
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return 150;
		}

	}

	/// <summary>
	/// Similar to Header()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// tips: Modifying the Decorator parameters in Shader requires manually refreshing the GUI instance by throwing an exception
	/// </summary>
	internal class LogicalTitleDecorator : LogicalSubDrawer
	{
		private string _header;

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight + 6f; }

		public LogicalTitleDecorator(string header) : this("_", header) { }
		public LogicalTitleDecorator(string logicalGroup, string header)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}
			// this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position.y += 2;
			position.x -= 12;
			position = EditorGUI.IndentedRect(position);
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			GUI.Label(position, _header, style);
		}
	}

	internal class LogicalEmissionDrawer : LogicalSubDrawer
	{
		public LogicalEmissionDrawer() { }
		public LogicalEmissionDrawer(string logicalGroup)
		{

			if (logicalGroup.Contains(" indent"))
			{
				logicalGroup = logicalGroup.Replace(" indent", "");
				this.isIndent = true;
			}
			else
			{
				this.isIndent = false;
			}

			if (logicalGroup.Contains(" or "))
			{
				this._groups = logicalGroup.Split(" or ");
				_logical = true;
			}
			else if (logicalGroup.Contains(" and "))
			{
				this._groups = logicalGroup.Split(" and ");
				_logical = false;
			}
			else
			{
				this._groups = new string[1];
				_groups[0] = logicalGroup;
				_logical = true;
			}

		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.LightmapEmissionFlagsProperty(0, true);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}

	}


} //namespace LogicalSGUI