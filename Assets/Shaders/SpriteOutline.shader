Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float4 _MainTex_TexelSize; // Declaração faltante para corrigir o erro

            struct v2f_outline
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f_outline vert(appdata_t IN)
            {
                v2f_outline OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f_outline IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                
                // Se o pixel atual for transparente, checamos os vizinhos
                if (c.a == 0)
                {
                    float2 texelsize = _MainTex_TexelSize.xy * _OutlineWidth;
                    
                    fixed4 totalAlpha = fixed4(0,0,0,0);
                    // Amostragem simples em 4 direções (Cima, Baixo, Esquerda, Direita)
                    totalAlpha += SampleSpriteTexture(IN.texcoord + float2(texelsize.x, 0));
                    totalAlpha += SampleSpriteTexture(IN.texcoord + float2(-texelsize.x, 0));
                    totalAlpha += SampleSpriteTexture(IN.texcoord + float2(0, texelsize.y));
                    totalAlpha += SampleSpriteTexture(IN.texcoord + float2(0, -texelsize.y));
                    
                    if (totalAlpha.a > 0)
                    {
                        fixed4 o = _OutlineColor;
                        o.rgb *= o.a;
                        return o;
                    }
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
