using System;
using System.Collections.Generic;

namespace CGL
{
    [Serializable]
    public static class TranslationData
    {
        /// <summary>
        /// Init function, to call in Start or in Awake
        /// </summary>
        private static void init()
        {
            new TranslationData_("t_record", "Рекорд", "High Score");
            new TranslationData_("t_score", "Счет", "Score");
            new TranslationData_("t_buy", "Купить", "Buy");
            new TranslationData_("t_apply", "Применить", "Apply");
            new TranslationData_("t_gameOver", "Поражение", "Game Over");
            new TranslationData_("t_start", "Нажмите Чтобы Начать", "Tap To Start");
            new TranslationData_("t_reset", "Вы уверены что хотите сбросить все данные игры?", "Are you sure you want to reset all game data?");

            IsInit = true;
        }

        #region Logic
        public static void Init() => Init(Language.Auto);
        public static void Init(Language _language)
        {
            if(_language == Language.Auto)
            {
                switch (UnityEngine.Application.systemLanguage)
                {
                    case UnityEngine.SystemLanguage.Russian: _language = Language.Ru; break;
                    case UnityEngine.SystemLanguage.English: _language = Language.En; break;
                    case UnityEngine.SystemLanguage.Spanish: _language = Language.Es; break;
                    case UnityEngine.SystemLanguage.French: _language = Language.Fr; break;
                    case UnityEngine.SystemLanguage.Turkish: _language = Language.Tr; break;

                    default: _language = Language.En; break;
                }

            }

            TranslationData._language = _language;
            init();
        }

        private static bool isInit = false;
        public static bool IsInit
        {
            get => isInit; private set
            {
                isInit = value;
                OnInitialized?.Invoke(language: _language);
            }
        }

        public delegate void onChangeLanguage(Language language);
        public static event onChangeLanguage OnChangeLanguage;
        public static event onChangeLanguage OnInitialized;

        internal static List<TranslationData_> data = new List<TranslationData_>();
        public static int counter = -1;
        private static Language _language = Language.Ru;
        public static Language language
        {
            get => _language; set
            {
                _language = value;
                OnChangeLanguage?.Invoke(value);
            }
        }

        private static string CheckData(string str, string key) => string.IsNullOrWhiteSpace(str) ? $"[no data - '{key}']" : str;

        public static string Get(int _index, Language _l)
        {
            if (!isInit) return "{TranslationData not initialized!}";
            switch (_l)
            {
                case Language.Ru: return CheckData(data[_index].Ru, data[_index].Key);
                case Language.En: return CheckData(data[_index].En, data[_index].Key);
                case Language.Es: return CheckData(data[_index].Es, data[_index].Key);
                case Language.Fr: return CheckData(data[_index].Fr, data[_index].Key);
                case Language.Tr: return CheckData(data[_index].Tr, data[_index].Key);

                default: return "{no language}";
            }
        }
        public static string Get(string _key, Language _l)
        {
            if (!isInit) return "{TranslationData not initialized!}";
            foreach (TranslationData_ i in data) if (i.Key == _key) return Get(i.Index, _l);
            return $"[no key - '{_key}']";
        }
        public static string Get(string _key)
        {
            return Get(_key, _language);
        }
        public static string Get(int _index)
        {
            return Get(_index, _language);
        }

        [Serializable]
        public enum Language
        {
            Auto,
            Ru,
            En,
            Es,
            Fr,
            Tr
        }
        #endregion
    }

    [Serializable]
    class TranslationData_
    {
        private TranslationData_(out int _index, string _key,
            string ru,
            string en,
            string es = null,
            string fr = null,
            string tr = null)
        {
            TranslationData.counter++;
            _index = TranslationData.counter;
            TranslationData.data.Add(new TranslationData_(_index, _key)
            {
                Ru = ru,
                En = en,
                Es = es,
                Fr = fr,
                Tr = tr
            });
        }
        internal TranslationData_(string _key, string ru, string en) : this(out int a, _key, ru, en) { }
        internal TranslationData_(string _key, string ru, string en, string es, string fr, string tr) : this(out int a, _key, ru, en, es, fr, tr) { }

        private TranslationData_(int _index, string _key) { index = _index; key = _key; }

        private int index;
        private string key;
        public int Index { get { return index; } }
        public string Key { get { return key; } }

        public string Ru;
        public string En;
        public string Es;
        public string Fr;
        public string Tr;
    }

}