Shader "Custom/URP Dual Panoramic Skybox"
{
    Properties
    {
        _Tint1("Tint Color 1", Color) = (1,1,1,1)
        _Tint2("Tint Color 2", Color) = (1,1,1,1)
        [Gamma] _Exposure1("Exposure 1", Range(0,8)) = 1
        [Gamma] _Exposure2("Exposure 2", Range(0,8)) = 1
        _Rotation1("Rotation 1", Range(0,360)) = 0
        _Rotation2("Rotation 2", Range(0,360)) = 0
        _Texture1("Texture 1", 2D) = "white" {}
        _Texture2("Texture 2", 2D) = "white" {}
        _Blend("Blend", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "IgnoreProjector"="True" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define UNITY_PI 3.14159265

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint1;
                float4 _Tint2;
                float _Exposure1;
                float _Exposure2;
                float _Rotation1;
                float _Rotation2;
                float _Blend;
                TEXTURE2D(_Texture1);
                SAMPLER(sampler_Texture1);
                TEXTURE2D(_Texture2);
                SAMPLER(sampler_Texture2);
            CBUFFER_END

            float3 RotateAroundY(float3 dir, float degrees)
            {
                float radians = degrees * UNITY_PI / 180.0;
                float cosA = cos(radians);
                float sinA = sin(radians);
                return float3(
                    dir.x * cosA + dir.z * sinA,
                    dir.y,
                    -dir.x * sinA + dir.z * cosA);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 worldDir = normalize(input.positionOS);
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.directionWS = worldDir;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 dir1 = RotateAroundY(input.directionWS, _Rotation1);
                float3 dir2 = RotateAroundY(input.directionWS, _Rotation2);

                float2 uv1 = float2(
                    0.5 + atan2(dir1.z, dir1.x) / (2 * UNITY_PI),
                    0.5 + asin(clamp(dir1.y, -1, 1)) / UNITY_PI);  // y축 뒤집음 반영

                float2 uv2 = float2(
                    0.5 + atan2(dir2.z, dir2.x) / (2 * UNITY_PI),
                    0.5 + asin(clamp(dir2.y, -1, 1)) / UNITY_PI);

                half4 col1 = SAMPLE_TEXTURE2D(_Texture1, sampler_Texture1, uv1);
                half4 col2 = SAMPLE_TEXTURE2D(_Texture2, sampler_Texture2, uv2);

                half4 finalColor = lerp(col1, col2, _Blend);
                finalColor.a = 1.0;
                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/BlitCopy"
}
