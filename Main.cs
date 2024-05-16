using SALT.Extensions;
using SALT.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

namespace SALT
{
    /// <summary>
    /// The entry point for SALT
    /// </summary>
    public static class Main
    {
        /// <summary>
        /// The current version of SALT
        /// </summary>
        public const string Version = "1.4";

        private const string RELEASE_URL = "https://github.com/MegaPiggy/SALT/releases/tag/";
        public static string ReleaseURL => RELEASE_URL + GithubVersion;
        private const string GITHUB_URL = "https://raw.githubusercontent.com/MegaPiggy/SALT/main/version";

        public static Version CurrentVersion => new Version(1, 4);

        public static Version GithubVersion
        {
            get
            {
                try
                {
                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        string s = client.DownloadString("https://raw.githubusercontent.com/MegaPiggy/SALT/main/version");
                        return new Version(s);
                    }
                }
                catch
                {
                    return CurrentVersion;
                }
            }
        }

        public static bool CompareVersions() => CurrentVersion < GithubVersion;

        public static bool CheckIfNeedsNewest()
        {
            if (CurrentVersion < GithubVersion)
            {
                System.Diagnostics.Process.Start(ReleaseURL);
                return true;
            }
            return false;
        }

        private static string NewLine { get => System.Environment.NewLine + "  "; }

        private static bool isPreInitialized;
        private static bool isInitialized;
        private static bool isPostInitialized;
        internal static Assembly execAssembly => Assembly.GetExecutingAssembly();

        internal static Transform prefabParent;

        /// <summary>
        /// The <see cref="MainScript"/>'s object
        /// </summary>
        public static GameObject context { get; internal set; }
        /// <summary>
        /// The <see cref="MainScript"/> for Smol Ame
        /// </summary>
        public static MainScript mainScript => MainScript.main != null ? MainScript.main : (Main.context != null ? Main.context.GetComponent<MainScript>() : null);

        /// <summary>
        /// The old options script from before pause menu
        /// </summary>
        [Obsolete("Please use characterOption instead.")]
        public static OptionsScript options { get; internal set; }
        /// <summary>
        /// The character change option in the pause menu
        /// </summary>
        public static CharacterOption characterOption { get; internal set; }

        /// <summary>
        /// The current player
        /// </summary>
        public static GameObject player { get; internal set; }
        /// <summary>
        /// The <see cref="PlayerScript"/> for the current player
        /// </summary>
        public static PlayerScript actualPlayer => PlayerScript.player != null ? PlayerScript.player : (Main.player != null ? Main.player.GetComponent<PlayerScript>() : null);

        internal static void NextCharacter()
        {
            int num = (MainScript.currentCharacter + 1) % actualPlayer.characterPacks.Count;
            SetCharacter(num);
        }

        internal static void LastCharacter()
        {
            int num = MainScript.currentCharacter;
            --num;
            if (num < 0)
                num = actualPlayer.characterPacks.Count - 1;
            SetCharacter(num);
        }

        internal static void SetCharacter(int num)
        {
            MainScript.currentCharacter = num;
            PlayerPrefs.SetInt("character", num);
            actualPlayer.currentCharacter = num;
            if (characterOption != null)
            {
                characterOption.po.currentSelection = MainScript.currentCharacter;
                characterOption.charSprite.sprite = characterOption.charIcons[MainScript.currentCharacter];
            }
            //if (Main.options != null)
            //    Main.options.charSpriteRend.sprite = Main.options.charSprites[MainScript.currentCharacter];
            //if (actualPlayer.GetComponent<Rigidbody2D>().velocity.magnitude <= 0.25f)
            //    actualPlayer.SpawnCharacter(num);
        }

        private static void AddCallbacks()
        {
            Callbacks.OnMainMenuLoaded += MainMenu;
            Callbacks.OnLevelLoaded += Level;
        }

        private static Dictionary<int, string> layerNames = new Dictionary<int, string>();
        /// <summary>
        /// Every layer name in Smol Ame
        /// </summary>
        public static Dictionary<int, string> LayerNames => layerNames;
        internal static Dictionary<int, string> sceneNames = new Dictionary<int, string>();

        internal static AssetBundle LoadAssetbundle(string name)
        {
            Stream stream = execAssembly.GetManifestResourceStream(typeof(Main), "Resources." + name);
            if (stream == null)
                throw new Exception("AssetBundle " + name + " was not found");
            return AssetBundle.LoadFromStream(stream);
        }

        //internal static AssetBundle _atwriter = LoadAssetbundle("atwriter");
        //public static TMP_FontAsset atwriter = _atwriter.LoadAsset<TMP_FontAsset>("atwriter SDF");

        //internal static AssetBundle liberationsans = LoadAssetbundle("liberationsans");
        //public static TMP_FontAsset LiberationSans = liberationsans.LoadAsset<TMP_FontAsset>("LiberationSans SDF");

        internal static AssetBundle game = LoadAssetbundle("game");
        public static AudioClip hicBloop = game.LoadAsset<AudioClip>("hicBloop");
        public static Sprite bubba_charIcon = game.LoadAsset<Sprite>("bubba_charIcon");
        public static Sprite bubba_outline = game.LoadAsset<Sprite>("bubba_outline");
        public static Sprite bubba_outline2 = game.LoadAsset<Sprite>("bubba_outline2");
        public static TMP_FontAsset atwriter_Red = game.LoadAsset<TMP_FontAsset>("atwriter SDF Red");
        public static Material atwriter_RedMat => atwriter_Red.material;
        public static TMP_FontAsset Typo = game.LoadAsset<TMP_FontAsset>("Typo SDF");
        public static Material TypoMat => Typo.material;
        public static GameObject DeathsGlassEmpty = game.LoadAsset<GameObject>("DeathsGlassEmpty");
        public static GameObject BubbaGlassEmpty = game.LoadAsset<GameObject>("BubbaGlassEmpty");
        public static GameObject StacheGlassEmpty = game.LoadAsset<GameObject>("StacheGlassEmpty");
        public static GameObject Bubba1 = game.LoadAsset<GameObject>("Bubba1");
        public static RuntimeAnimatorController glassAnim = game.LoadAsset<RuntimeAnimatorController>("glassAnim");
        public static RuntimeAnimatorController levelClearButton_anim = game.LoadAsset<RuntimeAnimatorController>("levelClearButton_anim");

        internal static AssetBundle notoserifjp = LoadAssetbundle("notoserifjp");
        public static TMP_FontAsset NotoSerifJP_Bold = notoserifjp.LoadAsset<TMP_FontAsset>("NotoSerifJP-Bold SDF");

        internal static AssetBundle GUIPack = LoadAssetbundle("SystemGUI.pack");
        public static GUISkin GUISkin = GUIPack.LoadAsset<GUISkin>("guiSkin");

        static Main()
        {
            if (atwriter_Red == null)
                Console.Console.LogWarning("atwriter_Red is null");
            else
                Console.Console.LogError(atwriter_Red.name + " [" + atwriter_Red.GetType() + "]");
            if (Typo == null)
                Console.Console.LogWarning("Typo is null");
            else
                Console.Console.LogError(Typo.name + " [" + Typo.GetType() + "]");
            if (NotoSerifJP_Bold == null)
                Console.Console.LogWarning("NotoSerifJP_Bold is null");
            else
                Console.Console.LogError(NotoSerifJP_Bold.name + " [" + NotoSerifJP_Bold.GetType() + "]");
        }

        internal static void PreLoad()
        {
            if (Main.isPreInitialized)
                return;
            Main.isPreInitialized = true;
            Debug.Log("SALT has successfully invaded the game!");
            EntryPoint.Main();
            prefabParent = new GameObject("PrefabParent").transform;
            prefabParent.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(prefabParent.gameObject);
            for (int i = 0; i < 32; i++)
                layerNames.Add(i, LayerMask.LayerToName(i));
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
                sceneNames.Add(i, SceneManager.GetSceneByBuildIndex(i).name);
            foreach (System.Type type in execAssembly.GetTypes())
            {
                try
                {
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                catch (Exception e)
                {
                    string message = e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace();
                    Debug.LogError(message);
                    UI.ErrorUI.CreateError(message);
                }
            }
            HarmonyPatcher.PatchAll();
            AddCallbacks();
            try
            {
                ModLoader.InitializeMods();
            }
            catch (Exception e)
            {
                string message = e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace();
                Debug.LogError(message);
                UI.ErrorUI.CreateError(message);
                return;
            }
            FileLogger.Init();
            Console.Console.Init();
            HarmonyOverrideHandler.PatchAll();
            EntryPoint.IntializeInternalServices();
            try
            {
                ModLoader.PreLoadMods();
            }
            catch (Exception e)
            {
                string message = e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace();
                Debug.LogError(message);
                UI.ErrorUI.CreateError(message);
                return;
            }
            MethodInfo start = typeof(MainScript).GetInstanceMethod(nameof(MainScript.Start));
            MethodInfo callbacks = typeof(Callbacks).GetStaticMethod(nameof(Callbacks.OnLoad));
            MethodInfo load = typeof(Main).GetStaticMethod(nameof(Load));
#if POST
            MethodInfo postload = typeof(Main).GetStaticMethod(nameof(PostLoad));
#endif
            HarmonyPatcher.Instance.Patch(start, postfix: new HarmonyMethod(callbacks));
            HarmonyPatcher.Instance.Patch(start, new HarmonyMethod(load)
#if POST
                , new HarmonyMethod(postload));
#else
                );
#endif
            CheckIfNeedsNewest();
        }

        internal static GameObject watermark = null;
        internal static GameObject timer = null;

        private static void Load()
        {
            if (Main.isInitialized)
                return;
            Main.isInitialized = true;
            Console.KeyBindManager.ReadBinds();
            mainScript.AddComponent<UserInputService>();
            mainScript.AddComponent<ModManager>();
            //mainScript.AddComponent<MemoryStatsScript>();
            Console.KeyBindManager.Init();
            try
            {
                ModLoader.LoadMods();
            }
            catch (Exception e)
            {
                string message = e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace();
                Debug.LogError(message);
                UI.ErrorUI.CreateError(message);
                return;
            }
#if !POST
            Main.PostLoad();
#endif
        }

        private static void PostLoad()
        {
            if (Main.isPostInitialized)
                return;
            Main.isPostInitialized = true;
            try
            {
                ModLoader.PostLoadMods();
            }
            catch (Exception e)
            {
                string message = e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace();
                Debug.LogError(message);
                UI.ErrorUI.CreateError(message);
            }

            // Clears all the temporary memory
            GC.Collect();
        }

        internal static void ReLoad()
        {
            try
            {
                ModLoader.ReLoadMods();
            }
            catch (Exception e)
            {
                Debug.LogError(e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace());
            }
        }

        internal static void UnLoad()
        {
            try
            {
                ModLoader.UnLoadMods();
            }
            catch (Exception e)
            {
                Debug.LogError(e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace());
            }
        }

        internal static void Update()
        {
            try
            {
                ModLoader.UpdateMods();
            }
            catch (Exception e)
            {
                Debug.LogError(e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace());
            }
        }

        internal static void FixedUpdate()
        {
            try
            {
                ModLoader.UpdateModsFixed();
            }
            catch (Exception e)
            {
                Debug.LogError(e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace());
            }
        }

        internal static void LateUpdate()
        {
            try
            {
                ModLoader.UpdateModsLate();
            }
            catch (Exception e)
            {
                Debug.LogError(e.GetType() == typeof(Exception) ? e.Message : e.ParseTrace());
            }
        }
        
        internal static Collider2D CreatePlayerCollider()
        {
            var fakePlayer = new GameObject("fakePlayer");
            fakePlayer.layer = 16;
            return fakePlayer.AddComponent<BoxCollider2D>();
        }

        /// <summary>
        /// Whether the player can save thier score when the current level is completed
        /// </summary>
        public static bool SavesEnabled => ModLoader.AllowSaves && !SaveScript.stopSave;

        /// <summary>
        /// Disable saving for the current level
        /// </summary>
        public static void StopSave()
        {
            SaveScript.stopSave = true;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
        }

        private static void SetFonts()
        {
            TextMeshProUGUI[] textMeshes = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            TMP_SubMeshUI[] subMeshes = UnityEngine.Object.FindObjectsOfType<TMP_SubMeshUI>();

            foreach (TextMeshProUGUI textMesh in textMeshes)
            {
                var fallbackTable = textMesh.font.fallbackFontAssetTable ?? textMesh.font.m_FallbackFontAssetTable;
                if (fallbackTable != null && fallbackTable.Contains(font => font.name.StartsWith("NotoSerifJP")) && fallbackTable.Count == 1)
                    fallbackTable.Add(NotoSerifJP_Bold);
                //if (textMesh.font.name.StartsWith("atwriter"))
                //    textMesh.font = atwriter;
                //if (textMesh.font.name.StartsWith("LiberationSans"))
                //    textMesh.font = LiberationSans;
                if (textMesh.font.name.StartsWith("NotoSerifJP"))
                    textMesh.font = NotoSerifJP_Bold;
            }
            foreach (TMP_SubMeshUI subMesh in subMeshes)
            {
                var fallbackTable = subMesh.fontAsset.fallbackFontAssetTable ?? subMesh.fontAsset.m_FallbackFontAssetTable;
                if (fallbackTable != null && fallbackTable.Contains(font => font.name.StartsWith("NotoSerifJP")) && fallbackTable.Count == 1)
                    fallbackTable.Add(NotoSerifJP_Bold);
                //if (subMesh.fontAsset.name.StartsWith("atwriter"))
                //    subMesh.fontAsset = atwriter;
                //if (subMesh.fontAsset.name.StartsWith("LiberationSans"))
                //    subMesh.fontAsset = LiberationSans;
                if (subMesh.fontAsset.name.StartsWith("NotoSerifJP"))
                    subMesh.fontAsset = NotoSerifJP_Bold;
            }
        }

        private static void Level()
        {
            SaveScript.stopSave = false;

            SetFonts();

            //var callie = SAObjects.GetRootGameObject("Callie");
            //if (callie != null)
            //    callie.SetActive(true);
        }

        private const string SubMesh = "TMP SubMeshUI [atwriter SDF Material + LiberationSans SDF Atlas]";
        private const string SubMeshN = "TMP SubMeshUI [atwriter SDF Material + NotoSerifJP-Bold_fallback Atlas]";
        private const string SubObject = "TMP UI SubObject [atwriter SDF Material]";


        private static void MainMenu()
        {
            SaveScript.stopSave = false;

            SetFonts();

            try
            {
                Transform versionTransform = UnityEngine.Object.FindObjectsOfType<RectTransform>().FirstOrDefault(tmp => tmp.gameObject.name == "Version");
                GameObject versionObject = versionTransform != null ? versionTransform.gameObject : null;

                if (versionObject == null)
                    goto end;

                if (watermark == null)
                {
                    GameObject rootWatermark = UnityObjectUtils.GetDontDestroyOnLoadRootGameObject("Watermark");
                    if (rootWatermark != null)
                    {
                        watermark = rootWatermark;
                        goto time;
                    }
                    GameObject speedrunWatermark = versionObject.Instantiate();
                    speedrunWatermark.name = "Watermark";
                    versionObject.transform.parent.gameObject.AddChild(speedrunWatermark, false);
                    RectTransform vRT = versionObject.GetComponent<RectTransform>();
                    RectTransform spwtRT = speedrunWatermark.GetComponent<RectTransform>().GetCopyOf(vRT);
                    spwtRT.localPosition = vRT.localPosition;
                    spwtRT.localScale = vRT.localScale;
                    spwtRT.anchoredPosition = vRT.anchoredPosition;
                    spwtRT.sizeDelta = vRT.sizeDelta;
                    spwtRT.offsetMin = vRT.offsetMin;
                    spwtRT.offsetMax = vRT.offsetMax;
                    spwtRT.anchoredPosition3D = vRT.anchoredPosition3D;
                    spwtRT.SetSiblingIndex(vRT.GetSiblingIndex() + 1);
                    spwtRT.localPosition += new Vector3(0, 12.5f, 0);
                    speedrunWatermark.GetComponent<TextMeshProUGUI>().text = typeof(Main).Namespace + " - v" + Version;
                    spwtRT.name = "Watermark";
                    watermark = speedrunWatermark;
                }

            time:

                if (timer == null)
                {
                    GameObject rootTimer = UnityObjectUtils.GetDontDestroyOnLoadRootGameObject("Timer");
                    if (rootTimer != null)
                    {
                        timer = rootTimer;
                        goto end;
                    }
                    GameObject timerObject = versionObject.Instantiate();
                    timerObject.name = "Timer";
                    versionObject.transform.parent.gameObject.AddChild(timerObject, false);
                    RectTransform vRT = versionObject.GetComponent<RectTransform>();
                    RectTransform tmrRT = timerObject.GetComponent<RectTransform>().GetCopyOf(vRT);
                    tmrRT.SetPivotAndAnchors(new Vector2(1f, 1f));
                    tmrRT.localPosition = vRT.localPosition;
                    tmrRT.localScale = vRT.localScale;
                    tmrRT.anchoredPosition = vRT.anchoredPosition;
                    tmrRT.sizeDelta = vRT.sizeDelta;
                    tmrRT.offsetMin = vRT.offsetMin;
                    tmrRT.offsetMax = vRT.offsetMax;
                    tmrRT.anchoredPosition3D = vRT.anchoredPosition3D;
                    tmrRT.SetSiblingIndex(vRT.GetSiblingIndex() + 2);
                    tmrRT.localPosition = tmrRT.localPosition.SetY((-vRT.localPosition.y) + 4.5f);
                    var txt = timerObject.GetComponent<TextMeshProUGUI>();
                    txt.text = UI.TimerUI.defaultTime;
                    //txt.alignment = TextAlignmentOptions.TopRight;
                    timerObject.AddComponent<UI.TimerUI>();
                    tmrRT.name = "Timer";
                    timer = timerObject;
                }
            end:
                ;
            }
            catch (Exception e)
            {
                Console.Console.LogException(e);
            }

            try
            {
                GameObject finalUpdate = UnityObjectUtils.GetRootGameObject("FinalUpdateEmpty");
                GameObject canvas = finalUpdate.FindChild("Canvas");
                GameObject text = canvas.FindChild("Text (TMP)");
                TextMeshProUGUI TMP = text.GetComponent<TextMeshProUGUI>();
                if (text.GetComponent<TextLanguageScript>() == null)
                    text.AddComponent<TextLanguageScript>().Edit(TMP.text, TMP.text.Replace("The Final Update", "ファイナルアップデート").Replace("Coming... Eventually", "いずれは"));
            }
            catch (Exception e)
            {
                Console.Console.LogException(e);
            }

            try
            {
                if (UnityObjectUtils.GetRootGameObject("ModContainer") != null)
                    return;
                GameObject optionsContainer = UnityObjectUtils.GetRootGameObject("Options");//UnityEngine.Object.FindObjectsOfType<TextArea>().FirstOrDefault(go => go.transform.parent.name == "CreditsEmpty").transform.root.gameObject;
                GameObject credits = optionsContainer.FindChild("Credits", true);

                GameObject gameBy = credits.FindChild("Text (TMP) (1)", true);
                TextMeshProUGUI gameByText = gameBy.GetComponent<TextMeshProUGUI>();
                TextLanguageScript gameByTextLanguage = gameBy.GetOrAddComponent<TextLanguageScript>();
                gameByTextLanguage.Edit(gameByText.text, "Kevin Stevensが作ったゲーム");//"ゲーム by Kevin Stevens");//Kevin Stevensによるゲーム");

                GameObject creditsList = credits.FindChild("Text (TMP) (2)", true);
                TextArea creditsArea = creditsList.GetComponent<TextArea>();
                //(creditsArea.GetEnglishText() + "|" + creditsArea.GetJapaneseText()).CopyToClipboard();

                string cText = creditsArea.GetEnglishText()
                                          .Replace("Ninomae Ina'nis BGM", "ensolarado")
                                          .Replace("Usada Pekora BGM", "Tamekichi no Bouken")
                                          .Replace("Amelia Watson BGM <", "Omocha no Dansu <");

                string cJaText = creditsArea.GetJapaneseText()
                                            .Replace("フリーBGM", "ensolarado")
                                            .Replace("兎田 ぺこら BGM", "たぬきちの冒険")
                                            .Replace("アメリア ワトソン BGM", "おもちゃのダンス")
                                            .Replace("Smol Ame sprite by Walfie", "Walfieが作ったスモール アメsprite")
                                            .Replace("Wiggly Coco sprite by BobberWCC", "BobberWCCが作った波打つココsprite")
                                            .Replace("Ollie sprite by byong", "byongが作ったオリーsprite")
                                            .Replace("Lofi Amelia Watson BGM Remix", "アメリア ワトソン BGM(ロフィリミックス)")
                                            .Replace("Amelia Watson Eurobeat Remix", "アメリア ワトソン BGM(ユーロビートリミックス)");

                creditsArea.Edit(cText, cJaText);

                GameObject container = new GameObject("ModContainer");
                container.transform.position = optionsContainer.transform.position;
                container.transform.rotation = optionsContainer.transform.rotation;
                container.transform.localScale = optionsContainer.transform.localScale;

                GameObject camOverride = optionsContainer.FindChild("CamOverride", true);
                GameObject modCamOverride = camOverride.Instantiate();
                modCamOverride.name = "CamOverride_Mods";
                modCamOverride.transform.SetParent(container.transform, true);
                modCamOverride.transform.localPosition = camOverride.transform.localPosition;
                modCamOverride.transform.localPosition += new Vector3(0, 2.7f, 0);
                modCamOverride.transform.localRotation = camOverride.transform.localRotation;
                modCamOverride.transform.localScale = camOverride.transform.localScale;

                GameObject modUI = credits.Instantiate();
                modUI.name = "Mods";
                modUI.transform.SetParent(container.transform, true);
                modUI.transform.localPosition = credits.transform.localPosition;
                modUI.transform.localRotation = credits.transform.localRotation;
                modUI.transform.localScale = credits.transform.localScale;

                GameObject modsTitle = modUI.FindChild("Text (TMP)", true);
                modsTitle.name = "modsTitle";
                TextLanguageScript textLanguageScript = modsTitle.GetComponent<TextLanguageScript>();
                //Console.Console.Log("<code>" + textLanguageScript.GetEnglishText() + "</code>");
                //Console.Console.Log("<code>" + textLanguageScript.GetJapaneseText() + "</code>");
                textLanguageScript.translations = new List<Translation>();
                textLanguageScript.Edit("SALT (Smol Ame Loader Thing)", "SALT (スモール アメ ローダー シング)");

                GameObject loaderCreator = modUI.FindChild("Text (TMP) (1)", true);
                loaderCreator.name = "LoaderCreator";
                TextMeshProUGUI loaderText = loaderCreator.GetComponent<TextMeshProUGUI>();
                TextLanguageScript loaderTextLanguage = loaderCreator.GetOrAddComponent<TextLanguageScript>();
                string oldText = loaderText.text;
                string enText = oldText.Replace("Game by Kevin Stevens", "Mod Loader by MegaPiggy");
                string jaText = oldText.Replace("Game by Kevin Stevens", "MegaPiggyが作ったModLoader");//"Modローダー by MegaPiggy");//"MegaPiggyによるModローダー");
                loaderText.text = enText;
                loaderTextLanguage.Edit(enText, jaText);

                GameObject modEmpty = modUI.FindChild("CreditsEmpty", true);
                modEmpty.name = "ModsEmpty";
                GameObject modList = modUI.FindChild("Text (TMP) (2)", true);
                modEmpty.name = "ModsList";

                TextArea modsArea = modList.GetComponent<TextArea>();
                modsArea.textBox = modList.GetComponent<TextMeshProUGUI>();
                modsArea.texts = new List<TextAssetTranslation>();
                string modText = $"<size=25>Press Ctrl+Tab to open up the command console.</size><size=5>{System.Environment.NewLine}{System.Environment.NewLine}</size><size=40>Mods:</size><size=10>{System.Environment.NewLine}{System.Environment.NewLine}</size>";
                string modJaText = $"<size=25>Ctrl キーを押したまま Tab キーを押し: コマンドターミナルが開きます</size><size=5>{System.Environment.NewLine}{System.Environment.NewLine}</size><size=40>Mods:</size><size=10>{System.Environment.NewLine}{System.Environment.NewLine}</size>";
                modsArea.Edit(modText, modJaText);
                modEmpty.AddComponent<UI.ModListUI>();

                //float moveToLeft = 16f;
                float moveDown = 11.5f;//8.8f;
                container.transform.position += new Vector3(0, -moveDown, 0);//-moveToLeft, 0, 0);
                //credits.DestroyImmediate();

                UnityObjectUtils.GetActiveRootGameObject("Book").Activate();
                UnityObjectUtils.GetActiveRootGameObject("Book (1)").Activate();
                UnityObjectUtils.GetActiveRootGameObject("Desk (2)").Activate();
                UnityObjectUtils.GetActiveRootGameObject("Desk (3)").Activate();
            }
            catch (Exception e)
            {
                Console.Console.LogException(e);
            }
        }

        private static void CopyComponentsTo(this GameObject source, GameObject destination)
        {
            if (source.HasComponent<RectTransform>() && destination.HasComponent<RectTransform>())
            {
                RectTransform stransform = source.GetComponent<RectTransform>();
                RectTransform dtransform = destination.GetComponent<RectTransform>();
                dtransform.anchorMin = stransform.anchorMin;
                dtransform.anchorMax = stransform.anchorMax;
                dtransform.offsetMin = stransform.offsetMin;
                dtransform.offsetMax = stransform.offsetMax;
                dtransform.pivot = stransform.pivot;
                dtransform.sizeDelta = stransform.sizeDelta;
                dtransform.anchoredPosition = stransform.anchoredPosition;
                dtransform.anchoredPosition3D = stransform.anchoredPosition3D;
                dtransform.position = stransform.position;
                dtransform.rotation = stransform.rotation;
                dtransform.localPosition = stransform.localPosition;
                dtransform.localRotation = stransform.localRotation;
                dtransform.localScale = stransform.localScale;
                dtransform.tag = stransform.tag;
                foreach (Component component in source.GetComponents())
                {
                    if (component == null)
                        continue;
                    Type type = component.GetType();
                    if (type == typeof(Transform) || type == typeof(RectTransform))
                        continue;
                    Component dcomponent = destination.GetComponent(type);
                    if (dcomponent == null)
                        continue;
                    foreach (FieldInfo field in type.GetInstanceFields(Accessibility.Public))
                    {
                        string lower = field.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (field.FieldType.IsSubclassOf(typeof(Component)) || field.FieldType == typeof(GameObject))
                            continue;
                        dcomponent.SetField(field.Name, component.GetField(field.Name));
                    }
                    foreach (FieldInfo field in type.GetInstanceFields(Accessibility.NonPublic))
                    {
                        if (!(field.GetCustomAttributes(typeof(SerializeField), false).Length > 0))
                            continue;
                        string lower = field.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (field.FieldType.IsSubclassOf(typeof(Component)) || field.FieldType == typeof(GameObject))
                            continue;
                        dcomponent.SetField(field.Name, component.GetField(field.Name));
                    }
                    foreach (PropertyInfo property in type.GetInstanceProperties(Accessibility.Public))
                    {
                        string lower = property.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (property.PropertyType.IsSubclassOf(typeof(Component)) || property.PropertyType == typeof(GameObject))
                            continue;
                        dcomponent.SetProperty(property.Name, component.GetProperty(property.Name));
                    }
                }
            }
            else
            {
                Transform stransform = source.GetComponent<Transform>();
                Transform dtransform = destination.GetComponent<Transform>();
                dtransform.position = stransform.position;
                dtransform.rotation = stransform.rotation;
                dtransform.localPosition = stransform.localPosition;
                dtransform.localRotation = stransform.localRotation;
                dtransform.localScale = stransform.localScale;
                dtransform.tag = stransform.tag;
                foreach (Component component in source.GetComponents())
                {
                    if (component == null)
                        continue;
                    Type type = component.GetType();
                    if (type == typeof(Transform) || type == typeof(RectTransform))
                        continue;
                    Component dcomponent = destination.GetComponent(type);
                    if (dcomponent == null)
                        continue;
                    foreach (FieldInfo field in type.GetInstanceFields(Accessibility.Public))
                    {
                        string lower = field.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (field.FieldType.IsSubclassOf(typeof(Component)) || field.FieldType == typeof(GameObject))
                            continue;
                        dcomponent.SetField(field.Name, component.GetField(field.Name));
                    }
                    foreach (FieldInfo field in type.GetInstanceFields(Accessibility.NonPublic))
                    {
                        if (!(field.GetCustomAttributes(typeof(SerializeField), false).Length > 0))
                            continue;
                        string lower = field.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (field.FieldType.IsSubclassOf(typeof(Component)) || field.FieldType == typeof(GameObject))
                            continue;
                        dcomponent.SetField(field.Name, component.GetField(field.Name));
                    }
                    foreach (PropertyInfo property in type.GetInstanceProperties(Accessibility.Public))
                    {
                        string lower = property.Name.ToLowerInvariant();
                        if (lower == "name" || lower == "transform" || lower == "canvas" || lower == "canvasRenderer" || lower == "gameObject")
                            continue;
                        if (property.PropertyType.IsSubclassOf(typeof(Component)) || property.PropertyType == typeof(GameObject))
                            continue;
                        dcomponent.SetProperty(property.Name, component.GetProperty(property.Name));
                    }
                }
            }
        }

        private static void MainMenuOld()
        {
            SaveScript.stopSave = false;

            GameObject versionObject = UnityEngine.Object.FindObjectsOfType<RectTransform>().FirstOrDefault(tmp => tmp.gameObject.name == "Version").gameObject;

            var waterMark = UnityEngine.Object.FindObjectsOfType<RectTransform>().FirstOrDefault(tmp => tmp.gameObject.name == "speedrunWatermark");
            if (waterMark == null)
            {
                GameObject speedrunWatermark = versionObject.Instantiate();
                speedrunWatermark.name = "speedrunWatermark";
                versionObject.transform.parent.gameObject.AddChild(speedrunWatermark, false);
                RectTransform vRT = versionObject.GetComponent<RectTransform>();
                RectTransform spwtRT = speedrunWatermark.GetComponent<RectTransform>().GetCopyOf(vRT);
                spwtRT.localPosition = vRT.localPosition;
                spwtRT.localScale = vRT.localScale;
                spwtRT.anchoredPosition = vRT.anchoredPosition;
                spwtRT.sizeDelta = vRT.sizeDelta;
                spwtRT.offsetMin = vRT.offsetMin;
                spwtRT.offsetMax = vRT.offsetMax;
                spwtRT.anchoredPosition3D = vRT.anchoredPosition3D;
                spwtRT.SetSiblingIndex(vRT.GetSiblingIndex() + 1);
                spwtRT.localPosition += new Vector3(0, 12.5f, 0);
                speedrunWatermark.GetComponent<TextMeshProUGUI>().text = typeof(Main).Namespace + " - v" + Version;
                watermark = speedrunWatermark;
            }
            else if (watermark == null)
                watermark = waterMark.gameObject;

            var Timer = UnityEngine.Object.FindObjectsOfType<RectTransform>().FirstOrDefault(tmp => tmp.gameObject.name == "timer");
            if (Timer == null)
            {
                GameObject timerObject = versionObject.Instantiate();
                timerObject.name = "timer";
                versionObject.transform.parent.gameObject.AddChild(timerObject, false);
                RectTransform vRT = versionObject.GetComponent<RectTransform>();
                RectTransform tmrRT = timerObject.GetComponent<RectTransform>().GetCopyOf(vRT);
                tmrRT.SetPivotAndAnchors(new Vector2(1f, 1f));
                tmrRT.localPosition = vRT.localPosition;
                tmrRT.localScale = vRT.localScale;
                tmrRT.anchoredPosition = vRT.anchoredPosition;
                tmrRT.sizeDelta = vRT.sizeDelta;
                tmrRT.offsetMin = vRT.offsetMin;
                tmrRT.offsetMax = vRT.offsetMax;
                tmrRT.anchoredPosition3D = vRT.anchoredPosition3D;
                tmrRT.SetSiblingIndex(vRT.GetSiblingIndex() + 2);
                tmrRT.localPosition = tmrRT.localPosition.SetY((-vRT.localPosition.y) + 4.5f);
                var txt = timerObject.GetComponent<TextMeshProUGUI>();
                txt.text = UI.TimerUI.defaultTime;
                //txt.alignment = TextAlignmentOptions.TopRight;
                timerObject.AddComponent<UI.TimerUI>();
                timer = timerObject;
            }
            else if (timer == null)
                timer = Timer.gameObject;

            if (UnityEngine.Object.FindObjectsOfType<TextArea>().FirstOrDefault(go => go.transform.parent.name == "ModsEmpty") != null)
                return;
            GameObject creditsObject = UnityEngine.Object.FindObjectsOfType<TextArea>().FirstOrDefault(go => go.transform.parent.name == "CreditsEmpty").transform.parent.gameObject;
            //TextArea creditsText = creditsObject.GetComponentInChildren<TextArea>();
            //creditsText.text.text.CopyToClipboard();
            //string text = creditsText.text.text.Replace("Smol Ame sprite by Walfie", "Smol Ame sprite by Walfie" + NewLine + "Mod Loader by MegaPiggy");
            //creditsText.Edit(text);
            GameObject creditsDesk = creditsObject.transform.root.gameObject;
            GameObject topDesk = creditsDesk.Instantiate();
            topDesk.name = "Desk2 (Mod)";
            topDesk.FindChild("Credits", true).Destroy();
            float moveToLeft = 16f;
            topDesk.transform.position += new Vector3(-moveToLeft, 8.76f, 0);
            GameObject desk = creditsDesk.Instantiate();
            desk.name = "Desk (Mod)";
            desk.transform.position += new Vector3(-moveToLeft, 0, 0);
            GameObject modUI = desk.FindChild("Credits", true);
            modUI.name = "Mods";
            GameObject modsTitle = modUI.FindChild("Text (TMP)", true);
            modsTitle.name = "modsTitle";
            TextLanguageScript textLanguageScript = modsTitle.GetComponent<TextLanguageScript>();
            textLanguageScript.Edit(textLanguageScript.GetEnglishText().Replace("Credits", "SALT (Smol Ame Loader Thing)"), textLanguageScript.GetEnglishText().Replace("Credits", "SALT (スモール アメ ローダー シング)"));
            GameObject loaderCreator = modUI.FindChild("Text (TMP) (1)", true);
            loaderCreator.name = "LoaderCreator";
            string oldText = loaderCreator.GetComponent<TextMeshProUGUI>().text;
            string enText = oldText.Replace("Game by Kevin Stevens", "Mod Loader by MegaPiggy");
            string jaText = oldText.Replace("Game by Kevin Stevens", "Modローダー by MegaPiggy");//"MegaPiggyによるModローダー");
            loaderCreator.GetComponent<TextMeshProUGUI>().text = enText;
            loaderCreator.AddComponent<TextLanguageScript>().Edit(enText, jaText);
            GameObject modList = desk.FindChild(creditsObject.name, true);
            modList.name = "ModsEmpty";
            modList.AddComponent<UI.ModListUI>();
            TextArea modsArea = modList.GetComponentInChildren<TextArea>();
            string modText = $"<size=25>Press Ctrl+Tab to open up the command console.</size><size=5>{System.Environment.NewLine}{System.Environment.NewLine}</size><size=40>Mods:</size><size=10>{System.Environment.NewLine}{System.Environment.NewLine}</size>";
            string modJaText = $"<size=25>CTRL+TABを押し: オープンcommand console</size><size=5>{System.Environment.NewLine}{System.Environment.NewLine}</size><size=40>Mods:</size><size=10>{System.Environment.NewLine}{System.Environment.NewLine}</size>";
            modsArea.Edit(modText, modJaText);

            GameObject bookObject = UnityEngine.Object.FindObjectsOfType<BookRandomColor>().FirstOrDefault(b => b.GetComponent<BouncyScript>().bounceFactor == 1.5f).gameObject;
            GameObject book = bookObject.Instantiate();
            book.name = "BookMod";
            book.transform.position = book.transform.position.SetXY(-43f, -12.1f);
            book.transform.localScale = bookObject.transform.localScale;
            book.AddComponent<NoRotation>();
            book.GetComponent<BouncyScript>().bounceFactor = 1.8f;

            //GameObject cheese = SAObjects.Cheese.Clone();
            //cheese.SetActive(true);
            //cheese.transform.position = new Vector3(10, 5);
        }
    }
}
