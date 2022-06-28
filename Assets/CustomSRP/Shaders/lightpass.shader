Shader "SRP/lightpass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite On ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "BRDF.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _gdepth;
            sampler2D _GT0;
            sampler2D _GT1;
            sampler2D _GT2;
            sampler2D _GT3;
            float _split0;
            float _split1;
            float _split2;
            float _split3;

            samplerCUBE _diffuseIBL;
            samplerCUBE _specularIBL;
            sampler2D _brdfLut;

            float4x4 _vpMatrix;
            float4x4 _vpMatrixInv;
            sampler2D _shadowtex0;
            sampler2D _shadowtex1;
            sampler2D _shadowtex2;
            sampler2D _shadowtex3;
            float4x4 _shadowVpMatrix0;
            float4x4 _shadowVpMatrix1;
            float4x4 _shadowVpMatrix2;
            float4x4 _shadowVpMatrix3;
            float ShadowMap01(float4 worldPos, sampler2D _shadowtex, float4x4 _shadowVpMatrix)
            {
                float4 shadowNdc = mul(_shadowVpMatrix, worldPos);
                shadowNdc /= shadowNdc.w;
                float2 uv = shadowNdc.xy * 0.5 + 0.5;

                if(uv.x<0 || uv.x>1 || uv.y<0 || uv.y>1) return 1.0f;

                float d = shadowNdc.z;
                float d_sample = tex2D(_shadowtex, uv).r;

            #if defined (UNITY_REVERSED_Z)
                if(d_sample>d) return 0.0f;
            #else
                if(d_sample<d) return 0.0f;
            #endif

                return 1.0f;
            }
            fixed4 frag (v2f i, out float depthOut : SV_Depth) : SV_Target
            {
                float2 uv = i.uv;
                float4 GT2 = tex2D(_GT2, uv);
                float4 GT3 = tex2D(_GT3, uv);

                // 从 Gbuffer 解码数据
                float3 albedo = tex2D(_GT0, uv).rgb;
                float3 normal = tex2D(_GT1, uv).rgb * 2 - 1;
                float2 motionVec = GT2.rg;
                float roughness = GT2.b;
                float metallic = GT2.a;
                float3 emission = GT3.rgb;
                float occlusion = GT3.a;

                float d = UNITY_SAMPLE_DEPTH(tex2D(_gdepth, uv));
                float d_lin = Linear01Depth(d);
                depthOut = d;

                // 反投影重建世界坐标
                float4 ndcPos = float4(uv*2-1, d, 1);
                float4 worldPos = mul(_vpMatrixInv, ndcPos);
                worldPos /= worldPos.w;

                // 计算参数
                float3 N = normalize(normal);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 V = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                float3 radiance = _LightColor0.rgb;

                // 计算直接光照
                float3 direct = PBR(N, V, L, albedo, radiance, roughness, metallic);


                // 向着法线偏移采样点
                float4 worldPosOffset = worldPos;
                worldPosOffset.xyz += normal * 0.01;

                float shadow = 1.0;
                float shadow0 = ShadowMap01(worldPosOffset, _shadowtex0, _shadowVpMatrix0);
                float shadow1 = ShadowMap01(worldPosOffset, _shadowtex1, _shadowVpMatrix1);
                float shadow2 = ShadowMap01(worldPosOffset, _shadowtex2, _shadowVpMatrix2);
                float shadow3 = ShadowMap01(worldPosOffset, _shadowtex3, _shadowVpMatrix3);

                if(d_lin<_split0) 
                    shadow *= shadow0;
                else if(d_lin<_split0+_split1) 
                    shadow *= shadow1;
                else if(d_lin<_split0+_split1+_split2) 
                    shadow *= shadow2;
                else if(d_lin<_split0+_split1+_split2+_split3)
                    shadow *= shadow3;

                // 受阴影影响的直接光照
                float3 color = direct * shadow;

                // 计算环境光照
                float3 ambient = IBL(
                    N, V,
                    albedo, roughness, metallic,
                    _diffuseIBL, _specularIBL, _brdfLut
                );

                color += ambient * occlusion;
                color += emission;

                return float4(color, 1);
            }
            ENDCG
        }
    }
}