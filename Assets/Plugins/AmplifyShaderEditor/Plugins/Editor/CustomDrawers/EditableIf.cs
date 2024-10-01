
































































using UnityEngine;
using UnityEditor;
using System;

public enum ComparisonOperators
{
	EqualTo, NotEqualTo, GreaterThan, LessThan, EqualsOrGreaterThan, EqualsOrLessThan, ContainsFlags,
	DoesNotContainsFlags
}

public class EditableIf : MaterialPropertyDrawer
{
	ComparisonOperators op;
	string FieldName = "";
	object ExpectedValue;
	bool InputError;
	public EditableIf()
	{
		InputError = true;
	}
	public EditableIf( object fieldname, object comparison, object expectedvalue )
	{
		if( expectedvalue.ToString().ToLower() == "true" )
		{
			expectedvalue = (System.Single)1;
		}
		else if( expectedvalue.ToString().ToLower() == "false" )
		{
			expectedvalue = (System.Single)0;

		}
		Init( fieldname, comparison, expectedvalue );

	}
	public EditableIf( object fieldname, object comparison, object expectedvaluex, object expectedvaluey )
	{
		float? x = expectedvaluex as float?;
		float? y = expectedvaluey as float?;
		float? z = float.NegativeInfinity;
		float? w = float.NegativeInfinity;
		x = GetVectorValue( x );
		y = GetVectorValue( y );

		Init( fieldname, comparison, new Vector4( x.Value, y.Value, z.Value, w.Value ) );
	}
	public EditableIf( object fieldname, object comparison, object expectedvaluex, object expectedvaluey, object expectedvaluez )
	{
		float? x = expectedvaluex as float?;
		float? y = expectedvaluey as float?;
		float? z = expectedvaluez as float?;
		float? w = float.NegativeInfinity;
		x = GetVectorValue( x );
		y = GetVectorValue( y );
		z = GetVectorValue( z );

		Init( fieldname, comparison, new Vector4( x.Value, y.Value, z.Value, w.Value ) );

	}
	public EditableIf( object fieldname, object comparison, object expectedvaluex, object expectedvaluey, object expectedvaluez, object expectedvaluew )
	{
		var x = expectedvaluex as float?;
		var y = expectedvaluey as float?;
		var z = expectedvaluez as float?;
		var w = expectedvaluew as float?;
		x = GetVectorValue( x );
		y = GetVectorValue( y );
		z = GetVectorValue( z );
		w = GetVectorValue( w );

		Init( fieldname, comparison, new Vector4( x.Value, y.Value, z.Value, w.Value ) );

	}

	private void Init( object fieldname, object comparison, object expectedvalue )
	{
		FieldName = fieldname.ToString();
		var names = Enum.GetNames( typeof( ComparisonOperators ) );
		var name = comparison.ToString().ToLower().Replace( " ", "" );

		for( int i = 0; i < names.Length; i++ )
		{
			if( names[ i ].ToLower() == name )
			{
				op = (ComparisonOperators)i;
				break;
			}
		}

		ExpectedValue = expectedvalue;
	}

	private static float? GetVectorValue( float? x )
	{
		if( x.HasValue == false )
		{
			x = float.NegativeInfinity;
		}

		return x;
	}

	
	public override void OnGUI( Rect position, MaterialProperty prop, String label, MaterialEditor editor )
	{
		if( InputError )
		{
			EditorGUI.LabelField( position, "EditableIf Attribute Error: Input parameters are invalid!" );
			return;
		}
		var LHSprop = MaterialEditor.GetMaterialProperty( prop.targets, FieldName );
		if( string.IsNullOrEmpty( LHSprop.name ) )
		{
			LHSprop = MaterialEditor.GetMaterialProperty( prop.targets, "_" + FieldName.Replace( " ", "" ) );
			if( string.IsNullOrEmpty( LHSprop.name ) )
			{
				EditorGUI.LabelField( position, "EditableIf Attribute Error: " + FieldName + " Does not exist!" );
				return;
			}
		}
		object LHSVal = null;

		bool test = false;
		switch( LHSprop.type )
		{
			case MaterialProperty.PropType.Color:
			case MaterialProperty.PropType.Vector:
			LHSVal = LHSprop.type == MaterialProperty.PropType.Color ? (Vector4)LHSprop.colorValue : LHSprop.vectorValue;
			var v4 = ExpectedValue as Vector4?;
			v4 = v4.HasValue ? v4 : new Vector4( (System.Single)ExpectedValue, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity );

			if( LHSprop.type == MaterialProperty.PropType.Color )
			{
				test = VectorCheck( (Vector4)LHSVal, op, v4 / 255 );

			}
			else
				test = VectorCheck( (Vector4)LHSVal, op, v4 );
			break;
			case MaterialProperty.PropType.Range:
			case MaterialProperty.PropType.Float:
			LHSVal = LHSprop.floatValue;
			test = ( Check( LHSVal, op, ExpectedValue ) );
			break;
			case MaterialProperty.PropType.Texture:
			LHSVal = LHSprop.textureValue;
			test = ( CheckObject( LHSVal, op, ExpectedValue ) );
			break;
		}

		GUI.enabled = test;
		editor.DefaultShaderProperty( position, prop, label );
		GUI.enabled = true;
	}

	private bool VectorCheck( Vector4 LHS, ComparisonOperators op, object expectedValue )
	{
		var RHS = (Vector4)expectedValue;
		if( RHS.x != float.NegativeInfinity )
		{
			if( !Check( LHS.x, op, RHS.x ) )
				return false;
		}
		if( RHS.y != float.NegativeInfinity )
		{
			if( !Check( LHS.y, op, RHS.y ) )
				return false;
		}
		if( RHS.z != float.NegativeInfinity )
		{
			if( !Check( LHS.z, op, RHS.z ) )
				return false;
		}
		if( RHS.w != float.NegativeInfinity )
		{
			if( !Check( LHS.w, op, RHS.w ) )
				return false;
		}

		return true;
	}

	protected bool Check( object LHS, ComparisonOperators op, object RHS )
	{
		if( !( LHS is IComparable ) || !( RHS is IComparable ) )
			throw new Exception( "Check using non basic type" );

		switch( op )
		{
			case ComparisonOperators.EqualTo:
			return ( (IComparable)LHS ).CompareTo( RHS ) == 0;

			case ComparisonOperators.NotEqualTo:
			return ( (IComparable)LHS ).CompareTo( RHS ) != 0;

			case ComparisonOperators.EqualsOrGreaterThan:
			return ( (IComparable)LHS ).CompareTo( RHS ) >= 0;

			case ComparisonOperators.EqualsOrLessThan:
			return ( (IComparable)LHS ).CompareTo( RHS ) <= 0;

			case ComparisonOperators.GreaterThan:
			return ( (IComparable)LHS ).CompareTo( RHS ) > 0;

			case ComparisonOperators.LessThan:
			return ( (IComparable)LHS ).CompareTo( RHS ) < 0;
			case ComparisonOperators.ContainsFlags:
			return ( (int)LHS & (int)RHS ) != 0; 
			case ComparisonOperators.DoesNotContainsFlags:
			return ( ( (int)LHS & (int)RHS ) == (int)LHS ); 

			default:
			break;
		}
		return false;
	}
	private bool CheckObject( object LHS, ComparisonOperators comparasonOperator, object RHS )
	{
		switch( comparasonOperator )
		{
			case ComparisonOperators.EqualTo:
			return ( LHS == null );

			case ComparisonOperators.NotEqualTo:
			return ( LHS != null );
		}
		return true;
	}

}
