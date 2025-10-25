using UnityEngine;

namespace CGL
{
    public static class SaveLogic
    {
        [Header("Settings")]
        private static SaveData _saveData = new SaveData();
        public static SaveData SaveData { 
            get
            {
                if (!_isLoadData) Debug.LogWarning("The data has not been uploaded. The default object was passed.");
                return _saveData;
            }
            set => _saveData = value;
        }

        public delegate void onLoad(SaveData saveData);
        public static event onLoad OnLoad;

        private static bool _isLoadData = false;
        public static bool IsLoadData { get => _isLoadData; }

        #region Load
        public static void Load()
        {
#if PLATFORM_WEBGL
        Load_WebGL();
#endif
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
            Load_PlayerPrefs();
#endif

            if(!_isLoadData && _saveData != null) _isLoadData = true;
        }

        #region Load WebGL
#if PLATFORM_WEBGL
    public static void Load_WebGL()
    {
        InstantGamesBridge.Bridge.storage.Get("data", OnStorageGetCompleted);
    }

    private static void OnStorageGetCompleted(bool success, string data)
    {
        SaveData = new SaveData();

        // Загрузка произошла успешно
        if (success)
        {
            if (data != null)
            {
                SaveData = JsonUtility.FromJson<SaveData>(data);
            }
        }
        OnLoad?.Invoke(SaveData);

    }
#endif
        #endregion

        #region Load PlayerPrefs
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
        public static void Load_PlayerPrefs()
        {
            SaveData savaData = new SaveData();
            string json = PlayerPrefs.GetString("data", "none");
            if (json == "none") OnLoad?.Invoke(savaData);
            else
            {
                savaData = JsonUtility.FromJson<SaveData>(json);
                SaveData = savaData;
                OnLoad?.Invoke(savaData);
            }
        }
#endif
        #endregion
        #endregion

        #region Save
        public static void Save() => Save(_saveData);

        public static void Save(SaveData _saveData)
        {
#if PLATFORM_WEBGL
        Save_WebGL(_saveData);
#endif
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
            Save_PlayerPrefs(_saveData);
#endif
        }

        #region Save WebGL
#if PLATFORM_WEBGL
    public static void Save_WebGL(SaveData _saveData)
    {
        InstantGamesBridge.Bridge.storage.Set("data", JsonUtility.ToJson(_saveData), OnStorageSetCompleted);
    }

    private static void OnStorageSetCompleted(bool success)
    {
        Debug.Log($"OnStorageSetCompleted, success: {success}");
    }
#endif
        #endregion

        #region Save PlayerPrefs
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
        public static void Save_PlayerPrefs(SaveData _saveData)
        {
            PlayerPrefs.SetString("data", JsonUtility.ToJson(_saveData));
            PlayerPrefs.Save();
        }
#endif
        #endregion
        #endregion

        #region Reset
        public static void Reset()
        {
#if PLATFORM_WEBGL
        Reset_WebGL();
#endif
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
            Reset_PlayerPrefs();
#endif
        }

        #region Reset WebGL
#if PLATFORM_WEBGL
    public static void Reset_WebGL()
    {
        InstantGamesBridge.Bridge.storage.Delete("data", OnStorageDeleteCompleted);
    }

    private static void OnStorageDeleteCompleted(bool obj)
    {
        Save(new SaveData());
    }
#endif
        #endregion

        #region Reset PlayerPrefs
#if PLATFORM_ANDROID || PLATFORM_ARCH_64 || PLATFORM_STANDALONE_WIN
        public static void Reset_PlayerPrefs()
        {
            PlayerPrefs.SetString("data", JsonUtility.ToJson(new SaveData()));
            PlayerPrefs.Save();
        }
#endif
        #endregion
        #endregion
    }
}