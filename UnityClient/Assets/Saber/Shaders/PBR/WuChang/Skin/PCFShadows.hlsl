#ifndef SHADOWS_INCLUDED
#define SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

static const float2 poisson_disk[] = {
   float2( -0.94201624, -0.39906216 ), 
   float2( 0.94558609, -0.76890725 ), 
   float2( -0.094184101, -0.92938870 ), 
   float2( 0.34495938, 0.29387760 ), 
   float2( -0.91588581, 0.45771432 ), 
   float2( -0.81544232, -0.87912464 ), 
   float2( -0.38277543, 0.27676845 ), 
   float2( 0.97484398, 0.75648379 ), 
   float2( 0.44323325, -0.97511554 ), 
   float2( 0.53742981, -0.47373420 ), 
   float2( -0.26496911, -0.41893023 ), 
   float2( 0.79197514, 0.19090188 ), 
   float2( -0.24188840, 0.99706507 ), 
   float2( -0.81409955, 0.91437590 ), 
   float2( 0.19984126, 0.78641367 ), 
   float2( 0.14383161, -0.14100790 ) 
};

float sample_shadowmap_poisson(TEXTURE2D_SHADOW_PARAM(shadow_map, sampler_shadow_map), float4 shadow_coord, half4 shadow_params)
{
    // 只要在编译时已知isPerspectiveProjection，编译器就会优化这个分支
    // if (is_perspective_projection)
    //     shadow_coord.xyz /= shadow_coord.w;

    float attenuation = 0;

    UNITY_UNROLL
    for (int i = 0; i < 16; i++)
    {
        float3 sample_shadow_coord = shadow_coord.xyz + float3(poisson_disk[i] * 0.001, 0);
        attenuation += SAMPLE_TEXTURE2D_SHADOW(shadow_map, sampler_shadow_map, sample_shadow_coord);
    }

    attenuation /= 16;

    const float shadow_strength = shadow_params.x;

    attenuation = LerpWhiteTo(attenuation, shadow_strength);

    // 落在光锥体体积之外的阴影坐标的衰减为 1
    // 可以在此处使用分支来节省性能
    return BEYOND_SHADOW_FAR(shadow_coord) ? 1.0 : attenuation;
}

Light get_main_light_poisson(const float4 shadow_coord, const float3 position_ws)
{
    Light light = GetMainLight();
    const half4 shadow_params = GetMainLightShadowParams();
    light.shadowAttenuation = sample_shadowmap_poisson(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadow_coord, shadow_params);
    //Light light = GetMainLight(shadow_coord);
    light.shadowAttenuation = lerp(light.shadowAttenuation, 1, GetMainLightShadowFade(position_ws));

    return light;
}

#endif
