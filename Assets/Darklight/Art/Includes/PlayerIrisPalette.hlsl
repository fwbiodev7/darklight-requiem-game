#ifndef DARKLIGHT_PLAYER_IRIS_PALETTE_INCLUDED
#define DARKLIGHT_PLAYER_IRIS_PALETTE_INCLUDED
// Input/output are sRGB pigment values. Call only for the existing semantic
// iris selector, never for a global red-color range. Keep pupils and whites.
float3 DarklightPlayerIris(float3 sourcePigment)
{
    float high=max(sourcePigment.r,max(sourcePigment.g,sourcePigment.b));
    float low=min(sourcePigment.r,min(sourcePigment.g,sourcePigment.b));
    if(high<65.0/255.0||low>205.0/255.0)return sourcePigment;
    float level=saturate((high-65.0/255.0)/(140.0/255.0));
    return lerp(float3(90,0,0)/255.0,float3(196,8,20)/255.0,level);
}
#endif
