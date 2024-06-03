using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class GpuAnimBakeData
    {
        [LabelText("动画切片")]
        public AnimationClip Clip;

        [LabelText("过渡时间")] 
        public float Transition = 0.25f;

        [LabelText("帧事件")] 
        public List<GpuAnimFrameEvent> FrameEvents;

        [Button("读取Clip信息")]
        public void ReadClip()
        {
            for (int i = 0; i < Clip.events.Length; i++)
            {
                var clipEvent = Clip.events[i];
                GpuAnimFrameEvent frameEvent = new GpuAnimFrameEvent();
                frameEvent.Frame = Mathf.FloorToInt(Clip.frameRate * clipEvent.time);
                frameEvent.EventName = clipEvent.stringParameter;
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