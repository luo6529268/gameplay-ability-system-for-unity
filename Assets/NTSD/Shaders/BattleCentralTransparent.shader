Shader "NTSD/BattleCentralTransparent"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "NTSDAlphaContract" = "PremultipliedSpriteAlpha"
        }

        Pass
        {
            Name "BattleCentralTransparent"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float atlasSlice : TEXCOORD1;
                float4 uvSampleBounds : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float atlasSlice : TEXCOORD1;
                float4 uvSampleBounds : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color * _Color;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.atlasSlice = input.atlasSlice;
                output.uvSampleBounds = 0;
                if (input.uvSampleBounds.z > 0 && input.uvSampleBounds.w > 0)
                {
                    float2 first = TRANSFORM_TEX(input.uvSampleBounds.xy, _MainTex);
                    float2 last = TRANSFORM_TEX(input.uvSampleBounds.zw, _MainTex);
                    output.uvSampleBounds = float4(min(first, last), max(first, last));
                }
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                if (input.uvSampleBounds.z > 0 && input.uvSampleBounds.w > 0)
                    uv = clamp(uv, input.uvSampleBounds.xy, input.uvSampleBounds.zw);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
