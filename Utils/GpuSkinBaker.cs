using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class GpuSkinBaker : MonoBehaviour
    {
        public TextureFormat TexFormat = TextureFormat.RGBAHalf;
        public int FrameRate = 30;
        public int TexWidth = 512;

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
            
            List<float4> aniHeader = new List<float4>();
            List<float4> aniTexColor = new List<float4>();
            
            // 头长度，骨骼数，动画数，帧率
            aniHeader.Add(new float4(animCount + 1, boneCount, animCount, FrameRate));
            
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
                        aniTexColor.Add(new float4(matrix.m00, matrix.m01, matrix.m02, matrix.m03)); 
                        aniTexColor.Add(new float4(matrix.m10, matrix.m11, matrix.m12, matrix.m13)); 
                        aniTexColor.Add(new float4(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                    }
                }

                var endIndex = aniTexColor.Count;
                var clipLength = endIndex - startIndex;
                
                //动画片段的开始/结束索引, 长度, 是否循环
                float4 headerInfo = new float4(startIndex + animCount + 1, endIndex + animCount + 1, clipLength, clip.isLooping ? 1 : 0);
                aniHeader.Add(headerInfo);
            }
            
            List<float4> aniTex = new List<float4>(aniHeader);
            aniTex.AddRange(aniTexColor);
            
            Texture2D tex = new Texture2D(TexWidth, Mathf.CeilToInt(aniTex.Count / (float)TexWidth), TexFormat, false);
            tex.name = $"GpuSkin_{m_Fbx.name}_AnimTex";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            
            tex.SetPixelData(aniTex.ToArray(), 0);
            // tex.SetPixel(0,0, new Color(2, 3, 1, 4));
            tex.Apply();

            var path = Application.dataPath + $"/Generate/{tex.name}.exr";
            byte[] bytes = ImageConversion.EncodeArrayToEXR(tex.GetRawTextureData(), tex.graphicsFormat, (uint)tex.width, (uint)tex.height);
            System.IO.File.WriteAllBytes(path, bytes);

            SetImportSettings($"Assets/Generate/{tex.name}.exr");
        }
        
        private static void SetImportSettings(string filePath)
        {
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);

            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                // Change settings
                importer.mipmapEnabled = false;
                importer.sRGBTexture = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.isReadable = true;
                importer.compressionQuality = 100;
                var importSetting = importer.GetDefaultPlatformTextureSettings();
                importSetting.format = TextureImporterFormat.RGBAHalf;
                importSetting.textureCompression = TextureImporterCompression.Uncompressed;
                importSetting.overridden = true;
                
                importer.SetPlatformTextureSettings(importSetting);

                //Reimport
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.LogError("Failed to get Texture Importer at path: " + filePath);
            }
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
    }
}

