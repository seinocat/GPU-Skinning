using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class GpuAnimData
    {
        [LabelText("编号"), Tooltip("编号从1开始")]
        public int Index;

        [LabelText("动画切片名称")]
        public string ClipName;

        [LabelText("是否需要融合过渡")]
        public bool Transition;
    }
}