Shader "Seino/Animation/GpuSkin"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AnimTex ("Anim Tex", 2D) = "white"{}
        _Speed ("Speed", float)  = 1
        // base层当前和上一动画播放时间, top层当前和上一动画播放时间
        _TimeParam("TimeParam", Vector) = (0, 0, 0, 0) 
        // base层层级，当前|上一动画索引,  top层层级，top层当前|上一动画索引
        _LayerParam ("LayerParam", Vector) = (1, 1, 1, 1) 
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
            float4 _LayerParam;
            float4 _TimeParam;
            float _Speed;
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
                float2 uv3 : TEXCOORD3;
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

            int GetIndex0(int combine)
            {
                return combine & 0xff;
            }

            int GetIndex1(int combine)
            {
                return (combine >> 8) & 0xff;
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
                
                float4 positionOS = lerp(mul(boneMatrix2, v.vertex), mul(boneMatrix1, v.vertex), boneWeight);
                return positionOS;
            }

            int GetFrame(int frameCount, float frameRate, float time, int loop)
            {
                int totalFrame = max(_Time.y - time, 0) * frameRate * _Speed; //已播放的总帧数
                int loopFrame = fmod(totalFrame, frameCount); //循环帧数
                int playEnd = step(frameCount, totalFrame); //判断一下是否播放完
                int frame = lerp(lerp(loopFrame, frameCount - 1, playEnd), loopFrame, loop);

                return frame;
            }
            
            float4 GetAnimation(appdata v, int curAnimIndex, float curAnimPlayTime, int lastAnimIndex, float lastAnimPlayTime)
            {
                float4 headInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * 0.5, _AnimTex_TexelSize.y * 0.5), 0);
                float4 curAnimInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * (0.5 + curAnimIndex), _AnimTex_TexelSize.y * 0.5), 0);
                float4 lastAnimInfo = SAMPLE_TEXTURE2D_LOD(_AnimTex, sampler_AnimTex, float2(_AnimTex_TexelSize.x * (0.5 + lastAnimIndex), _AnimTex_TexelSize.y * 0.5), 0);

                int frameRate = headInfo.w;
                int boneCount = headInfo.y;
                int curFrameCount = curAnimInfo.y; //当前动画帧数
                int lastFrameCount = lastAnimInfo.y;//上一动画帧数
                float duration = curAnimInfo.z;

                //计算动画所处帧数
                int curAnimFrame = GetFrame(curFrameCount, frameRate, curAnimPlayTime, curAnimInfo.w);
                int lastAnimFrame = GetFrame(lastFrameCount, frameRate, lastAnimPlayTime, lastAnimInfo.w);
                
                float4 curAnimPos = GetBonePos(curAnimInfo.x, curAnimFrame, boneCount, v);
                float4 lastAnimPos = GetBonePos(lastAnimInfo.x, lastAnimFrame, boneCount, v);
                
                float weight = step(0.01, duration);
                float t = sin(saturate((_Time.y - curAnimPlayTime) / duration) * (PI / 2.0));

                //是否使用融合
                float4 blendPos = lerp(lastAnimPos, curAnimPos, t);
                float4 finalPos = lerp(curAnimPos, blendPos, weight);

                return finalPos;
            }
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;

                int baseLayer = _LayerParam.x;
                int topLayer = _LayerParam.z;
                
                //只支持两层动画
                int baseCurIndex = GetIndex0(_LayerParam.y);
                int topCurIndex = GetIndex0(_LayerParam.w);
                
                float4 baseLayerPos = GetAnimation(v, baseCurIndex, _TimeParam.x, GetIndex1(_LayerParam.y), _TimeParam.y); //base层layer固定为1
                float4 topLayerPos = GetAnimation(v, topCurIndex, _TimeParam.z, GetIndex1(_LayerParam.w), _TimeParam.w);

                int curLayer = v.uv3.y; //当前顶点所属层级
                int inLayer = step(0.1, curLayer & topLayer);//是否属于该层级
                int layerWeight = step(baseLayer + 0.1, topLayer);
                int sameAnim = step(topCurIndex, baseCurIndex);
                
                topLayerPos = lerp(baseLayerPos, topLayerPos, layerWeight);

                float t = sin(saturate((_Time.y - _TimeParam.z) / 0.25) * (PI / 2.0));
                topLayerPos = lerp(topLayerPos, lerp(topLayerPos, baseLayerPos, t), sameAnim);
                
                float4 finalPos = lerp(baseLayerPos, topLayerPos, inLayer);

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
