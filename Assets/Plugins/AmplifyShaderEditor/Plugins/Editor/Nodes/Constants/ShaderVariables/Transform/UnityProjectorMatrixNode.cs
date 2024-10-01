


namespace AmplifyShaderEditor
{
    [System.Serializable]
    [NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Projector Matrix"
#else
"投影仪矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Current Projector Clip matrix. To be used when working with Unity projector."
#else
"当前投影仪剪辑矩阵。在使用Unity投影仪时使用。"
#endif
)]
    public sealed class UnityProjectorMatrixNode : ConstantShaderVariable
    {
        protected override void CommonInit( int uniqueId )
        {
            base.CommonInit( uniqueId );
            ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
            m_value = "unity_Projector";
        }

        public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
        {
            dataCollector.AddToUniforms( UniqueId, "float4x4 unity_Projector;" );
            return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
        }
    }
}
