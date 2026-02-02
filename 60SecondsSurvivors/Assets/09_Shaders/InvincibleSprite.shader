Shader "Custom/InvincibleSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Invincible ("Invincible", Float) = 0
        _HueSpeed ("Hue Speed", Float) = 1
        _HitFlash ("Hit Flash", Float) = 0
        _HitFlashStrength ("Hit Flash Strength", Float) = 0.4
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float _Invincible;
            float _HueSpeed;
            float _HitFlash;
            float _HitFlashStrength;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            // HSV → RGB 변환
            float3 HSVToRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                // 1. 무적 무지개
                if (_Invincible > 0.5)
                {
                    float hue = frac(_Time.y * _HueSpeed);
                    float3 rainbow = HSVToRGB(float3(hue, 1, 1));
                    col.rgb *= rainbow;
                }

                // 2. 히트 플래시
                float hit = saturate(_HitFlash);
                if (hit > 0.001)
                {
                    float strength = hit * _HitFlashStrength;
                    col.rgb = lerp(col.rgb, float3(1,1,1), strength);
                }

                return col;
            }
            ENDHLSL
        }
    }
}
