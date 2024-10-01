#ifndef _MAINLIGHTSHADOW
#define _MAINLIGHTSHADOW

#define _MAIN_LIGHT_SHADOWS_SCREEN 1

void MainLightShadow_float(float3 PositionWS, out float shadow)
{
    shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(PositionWS));
}


#endif