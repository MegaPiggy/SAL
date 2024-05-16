using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SALT
{
    public class FramerateLockOption : MonoBehaviour
    {
        private PauseOption po;
        private Language currentLanguage;

        public void Start()
        {
            po = GetComponent<PauseOption>();
            SetStrings();
            po.currentSelection = PlayerPrefs.GetInt("framerateLock", 0);
            SetFramerateLock();
        }

        public void Update()
        {
            if (currentLanguage != MainScript.language)
            {
                SetStrings();
            }
        }

        public void SetStrings()
        {
            List<string> list = new List<string>();
            if (MainScript.language == Language.Japanese)
            {
                list.Add("垂直同期オン");
                list.Add("リフレッシュレート");
                list.Add("なし");
                list.Add("60fps");
                list.Add("100fps");
                list.Add("144fps");
            }
            else
            {
                list.Add("Vsync On");
                list.Add("Refresh Rate");
                list.Add("Uncapped");
                list.Add("60fps");
                list.Add("100fps");
                list.Add("144fps");
            }

            po.selectionStrings = list;
            currentLanguage = MainScript.language;
        }

        public void SetFramerateLock()
        {
            if (po.currentSelection == 0)
                MainScript.SetFramerateLock(-1);
            else if (po.currentSelection == 1)
                MainScript.SetFramerateLock(Screen.currentResolution.refreshRate);
            else if (po.currentSelection == 2)
                MainScript.SetFramerateLock(0);
            else if (po.currentSelection == 3)
                MainScript.SetFramerateLock(60);
            else if (po.currentSelection == 4)
                MainScript.SetFramerateLock(100);
            else if (po.currentSelection == 5)
                MainScript.SetFramerateLock(144);

            PlayerPrefs.SetInt("framerateLock", po.currentSelection);
        }

        public void Awake()
        {
        }

        public void OnDestroy()
        {
        }
    }
}
