Shader "Unlit/ElectricFieldShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ElectricColor ("Electric Color", Color) = (0.5, 0.8, 1.0, 0.8)
        _ElectricIntensity ("Electric Intensity", Range(0, 10)) = 3.0
        _NoiseScale ("Noise Scale", Range(0.5, 20)) = 5.0
        _AnimSpeed ("Animation Speed", Range(0.1, 5)) = 2.0
        _ElectricThreshold ("Electric Threshold", Range(0, 1)) = 0.3
        _DistortionStrength ("Distortion Strength", Range(0, 0.05)) = 0.01
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 8.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.2
        _RandomFlickerSpeed ("Random Flicker Speed", Range(0, 30)) = 25.0
        _RandomFlickerIntensity ("Random Flicker Intensity", Range(0, 1)) = 0.3
        _SideFadeSharpness ("Side Fade Sharpness", Range(0.1, 10)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ElectricColor;
            float _ElectricIntensity;
            float _NoiseScale;
            float _AnimSpeed;
            float _ElectricThreshold;
            float _DistortionStrength;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _RandomFlickerSpeed;
            float _RandomFlickerIntensity;
            float _SideFadeSharpness;
            
            // 노이즈 함수들
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            // 전기 패턴 생성 (더 명확하게)
            float electricPattern(float2 uv, float time)
            {
                float2 p = uv * _NoiseScale;
                
                // 기본 노이즈
                float n1 = fbm(p + time * _AnimSpeed);
                float n2 = fbm(p * 2.0 + time * _AnimSpeed * 1.5);
                
                // 수직/수평 전기 라인 생성
                float verticalLines = abs(sin((uv.x + n1 * 0.1) * 20.0 + time * 3.0));
                float horizontalLines = abs(sin((uv.y + n2 * 0.1) * 15.0 + time * 2.0));
                
                // 노이즈 기반 전기
                float electric = max(n1, n2);
                electric = pow(electric, 2.0);
                
                // 라인과 노이즈 조합
                float lines = min(verticalLines, horizontalLines);
                lines = smoothstep(0.8, 1.0, lines);
                
                electric = max(electric, lines * 0.8);
                
                // 더 명확한 임계값 처리
                electric = smoothstep(_ElectricThreshold - 0.1, _ElectricThreshold + 0.1, electric);
                electric *= _ElectricIntensity;
                
                return electric;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                
                // 노말 벡터를 이용한 실린더 측면 마스킹
                float3 worldNormal = normalize(i.worldNormal);
                
                // Y축(위아래) 노말은 제외하고 측면만 적용
                float sideMask = 1.0 - abs(worldNormal.y);
                sideMask = pow(sideMask, _SideFadeSharpness);
                
                // UV 왜곡을 위한 노이즈 (더 약하게)
                float2 distortion = float2(
                    fbm(i.uv * 3.0 + time * 0.3) - 0.5,
                    fbm(i.uv * 3.0 + time * 0.2 + 50.0) - 0.5
                ) * _DistortionStrength;
                
                float2 distortedUV = i.uv + distortion;
                
                // 기본 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, distortedUV);
                
                // 전기 효과 계산
                float electric = electricPattern(i.uv, time);
                
                // 깜빡이는 효과 (조절 가능)
                float flicker = 1.0;
                if(_FlickerSpeed > 0)
                {
                    flicker = sin(time * _FlickerSpeed) * _FlickerIntensity + (1.0 - _FlickerIntensity);
                }
                
                float randomFlicker = 1.0;
                if(_RandomFlickerSpeed > 0)
                {
                    randomFlicker = sin(time * _RandomFlickerSpeed + sin(time * 3.0)) * _RandomFlickerIntensity + (1.0 - _RandomFlickerIntensity);
                }
                
                electric *= flicker * randomFlicker;
                
                // 측면 마스크 적용 - 전기 효과를 측면에만
                electric *= sideMask;
                
                // 전기 색상 (더 밝고 명확하게)
                float4 electricColor = _ElectricColor;
                electricColor.rgb *= (1.0 + electric * 3.0); // 전기 부분을 더 밝게
                
                // 기본 파란색 반투명 베이스 (더 투명하게)
                float4 baseColor = float4(0.1, 0.3, 0.8, 0.15);
                
                // 색상 합성
                col = baseColor;
                col.rgb += electricColor.rgb * electric;
                
                // 알파값도 측면에서만 증가
                col.a = baseColor.a + electric * 0.4;
                
                // 전체적으로 더 투명하게
                col.a *= 0.7;
                
                // 포그 적용
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}