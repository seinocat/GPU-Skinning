Shader "Seino/Animation/gpu_skinning"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AnimTex ("Anim Tex", 2D) = "white"{}
        _AnimBlendTex ("AnimBlend Tex", 2D) = "white"{}
        _FrameRate("Frame Rate", int) = 30
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
                 
            #pragma vertex vert
            #pragma fragment frag

            #pragma enable_d3d11_debug_symbols

            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            sampler2D _AnimTex;
            float4 _AnimTex_TexelSize;
            sampler2D _AnimBlendTex;
            float4 _AnimBlendTex_TexelSize;
            int _FrameRate;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float DecodeFloatRGBA( float4 enc )
            {
                float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
                return dot(enc, kDecodeDot);
            }

            float2 getUV(int texIndex)
            {
                int row = texIndex / _AnimTex_TexelSize.z ;
                int col = texIndex - row * _AnimTex_TexelSize.z;
                return float2(col / _AnimTex_TexelSize.z,  row / _AnimTex_TexelSize.w);
            }
            
            float4 getMatrixRow(int texIndex)
            {
                float2 uv = getUV(texIndex);
                float r = DecodeFloatRGBA(tex2Dlod(_AnimTex, float4(float2(uv.x + 0.5 * _AnimTex_TexelSize.x, uv.y + 0.5 * _AnimTex_TexelSize.y), 0, 0)));

                uv = getUV(texIndex + 1);
                float g = DecodeFloatRGBA(tex2Dlod(_AnimTex, float4(float2(uv.x + 0.5 * _AnimTex_TexelSize.x, uv.y + 0.5 * _AnimTex_TexelSize.y), 0, 0)));

                uv = getUV(texIndex + 2);
                float b = DecodeFloatRGBA(tex2Dlod(_AnimTex, float4(float2(uv.x + 0.5 * _AnimTex_TexelSize.x, uv.y + 0.5 * _AnimTex_TexelSize.y), 0, 0)));

                uv = getUV(texIndex + 3);
                float a = DecodeFloatRGBA(tex2Dlod(_AnimTex, float4(float2(uv.x + 0.5 * _AnimTex_TexelSize.x, uv.y + 0.5 * _AnimTex_TexelSize.y), 0, 0)));

                return float4(r,g,b,a) * 100 - 50;
            }

            float4x4 getMatrix(int texIndex)
            {
                float4 row1 = getMatrixRow(texIndex);
                float4 row2 = getMatrixRow(texIndex + 4);
                float4 row3 = getMatrixRow(texIndex + 8);
                float4 row4 = float4(0,0,0,1);
                return float4x4(row1, row2, row3, row4);
            }
            
            v2f vert (appdata v)
            {
                v2f o;

                int boneCount = _AnimTex_TexelSize.z / 12;
                int frameCount = _AnimTex_TexelSize.w;
                
                int frame = fmod(_Time.y * _FrameRate, frameCount);
                float boneIndex = (int)v.uv1.x;
                float boneWeight = v.uv1.y;
                float texIndex = (boneIndex + frame * boneCount) * 12;

                float4x4 mat1 = getMatrix(texIndex);

                boneIndex = (int)v.uv2.x;
                texIndex = (boneIndex + frame * boneCount) * 12;
                float4x4 mat2 = getMatrix(texIndex);

                float4 pos = mul(mat1, v.vertex) * boneWeight + mul(mat2, v.vertex) * (1 - boneWeight);
                o.vertex = TransformObjectToHClip(pos);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
                return col;
            }
            ENDHLSL
        }
    }
}
