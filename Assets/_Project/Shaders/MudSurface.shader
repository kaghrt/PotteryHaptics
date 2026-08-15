Shader "Custom/MudSurface"
{
    Properties
    {
        _BaseColor ("Base Color (Mud)", Color) = (0.25, 0.18, 0.12, 1)
        _WetColor ("Wet Highlight Color", Color) = (0.9, 0.85, 0.7, 1)
        _NoiseScale ("Noise Scale", Float) = 8.0
        _NoiseSpeed ("Noise Speed", Float) = 0.3
        _FresnelPower ("Fresnel Power", Float) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

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
                float4 _WetColor;
                float _NoiseScale;
                float _NoiseSpeed;
                float _FresnelPower;
            CBUFFER_END

            // スクリプト(MudStretchDriver)からMaterialPropertyBlock経由で毎フレーム更新される
            float _StretchAmount;      // 伸びの大きさ[m]
            float3 _StretchDirectionWS; // 伸びる方向(ワールド空間、正規化済み想定)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            // 簡易value noise(テクスチャ不要でドロドロ感を出す)
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // オブジェクト空間で、移動方向側の頂点だけを伸ばす(泥を引っ張るイメージ)
                float3 dirOS = normalize(mul((float3x3)unity_WorldToObject, _StretchDirectionWS) + 1e-5);
                float influence = saturate(dot(normalize(IN.positionOS.xyz + 1e-5), dirOS));
                float3 stretchedOS = IN.positionOS.xyz + dirOS * influence * _StretchAmount;

                float3 positionWS = TransformObjectToWorld(stretchedOS);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionWS = positionWS;
                OUT.normalWS = normalWS;
                OUT.uv = IN.uv;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float n = noise(IN.uv * _NoiseScale + _Time.y * _NoiseSpeed);

                // ノイズで法線を少し揺らして、表面がドロドロ波打ってるように見せる
                float3 normalWS = normalize(IN.normalWS + float3(0, 0, (n - 0.5) * 0.4));

                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(normalWS, mainLight.direction));

                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);

                float3 ambient = SampleSH(normalWS) * _BaseColor.rgb * 0.4;
                float3 diffuse = _BaseColor.rgb * mainLight.color * ndotl;
                float3 wetHighlight = _WetColor.rgb * fresnel * mainLight.color;

                // ノイズをほんの少し明暗にも反映(表面のムラ)
                float3 mottling = _BaseColor.rgb * (n - 0.5) * 0.15;

                float3 color = ambient + diffuse + wetHighlight + mottling;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
