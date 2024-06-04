using System.Collections.Generic;
using System.IO;
using GPU_Skinning.Runtime.Data;
using Seino.GpuSkin.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

namespace GPU_Skinning.Editor
{
    public class GpuSkinBaker : OdinEditorWindow
    {
        [MenuItem("Tools/GpuSkin/GpuSkinBaker %#z")]
        private static void ShowWindow()
        {
            var window = GetWindow<GpuSkinBaker>();
            window.titleContent = new GUIContent("GpuSKinBaker");
            window.Show();
        }
        
        [PropertyOrder(0)]
        [Title("基本设置")]
        [LabelText("动画帧率", SdfIconType.Activity)]
        public AnimFrame FrameRate = AnimFrame.Frame30;
        
        [LabelText("贴图宽度", SdfIconType.BorderWidth), Tooltip("高度将自动计算")]
        public int TexWidth = 512;

        [LabelText("Shader", SdfIconType.Brush)]
        public Shader GpuSkinShader;

        [LabelText("根目录", SdfIconType.Folder), FolderPath, OnValueChanged("AssetPathChange")] 
        public string FolderPath = "Assets/Generate";

        [LabelText("输出目录", SdfIconType.Folder), ReadOnly] 
        public string AssetPath;

        [PropertyOrder(50)]
        [Title("模型设置")]
        [LabelText("烘焙物体", SdfIconType.Stars), OnValueChanged("AssetPathChange")]
        public GameObject BakeTarget;
        
        [PropertyOrder(100)]
        [Title("动画信息")]
        [LabelText("切片列表"), TableList]
        public List<GpuAnimBakeData> AnimDatas;
        
        // 精度问题，不开放配置
        private TextureFormat TexFormat = TextureFormat.RGBAFloat;

        #region 烘焙方法

        [PropertyOrder(200)]
        [Title("分步烘焙")]
        [Button("烘焙Mesh")]
        public void BakeMesh()
        {
            var mesh = BakeTarget.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
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
            skinMesh.name = $"GpuSkin_{BakeTarget.name}_Mesh";;
            
            CreateAssets(skinMesh, $"{skinMesh.name}.asset");
        }
        
        [PropertyOrder(201)]
        [Button("烘焙骨骼动画")]
        public void BakeBoneAnim()
        {
            var renderer = BakeTarget.GetComponentInChildren<SkinnedMeshRenderer>();
            var bones = renderer.bones;
            var boneCount = bones.Length;
            var bindposes = renderer.sharedMesh.bindposes;
            var animCount = AnimDatas.Count;
            var frameRate = (int)FrameRate;
            
            List<Color> aniHeader = new List<Color>();
            List<Color> aniTexColor = new List<Color>();
            
            // 头长度，骨骼数，动画数，帧率
            aniHeader.Add(new Color(animCount + 1, boneCount, animCount, frameRate));
            float offset = animCount + 1;
            
            for (int animIndex = 0; animIndex < AnimDatas.Count; animIndex++)
            {
                var clip = AnimDatas[animIndex].Clip;
                var frameCount = (int)(frameRate * clip.length);
                float startIndex = aniTexColor.Count + offset;

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    clip.SampleAnimation(BakeTarget, frameIndex / clip.frameRate);
                    
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
            tex.name = $"GpuSkin_{BakeTarget.name}_AnimTex";
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

            CreateAssets(tex, $"{tex.name}.asset");
        }
        
        [PropertyOrder(205)]
        [Title("快速烘焙")]
        [Button("一键烘焙", ButtonSizes.Large)]
        public void Bake()
        {
            BakeBoneAnim();
            BakeMesh();
        }

        #endregion

        #region 辅助方法

        [PropertyOrder(101)]
        [Button("读取帧事件")]
        public void ReadAnimFrameEvents()
        {
            if (AnimDatas == null)
                return;

            for (int i = 0; i < AnimDatas.Count; i++)
            {
                var animData = AnimDatas[i];
                for (int j = 0; j < animData.Clip.events.Length; j++)
                {
                    var clipEvent = animData.Clip.events[j];
                    GpuAnimFrameEvent frameEvent = new GpuAnimFrameEvent();
                    frameEvent.Frame = Mathf.FloorToInt(animData.Clip.frameRate * clipEvent.time);
                    frameEvent.EventName = clipEvent.stringParameter;
                    animData.FrameEvents.Add(frameEvent);
                } 
            }
        }
        
        private void AssetPathChange()
        {
            AssetPath = $"{FolderPath}/GpuSkinRes_{BakeTarget?.name}";
        }

        private void CreateAssets(Object asset, string assetName)
        {
            string path = $"{AssetPath}/{assetName}";
            string directoryPath = Path.Combine(Application.dataPath.Replace("Assets", ""), AssetPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}