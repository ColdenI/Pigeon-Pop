using UnityEngine;

namespace CGL
{
    [ExecuteAlways]
    public class SaveLogicTool : MonoBehaviour
    {
        [SerializeField] private SaveData Data;
        [SerializeField, Space(10)] private bool IsLoadDataOnAwake = true;

        private void Awake()
        {
            SaveLogic.OnLoad += SaveLogic_OnLoad;

            if (IsLoadDataOnAwake) SaveLogic.Load();
        }

        private void OnDisable()
        {
            SaveLogic.OnLoad -= SaveLogic_OnLoad;
        }

        [SerializeField, ContextMenu("Data Reset")]
        private void Button_ResetData()
        {
            SaveLogic.Reset();
        }

        [SerializeField, ContextMenu("Data Load")]
        private void Button_LoadData()
        {
            SaveLogic.Load();
        }
        
        [SerializeField, ContextMenu("Data Save")]
        private void Button_SaveData()
        {
            SaveLogic.Save(Data);
        }

        private void SaveLogic_OnLoad(SaveData saveData)
        {
            Data = SaveLogic.SaveData;
        }
    }
}
