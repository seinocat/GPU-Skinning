using System.Collections.Generic;
using Seino.GpuSkin.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GPU_Skinning.Editor
{
    public class GpuSkinBaker : OdinEditorWindow
    {
        [MenuItem("Tools/GpuSkin/GpuSkinBaker")]
        private static void ShowWindow()
        {
            var window = GetWindow<GpuSkinBaker>();
            window.titleContent = new GUIContent("GpuSKinBaker");
            window.Show();
        }
        
        [Title("基本参数")]
        [LabelText("帧率")]
        public int FrameRate = 30;
        
        [LabelText("贴图宽度"), Tooltip("高度将自动计算")]
        public int TexWidth = 512;

        [LabelText("Shader")]
        public Shader GpuSkinShader;

        [Title("目标物体")]
        [LabelText("GameObject")]
        public GameObject Target;
        
        [Title("动画信息")]
        [LabelText("切片列表"), TableList]
        public List<GpuAnimBakeData> AnimDatas;

        public Transform transform;

        public GameObject gameObject;
        
        // 精度问题，不开放配置
        private TextureFormat TexFormat = TextureFormat.RGBAFloat;
        
        [Title("分步烘焙")]
        [Button("烘焙Mesh")]
        public void BakeMesh()
        {
            var mesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
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
            skinMesh.name = $"GpuSkin_{gameObject.name}_Mesh";;
            AssetDatabase.CreateAsset(skinMesh, $"Assets/Generate/{skinMesh.name}.asset");
            AssetDatabase.SaveAssets();
        }
        

        [Button("烘焙骨骼动画")]
        public void BakeBoneAnim()
        {
            var renderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
            var bones = renderer.bones;
            var boneCount = bones.Length;
            var bindposes = renderer.sharedMesh.bindposes;
            var animCount = AnimDatas.Count;
            
            List<Color> aniHeader = new List<Color>();
            List<Color> aniTexColor = new List<Color>();
            
            // 头长度，骨骼数，动画数，帧率
            aniHeader.Add(new Color(animCount + 1, boneCount, animCount, FrameRate));
            float offset = animCount + 1;
            
            for (int animIndex = 0; animIndex < AnimDatas.Count; animIndex++)
            {
                var clip = AnimDatas[animIndex].Clip;
                var frameCount = (int)(FrameRate * clip.length);
                float startIndex = aniTexColor.Count + offset;

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    clip.SampleAnimation(gameObject, frameIndex / clip.frameRate);
                    
                    for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                    {
                        var matrix = bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];
                        aniTexColor.Add(new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03)); 
                        aniTexColor.Add(new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13)); 
                        aniTexColor.Add(new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                    }
                }
                
                //开始索引, 帧数, 是否需要融合, 是否循环
                Color headerInfo = new Color(startIndex, frameCount, 1f, clip.isLooping ? 1 : 0);
                aniHeader.Add(headerInfo);
            }
            
            List<Color> aniTex = new List<Color>(aniHeader);
            aniTex.AddRange(aniTexColor);

            int width = TexWidth;
            int height = Mathf.CeilToInt(aniTex.Count / (float)width);
            
            Texture2D tex = new Texture2D(width, height, TexFormat, false);
            tex.name = $"GpuSkin_{gameObject.name}_AnimTex";
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
        
        [Title("快速烘焙")]
        [Button("一键烘焙", ButtonSizes.Large)]
        public void Bake()
        {
            BakeBoneAnim();
            BakeMesh();
        }
    }
}