﻿Shader "Seino/Animation/gpuskin"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AnimTex ("Anim Tex", 2D) = "white"{}
        _Speed ("Speed", float)  = 1
        _DurationTime("DurationTime", float) = 0.25
        _BlendParam ("_BlendParam", Vector) = (1, 0, 1, 0) //当前动画索引, 当前动画播放时间，上一动画索引，上一动画播放时间
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
            float4 _BlendParam;
            float _Speed;
            float _DurationTime;
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

            float2 GetUV(int index)
            {
                int row = index / _AnimTex_TexelSize.z;
                int col = fmod(index, _AnimTex_TexelSize.z);
                float u = (col + 0.5) * _AnimTex_TexelSize.x;
                float v = (row + 0.5) * _AnimTex_TexelSize.y;
                return float2(u, v);
            }

            float4x4 GetBoneMatrix(int index)
            {
                float2 uv = GetUV(index);
                float4 row0 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);

                uv = GetUV(index + 1);
                float4 row1 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);

                uv = GetUV(index + 2);
                float4 row2 = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, uv, 0);
                
                return float4x4(row0, row1, row2, float4(0, 0, 0, 1));
            }

            float4 GetBonePos(int index, int frame, int boneCount, appdata v)
            {
                int boneIndex = v.uv1.x;
                float boneWeight = v.uv1.y;
                int texIndex = index + 3 * (boneCount * frame + boneIndex);
                float4x4 boneMatrix1 = GetBoneMatrix(texIndex);
                
                boneIndex = v.uv2.x;
                texIndex = index + 3 * (boneCount * frame + boneIndex);
                float4x4 boneMatrix2 = GetBoneMatrix(texIndex);

                float4 positionOS = mul(boneMatrix1, v.vertex) * boneWeight + mul(boneMatrix2, v.vertex) * (1 - boneWeight);
                return positionOS;
            }
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                int curAnimIndex = _BlendParam.x;
                float curAnimPlayTime = _BlendParam.y;
                int lastAnimIndex = _BlendParam.z;
                float lastAnimPlayTime = _BlendParam.w;
                
                float4 headInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * 0.5, _AnimTex_TexelSize.y * 0.5), 0);
                float4 curAnimInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * (0.5 + curAnimIndex), _AnimTex_TexelSize.y * 0.5), 0);
                float4 lastAnimInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * (0.5 + lastAnimIndex), _AnimTex_TexelSize.y * 0.5), 0);

                int frameRate = headInfo.w;
                int boneCount = headInfo.y;
                int curFrameCount = curAnimInfo.y;
                int lastFrameCount = lastAnimInfo.y;
                
                int curAnimFrame = fmod(max(_Time.y - curAnimPlayTime, 0) * frameRate * _Speed, curFrameCount);
                int lastAnimFrame = fmod(max(_Time.y - lastAnimPlayTime, 0) * frameRate * _Speed, lastFrameCount);
                
                float4 curAnimPos = GetBonePos(curAnimInfo.x, curAnimFrame, boneCount, v);
                float4 lastAnimPos = GetBonePos(lastAnimInfo.x, lastAnimFrame, boneCount, v);
                
                float weight = step(1, curAnimInfo.z);
                float t = sin(saturate((_Time.y - curAnimPlayTime) / _DurationTime) * (PI / 2.0));
                float4 blendPos = curAnimPos * t + lastAnimPos * (1 - t);
                float4 finalPos = curAnimPos * (1 - weight) + blendPos * weight;
                
                o.vertex = TransformObjectToHClip(finalPos);
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