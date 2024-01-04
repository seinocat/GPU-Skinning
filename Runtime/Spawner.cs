using System;
using UnityEngine;

namespace GPU_Skinning.Runtime
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Model;
        
        private void Start()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Instantiate(Model, new Vector3(i, 1.06f, j), Quaternion.identity);
                }
            }
        }
    }
}