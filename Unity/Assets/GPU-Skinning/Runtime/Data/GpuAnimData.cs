using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class GpuAnimData
    {
        [LabelText("切片", SdfIconType.Activity)]
        public AnimationClip Clip;

        [LabelText("过渡时间", SdfIconType.Clock), Tooltip("0表示直接切换")] 
        public float Transition;

        [LabelText("事件", SdfIconType.Alarm)] 
        public List<GpuAnimFrameEvent> FrameEvents;

        public void GetFrameEvents()
        {
            FrameEvents = new List<GpuAnimFrameEvent>();
            for (int i = 0; i < Clip.events.Length; i++)
            {
                var clipEvent = Clip.events[i];
                GpuAnimFrameEvent frameEvent = new GpuAnimFrameEvent();
                frameEvent.Frame = Mathf.FloorToInt(Clip.frameRate * clipEvent.time);
                frameEvent.EventName = clipEvent.functionName;
                FrameEvents.Add(frameEvent);
            }
        }
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