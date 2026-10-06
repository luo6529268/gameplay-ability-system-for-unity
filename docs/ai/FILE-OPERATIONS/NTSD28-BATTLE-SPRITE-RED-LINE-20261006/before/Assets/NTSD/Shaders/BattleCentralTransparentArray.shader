Shader "NTSD/BattleCentralTransparentArray"
{
    Properties
    {
        [MainTexture] _MainTexArray("Texture Array", 2DArray) = "" {}
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
            Name "BattleCentralTransparentArray"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma require 2darray
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float atlasSlice : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                nointerpolation float atlasSlice : TEXCOORD1;
            };

            TEXTURE2D_ARRAY(_MainTexArray);
            SAMPLER(sampler_MainTexArray);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color * _Color;
                output.uv = input.uv;
                output.atlasSlice = input.atlasSlice;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D_ARRAY(
                    _MainTexArray,
                    sampler_MainTexArray,
                    input.uv,
                    input.atlasSlice) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
