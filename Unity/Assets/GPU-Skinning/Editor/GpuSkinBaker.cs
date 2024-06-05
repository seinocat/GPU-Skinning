﻿using System;
using System.Collections.Generic;
using System.IO;
using GPU_Skinning.Runtime.Data;
using Seino.GpuSkin.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

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
        
        // 精度问题，不开放配置
        private TextureFormat TexFormat = TextureFormat.RGBAFloat;

        [PropertyOrder(0)] 
        [Title("GpuSkin配置")] 
        [LabelText("配置", SdfIconType.FileEarmarkFill), InlineButton("SaveConfig", "保存"), InlineButton("LoadConfig", "加载")]
        public GpuSkinConfig Config;
        
        [PropertyOrder(10)]
        [Title("基本设置")]
        [LabelText("动画帧率", SdfIconType.Activity)]
        public AnimFrame FrameRate = AnimFrame.Frame30;
        
        [PropertyOrder(11)] 
        [LabelText("贴图宽度", SdfIconType.BorderWidth), Tooltip("高度将自动计算")]
        public int TexWidth = 512;
        
        [PropertyOrder(12)] 
        [LabelText("根目录", SdfIconType.FolderFill), FolderPath, OnValueChanged("AssetPathChange")] 
        public string FolderPath = "Assets/Generate";

        [PropertyOrder(13)] 
        [LabelText("输出目录", SdfIconType.FolderFill), ReadOnly] 
        public string AssetPath;

        [PropertyOrder(50)]
        [Title("模型和材质")]
        [LabelText("烘焙模型", SdfIconType.PersonPlusFill), OnValueChanged("AssetPathChange")]
        public GameObject BakeTarget;
        
        [PropertyOrder(51)]
        [LabelText("Mesh", SdfIconType.Grid3x3GapFill)]
        public Mesh BakeMesh;
        
        [PropertyOrder(51)]
        [LabelText("Material", SdfIconType.CircleFill)]
        public Material BakeMaterial;
        
        [PropertyOrder(52)]
        [LabelText("Shader", SdfIconType.EyeFill)]
        public Shader GpuSkinShader;
        
        [PropertyOrder(53)]
        [LabelText("漫反射参数", SdfIconType.BodyText)]
        public string MainTexProperty = "_MainTex";
        
        [PropertyOrder(54)]
        [LabelText("漫反射贴图", SdfIconType.Image)]
        public Texture MainTex;

        [PropertyOrder(100)] 
        [Title("动画信息")]
        [LabelText("动画路径", SdfIconType.FolderFill), FolderPath, 
         InlineButton("GetAnimClips", "搜索"), InlineButton("ReadAnimClipEvents", "读取事件")]
        public string AnimFolderPath;
        
        [PropertyOrder(102)]
        [LabelText("列表"), TableList]
        public List<GpuAnimData> AnimDatas;
        
        #region 烘焙方法
        
        private static readonly int SHADER_PROPERTY_ANIMTEX = Shader.PropertyToID("_AnimTex");

        [Title("分步执行")]
        [PropertyOrder(200)]
        [Button("创建材质")]
        public void CreateMaterial()
        {
            if (GpuSkinShader == null)
            {
                EditorUtility.DisplayDialog("提示", "shader不能为空!", "ok");
                return;
            }
            
            BakeMaterial = new Material(GpuSkinShader);
            BakeMaterial.SetTexture(MainTexProperty, MainTex);
            CreateAssets(BakeMaterial, $"GpuSkin_{BakeTarget.name}_Mat.mat");
        }
        
        [PropertyOrder(205)]
        [Button("创建Mesh")]
        public void CreateMesh()
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
            skinMesh.name = $"GpuSkin_{BakeTarget.name}_Mesh";

            BakeMesh = skinMesh;
            
            CreateAssets(skinMesh, $"{skinMesh.name}.asset");
        }
        
        [PropertyOrder(206)]
        [Button("烘焙贴图")]
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
                var animData = AnimDatas[animIndex];
                var clip = animData.Clip;
                var frameCount = (int)(frameRate * clip.length);
                float startIndex = aniTexColor.Count + offset;

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    clip.SampleAnimation(BakeTarget, (float)frameIndex / frameRate);
                    
                    for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                    {
                        var matrix = bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];
                        aniTexColor.Add(new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03)); 
                        aniTexColor.Add(new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13)); 
                        aniTexColor.Add(new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                    }
                }
                
                //开始索引, 帧数, 是否需要融合, 是否循环
                Color headerInfo = new Color(startIndex, frameCount, animData.Transition, clip.isLooping ? 1 : 0);
                aniHeader.Add(headerInfo);
            }
            
            List<Color> aniTex = new List<Color>(aniHeader);
            aniTex.AddRange(aniTexColor);

            int width = TexWidth;
            int height = Mathf.CeilToInt(aniTex.Count / (float)width);
            
            Texture2D tex = new Texture2D(width, height, TexFormat, false);
            tex.name = $"GpuSkin_{BakeTarget.name}_Tex";
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

            BakeMaterial.SetTexture(SHADER_PROPERTY_ANIMTEX, tex);
            CreateAssets(tex, $"{tex.name}.asset");
        }
        
        
        [PropertyOrder(207)]
        [Button("保存配置")]
        public void CreateSaveConfig()
        {
            if (Config == null)
            {
                Config = CreateInstance<GpuSkinConfig>();
                CreateAssets(Config, $"GpuSkin_{BakeTarget.name}_Config.asset");
            }
            
            SaveConfig();
        }

        
        
        [PropertyOrder(250)]
        [Title("快速操作")]
        [Button("全部执行", ButtonSizes.Large)]
        public void Bake()
        {
            CreateMaterial();
            CreateMesh();
            BakeBoneAnim();
            SaveConfig();
        }
        
        

        #endregion

        #region 辅助方法
        
        private void ReadAnimClipEvents()
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
                    frameEvent.EventName = clipEvent.functionName;
                    animData.FrameEvents.Add(frameEvent);
                } 
            }
        }
        
        private void LoadConfig()
        {
            if (Config == null)
                return;

            FrameRate = Config.FrameRate;
            BakeTarget = Config.BakeTarget;
            BakeMesh = Config.BakeMesh;
            BakeMaterial = Config.BakeMaterial;
            GpuSkinShader = Config.GpuSkinShader;
            MainTexProperty = Config.MainTexProperty;
            MainTex = Config.MainTex;
            AnimDatas = Config.AnimDatas;
            AssetPathChange();
        }

        private void SaveConfig()
        {
            if (Config == null)
                return;
            
            Config.FrameRate = FrameRate;
            Config.BakeTarget = BakeTarget;
            Config.BakeMesh = BakeMesh;
            Config.BakeMaterial = BakeMaterial;
            Config.GpuSkinShader = GpuSkinShader;
            Config.MainTexProperty = MainTexProperty;
            Config.MainTex = MainTex;
            Config.AnimDatas = AnimDatas;
        }
        
        private void AssetPathChange()
        {
            AssetPath = $"{FolderPath}/GpuSkinRes_{BakeTarget?.name}";
        }

        private void GetAnimClips()
        {
            if (EditorUtility.DisplayDialog("提示", "自动获取动画切片将清空现有配置信息，是否继续？", "继续", "取消"))
            {
                GetAnimClipInternal();
            }
        }
        
        internal void GetAnimClipInternal()
        {
            // 查重用
            Dictionary<string, AnimationClip> animClips = new Dictionary<string, AnimationClip>();
            FindAnimClipInternal("*.fbx", ref animClips);
            FindAnimClipInternal("*.anim", ref animClips);

            AnimDatas.Clear();
            foreach (var paris in animClips)
            {
                GpuAnimData animData = new GpuAnimData();
                animData.Clip = paris.Value;
                animData.GetFrameEvents();
                AnimDatas.Add(animData);
            }
        }

        internal void FindAnimClipInternal(string suffix, ref Dictionary<string, AnimationClip> animClips)
        {
            if (string.IsNullOrEmpty(AnimFolderPath))
                return;
            
            string[] animFiles = Directory.GetFiles(AnimFolderPath, suffix, SearchOption.AllDirectories);
            foreach (string animFile in animFiles)
            {
                string relativePath = animFile[animFile.IndexOf("Assets", StringComparison.Ordinal)..];
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(relativePath);

                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        if (animClips.ContainsKey(clip.name))
                        {
                            Debug.LogError($"发现重名AnimationClip:{clip.name}");
                            continue;
                        }
                        
                        animClips.Add(clip.name, clip);
                    }
                }
            }
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