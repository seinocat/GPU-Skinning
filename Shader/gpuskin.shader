Shader "Seino/Animation/gpuskin"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AnimTex ("Anim Tex", 2D) = "white"{}
        _CurAnimIndex ("CurAnim Index", float) = 1
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
                 
            #pragma vertex vert
            #pragma fragment frag

            #pragma enable_d3d11_debug_symbols
            #pragma multi_compile_instancing

            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float4 _AnimTex_TexelSize;
            float _CurAnimIndex;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_AnimTex);
            SAMPLER(sampler_AnimTex);
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float2 getUV(int index)
            {
                int row = index / _AnimTex_TexelSize.z;
                int col = fmod(index, _AnimTex_TexelSize.z);
                float u = (col + 0.5) * _AnimTex_TexelSize.x;
                float v = (row + 0.5) * _AnimTex_TexelSize.y;
                return float2(u, v);
            }

            float4x4 getMatrix(int index)
            {
                float2 uv = getUV(index);
                float4 row0 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);

                uv = getUV(index + 1);
                float4 row1 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);

                uv = getUV(index + 2);
                float4 row2 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);
                
                return float4x4(row0, row1, row2, float4(0, 0, 0, 1));
            }
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                
                float4 headInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * 0.5, _AnimTex_TexelSize.y * 0.5), 0);
                float4 curAnimInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * (0.5 + _CurAnimIndex), _AnimTex_TexelSize.y * 0.5), 0);
                int frameRate = headInfo.w;
                int frameCount = curAnimInfo.z;
                int boneCount = headInfo.y;
                
                int frame = fmod(_Time.y * frameRate, frameCount);
                
                int boneIndex = v.uv1.x;
                float boneWeight = v.uv1.y;
                int texIndex = curAnimInfo.x + 3 * (boneCount * frame + boneIndex);
                float4x4 boneMatrix1 = getMatrix(texIndex);
                
                boneIndex = v.uv2.x;
                texIndex = curAnimInfo.x + 3 * (boneCount * frame + boneIndex);
                float4x4 boneMatrix2 = getMatrix(texIndex);

                float4 positionOS = mul(boneMatrix1, v.vertex) * boneWeight + mul(boneMatrix2, v.vertex) * (1 - boneWeight);
                
                o.vertex = TransformObjectToHClip(positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, 0);
                return col;
            }
            
            ENDHLSL
        }
    }
}
