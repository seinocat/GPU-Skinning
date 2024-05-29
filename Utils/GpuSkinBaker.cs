using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class GpuSkinBaker : MonoBehaviour
    {
        public TextureFormat TexFormat = TextureFormat.RGBAHalf;
        public int FrameRate = 30;
        public int TexWidth = 512;
        public Animator m_Animator;
        
        [Button("检查")]
        public void Sample(int frame = 0)
        {
            // var clip = m_Animator.runtimeAnimatorController.animationClips[0];
            // m_Animator.Play(clip.name);
            m_Clips[0].SampleAnimation(gameObject, frame / 30f);
        }

        [Button("检查")]
        public void Check()
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Generate/GpuSkin_Baker_AnimTex.exr");
            var colors = tex.GetPixels();
            var bytes = tex.GetRawTextureData();
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Generate/RobotKile.asset");
        }

        public List<AnimationClip> m_Clips;
        public GameObject m_Fbx;
        
        [Button("烘焙骨骼动画")]
        public void BakeBoneAnim()
        {
            var renderer = m_Fbx.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            var bones = renderer.bones;
            var boneCount = bones.Length;
            var bindposes = renderer.sharedMesh.bindposes;
            var animCount = m_Clips.Count;
            
            List<Color> aniHeader = new List<Color>();
            List<Color> aniTexColor = new List<Color>();
            
            // 头长度，骨骼数，动画数，帧率
            aniHeader.Add(new Color(animCount + 1, boneCount, animCount, FrameRate));
            
            for (int animIndex = 0; animIndex < m_Clips.Count; animIndex++)
            {
                var clip = m_Clips[animIndex];
                var frameCount = (int)(FrameRate * clip.length);
                var startIndex = aniTexColor.Count;

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    clip.SampleAnimation(gameObject, frameIndex / clip.frameRate);
                    
                    for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                    {
                        var matrix = transform.worldToLocalMatrix * bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];
                        aniTexColor.Add(new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03)); 
                        aniTexColor.Add(new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13)); 
                        aniTexColor.Add(new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                    }
                }

                var endIndex = aniTexColor.Count;

                //动画片段的开始/结束索引, 帧数, 是否循环
                Color headerInfo = new Color(startIndex + animCount + 1, endIndex + animCount, frameCount, clip.isLooping ? 1 : 0);
                aniHeader.Add(headerInfo);
            }
            
            List<Color> aniTex = new List<Color>(aniHeader);
            aniTex.AddRange(aniTexColor);

            int width = TexWidth;
            int height = Mathf.CeilToInt(aniTex.Count / (float)width);

            // 检查一下精度
            if (width * height > ushort.MaxValue)
                TexFormat = TextureFormat.RGBAFloat;
            
            Texture2D tex = new Texture2D(width, height, TexFormat, false);
            tex.name = $"GpuSkin_{m_Fbx.name}_AnimTex";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 0;

            for (int i = 0; i < aniTex.Count; i++)
            {
                int u = i % width;
                int v = i / width;
                tex.SetPixel(u, v, aniTex[i]);
            }
            
            tex.Apply();
            
            AssetDatabase.CreateAsset(tex, $"Assets/Generate/{tex.name}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [Button("烘焙Mesh")]
        public void BakeMesh()
        {
            var mesh = m_Fbx.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            var skinMesh = Instantiate(mesh);
            var boneWeights = mesh.boneWeights;
            Vector2[] uv2 = new Vector2[boneWeights.Length] ;
            Vector2[] uv3 = new Vector2[boneWeights.Length] ;
            
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var boneWeight = boneWeights[i];
                uv2[i] = new Vector2(boneWeight.boneIndex0, boneWeight.weight0);
                uv3[i] = new Vector2(boneWeight.boneIndex1, boneWeight.weight1);
            }
            
            skinMesh.SetUVs(1, uv2);
            skinMesh.SetUVs(2, uv3);
            skinMesh.name = $"GpuSkin_{m_Fbx.name}_Mesh";;
            AssetDatabase.CreateAsset(skinMesh, $"Assets/Generate/{skinMesh.name}.asset");
            AssetDatabase.SaveAssets();
        }

        [Button("一键烘焙", ButtonSizes.Large)]
        public void Bake()
        {
            BakeBoneAnim();
            BakeMesh();
        }
    }
}

