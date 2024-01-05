using UnityEngine;

namespace GPU_Skinning.Runtime
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Model;
        public int Row = 30;
        public int Col = 30;

        private void Awake()
        {
            Camera.main.transform.position = new Vector3(24.24898f, 4.329008f, 40.39124f);
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(13.9589987f, -160f, 0.0339996368f));
        }

        private void Start()
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    var go = Instantiate(Model, new Vector3(i, 0f, j), Quaternion.identity);
                    go.SetActive(true);
                }
            }
        }
    }
}