#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED


#include <UnityCG.cginc>

float3 _LightSource;
float4 _LightColor;
float _SpecularThreshold;
float _SpecularMultiplier;
float _Shininess;

float3 applyCustomLighting(float3 diffuse_color, float3 world_pos, float3 world_normal)
{
    const float3 normal_dir = normalize(world_normal);
    const float3 ambient = ShadeSH9(float4(normal_dir, 1));
    const float3 light_dir = normalize(_LightSource);
    const float3 view_dir = normalize(UnityWorldSpaceViewDir(world_pos));
    const float3 halfway_dir = normalize(light_dir + view_dir);
    const float l = saturate(dot(normal_dir, light_dir));
    const float3 diffuse_lighting = l * _LightColor.rgb;
    const float h = saturate(dot(normal_dir, halfway_dir));
    const float specular_value = saturate(l * _SpecularThreshold) * _SpecularMultiplier;
    const float3 specular_lighting = pow(h, _Shininess) * specular_value * _LightColor.rgb;
    return (diffuse_lighting + ambient) * diffuse_color + specular_lighting;
}

#endif
