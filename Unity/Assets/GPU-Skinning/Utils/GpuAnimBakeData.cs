using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class GpuAnimBakeData
    {
        [LabelText("切片", SdfIconType.Activity)]
        public AnimationClip Clip;

        [LabelText("过渡时间", SdfIconType.Clock), Tooltip("0表示直接切换")] 
        public float Transition = 0.25f;

        [LabelText("事件", SdfIconType.Alarm)] 
        public List<GpuAnimFrameEvent> FrameEvents;
    }

    [Serializable]
    public class GpuAnimFrameEvent
    {
        [LabelText("帧数")]
        public int Frame;

        [LabelText("事件名称")]
        public string EventName;
    }
}