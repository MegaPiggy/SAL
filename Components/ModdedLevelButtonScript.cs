using SALT;
using SALT.Extensions;
using SALT.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModdedLevelButtonScript : LevelButtonScript
{
    internal static List<ModdedLevelButtonScript> buttons = new List<ModdedLevelButtonScript>();

    public ModdedLevelButtonScript()
    {
        level = -1;
        levelName = string.Empty;
    }

    protected virtual void Test()
    {
        SALT.Console.Console.LogError("MLBS");
    }

    internal CheckpointScript Checkpoint
    {
        get
        {
            if (this == null)
            {
                if ((object)this != null)
                    SALT.Console.Console.LogError("Native Object is not alive!");
                return null;
            }
            if (this.gameObject == null)
                return null;
            return GetComponentInChildren<CheckpointScript>();
        }
    }

    internal Transform Spawnpoint
    {
        get
        {
            if (spawnPoint == null)
            {
                CheckpointScript checkpoint = Checkpoint;
                if (checkpoint != null)
                    return checkpoint.transform;
                else
                    return null;
            }
            return spawnPoint;
        }
    }

    public override void Awake()
    {
        if (levelEnum != Levels.DONT_DESTROY_ON_LOAD)
        {
            Callbacks.OnMainMenuLoaded += OnMainMenuLoaded;
            Callbacks.OnLevelLoaded += OnLevelLoaded;
            UpdateLevelStuff();
        }
        if (!buttons.Contains(mlbs => mlbs == this))
            buttons.Add(this);
    }

    public virtual void OnEnable()
    {
        if (levelEnum != Levels.DONT_DESTROY_ON_LOAD)
            UpdateLevelStuff();
        if (!buttons.Contains(mlbs => mlbs == this))
            buttons.Add(this);
    }

    public virtual void OnDisable()
    {
        levelDataLoaded = false;
    }

    public virtual void OnMainMenuLoaded()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnLevelLoaded()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnDestroy()
    {
        if (levelEnum != Levels.DONT_DESTROY_ON_LOAD)
        {
            Callbacks.OnMainMenuLoaded -= OnMainMenuLoaded;
            Callbacks.OnLevelLoaded -= OnLevelLoaded;
        }

        int indexOf = -1;
        int index = 0;
        foreach (ModdedLevelButtonScript button in buttons)
        {
            if (button == this)
                indexOf = index;
            index++;
        }
        if (indexOf != -1)
            buttons.RemoveAt(indexOf);
    }

    private void UpdateLevelStuff()
    {
        level = (int)levelEnum;
        levelName = levelEnum.ToSceneName();
        string name = levelNameTextOverride.IsNullOrWhiteSpace() ? levelEnum.ToTitle(true) : levelNameTextOverride;
        if (levelNameText != null)
        {
            levelNameText.text = name;
            if (levelNameTextLanguage == null) levelNameTextLanguage = levelNameText.GetComponent<TextLanguageScript>();
        }
        if (levelNameTextLanguage != null)
        {
            if (levelNameTextOverrideJA.IsNullOrWhiteSpace())
                levelNameTextLanguage.Edit(name, name.ToKatakana());
            else
                levelNameTextLanguage.Edit(name, levelNameTextOverrideJA);
        }
    }

    public override void Start()
    {
        if (levelEnum != Levels.DONT_DESTROY_ON_LOAD)
            UpdateLevelStuff();
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        levelAnim = GetComponent<Animator>();
        buttonPlane = new Plane(transform.right, transform.position);
        if (spawnPoint == null)
        {
            CheckpointScript checkpoint = GetComponentInChildren<CheckpointScript>();
            if (checkpoint != null)
                spawnPoint = checkpoint.transform;
        }
        if (!LevelManager.levelManager.spawnPoints.ContainsKey(levelName))
            LevelManager.levelManager.spawnPoints.Add(levelName, spawnPoint);
        else
            LevelManager.levelManager.spawnPoints[levelName] = spawnPoint;
        LoadLevelData();
        SetTimeText();
    }

    private new void SetTimeText()
    {
        if (bestTimeText != null && MainScript.saveData.levelData.ContainsKey(levelName))
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(MainScript.saveData.levelData[levelName].bestTime);
            string str = string.Format("{0:00}:{1:00}.{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            if (MainScript.language == Language.Japanese)
                bestTimeText.text = "ベストタイム: " + str;
            else
                bestTimeText.text = "Best time: " + str;
        }
        else if (bestTimeText != null)
            bestTimeText.text = "";
        currentLanguage = MainScript.language;
    }

    private new void LoadLevelData()
    {
        if (levelDataText == null)
            return;
        if (MainScript.saveData.levelData.ContainsKey(levelName))
        {
            LevelSaveData levelSaveData = MainScript.saveData.levelData[levelName];
            if (levelSaveData.caseClosed)
            {
                completion = "CASE CLOSED";
                bubbaRend.enabled = false;
                stacheRend.enabled = false;
                skullRend.enabled = false;
            }
            else
            {
                completion = "";
                bubbaRend.enabled = true;
                stacheRend.enabled = true;
                skullRend.enabled = true;
                Color color = new Color(0, 0, 0, 0.3f);
                if (levelSaveData.zeroDeaths)
                    skullRend.sprite = skullSprite;
                else
                    skullRend.color = color;
                if (levelSaveData.allBubbas)
                    bubbaRend.sprite = bubbaSprite;
                else
                    bubbaRend.color = color;
                if (levelSaveData.stacheQuota)
                    stacheRend.sprite = stacheSprite;
                else
                    stacheRend.color = color;
            }
        }
        else
        {
            completion = "";
            bubbaRend.enabled = false;
            stacheRend.enabled = false;
            skullRend.enabled = false;
        }
        levelDataText.text = completion;
    }

    private new void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer != 8 || Vector2.Angle(-PlayerScript.player.gravDir, collision.contacts[0].normal) <= 135)
            return;
        levelTimer = levelTimerMax;
    }

    public override void Update()
    {
        if (!levelDataLoaded)
        {
            LoadLevelData();
            levelDataLoaded = true;
        }
        bool hasDataText = levelDataText != null;
        Vector2 b = MainScript.victory ? (PlayerScript.player.gravDir * 10) : ((startPos - rb.position) * 10);
        rb.velocity = Vector2.Lerp(rb.velocity, b, Time.deltaTime * 4);
        if (currentLanguage != MainScript.language)
            SetTimeText();
        if (levelTimer > 0)
        {
            levelAnim.SetTrigger("Show");
            levelAnim.ResetTrigger("Hide");
            levelTimer = Mathf.MoveTowards(levelTimer, -1, Time.deltaTime);
        }
        else
        {
            levelAnim.SetTrigger("Hide");
            levelAnim.ResetTrigger("Show");
        }
    }

    private new void FixedUpdate()
    {
        transform.position = buttonPlane.ClosestPointOnPlane(transform.position);
        float degrees = Vector2.SignedAngle(transform.up, Vector2.up);
        if (Tools.RotateV2(transform.position - Tools.V2toV3(startPos), degrees).y <= 0)
            return;
        rb.position = startPos;
        rb.velocity *= -1;
    }

    public string levelNameTextOverride = string.Empty;
    public string levelNameTextOverrideJA = string.Empty;
    public SALT.Level levelEnum = Levels.DONT_DESTROY_ON_LOAD;
    public TextMeshProUGUI levelNameText;
    public TextLanguageScript levelNameTextLanguage;

    public override void Pound()
    {
        MainScript.spawnPoint = spawnPoint.position;
        MainScript.lastLevelName = levelName;
        if (levelEnum.IsModded())
            SceneUtils.LoadModdedScene(levelEnum);
        else
            LevelLoader.loader.LoadLevel((int)levelEnum);
    }
}

public class ModdedLevelClearButton : ModdedLevelButtonScript
{
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI bubbaText;
    public TextMeshProUGUI stacheText;
    public GameObject deathsGlass;
    public GameObject bubbaGlass;
    public GameObject stacheGlass;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI caseClosedText;
    public AudioClip bloopSound;
    public AudioClip stampSound;
    public SpriteRenderer[] bubbaSprites;
    public Sprite bubbaCollected;
    public Sprite bubbaNotCollected;
    private bool victory;
    private Animator anim;
    private AudioSource aSource;
    private bool zeroDeaths;
    private bool allBubbas;
    private bool stacheQuota;
    private bool levelDataSaved = false;
    private float songEnd;
    public bool deathsGlassCompleted
    {
        get => deathsGlass.GetComponentInChildren<Image>().color != Color.black;
        set
        {
            if (value)
            {
                deathsGlass.GetComponentInChildren<Image>().color = Color.yellow;
                deathsGlass.GetComponent<Animator>().SetInteger("Completed", 1);
            }
            else
            {
                deathsGlass.GetComponentInChildren<Image>().color = Color.black;
                deathsGlass.GetComponent<Animator>().SetInteger("Completed", -1);
            }
        }
    }
    public bool bubbaGlassCompleted
    {
        get => bubbaGlass.GetComponentInChildren<Image>().color != Color.black;
        set
        {
            if (value)
            {
                bubbaGlass.GetComponentInChildren<Image>().color = Color.yellow;
                bubbaGlass.GetComponent<Animator>().SetInteger("Completed", 1);
            }
            else
            {
                bubbaGlass.GetComponentInChildren<Image>().color = Color.black;
                bubbaGlass.GetComponent<Animator>().SetInteger("Completed", -1);
            }
        }
    }
    public bool stacheGlassCompleted
    {
        get => stacheGlass.GetComponentInChildren<Image>().color != Color.black;
        set
        {
            if (value)
            {
                stacheGlass.GetComponentInChildren<Image>().color = Color.yellow;
                stacheGlass.GetComponent<Animator>().SetInteger("Completed", 1);
            }
            else
            {
                stacheGlass.GetComponentInChildren<Image>().color = Color.black;
                stacheGlass.GetComponent<Animator>().SetInteger("Completed", -1);
            }
        }
    }

    public ModdedLevelClearButton()
    {
        levelEnum = SALT.Level.MAIN_MENU;
        level = 0;
        levelName = "LevelSelect";
    }

    protected override void Test()
    {
        SALT.Console.Console.LogError("MLCB");
    }

    public override void Awake()
    {
    }

    public override void OnEnable()
    {

    }

    public override void OnDisable()
    {

    }

    public override void OnMainMenuLoaded()
    {
        gameObject.SetActive(false);
    }

    public override void OnLevelLoaded()
    {
        gameObject.SetActive(true);
    }

    public override void OnDestroy()
    {

    }

    public override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        aSource = GetComponent<AudioSource>();
    }

    public override void Update()
    {
        try
        {
            if (MusicLooper.looper == null)
            {
                SALT.Console.Console.LogWarning("Looper is null");
                base.Update();
                if (victory && !MainScript.loading)
                {
                    LevelLoader.loader.LoadLevel(level);
                    UnityEngine.Object.Destroy(this);
                }
                goto al;
            }
            if (songEnd == 0)
            {
                if (MusicLooper.looper.victoryMusic == null)
                {
                    //SALT.Console.Console.LogWarning("victoryMusic is null");
                    songEnd = 3;
                }
                else
                    songEnd = MusicLooper.looper.victoryMusic.length;
            }
            base.Update();
            //SALT.Console.Console.LogWarning(victory ? "Victory" : "Defeat");
            //SALT.Console.Console.LogWarning(MusicLooper.looper.MusicPlaying() ? "MusicPlaying" : "NoMusicPlaying");
            //SALT.Console.Console.LogWarning(MainScript.loading ? "loading" : "not loading");
            //SALT.Console.Console.LogWarning(Time.time.ToString() + " > " + songEnd);
            if (victory && !MusicLooper.looper.MusicPlaying() && !MainScript.loading && Time.time > songEnd)
            {
                LevelLoader.loader.LoadLevel(level);
                UnityEngine.Object.Destroy(this);
            }
            al:
            if (!victory)
                return;
            TimeSpan timeSpan = TimeSpan.FromSeconds(MainScript.main.levelTime);
            timeText.text = string.Format("{0:00}:{1:00}.{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            if (MainScript.language == Language.Japanese)
                deathsText.text = "デス数: " + LevelManager.levelManager.deaths;
            else
                deathsText.text = "Deaths: " + LevelManager.levelManager.deaths;
            zeroDeaths = true;
            if (LevelManager.levelManager.deaths != 0)
                zeroDeaths = false;
            if (MainScript.language == Language.Japanese)
                bubbaText.text = "Bubba: ";
            else
                bubbaText.text = "Bubba: ";
            allBubbas = true;
            for (int index = 0; index < LevelManager.levelManager.bubbaTokens.Length; ++index)
            {
                if (LevelManager.levelManager.bubbaTokens[index])
                {
                    bubbaSprites[index].sprite = bubbaCollected;
                }
                else
                {
                    bubbaSprites[index].sprite = bubbaNotCollected;
                    allBubbas = false;
                }
            }
            if (MainScript.language == Language.Japanese)
                stacheText.text = "集めたヒゲの数: " + LevelManager.levelManager.collectedMoustaches + "/" + LevelManager.levelManager.moustacheQuotaInt;
            else
                stacheText.text = "Stache Quota: " + LevelManager.levelManager.collectedMoustaches + "/" + LevelManager.levelManager.moustacheQuotaInt;
            stacheQuota = true;
            if (LevelManager.levelManager.collectedMoustaches < LevelManager.levelManager.moustacheQuotaInt)
                stacheQuota = false;
            bool _caseClosed = false;
            if (zeroDeaths && allBubbas && stacheQuota)
                _caseClosed = true;
            else
                caseClosedText.text = "";
            if (levelDataSaved)
                return;
            if (MainScript.saveData.levelData.ContainsKey(MainScript.lastLevelName))
            {
                LevelSaveData levelSaveData = MainScript.saveData.levelData[MainScript.lastLevelName];
                if (allBubbas)
                    levelSaveData.allBubbas = true;
                if (zeroDeaths)
                    levelSaveData.zeroDeaths = true;
                if (stacheQuota)
                    levelSaveData.stacheQuota = true;
                if (_caseClosed)
                    levelSaveData.caseClosed = true;
                if (levelSaveData.bestTime <= 0 || MainScript.main.levelTime < levelSaveData.bestTime)
                    levelSaveData.bestTime = MainScript.main.levelTime;
                MainScript.saveData.levelData[MainScript.lastLevelName] = levelSaveData;
            }
            else
            {
                LevelSaveData levelSaveData = new LevelSaveData(MainScript.lastLevelName, stacheQuota, allBubbas, zeroDeaths, _caseClosed, MainScript.main.levelTime);
                MainScript.saveData.levelData.Add(MainScript.lastLevelName, levelSaveData);
            }
            SaveScript.SaveSaveData();
            levelDataSaved = true;
        }
        catch (Exception ex)
        {
            SALT.Console.Console.LogException(ex);
        }
    }

    public override void Pound()
    {
        if (victory)
            return;
        anim.SetTrigger("Victory");
        victory = true;
        songEnd += Time.time;
        MainScript.victory = true;
        MusicLooper.looper.SetVictory();
    }

    private void PlayBloop(float pitch)
    {
        aSource.Stop();
        aSource.clip = bloopSound;
        aSource.pitch = pitch;
        aSource.Play();
    }

    public void SetDeathsGlass()
    {
        SALT.Console.Console.LogError("SetDeathsGlass");
        if (zeroDeaths)
        {
            deathsGlassCompleted = true;
            PlayBloop(0.7f);
        }
        else
            deathsGlassCompleted = false;
    }

    public void SetBubbaGlass()
    {
        SALT.Console.Console.LogError("SetBubbaGlass");
        if (allBubbas)
        {
            bubbaGlassCompleted = true;
            PlayBloop(0.85f);
        }
        else
            bubbaGlassCompleted = false;
    }

    public void SetStacheGlass()
    {
        SALT.Console.Console.LogError("SetStacheGlass");
        if (stacheQuota)
        {
            stacheGlassCompleted = true;
            PlayBloop(1);
        }
        else
            stacheGlassCompleted = false;
    }

    public void SetCaseClosed()
    {
        SALT.Console.Console.LogError("SetCaseClosed");
        if (zeroDeaths && allBubbas && stacheQuota)
        {
            caseClosedText.text = "CASE CLOSED";
            aSource.Stop();
            aSource.clip = stampSound;
            aSource.pitch = 1;
            aSource.Play();
        }
        else
            caseClosedText.text = "";
    }
}