using System;

namespace CGL
{
    [Serializable]
    public class SaveData
    {
        // general
        public int Score = 0;
        public int Level = 0;
        public bool isUseMusic = true;
        public bool isUseAllSound = true;


        // settings
        public TranslationData.Language language = TranslationData.Language.Ru;
    }
}