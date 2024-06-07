using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class BoneData
    {
        [LabelText("层级", SdfIconType.LayersFill)]
        public GpuSkinLayer Layer;
        
        [LabelText("骨骼名称", SdfIconType.BodyText)]
        public string BoneName;

        [LabelText("骨骼", SdfIconType.Diagram3), OnValueChanged("SetBoneName"), ShowIf("@string.IsNullOrEmpty(BoneName)")]
        public Transform Bone;
        
        private void SetBoneName()
        {
            if (Bone != null)
            {
                BoneName = Bone.name;
                Bone = null;
            }
        }
    }
}