Shader "Custom/ClayDeformation"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.75, 0.65, 0.55, 1)
        _TouchRadius ("Touch Radius (m)", Float) = 0.03
        _DeformationStrength ("Deformation Strength (m)", Float) = 0.02
        _Smoothness ("Smoothness (釉薬のツヤ)", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _TouchRadius;
                float _DeformationStrength;
                float _Smoothness;
            CBUFFER_END

            // スクリプト(DeformationShaderDriver)からMaterialPropertyBlock経由で毎フレーム更新される
            float3 _TouchPositionWS;
            float _TouchActive; // 0 または 1。接触中かどうか

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // 指位置(_TouchPositionWS)からの距離に応じて、法線方向にへこませる「押し込みブラシ」
                float dist = distance(positionWS, _TouchPositionWS);
                float falloff = 1.0 - saturate(dist / max(_TouchRadius, 0.0001));
                falloff = falloff * falloff * (3.0 - 2.0 * falloff); // smoothstep

                float dent = falloff * _DeformationStrength * _TouchActive;
                positionWS -= normalWS * dent;

                OUT.normalWS = normalWS;
                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                Light mainLight = GetMainLight();
                float3 N = normalize(IN.normalWS);
                // 裏側(器の内側の壁の裏など)を向いている面は法線を反転して、正しく光が当たるようにする
                if (!isFrontFace) N = -N;

                float3 L = mainLight.direction;
                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 H = normalize(L + V);

                float ndotl = saturate(dot(N, L));
                float ndoth = saturate(dot(N, H));

                // Smoothnessが高いほど、ハイライトが小さく鋭くなる(=釉薬のツヤ)
                float shininess = lerp(8.0, 256.0, _Smoothness * _Smoothness);
                float specular = pow(ndoth, shininess) * _Smoothness;

                float3 ambient = SampleSH(N) * _BaseColor.rgb;
                // ツヤが強いほど、拡散反射(のっぺりした発色)は少し弱める
                float3 diffuse = _BaseColor.rgb * mainLight.color * ndotl * (1.0 - _Smoothness * 0.4);
                float3 specColor = mainLight.color * specular;

                float3 color = ambient * 0.3 + diffuse * 0.8 + specColor;
                return half4(color, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
