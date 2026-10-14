Shader "NTSD/BattleD0Replacement"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _CompositeFrameOverride("Legacy frame", Float) = -1
        [PerRendererData] _RendererColor("Renderer color", Color) = (1,1,1,1)
        [PerRendererData] _Flip("Flip", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "NTSDAlphaContract"="PremultipliedSpriteAlpha" }
        Pass
        {
            Name "D0RgbaReplacement"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float3 positionOS:POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; float frame:TEXCOORD1; };
            struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; nointerpolation float frame:TEXCOORD1; };
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _CompositeBaseRect;
                float4 _PatchSources[34];
                float4 _PatchDestinations[34];
                float _CompositeFrameOverride;
            CBUFFER_END
            CBUFFER_START(UnityPerDrawSprite)
                half4 _RendererColor;
                half2 _Flip;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                bool legacy = _CompositeFrameOverride >= 0;
                if (legacy) input.positionOS.xy *= _Flip;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color * _Color;
                if (legacy) output.color *= _RendererColor;
                output.uv = input.uv;
                output.frame = legacy ? _CompositeFrameOverride : input.frame;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                int frame = clamp((int)round(input.frame), 0, 33);
                float2 pixel = clamp(floor(input.uv * _MainTex_TexelSize.zw - _CompositeBaseRect.xy),
                    float2(0,0), _CompositeBaseRect.zw - 1);
                float4 source = _PatchSources[frame];
                float4 destination = _PatchDestinations[frame];
                float2 samplePixel = _CompositeBaseRect.xy + pixel;
                // Alignment contract: NTSD-M12-D0-BASE-PATCH-001. Select RGBA before blending; alpha=0 replaces the base.
                if (destination.z > 0 && destination.w > 0 &&
                    all(pixel >= destination.xy) && all(pixel < destination.xy + destination.zw))
                    samplePixel = source.xy + pixel - destination.xy;
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (samplePixel + .5) * _MainTex_TexelSize.xy) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
