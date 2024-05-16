using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SALT
{
    public class VFXOption : MonoBehaviour
    {
        private PauseOption po;
        private Language currentLanguage;

        private void Start()
        {
            po = GetComponent<PauseOption>();
            SetStrings();
            po.currentSelection = PlayerPrefs.GetInt("vfx", 0);
            SetVFX();
        }

        private void Update()
        {
            if (currentLanguage == MainScript.language)
                return;
            SetStrings();
        }

        internal void SetStrings()
        {
            List<string> stringList = new List<string>();
            if (MainScript.language == Language.Japanese)
            {
                stringList.Add("オン");
                stringList.Add("オフ");
            }
            else
            {
                stringList.Add("On");
                stringList.Add("Off");
            }
            po.selectionStrings = stringList;
            currentLanguage = MainScript.language;
        }

        public void SetVFX()
        {
            Patches.PoundVFXPatch.SetEnabled(po.currentSelection == 0);
            PlayerPrefs.SetInt("vfx", po.currentSelection);
        }

        private void Awake()
        {
        }

        private void OnDestroy()
        {
        }
    }
}
