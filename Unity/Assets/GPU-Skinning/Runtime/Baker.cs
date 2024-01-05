using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Seino.GpuSkinning.Runtime
{
    public class Baker : MonoBehaviour
    {
        public Animator Animator;
        public TextureFormat TexFormat = TextureFormat.RGBA32;

        private void Awake()
        {
            this.Animator = GetComponent<Animator>();
        }

        [Button("烘焙动画")]
        public void Bake()
        {
            var clips = this.Animator.runtimeAnimatorController?.animationClips;
            var renderer = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (var clip in clips)
            {
                BakeBoneMatrixTex(renderer, clip);
            }
        }
        
        [Button("检查")]
        public void Check()
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Generate/Walk_N.asset");
            var colors = tex.GetPixels();
            
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Generate/RobotKile.asset");
        }

        private void BakeBoneMatrixTex(SkinnedMeshRenderer renderer, AnimationClip clip)
        {
            var frameCount = (int)(clip.frameRate * clip.length);
            var bones = renderer.bones;
            var boneCount = bones.Length;
            var bindposes = renderer.sharedMesh.bindposes;
            
            var width = boneCount * 12;
            var height = frameCount;
            Texture2D tex = new Texture2D(width, height, TexFormat, false);
            tex.name = clip.name;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            Color[] colors = new Color[width * height];
            
            for (int i = 0; i < frameCount; i++)
            {
                clip.SampleAnimation(gameObject, i / clip.frameRate);

                for (int j = 0; j < boneCount; j++)
                {
                    var matrix = transform.worldToLocalMatrix * bones[j].localToWorldMatrix * bindposes[j];
                    colors[i * width + j * 12] = EncodeFloatRGBA(matrix.m00);
                    colors[i * width + j * 12 + 1] = EncodeFloatRGBA(matrix.m01);
                    colors[i * width + j * 12 + 2] = EncodeFloatRGBA(matrix.m02);
                    colors[i * width + j * 12 + 3] = EncodeFloatRGBA(matrix.m03);
                    
                    colors[i * width + j * 12 + 4] = EncodeFloatRGBA(matrix.m10);
                    colors[i * width + j * 12 + 5] = EncodeFloatRGBA(matrix.m11);
                    colors[i * width + j * 12 + 6] = EncodeFloatRGBA(matrix.m12);
                    colors[i * width + j * 12 + 7] = EncodeFloatRGBA(matrix.m13);
                    
                    colors[i * width + j * 12 + 8] = EncodeFloatRGBA(matrix.m20);
                    colors[i * width + j * 12 + 9] = EncodeFloatRGBA(matrix.m21);
                    colors[i * width + j * 12 + 10] = EncodeFloatRGBA(matrix.m22);
                    colors[i * width + j * 12 + 11] = EncodeFloatRGBA(matrix.m23);
                }
            }
            
            tex.SetPixels(colors);
            tex.Apply();
            
            AssetDatabase.CreateAsset(tex, $"Assets/Generate/{tex.name}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Button("烘焙Mesh")]
        private void BakeMesh(SkinnedMeshRenderer renderer)
        {
            var mesh = renderer.sharedMesh;
            var skinMesh = Instantiate(mesh);
            var boneWeights = mesh.boneWeights;
            Vector2[] uv2 = new Vector2[boneWeights.Length] ;
            Vector2[] uv3 = new Vector2[boneWeights.Length] ;
            
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var boneWeight = boneWeights[i];
                uv2[i] = new Vector4(boneWeight.boneIndex0, boneWeight.weight0);
                uv3[i] = new Vector4(boneWeight.boneIndex1, boneWeight.weight1);
            }
            skinMesh.SetUVs(1, uv2);
            skinMesh.SetUVs(2, uv3);
            skinMesh.name = mesh.name;
            AssetDatabase.CreateAsset(skinMesh, $"Assets/Generate/{skinMesh.name}.asset");
            AssetDatabase.SaveAssets();
        }
        
        private static Vector4 EncodeFloatRGBA(float v)
        {
            v = v * 0.01f + 0.5f; //保证范围在[0,1]
            var kEncodeMul = new Vector4(1.0f, 255.0f, 65025.0f, 160581375.0f);
            var kEncodeBit = 1.0f / 255.0f;
            var enc = kEncodeMul * v;
            for (var i = 0; i < 4; i++) enc[i] %= 1;
            enc -= new Vector4(enc.y, enc.z, enc.w, enc.w) * kEncodeBit;
            return enc;
        }
        
    }
}