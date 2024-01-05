using UnityEngine;

namespace GPU_Skinning.Runtime
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Model;

        private void Awake()
        {
            Camera.main.transform.position = new Vector3(90.2143402f, 13.1285477f, 117.776497f);
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(13.9589987f, -160f, 0.0339996368f));
        }

        private void Start()
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    var go = Instantiate(Model, new Vector3(i, 0f, j), Quaternion.identity);
                    go.SetActive(true);
                }
            }
        }
    }
}