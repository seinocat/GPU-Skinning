using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static UnityEngine.Rendering.DebugUI.MessageBox;

namespace UnityEditor.GPUAnimation
{
    public class GPUAnimationTextureTool : EditorWindow
    {
        // Properties
        public GameObject targetFBX = null;
        public List<Mesh> targetMeshes = new List<Mesh>();
        public List<AnimationClip> animationClips = new List<AnimationClip>();
        public SaveUVOptions bonesSaveUV = SaveUVOptions.UV1;
        public SaveUVOptions weightsSaveUV = SaveUVOptions.UV2;
        public string texPrefix = "tex_gpuAni_";
        public string texSuffixPos = "_pos";
        public string texSuffixRot = "_rot";

        public GPUAniTexFormat texFormat = GPUAniTexFormat.RGBAHalf;
        public GPUAniTexFPSMode texFPS = GPUAniTexFPSMode.UseClipFPS;

        private bool userErrorTargetFBX = false;

        // SerializedProperty
        private SerializedObject serializedObject;
        private SerializedProperty targetFBXProp;
        private SerializedProperty targetMeshesProp;
        private SerializedProperty animationClipsProp;
        private SerializedProperty bonesSaveUVProp;
        private SerializedProperty weightsSaveUVProp;
        private SerializedProperty texPrefixProp;
        private SerializedProperty texSuffixPosProp;
        private SerializedProperty texSuffixRotProp;
        private SerializedProperty texFormatProp;
        private SerializedProperty texFPSProp;

        [MenuItem("Tools/GPUAnimationTexture")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(GPUAnimationTextureTool));
            window.position = new Rect(800, 300, 500, 809);
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            targetFBXProp = serializedObject.FindProperty("targetFBX");
            targetMeshesProp = serializedObject.FindProperty("targetMeshes");
            animationClipsProp = serializedObject.FindProperty("animationClips");
            bonesSaveUVProp = serializedObject.FindProperty("bonesSaveUV");
            weightsSaveUVProp = serializedObject.FindProperty("weightsSaveUV");

            texPrefixProp = serializedObject.FindProperty("texPrefix");
            texSuffixPosProp = serializedObject.FindProperty("texSuffixPos");
            texSuffixRotProp = serializedObject.FindProperty("texSuffixRot");

            texFormatProp = serializedObject.FindProperty("texFormat");
            texFPSProp = serializedObject.FindProperty("texFPS");
        }

        private static class Styles
        {
            public static readonly GUIContent RebuildMeshesLabel = EditorGUIUtility.TrTextContent("Step1: Rebuild meshes data");
            public static readonly GUIContent BakeGPUAnimationLabel = EditorGUIUtility.TrTextContent("Step2: Bake GPU Animation Texture");
            public static readonly GUIContent DropMesh = EditorGUIUtility.TrTextContent("Drop Meshes files here");
            public static readonly GUIContent DropClip = EditorGUIUtility.TrTextContent("Drop AnimationClip files here");
            public static readonly GUIContent TargetFBX = EditorGUIUtility.TrTextContent("Target FBX:");
            public static readonly GUIContent TargetMeshes = EditorGUIUtility.TrTextContent("Target Meshes:");
            public static readonly GUIContent AnimationClips = EditorGUIUtility.TrTextContent("AnimationClips:");
            public static readonly GUIContent BonesSaveUV = EditorGUIUtility.TrTextContent("Bones UV");
            public static readonly GUIContent WeightsSaveUV = EditorGUIUtility.TrTextContent("Weights UV");
            public static readonly GUIContent Prefix = EditorGUIUtility.TrTextContent("TexPrefix");
            public static readonly GUIContent SuffixPos = EditorGUIUtility.TrTextContent("SuffixPos");
            public static readonly GUIContent SuffixRot = EditorGUIUtility.TrTextContent("SuffixRot");
            public static readonly GUIContent TexFormat = EditorGUIUtility.TrTextContent("TexFormat");
            public static readonly GUIContent TexFPS = EditorGUIUtility.TrTextContent("TexFPS");
        }

        private void AddMeshesFromGameObject(GameObject gameObject)
        {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                AddMesh(meshFilter.sharedMesh);
            }

            SkinnedMeshRenderer[] skinnedMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinRender in skinnedMeshRenderer)
            {
                AddMesh(skinRender.sharedMesh);
            }
        }

        private void AddClipsFromGameObject(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset.GetType() == typeof(AnimationClip))
                {
                    var clip = (AnimationClip)asset;
                    if (!clip.name.Contains("preview"))
                    {
                        AddClip(clip);
                    }
                    
                }
            }
        }

        private void AddClip(AnimationClip clip)
        {
            animationClips.Add(clip);
        }

        private void AddMesh(Mesh mesh)
        {
            targetMeshes.Add(mesh);
        }

        private void OnGUI()
        {
            serializedObject.Update();

            // ReBuild Meshes data
            EditorGUILayout.LabelField(Styles.RebuildMeshesLabel, EditorStyles.boldLabel);

            // Handle drag and drop meshes events
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, Styles.DropMesh);
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject)
                            {
                                string path = AssetDatabase.GetAssetPath(draggedObject);
                                if (!path.ToLower().EndsWith(".fbx"))
                                {
                                    userErrorTargetFBX = true;
                                }
                                else
                                {
                                    userErrorTargetFBX = false;
                                    targetFBX = (GameObject)draggedObject;
                                }

                                GameObject go = (GameObject)draggedObject;
                                AddMeshesFromGameObject(go);
                            }
                            else if (draggedObject is Mesh)
                            {
                                Mesh mesh = (Mesh)draggedObject;
                                AddMesh(mesh);
                            }

                        }
                        GUI.changed = true;
                    }
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(targetMeshesProp, Styles.TargetMeshes);


            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (targetMeshes == null || targetMeshes.Count == 0)
            {
                GUI.enabled = false;
            }
            float savedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.BonesSaveUV).x;
            EditorGUILayout.PropertyField(bonesSaveUVProp, Styles.BonesSaveUV);
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.WeightsSaveUV).x;
            EditorGUILayout.PropertyField(weightsSaveUVProp, Styles.WeightsSaveUV);
            EditorGUIUtility.labelWidth = savedLabelWidth;

            serializedObject.ApplyModifiedProperties();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Rebuild Mesh"))
            {
                foreach (var mesh in targetMeshes)
                {
                    RebuildMeshData(mesh, bonesSaveUV, weightsSaveUV);
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();



            // Bake GPUAnimation Texture
            GUILayout.Space(50);
            EditorGUILayout.LabelField(Styles.BakeGPUAnimationLabel, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            Object userTarget = EditorGUILayout.ObjectField("Target FBX:", targetFBX, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (userTarget != null)
                {
                    string path = AssetDatabase.GetAssetPath(userTarget);
                    if (!path.ToLower().EndsWith(".fbx"))
                    {
                        userErrorTargetFBX = true;
                    }
                    else
                    {
                        userErrorTargetFBX = false;
                        targetFBX = (GameObject)userTarget;
                    }
                }
            }


            if (userErrorTargetFBX)
                EditorGUILayout.HelpBox("Please only drag and drop FBX files.", MessageType.Error);

            // Handle drag and drop events
            Event clipEvt = Event.current;
            Rect clipDropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(clipDropArea, Styles.DropClip);
            switch (clipEvt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!clipDropArea.Contains(clipEvt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (clipEvt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject)
                            {
                                AddClipsFromGameObject(draggedObject);
                            }
                            else if (draggedObject is AnimationClip)
                            {
                                AnimationClip clip = (AnimationClip)draggedObject;
                                AddClip(clip);
                            }

                        }
                        GUI.changed = true;
                    }
                    break;
                default:
                    break;
            }
            EditorGUILayout.PropertyField(animationClipsProp, Styles.AnimationClips);


            serializedObject.ApplyModifiedProperties();



            GUILayout.Space(10);
            if (targetFBX == null || animationClips == null || animationClips.Count == 0)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            savedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.TexFormat).x;
            EditorGUILayout.PropertyField(texFormatProp, Styles.TexFormat);
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.TexFPS).x;
            EditorGUILayout.PropertyField(texFPSProp, Styles.TexFPS);
            EditorGUIUtility.labelWidth = savedLabelWidth;

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            savedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.Prefix).x;
            EditorGUILayout.PropertyField(texPrefixProp, Styles.Prefix);
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.SuffixPos).x;
            EditorGUILayout.PropertyField(texSuffixPosProp, Styles.SuffixPos);
            EditorGUIUtility.labelWidth = GUI.skin.box.CalcSize(Styles.SuffixRot).x;
            EditorGUILayout.PropertyField(texSuffixRotProp, Styles.SuffixRot);
            EditorGUIUtility.labelWidth = savedLabelWidth;

            serializedObject.ApplyModifiedProperties();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Bake Texture"))
            {
                //GenerateGPUSkeletonAnimeTexture(m_TargetFBX, m_Clip);
                foreach (var clip in animationClips)
                {
                    GenerateGPUSkeletionSplitAnimeTexture(targetFBX, clip, texPrefix, texSuffixPos, texSuffixRot, texFormat, texFPS);
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
        }

        #region Rebuild Mesh Data
        public enum SaveUVOptions
        {
            UV0,
            UV1,
            UV2,
            UV3,
            UV4,
            UV5,
            UV6,
            UV7,
        }

        /// <summary>
        /// Job system
        /// Used to generate bone indices array nad bone weights array.
        /// </summary>
        private struct GenerateBoneWeightsDataJob : IJobParallelFor
        {
            public NativeArray<Vector4> vertBoneIndices;
            public NativeArray<Vector4> vertBoneWeights;
            [ReadOnly] public NativeArray<BoneWeight> boneWeights;

            public void Execute(int index)
            {
                var boneIndexWeights = boneWeights[index];
                vertBoneIndices[index] = new Vector4(boneIndexWeights.boneIndex0,
                                                      boneIndexWeights.boneIndex1,
                                                      boneIndexWeights.boneIndex2,
                                                      boneIndexWeights.boneIndex3);
                vertBoneWeights[index] = new Vector4(boneIndexWeights.weight0,
                                                      boneIndexWeights.weight1,
                                                      boneIndexWeights.weight2,
                                                      boneIndexWeights.weight3);
            }
        }

        /// <summary>
        /// Rebuild mesh data. Write bone indices and bone weights to UVs.
        /// </summary>
        /// <param name="targetFBX"></param>
        public static void RebuildMeshData(Mesh mesh, SaveUVOptions boneUV, SaveUVOptions weightsUV)
        {
            // New Mesh instance
            Mesh newMesh = GameObject.Instantiate(mesh) as Mesh;
            newMesh.name = mesh.name + "_skinned";

            BoneWeight[] boneWeights = newMesh.boneWeights;

            NativeArray<Vector4> vertBoneIndices = new NativeArray<Vector4>(newMesh.vertexCount, Allocator.TempJob);
            NativeArray<Vector4> vertBoneWeights = new NativeArray<Vector4>(newMesh.vertexCount, Allocator.TempJob);
            NativeArray<BoneWeight> nativeBoneWeights = new NativeArray<BoneWeight>(boneWeights, Allocator.TempJob);

            // We must use Job to accelerate
            GenerateBoneWeightsDataJob job = new GenerateBoneWeightsDataJob
            {
                vertBoneIndices = vertBoneIndices,
                vertBoneWeights = vertBoneWeights,
                boneWeights = nativeBoneWeights
            };
            JobHandle handle = job.Schedule(newMesh.vertexCount, 64);
            handle.Complete();

            // Apply to Mesh
            newMesh.SetUVs((int)boneUV, vertBoneIndices);
            newMesh.SetUVs((int)weightsUV, vertBoneWeights);

            // Release NativeArray
            vertBoneIndices.Dispose();
            vertBoneWeights.Dispose();
            nativeBoneWeights.Dispose();


            // Save to file
            string path = AssetDatabase.GetAssetPath(mesh);
            string directoryPath = Path.GetDirectoryName(path);
            string filePath = directoryPath + "\\" + newMesh.name + ".asset";
            WriteFileToAsset(newMesh, filePath);

            Debug.Log("Rebuild Mesh: " + newMesh.name);
        }


        #endregion


        #region Bake GPUAnimation Texture
        public enum GPUAniTexFormat
        { 
            RGBAHalf,
            RGBAFloat
        }

        public enum GPUAniTexFPSMode
        {
            UseClipFPS,
            _24FPS,
            _30FPS,
            _60FPS,
            _90FPS
        }

        public static void GenerateGPUSkeletionSplitAnimeTexture(GameObject targetFBX, AnimationClip aniClip, 
            string texPrefix = "GPUAni_", string texSuffixPos = "_pos", string texSuffixRot = "_rot", 
            GPUAniTexFormat textureFormat = GPUAniTexFormat.RGBAFloat,
            GPUAniTexFPSMode fpsMode = GPUAniTexFPSMode.UseClipFPS)
        {
            string path = AssetDatabase.GetAssetPath(aniClip);
            string directoryPath = Path.GetDirectoryName(path);


            var targetObj = GameObject.Instantiate(targetFBX);
            SkinnedMeshRenderer skinnedRenderer = targetObj.GetComponentInChildren<SkinnedMeshRenderer>();
            Mesh mesh = skinnedRenderer.sharedMesh;


            // Bake Animation Texture
            float frameRate = aniClip.frameRate;
            switch (fpsMode)
            {
                case GPUAniTexFPSMode._24FPS:
                    frameRate = 24.0f;
                    break;
                case GPUAniTexFPSMode._30FPS:
                    frameRate = 30.0f;
                    break;
                case GPUAniTexFPSMode._60FPS:
                    frameRate = 60.0f;
                    break;
                case GPUAniTexFPSMode._90FPS:
                    frameRate = 90.0f;
                    break;
            }

            int frameCount = (int)(aniClip.length * frameRate);
            float invFrameRate = 1.0f / frameRate;
            int bonesCount = skinnedRenderer.bones.Length;

            Vector2Int texSize = new Vector2Int(bonesCount, frameCount);
            string bakeTexNamePos = texPrefix + aniClip.name + "_" + frameRate + texSuffixPos;
            string bakeTexNameRot = texPrefix + aniClip.name + "_" + frameRate + texSuffixRot;

            if (textureFormat == GPUAniTexFormat.RGBAFloat)
            {
                float4[] posData = new float4[texSize.x * texSize.y];
                float4[] rotData = new float4[texSize.x * texSize.y];
                Matrix4x4[] bindpose = mesh.bindposes;
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    aniClip.SampleAnimation(targetObj, frameIndex * invFrameRate);

                    for (int boneIndex = 0; boneIndex < bonesCount; boneIndex++)
                    {
                        Matrix4x4 animeTransform = skinnedRenderer.bones[boneIndex].localToWorldMatrix * bindpose[boneIndex];

                        Vector3 pos = animeTransform.GetPosition();
                        Quaternion quaternion = animeTransform.rotation;
                        Vector3 scale = animeTransform.lossyScale;

                        posData[frameIndex * texSize.x + boneIndex] = new float4(pos.x, pos.y, pos.z, CheckUniformScale(scale) ? scale.x : 1.0f);
                        rotData[frameIndex * texSize.x + boneIndex] = new float4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                    }
                }

                //Save to exr, values can be negative.
                WriteFloat4DataToEXR(posData, directoryPath, bakeTexNamePos, texSize.x, texSize.y, textureFormat);
                WriteFloat4DataToEXR(rotData, directoryPath, bakeTexNameRot, texSize.x, texSize.y, textureFormat);
            }
            else if (textureFormat == GPUAniTexFormat.RGBAHalf)
            {
                half4[] posData = new half4[texSize.x * texSize.y];
                half4[] rotData = new half4[texSize.x * texSize.y];
                Matrix4x4[] bindpose = mesh.bindposes;
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    aniClip.SampleAnimation(targetObj, frameIndex * invFrameRate);

                    for (int boneIndex = 0; boneIndex < bonesCount; boneIndex++)
                    {
                        Matrix4x4 animeTransform = skinnedRenderer.bones[boneIndex].localToWorldMatrix * bindpose[boneIndex];

                        Vector3 pos = animeTransform.GetPosition();
                        Quaternion quaternion = animeTransform.rotation;
                        Vector3 scale = animeTransform.lossyScale;

                        posData[frameIndex * texSize.x + boneIndex] = (half4)(new float4(pos.x, pos.y, pos.z, CheckUniformScale(scale) ? scale.x : 1.0f));
                        rotData[frameIndex * texSize.x + boneIndex] = (half4)(new float4(quaternion.x, quaternion.y, quaternion.z, quaternion.w));
                    }
                }

                //Save to exr, values can be negative.
                WriteFloat4DataToEXR(posData, directoryPath, bakeTexNamePos, texSize.x, texSize.y, textureFormat);
                WriteFloat4DataToEXR(rotData, directoryPath, bakeTexNameRot, texSize.x, texSize.y, textureFormat);
            }


            AssetDatabase.Refresh();

            DestroyImmediate(targetObj);
        }

        public static void GenerateGPUSkeletonAnimeTexture(GameObject targetFBX, AnimationClip aniClip)
        {
            string path = AssetDatabase.GetAssetPath(aniClip);
            string directoryPath = Path.GetDirectoryName(path);


            var targetObj = GameObject.Instantiate(targetFBX);
            SkinnedMeshRenderer skinnedRenderer = targetObj.GetComponentInChildren<SkinnedMeshRenderer>();
            Mesh mesh = skinnedRenderer.sharedMesh;

            
            // Bake Animation Texture
            int frameCount = (int)(aniClip.length * aniClip.frameRate);
            //frameCount = 1;

            float invFrameRate = 1.0f / aniClip.frameRate;
            int bonesCount = skinnedRenderer.bones.Length;

            Texture2D animTex = new Texture2D(3 * bonesCount, frameCount, TextureFormat.RGBAFloat, false, true);
            float4[] texColors = new float4[animTex.width * animTex.height];
            Matrix4x4[] bindpose = mesh.bindposes;

            Matrix4x4[] animeBoneMatrix = new Matrix4x4[bonesCount];
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                aniClip.SampleAnimation(targetObj, frameIndex * invFrameRate);

                for (int boneIndex = 0; boneIndex < bonesCount; boneIndex++)
                {
                    Matrix4x4 animeTransform = skinnedRenderer.bones[boneIndex].localToWorldMatrix * bindpose[boneIndex];

                    animeBoneMatrix[boneIndex] = animeTransform;

                    texColors[frameIndex * animTex.width + 3 * boneIndex + 0] = animeTransform.GetRow(0);
                    texColors[frameIndex * animTex.width + 3 * boneIndex + 1] = animeTransform.GetRow(1);
                    texColors[frameIndex * animTex.width + 3 * boneIndex + 2] = animeTransform.GetRow(2);

                    Debug.Log(boneIndex + ", matrix: " + animeTransform);
                }
            }



            animTex.SetPixelData<float4>(texColors, 0);
            animTex.Apply(); // apply color to texture

            //Save to exr, values can be negative.
            {
                Debug.Log(animTex.graphicsFormat);
                byte[] texBytes = ImageConversion.EncodeArrayToEXR(animTex.GetRawTextureData(), animTex.graphicsFormat, (uint)animTex.width, (uint)animTex.height);
                string filePath = directoryPath + "\\AnimeTexture.exr";
                File.WriteAllBytes(filePath, texBytes);
            }

            AssetDatabase.Refresh();

            DestroyImmediate(targetObj);
            DestroyImmediate(animTex);
        }

        /// <summary>
        /// Check scale is uniform or not.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static bool CheckUniformScale(Vector3 scale)
        {
            if (Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.x, scale.z))
            {
                return true;
            }

            Debug.LogError("Bone scale must be uniform, please modify bones animation");
            return false;
        }

        private static TextureFormat GetTextureFormatFromGPUAniTexFormat(GPUAniTexFormat textureFormat)
        {
            TextureFormat format;
            switch (textureFormat)
            {
                case GPUAniTexFormat.RGBAHalf:
                    format = TextureFormat.RGBAHalf;
                    break;
                case GPUAniTexFormat.RGBAFloat:
                    format = TextureFormat.RGBAFloat;
                    break;
                default:
                    format = TextureFormat.RGBAFloat;
                    break;
            }
            return format;
        }

        private static TextureImporterFormat GetTextureImporterFormatFromGPUAniTexFormat(GPUAniTexFormat textureFormat)
        {
            TextureImporterFormat format;
            switch (textureFormat)
            {
                case GPUAniTexFormat.RGBAHalf:
                    format = TextureImporterFormat.RGBAHalf;
                    break;
                case GPUAniTexFormat.RGBAFloat:
                    format = TextureImporterFormat.RGBAFloat;
                    break;
                default:
                    format = TextureImporterFormat.RGBAFloat;
                    break;
            }
            return format;
        }

        private static void WriteFloat4DataToEXR<T>(T[] colorData, string directoryPath, string texName, int width, int height, GPUAniTexFormat texFormat = GPUAniTexFormat.RGBAFloat)
        {
            string filePath = directoryPath + "\\" + texName + ".exr";
            Texture2D tex = new Texture2D(width, height, GetTextureFormatFromGPUAniTexFormat(texFormat), false, true);
            tex.SetPixelData(colorData, 0);
            tex.Apply(); // apply color to texture

            WriteTextureToEXR(tex, filePath);

            DestroyImmediate(tex);

            // Change texture import settings
            InitTextureImportSettings(filePath, texFormat);
        }

        private static void InitTextureImportSettings(string filePath, GPUAniTexFormat texFormat = GPUAniTexFormat.RGBAFloat)
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
                importer.compressionQuality = 100;
                var textureImporterSettings = importer.GetDefaultPlatformTextureSettings();
                textureImporterSettings.format = GetTextureImporterFormatFromGPUAniTexFormat(texFormat);
                textureImporterSettings.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporterSettings.overridden = true;
                importer.SetPlatformTextureSettings(textureImporterSettings);

                //Reimport
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.LogError("Failed to get Texture Importer at path: " + filePath);
            }
        }
        
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="filePath">Should end up with ".exr".</param>
        private static void WriteTextureToEXR(Texture2D tex, string filePath)
        {
            byte[] texBytes = ImageConversion.EncodeArrayToEXR(tex.GetRawTextureData(), tex.graphicsFormat, (uint)tex.width, (uint)tex.height);
            File.WriteAllBytes(filePath, texBytes);

            Debug.Log("Write " + tex.name + " Texture at: " + filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="filePath">Should end up with ".asset".</param>
        private static void WriteTextureToAsset(Texture2D tex, string filePath)
        {
            AssetDatabase.CreateAsset(tex, filePath);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filePath"></param>
        private static void WriteFileToAsset(Object file, string filePath)
        {
            AssetDatabase.CreateAsset(file, filePath);
            AssetDatabase.SaveAssets();
        }


    }

}