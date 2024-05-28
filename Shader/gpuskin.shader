Shader "Seino/Animation/gpuskin"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AnimTex ("Anim Tex", 2D) = "white"{}
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
            CBUFFER_END

            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;
            TEXTURE2D(_AnimTex);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_AnimTex);

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float boneCount : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;

                float4 header = SAMPLE_TEXTURE2D(_AnimTex, sampler_AnimTex, float2(0, 0));
                // o.boneCount = header;
                
                // int frame = fmod(_Time.y * _FrameRate, frameCount);
                // float boneIndex = (int)v.uv1.x;
                // float boneWeight = v.uv1.y;
                // float texIndex = (boneIndex + frame * boneCount) * 3;
                //
                // float4x4 mat1 = getMatrix(texIndex);
                //
                // boneIndex = (int)v.uv2.x;
                // texIndex = (boneIndex + frame * boneCount) * 3;
                // float4x4 mat2 = getMatrix(texIndex);
                //
                // float4 pos = mul(mat1, v.vertex) * boneWeight + mul(mat2, v.vertex) * (1 - boneWeight);

                
                
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.boneCount = header.y;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
