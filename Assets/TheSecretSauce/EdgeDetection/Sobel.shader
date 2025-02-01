Shader "Hidden/Shader/Sobel"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
        _OutlineIntensity("Outline Intensity", Float) = 1
        _OutlineThickness("Outline Thickness", Float) = 1
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _DepthMultiplier("Depth Multiplier", Float) = 1
        _DepthBias("Depth Bias", Float) = 1
        _NormalMultiplier("Normal Multiplier", Float) = 1
        _NormalBias("Normal Bias", Float) = 1
        //_LuminanceMultiplier("luminance Multiplier", Float) = 1
        //_LuminanceBias("luminance Bias", Float) = 1
        _MaxRange("Max Range", Float) = 100
        _DistanceFalloffPower("Distance Falloff Power", Float) = 3
    }


    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _OutlineIntensity;
    float _OutlineThickness;
    float4 _OutlineColor;
    float _DepthMultiplier;
    float _DepthBias;
    float _NormalMultiplier;
    float _NormalBias;
    float _LuminanceMultiplier;
    float _LuminanceBias;
    float _MaxRange;
    float _DistanceFalloffPower;
    TEXTURE2D_X(_MainTex);

    // Original sobel
    float SobelBasic(float topLeft, float top, float topRight, float left, float right, float botLeft, float bot, float botRight) {
        float x = topLeft + 2 * left + botLeft - topRight - 2 * right - botRight;
        float y = -topLeft - 2 * top - topRight + botLeft + 2 * bot + botRight;

        return sqrt(x * x + y * y);
    }
    
    // Sobel Scharr generally picks up lines much better and more intensly
    float SobelScharr(float topLeft, float top, float topRight, float left, float right, float botLeft, float bot, float botRight) {
        float x = -3 * topLeft - 10 * left - 3 * botLeft + 3 * topRight + 10 * right + 3 * botRight;
        float y = 3 * topLeft + 10 * top + 3 * topRight - 3 * botLeft - 10 * bot - 3 * botRight;

        return sqrt(x * x + y * y);
    }

    // Could add Sobel RobertsCross which is more efficent with only 4 samples but at possible cost of visuals

    // Original sobel float3 ver for normals
    float SobelBasic(float3 topLeft, float3 top, float3 topRight, float3 left, float3 right, float3 botLeft, float3 bot, float3 botRight) {
        float3 x = topLeft + 2 * left + botLeft - topRight - 2 * right - botRight;
        float3 y = -topLeft - 2 * top - topRight + botLeft + 2 * bot + botRight;

        return sqrt(dot(x, x) + dot(y, y));
    }
    
    // Sobel Scharr generally picks up lines much better and more intensly float3 ver for normals
    float SobelScharr(float3 topLeft, float3 top, float3 topRight, float3 left, float3 right, float3 botLeft, float3 bot, float3 botRight) {
        float3 x = -3 * topLeft - 10 * left - 3 * botLeft + 3 * topRight + 10 * right + 3 * botRight;
        float3 y = 3 * topLeft + 10 * top + 3 * topRight - 3 * botLeft - 10 * bot - 3 * botRight;

        return sqrt(dot(x, x) + dot(y, y));
    }

    float SobelSampleDepth(float2 uv, float offsetU, float offsetV) {
        float topLeft = SampleCameraDepth(uv + float2(-offsetU, offsetV));
        float top = SampleCameraDepth(uv + float2(0, offsetV));
        float topRight = SampleCameraDepth(uv + float2(offsetU, offsetV));

        float botLeft = SampleCameraDepth(uv + float2(-offsetU, -offsetV));
        float bot = SampleCameraDepth(uv + float2(0, -offsetV));
        float botRight = SampleCameraDepth(uv + float2(offsetU, -offsetV));

        float left = SampleCameraDepth(uv + float2(-offsetU, 0));
        float center = SampleCameraDepth(uv);
        float right = SampleCameraDepth(uv + float2(offsetU, 0));

        return SobelScharr(abs(topLeft - center), abs(top - center), abs(topRight - center), abs(left - center), abs(right - center), abs(botLeft - center), abs(bot - center), abs(botRight - center));
    }

    float3 SampleWorldNormal(float2 uv) {
        // If cameradepth is invalid bail
        if (SampleCameraDepth(uv) <= 0)
            return float3(0, 0, 0);

        NormalData normalData;
        DecodeFromNormalBuffer(uv * _ScreenSize.xy, normalData);

        return normalData.normalWS;
    }

    float SobelSampleNormal(float2 uv, float offsetU, float offsetV) {
        float3 topLeft = SampleWorldNormal(uv + float2(-offsetU, offsetV));
        float3 top = SampleWorldNormal(uv + float2(0, offsetV));
        float3 topRight = SampleWorldNormal(uv + float2(offsetU, offsetV));

        float3 botLeft = SampleWorldNormal(uv + float2(-offsetU, -offsetV));
        float3 bot = SampleWorldNormal(uv + float2(0, -offsetV));
        float3 botRight = SampleWorldNormal(uv + float2(offsetU, -offsetV));

        float3 left = SampleWorldNormal(uv + float2(-offsetU, 0));
        float3 center = SampleWorldNormal(uv);
        float3 right = SampleWorldNormal(uv + float2(offsetU, 0));

        // Not using SobelScharr here as with the scene setup its too good at getting edges and makes the game unplayable
        return SobelBasic(abs(topLeft - center), abs(top - center), abs(topRight - center), abs(left - center), abs(right - center), abs(botLeft - center), abs(bot - center), abs(botRight - center));
    }

    float SampleSceneLuminance(float2 uv) {
        float3 color = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(uv)).rgb;
        return color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
    }

    float SobelSampleLuminance(float2 uv, float offsetU, float offsetV) {
        float topLeft = SampleSceneLuminance(uv + float2(-offsetU, offsetV));
        float top = SampleSceneLuminance(uv + float2(0, offsetV));
        float topRight = SampleSceneLuminance(uv + float2(offsetU, offsetV));

        float botLeft = SampleSceneLuminance(uv + float2(-offsetU, -offsetV));
        float bot = SampleSceneLuminance(uv + float2(0, -offsetV));
        float botRight = SampleSceneLuminance(uv + float2(offsetU, -offsetV));

        float left = SampleSceneLuminance(uv + float2(-offsetU, 0));
        float center = SampleSceneLuminance(uv);
        float right = SampleSceneLuminance(uv + float2(offsetU, 0));

        return SobelScharr(abs(topLeft - center), abs(top - center), abs(topRight - center), abs(left - center), abs(right - center), abs(botLeft - center), abs(bot - center), abs(botRight - center));
    }

    float LinearEyeDepth(float depth) {
        return 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y);
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord)).xyz;

        // Determine offsets and do sobel depth sampling
        float offsetU = _OutlineThickness / _ScreenSize.x;
        float offsetV = _OutlineThickness / _ScreenSize.y;

        float sobelDepth = SobelSampleDepth(input.texcoord.xy, offsetU, offsetV);
        sobelDepth = pow(abs(saturate(sobelDepth)) * _DepthMultiplier, _DepthBias);

        // Sobel normal sampling
        float sobelNormal = SobelSampleNormal(input.texcoord.xy, offsetU, offsetV);
        sobelNormal = pow(abs(saturate(sobelNormal)) * _NormalMultiplier, _NormalBias);

        // Sobel luminance sampling
        float sobelLuminance = SobelSampleLuminance(input.texcoord.xy, offsetU, offsetV);
        sobelLuminance = pow(abs(saturate(sobelLuminance)) * _LuminanceMultiplier, _LuminanceBias);

        // Dictate Intensity combining all 3
        float outlineIntensity = max(sobelDepth, max(sobelNormal, sobelLuminance));
        //float outlineIntensity = max(sobelDepth, sobelNormal);

        // Sobel depth test
        //return float4(sobelDepth, sobelDepth, sobelDepth, 1);

        // Sobel normal test
        //return float4(sobelNormal, sobelNormal, sobelNormal, 1);

        // Sobel luminance test
        //return float4(sobelLuminance, sobelLuminance, sobelLuminance, 1);

        // Calculate distance falloff
        float depth = _ProjectionParams.z * LinearEyeDepth(SampleCameraDepth(input.texcoord));
        float distanceFalloff = saturate(pow(_MaxRange / depth, _DistanceFalloffPower));

        // Apply sobel effect
        float3 color = lerp(sourceColor.rgb, _OutlineColor.rgb, (outlineIntensity * _OutlineIntensity) * distanceFalloff);
        return float4(color, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Sobel"

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            //ZTest Always
            //Blend Off
            //Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
