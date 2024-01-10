using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GPU_Skinning.Runtime
{
    public class Options : MonoBehaviour
    {
        public RectTransform m_Options;
        public Button m_ShowBtn;
        public Button m_OkBtn;
        public Button m_HideBtn;
        public TMP_InputField m_InputField;
        public TMP_Dropdown m_Dropdown;
        public Slider m_Slider;

        public Spawner GameObjectSpw;
        public Spawner GpuSkinningSpw;
        public InstancingSpawner GpuSkinInstancing;
        public Transform Root;
        
        private Vector3 m_initPos = new Vector3(14.4f, 7.2f, 13.3f);

        private void Awake()
        {
            Application.targetFrameRate = 200;
            m_ShowBtn.onClick.AddListener(OnShowBtnClick);
            m_OkBtn.onClick.AddListener(OnOkBtnClick);
            m_HideBtn.onClick.AddListener(OnHideBtnClick);
            m_Slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void Start()
        {
            Camera.main.transform.position = m_initPos;
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(16.365f, -129.969f, 0.034f));
            m_InputField.text = "80";
        }

        private void OnShowBtnClick()
        {
            m_ShowBtn.gameObject.SetActive(false);
            m_Options.gameObject.SetActive(true);
        }

        private void OnHideBtnClick()
        {
            m_ShowBtn.gameObject.SetActive(true);
            m_Options.gameObject.SetActive(false);
        }

        private void OnOkBtnClick()
        {
            Reset();
            int count = int.Parse(m_InputField.text);
            int type = m_Dropdown.value;
            switch (type)
            {
                case 0 : 
                    GameObjectSpw.Generate(count, count);
                    break;
                case 1 : 
                    GpuSkinningSpw.Generate(count, count);
                    break;
                case 2 : 
                    GpuSkinInstancing.Generate(count, count);
                    break;
            }
        }

        private void Reset()
        {
            GpuSkinInstancing.Stop();
            int count = Root.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Destroy(Root.GetChild(i).gameObject);
            }
            
            GC.Collect();
        }

        private void OnSliderValueChanged(float value)
        {
            Camera.main.transform.position = m_initPos - Camera.main.transform.forward * value * 200;
        }
    }
}