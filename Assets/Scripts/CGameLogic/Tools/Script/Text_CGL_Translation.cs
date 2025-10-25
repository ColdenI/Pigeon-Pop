using UnityEngine;

namespace CGL
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class Text_CGL_Translation : MonoBehaviour
    {
        public string key;
        [Space(10)]
        public bool isAutoLanguage = true;
        public TranslationData.Language Language = TranslationData.Language.En;

        private UnityEngine.UI.Text Text;


        private void Awake()
        {
            TranslationData.OnInitialized += TranslationData_OnChange;
            TranslationData.OnChangeLanguage += TranslationData_OnChange;

            Text = GetComponent<UnityEngine.UI.Text>();
        }

        private void OnDisable()
        {
            TranslationData.OnInitialized -= TranslationData_OnChange;
            TranslationData.OnChangeLanguage -= TranslationData_OnChange;
        }

        private void TranslationData_OnChange(TranslationData.Language language)
        {
            string _text;
            if(isAutoLanguage) _text = TranslationData.Get(key);
            else _text = TranslationData.Get(key, Language);

            Text.text = _text;
        }

    }
}