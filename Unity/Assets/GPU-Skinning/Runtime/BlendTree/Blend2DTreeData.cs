using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class Blend2DTreeData
    {
        public AnimationClip Clip;

        [Range(0, 1)]
        public float Threshold;
    }
}