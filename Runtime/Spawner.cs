using Sirenix.OdinInspector;
using UnityEngine;

namespace GPU_Skinning.Runtime
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Model;
        public Transform Root;
        
        [Button("生成")]
        public void Generate(int row, int col)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    var position = new Vector3(i, 0f, j);
                    var go = Instantiate(Model, Root);
                    go.transform.position = position;
                    go.SetActive(true);
                }
            }
        }
    }
}