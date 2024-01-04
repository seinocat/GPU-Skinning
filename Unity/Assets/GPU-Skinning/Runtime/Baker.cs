using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkinning.Runtime
{
    public class Baker : MonoBehaviour
    {
        public Animator Animator;

        private void Awake()
        {
            this.Animator = GetComponent<Animator>();
        }


        [Button("烘焙")]
        public void Bake()
        {

        }
    }
}