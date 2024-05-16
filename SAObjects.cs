using System.Collections.Generic;
using System.Linq;
using SALT.Extensions;
using SALT.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;

namespace SALT
{
	/// <summary>
	/// A class that can access all objects within the game
	/// </summary>
	public static class SAObjects
	{
		public static GameObject OptionPrefab;
		public static GameObject DeathZone;
		public static GameObject Block;
		public static GameObject Book;
		public static GameObject Button;
		public static GameObject Brick;
		public static GameObject Carrot;
		public static GameObject CloudSolid;
		public static GameObject CloudSpawner;
		public static GameObject Desk;
		public static GameObject Folder;
		public static GameObject LevelButton;
		public static GameObject LevelClearButton;
		public static GameObject ModdedLevelButton;
		public static GameObject Moon;
		public static GameObject NukeButton;
		public static GameObject GenericButton;
		public static GameObject Spawnpoint;
		public static GameObject Sandwich;
		public static GameObject GravityOverride;
		public static GameObject Tako;
		public static GameObject BubbaToken;
		public static GameObject VFX_sparkle_small;
		public static GameObject VFX_sparkle_small_pink;
		public static GameObject VFX_sparkle_big;

		internal static Material Skybox;
		//internal static Material Skybox2;
		internal static GameObject MainLevelStuff;
		internal static GameObject Player;

		//internal static GameObject Cheese;

		static GameObject CreateEmpty(string name)
		{
			GameObject newGO = new GameObject(name);
			GameObjectUtils.Prefabitize(newGO);
			return newGO;
		}

		static GameObject GetResource(string name)
		{
			GameObject[] roots = Resources.FindObjectsOfTypeAll<GameObject>();
			GameObject selected = roots.FirstOrDefault(gameObject => gameObject.name == name);
			roots.ForEach(gameObject => Console.Console.Log(gameObject.name, true));
			//GameObject selected = scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.name == name);
			//if (selected != null)
			//{
			//	bool previous = selected.activeSelf;
			//	selected.SetActive(false);
			//	GameObject ret = PrefabUtils.CopyPrefab(selected);
			//	selected.SetActive(previous);
			//	return ret;
			//}
			return CreateEmpty(name);//selected != null ? PrefabUtils.CopyPrefab(selected) : CreateEmpty(name);
		}

		public static GameObject CreateMainLevelStuff() => MainLevelStuff.InstantiateInactive(true);

		private static void GetMainStuff()
		{
			MainLevelStuff = CreateEmpty("MainLevelStuff");
		//	MainLevelStuff = GetRoot(scene, "MainLevelStuff");
			MainLevelStuff.SetActive(false);
		//	MainLevelStuff.FindChild("Player").DestroyImmediate();
		//	//MainLevelStuff.RemoveComponentImmediate<RotateSkybox>();
			LevelManager lvlManager = MainLevelStuff.AddComponent<LevelManager>();
		//	LevelManager lvlManager = MainLevelStuff.GetOrAddComponent<LevelManager>();
			lvlManager.moustacheQuota = 0.8f;
			lvlManager.moustacheQuotaInt = 213;
			lvlManager.bubbaTokens = new bool[3] { false, false, false };
			lvlManager.totalMoustaches = 266;
			lvlManager.collectedMoustaches = 0;
			lvlManager.collectedMoustachesSaved = 0;
			lvlManager.collectedMoustachePercent = 0;
			lvlManager.deaths = 0;
			lvlManager.spawnPoints = new Dictionary<string, Transform> { { "", null } };

			GameObject MusicCycle = new GameObject("MusicCycle");
			MusicCycle.transform.parent = MainLevelStuff.transform;
			MusicCycleScript musicCycle = MusicCycle.AddComponent<MusicCycleScript>();
			musicCycle.numNotes = 8;
			musicCycle.samplesPerBeat = 15395;
			musicCycle.bpm = 120;
			musicCycle.timePerBeat = 0.5f;
			CycleManager cm = MusicCycle.AddComponent<CycleManager>();
			musicCycle.cm = cm;

			GameObject CamTarget = new GameObject("CamTarget");
			CamTarget.transform.parent = MainLevelStuff.transform;
			CamTarget.transform.position = new Vector3(-2.2f, 5.6f, 5.5f);
			GameObject CamLookPos = new GameObject("CamLookPos");
			CamLookPos.transform.parent = MainLevelStuff.transform;
			CamLookPos.transform.position = new Vector3(-2.1f, 8.1f, 4.7f);
			GameObject CamLookTarget = new GameObject("CamLookTarget");
			CamLookTarget.transform.parent = MainLevelStuff.transform;
			CamLookTarget.transform.position = new Vector3(-2.1f, 5.5f, 4.5f);
			GameObject CamLookTarget1 = new GameObject("CamLookTarget1");
			CamLookTarget1.transform.parent = CamLookTarget.transform;
			CamLookTarget1.transform.localPosition = new Vector3(0, 2.5f, 0);
			GameObject TransitionPlane = new GameObject("TransitionPlane");

			GameObject CamRig = new GameObject("CamRig");
			CamRig.transform.parent = MainLevelStuff.transform;
			CamScript camScript = CamRig.AddComponent<CamScript>();
			camScript.camSpeed = 20;
			camScript.camLookSpeed = 20;
			camScript.camAcc = 2;
			camScript.lookAcc = 3;
			camScript.lookPos = CamLookPos.transform;
			camScript.targetPos = CamTarget.transform;
			camScript.targetLook = CamLookTarget1.transform;
			camScript.transQuad = TransitionPlane.transform; // TransitionPlane
			camScript.snap = false;
			GameObject MainCamera = new GameObject("Main Camera");
			camScript.cam = MainCamera.transform;
			MainCamera.tag = "MainCamera";
			MainCamera.transform.parent = CamRig.transform;
			MainCamera.transform.localPosition = new Vector3(0, 2, -10);
			MainCamera.transform.localEulerAngles = new Vector3(360, 360, 0);
			Camera camera = MainCamera.AddComponent<Camera>();
			camera.nearClipPlane = 0.3f;
			camera.farClipPlane = 100;
			camera.fieldOfView = 60;
			camera.renderingPath = RenderingPath.UsePlayerSettings;
			camera.allowHDR = true;
			camera.allowMSAA = true;
			camera.allowDynamicResolution = false;
			camera.forceIntoRenderTexture = false;
			camera.orthographicSize = 4.9f;
			camera.orthographic = false;
			camera.opaqueSortMode = OpaqueSortMode.Default;
			camera.transparencySortMode = TransparencySortMode.Default;
			camera.transparencySortAxis = new Vector3Int(0,0,1);
			camera.depth = -1;
			camera.aspect = 96f/54f;
			camera.cullingMask = 410935;
			camera.eventMask = -1;
			camera.layerCullSpherical = false;
			camera.cameraType = UnityEngine.CameraType.Game;
			camera.overrideSceneCullingMask = 0;
			camera.layerCullDistances = 0f.MultiArray(32);
			camera.useOcclusionCulling = true;
			camera.cullingMatrix = new Matrix4x4(new Vector4(0.97428f, 0, 0, -0.04868f),
				new Vector4(0, 1.73013f, -0.08152f, 0.88f),
				new Vector4(0, 0.04709f, 0.99949f, 9.44088f),
				new Vector4(0, 0.04706f, 0.99889f, 10.03503f));
			camera.backgroundColor = new Color(0.078f, 0.110f, 0.145f, 0);
			camera.clearFlags = CameraClearFlags.Skybox;
			camera.depthTextureMode = DepthTextureMode.None;
			camera.clearStencilAfterLightingPass = false;
			camera.usePhysicalProperties = false;
			camera.sensorSize = new Vector2Int(36, 24);
			camera.lensShift = Vector2Int.zero;
			camera.focalLength = 50;
			camera.gateFit = Camera.GateFitMode.Horizontal;
			camera.rect = new Rect(0, 0, 1, 1);
			camera.pixelRect = new Rect(0, 0, 1920, 1080);
			camera.targetTexture = null;
			camera.targetDisplay = 0;
			camera.stereoSeparation = 0.022f;
			camera.stereoConvergence = 10;
			camera.stereoTargetEye = StereoTargetEyeMask.Both;
			AudioListener listener = MainCamera.AddComponent<AudioListener>();
			listener.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
			AudioSource source = MainCamera.AddComponent<AudioSource>();
			musicCycle.musicSource = source;
			source.volume = 0.3f;
			source.pitch = 1;
			AudioClip clip = Sinusoid.Clip;
			source.clip = clip;
			source.loop = false;
			source.ignoreListenerVolume = false;
			source.playOnAwake = false;
			source.ignoreListenerPause = false;
			source.spatialize = false;
			source.spatializePostEffects = false;
			source.bypassEffects = false;
			source.bypassListenerEffects = false;
			source.bypassReverbZones = false;
			source.mute = false;
			source.panStereo = 0;
			source.spatialBlend = 0;
			source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
			source.reverbZoneMix = 1;
			source.dopplerLevel = 1;
			source.spread = 0;
			source.priority = 128;
			source.minDistance = 1;
			source.maxDistance = 500;
			source.rolloffMode = AudioRolloffMode.Logarithmic;
			MusicLooper musicLooper = MainCamera.AddComponent<MusicLooper>();
			//	//GameObject MainCamera = MainLevelStuff.FindChild("CamRig").FindChild("Main Camera");
			//	//AudioSource audioSource = MainCamera.GetComponent<AudioSource>();
			//	//audioSource.volume = 0.15f;
			//	//MusicLooper musicLooper = MainCamera.GetComponent<MusicLooper>();
			musicLooper.musicTracks = new List<MusicTrack> { new MusicTrack { clip = clip, nextTrack = 0 } };
			//AltMusicScript altMusic = MainCamera.AddComponent<AltMusicScript>();
			//altMusic.track = 2;
			//altMusic.volume = 0.15f;
			//altMusic.probability = 0.25f;
			TransitionPlane.transform.parent = MainCamera.transform;
			TransitionPlane.transform.localPosition = new Vector3(0, 0, 0.4f);
			TransitionPlane.layer = LayerMask.NameToLayer("Transition");
			TransitionPlane.AddComponent<MeshFilter>().mesh = SAObjects.Get<Mesh>("Quad");
			TransitionPlane.AddComponent<MeshRenderer>().material = SAObjects.Get<Material>("ClockTransitionMat");

			GameObject EventSystem = new GameObject("EventSystem");
			GameObject Pointer = new GameObject("Pointer");
			Pointer.transform.parent = MainLevelStuff.transform;
			Pointer.transform.localPosition = new Vector3(-4, 4, 0);
			Pointer.SetActive(false);
			GameObject Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Cube.name = "Cube";
			Cube.transform.parent = Pointer.transform;
			Cube.transform.localPosition = new Vector3(0, 0.5f, 0);

			GameObject Checkpoint = new GameObject("Checkpoint");
			Checkpoint.transform.parent = MainLevelStuff.transform;
			Checkpoint.layer = LayerMask.NameToLayer("Trigger");
			CheckpointScript checkpointScript = Checkpoint.AddComponent<CheckpointScript>();
			checkpointScript.isStartCheckpoint = true;
			checkpointScript.destructible = true;
			checkpointScript.currentColor = new Color(0.623f, 0.623f, 0.623f, 1);
			BoxCollider2D bc2 = Checkpoint.AddComponent<BoxCollider2D>();
			bc2.size = new Vector2(1.8f, 1.7f);
			bc2.offset = new Vector2(0, 1.6f);
			bc2.isTrigger = true;

			GameObject GroundNormalEmpty = new GameObject("GroundNormalEmpty");
			GroundNormalEmpty.transform.parent = MainLevelStuff.transform;
			Rigidbody2D rigidbody2D = GroundNormalEmpty.AddComponent<Rigidbody2D>();
			rigidbody2D.position = new Vector2(-8.6f, -1.7f);

			GameObject RotEmpty = new GameObject("RotEmpty");
			RotEmpty.transform.parent = MainLevelStuff.transform;
			RotEmpty.AddComponent<Rigidbody2D>();

			GameObject DirectionalLight = new GameObject("Directional Light");
			DirectionalLight.transform.parent = MainLevelStuff.transform;
			DirectionalLight.transform.position = new Vector3Int(0, 3, 0);
			DirectionalLight.transform.eulerAngles = new Vector3Int(50, 330, 0);
			DirectionalLight.AddComponent<AutoUnparent>();
			Light light = DirectionalLight.AddComponent<Light>();
			light.type = LightType.Directional;
			light.spotAngle = 30;
			light.innerSpotAngle = 21.80208f;
			light.color = new Color(0.271f, 0.579f, 0.695f, 1);
			light.colorTemperature = 6570;
			light.intensity = 1.5f;
			light.bounceIntensity = 1;
			light.useBoundingSphereOverride = false;
			light.shadowCustomResolution = -1;
			light.shadowBias = 0.05f;
			light.shadowNormalBias = 0.4f;
			light.shadowNearPlane = 0.2f;
			light.useShadowMatrixOverride = false;
			light.range = 10;
			light.cullingMask = 1335;
			light.shadows = LightShadows.Soft;
			light.shadowStrength = 1;
			light.cookieSize = 10;
		}

		public static GameObject CreatePlayer() => Player.InstantiateInactive(true);

		public static ModdedLevelButtonScript CreateLevelButtonOnMainMenu(Level level, Transform parent, Vector3 localPosition)
		{
			var button = SceneUtils.GetSceneByLevel(SALT.Level.MAIN_MENU).InstantiateInactive(SAObjects.ModdedLevelButton, true);
			button.name = level.ToTitle() + "Button";
			button.transform.parent = parent;
			button.transform.localPosition = localPosition;
			var mlbs = button.GetComponent<ModdedLevelButtonScript>();
			mlbs.levelEnum = level;
			button.SetActive(true);
			return mlbs;
		}

		private static void GetPlayer()
		{
			LayerMask playerMask = LayerMask.NameToLayer("Player");
			LayerMask tsMask = LayerMask.NameToLayer("TriggerSensor");
			Player = CreateEmpty("Player");
			Player.layer = playerMask;
			Player.SetActive(false);
			Animator animator = Player.AddComponent<Animator>();

			PlayerScript playerScript = Player.AddComponent<PlayerScript>();
			playerScript.characterPacks = Registries.CharacterRegistry.GetVanillaPrefabs();
			playerScript.currentState = PlayerState.Loading;
			playerScript.footstepSound = SAObjects.Get<AudioClip>("deskTap1");
			playerScript.squishSound = SAObjects.Get<AudioClip>("hic7");
			playerScript.hicBounce = SAObjects.Get<AudioClip>("hicbounce");
			playerScript.groundPoundSound = SAObjects.Get<AudioClip>("hic2");
			playerScript.camMoveSpeed = 1.1f;
			playerScript.camLookSpeed = 1.2f;
			playerScript.maxSpeed = 6;
			playerScript.acceleration = 15;
			playerScript.bounceMoveSens = 3;
			playerScript.gravity = 18;
			playerScript.fallGravity = 28;
			playerScript.bounceGravity = 28;
			playerScript.jumpStrength = 9.5f;
			playerScript.bounceStrength = 16;
			playerScript.canJump = false;
			playerScript.groundPoundFall = false;
			playerScript.groundPoundSpeed = -100;
			playerScript.groundPoundingAnim = true;
			playerScript.canGroundPound = false;
			playerScript.grounded = 0.05f;
			playerScript.bubbaSens = 0.25f;

			Rigidbody2D rigidbody2D = Player.AddComponent<Rigidbody2D>();
			rigidbody2D.rotation = 0;
			rigidbody2D.gravityScale = 0;
			rigidbody2D.centerOfMass = new Vector2(0,0.6f);
			rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			SpriteRenderer spriteRenderer = Player.AddComponent<SpriteRenderer>();
			spriteRenderer.size = new Vector2(5.3f, 7.3f);
			AudioSource source = Player.AddComponent<AudioSource>();
			source.playOnAwake = false;
			CanvasGroup group = Player.AddComponent<CanvasGroup>();
			CompositeCollider2D coc2D = Player.AddComponent<CompositeCollider2D>();
			coc2D.edgeRadius = 0.06f;
			coc2D.sharedMaterial = SAObjects.Get<PhysicsMaterial2D>("DefaultPhysMat");

			GameObject Colliders = new GameObject("Colliders");
			Colliders.layer = playerMask;
			Colliders.transform.parent = Player.transform;
			CapsuleCollider2D cac2D = Colliders.AddComponent<CapsuleCollider2D>();
			cac2D.size = new Vector2(0.3f, 1.1f);
			cac2D.offset = new Vector2(-0.3f, 0.6f);

			GameObject Colliders1 = new GameObject("Colliders1");
			Colliders1.layer = playerMask;
			Colliders1.transform.parent = Colliders.transform;
			Colliders1.SetActive(false);

			GameObject Colliders2 = new GameObject("Colliders2");
			Colliders2.layer = playerMask;
			Colliders2.transform.parent = Colliders.transform;
			Colliders2.SetActive(false);

			GameObject Colliders3 = new GameObject("Colliders3");
			Colliders3.layer = playerMask;
			Colliders3.transform.parent = Colliders.transform;
			Colliders3.SetActive(false);

			GameObject Colliders4 = new GameObject("Colliders4");
			Colliders4.layer = playerMask;
			Colliders4.transform.parent = Colliders.transform;
			Colliders4.SetActive(false);

			GameObject Colliders5 = new GameObject("Colliders5");
			Colliders5.layer = playerMask;
			Colliders5.transform.parent = Colliders.transform;
			Colliders5.SetActive(false);

			GameObject TriggerSensor = new GameObject("TriggerSensor");
			TriggerSensor.layer = tsMask;
			TriggerSensor.transform.parent = Colliders.transform;
			TriggerSensor.transform.localPosition = new Vector3(0, 0.564f, 0);
			BoxCollider2D trigger = TriggerSensor.AddComponent<BoxCollider2D>();
			trigger.isTrigger = true;
			trigger.size = Vector2.one;

			BoxCollider2D bc2D = Player.AddComponent<BoxCollider2D>();
			bc2D.edgeRadius = 0.06f;
			bc2D.size = new Vector2(0.4f, 1);
			bc2D.offset = new Vector2(0, 0.6f);

			GameObject bsp = new GameObject("BlobShadowProjector");
			bsp.transform.parent = Player.transform;
			bsp.transform.localPosition = new Vector3(0, 0.19f, 0);
			bsp.transform.localEulerAngles = new Vector3(90, 0, 0);
			Projector projector = bsp.AddComponent<Projector>();
			projector.nearClipPlane = -0.25f;
			projector.farClipPlane = 1.200647f;
			projector.fieldOfView = 8.85f;
			projector.aspectRatio = 1;
			projector.orthographic = true;
			projector.orthographicSize = 0.5f;
			projector.ignoreLayers = 0;
			projector.material = SAObjects.Get<Material>("ShadowProjector");

			playerScript.blobShadow = projector;

			GameObject fas = new GameObject("FootAudioSource");
			fas.layer = playerMask;
			fas.transform.parent = Player.transform;
			AudioSource fasource = fas.AddComponent<AudioSource>();
			fasource.volume = 0.8f;
			fasource.pitch = 1;
			fasource.playOnAwake = false;
			fasource.clip = SAObjects.Get<AudioClip>("deskTap1");

			playerScript.footASource = fasource;

			playerScript.poundVFXprefab = SAObjects.Get<GameObject>("PoundVFX");

			Player.transform.hierarchyCapacity = 28;
		}

		private static void GetPlayer2()
		{
			Player = GetResource("Player");
		}


		// Static Constructors to load the base objects
#if !OBJ
		static SAObjects()
		{
			DeathZone = GetInstInactive("DeathZone");
			DeathZone.name = "DeathZone";
			DeathZone.Prefabitize();
			Block = GetInstInactive("Block");
			Block.name = "Block";
			Block.Prefabitize();
			Book = GetInstInactive("Book");
			Book.name = "Book";
			Book.Prefabitize();
			Brick = CreateEmpty("Brick");
			var b = GetInstInactive("brick");
			b.name = "brick";
			var bc = GetInstInactive("BrickCollider");
			bc.name = "BrickCollider";
			Brick.AddChild(b);
			Brick.AddChild(bc);
			Brick.SetActive(false);
			b.SetActive(true);
			bc.SetActive(true);
			Button = GetInstInactive("Button", go => go.transform.parent.name != "Button");
			Button.name = "Button";
			Button.Prefabitize();
			Carrot = GetInstInactive("CarrotPlatform");
			Carrot.name = "CarrotPlatform";
			Carrot.Prefabitize();
			CloudSolid = GetInstInactive("CloudPlatform_Solid");
			CloudSolid.name = "CloudPlatform_Solid";
			CloudSolid.Prefabitize();
			CloudSpawner = GetInstInactive("CloudPlatformSpawner");
			CloudSpawner.name = "CloudPlatformSpawner";
			CloudSpawner.Prefabitize();
			Desk = GetInstInactive("Desk (1)");
			Desk.name = "Desk";
			Desk.Prefabitize();
			Folder = GetInstInactive("folder");
			Folder.name = "folder";
			Folder.Prefabitize();
			LevelButton = GetInstInactive("LevelButton");
			LevelClearButton = GetInstInactive("LevelButton");
			ModdedLevelButton = GetInstInactive("LevelButton");
			LevelButton.name = "LevelButton";
			LevelClearButton.name = "LevelClearButton";
			ModdedLevelButton.name = "ModdedLevelButton";
			LevelButton.Prefabitize();
			LevelClearButton.Prefabitize();
			ModdedLevelButton.Prefabitize();
			Rigidbody2D rb = LevelClearButton.GetComponent<Rigidbody2D>();
			rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
			BouncyScript bounce = LevelClearButton.GetComponent<BouncyScript>();
			bounce.bounceFactor = 1;
			bounce.springiness = 5;
			bounce.springinessAcc = 2;
			bounce.rb = rb;
			LevelButtonScript lcbs = LevelClearButton.GetComponent<LevelButtonScript>();
			ModdedLevelClearButton mlcbs = LevelClearButton.AddComponent<ModdedLevelClearButton>();
			mlcbs.bestTimeText = null;
			mlcbs.bubbaOutline = lcbs.bubbaOutline;
			mlcbs.bubbaRend = null;
			mlcbs.bubbaSprite = lcbs.bubbaSprite;
			mlcbs.buttonPlane = lcbs.buttonPlane;
			mlcbs.completion = lcbs.completion;
			mlcbs.currentLanguage = lcbs.currentLanguage;
			mlcbs.levelEnum = Level.MAIN_MENU;
			mlcbs.level = 0;
			mlcbs.levelAnim = lcbs.levelAnim;
			mlcbs.levelDataLoaded = lcbs.levelDataLoaded;
			mlcbs.levelDataText = null;
			mlcbs.levelName = "LevelSelect";
			mlcbs.levelTimer = lcbs.levelTimer;
			mlcbs.levelTimerMax = lcbs.levelTimerMax;
			mlcbs.rb = lcbs.rb;
			mlcbs.skullOutline = lcbs.skullOutline;
			mlcbs.skullRend = null;
			mlcbs.skullSprite = lcbs.skullSprite;
			mlcbs.spawnPoint = null;
			mlcbs.stacheOutline = lcbs.stacheOutline;
			mlcbs.stacheRend = null;
			mlcbs.stacheSprite = lcbs.stacheSprite;
			mlcbs.startPos = lcbs.startPos;
			lcbs.DestroyImmediate();
			mlcbs.bloopSound = Main.hicBloop;//Get<AudioClip>("hicBloop")
			mlcbs.stampSound = Get<AudioClip>("deskTap1");
			mlcbs.bubbaCollected = Main.bubba_charIcon;//Get<Sprite>("bubba_charIcon");
			mlcbs.bubbaNotCollected = Main.bubba_outline;//StreamExtensions.CreateSpriteFromImage("bubba_outline");// Get<Sprite>("bubba_outline2");
			mlcbs.deathsGlass = Main.DeathsGlassEmpty;//new GameObject("DeathsGlassEmpty", typeof(RectTransform), typeof(Animator));
			//GameObject Glass = new GameObject("Glass", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			//Glass.GetComponent<RectTransform>().offsetMin = new Vector2Int(-50, -50);
			//Glass.GetComponent<RectTransform>().offsetMax = new Vector2Int(50, 50);
			//Glass.GetComponent<RectTransform>().sizeDelta = new Vector2Int(100, 100);
			//Glass.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
			//Glass.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
			//Glass.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
			//Glass.GetComponent<Image>().sprite = StreamExtensions.CreateSpriteFromImage("magnifying_glass");//Get<Sprite>("magnifying_glass");
			//Glass.GetComponent<Image>().fillMethod = Image.FillMethod.Radial360;
			//Glass.GetComponent<Image>().fillCenter = true;
			//Glass.GetComponent<Image>().fillClockwise = true;
			//Glass.GetComponent<Image>().fillAmount = 1;
			//Glass.GetComponent<Image>().fillOrigin = 0;
			//Glass.GetComponent<Image>().type = Image.Type.Simple;
			//Glass.GetComponent<Image>().preserveAspect = true;
			//Glass.GetComponent<Image>().color = Color.black;
			//Glass.GetComponent<RectTransform>().parent = mlcbs.deathsGlass.transform;
			mlcbs.bubbaGlass = Main.BubbaGlassEmpty;//new GameObject("BubbaGlassEmpty", typeof(RectTransform), typeof(Animator));
			//GameObject bg = Glass.Instantiate(true);
			//bg.GetComponent<RectTransform>().parent = mlcbs.bubbaGlass.transform;
			mlcbs.stacheGlass = Main.StacheGlassEmpty;//new GameObject("StacheGlassEmpty", typeof(RectTransform), typeof(Animator));
			//GameObject sg = Glass.Instantiate(true);
			//sg.GetComponent<RectTransform>().parent = mlcbs.stacheGlass.transform;
			Animator anim = LevelClearButton.GetComponent<Animator>();
			//DumpUtils.DumpObject(anim.runtimeAnimatorController);
			//foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
			//	DumpUtils.DumpObject(clip);
			//Animator danim = mlcbs.deathsGlass.GetComponent<Animator>();
			//danim.SetInteger("Completed", -1);
			//Animator banim = mlcbs.bubbaGlass.GetComponent<Animator>();
			//banim.SetInteger("Completed", -1);
			//Animator sanim = mlcbs.stacheGlass.GetComponent<Animator>();
			//sanim.SetInteger("Completed", -1);
			//RuntimeAnimatorController glassFake = Main.glassAnim;
			//AnimatorOverrideController glassFake = new AnimatorOverrideController(anim.runtimeAnimatorController);
			//glassFake.name = "glassFakeAnim";
			//AnimationClip start = new AnimationClip();
			//start.name = "glassAnim_start";
			//AnimationClip yes = new AnimationClip();
			//yes.name = "glassAnim_yes";
			//AnimationClip no = new AnimationClip();
			//no.name = "glassAnim_no";
			//glassFake.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>
			//{
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[0], start),
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[1], yes),
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[2], no),
			//});
			//danim.runtimeAnimatorController = glassFake;//Get<RuntimeAnimatorController>("glassAnim");
			//banim.runtimeAnimatorController = glassFake;//Get<RuntimeAnimatorController>("glassAnim");
			//sanim.runtimeAnimatorController = glassFake;//Get<RuntimeAnimatorController>("glassAnim");
			RuntimeAnimatorController clearFake = Main.levelClearButton_anim;
			foreach (AnimationClip clip in clearFake.animationClips)
			{
				if (clip.name.EndsWith("clear"))
					clip.AddEvent(new AnimationEvent
					{
						functionName = "Test",
						time = 1f/3f,
						messageOptions = SendMessageOptions.RequireReceiver
					});
			}
			//AnimatorOverrideController clearFake = new AnimatorOverrideController(anim.runtimeAnimatorController);
			//clearFake.name = "levelClearButtonFake_anim";
			//AnimationClip clear = new AnimationClip();
			//clear.name = "levelClearButtonFake_clear";
			//AnimationClip notClear = new AnimationClip();
			//notClear.name = "levelClearButtonFake_notClear";
			//AnimationClip deathsGlass = new AnimationClip();
			//deathsGlass.name = "levelClearButtonFake_deathsGlass";
			//clearFake.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>
			//{
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[0], clear),
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[1], notClear),
			//	new KeyValuePair<AnimationClip, AnimationClip>(anim.runtimeAnimatorController.animationClips[2], deathsGlass),
			//});
			anim.runtimeAnimatorController = clearFake;//Get<RuntimeAnimatorController>("levelClearButton_anim");
			AudioSource asource = LevelClearButton.AddComponent<AudioSource>();
			GameObject lcbutton = LevelClearButton.FindChild("Button");

			MeshFilter lcbuttonf = lcbutton.GetComponent<MeshFilter>();
			MeshRenderer lcbuttonr = lcbutton.GetComponent<MeshRenderer>();
			lcbuttonr.shadowCastingMode = ShadowCastingMode.Off;
			lcbuttonr.receiveShadows = false;
			LevelClearButton.RemoveComponentImmediate<BoxCollider>();
			GameObject button = lcbutton.FindChild("button");
			button.transform.eulerAngles = new Vector3Int(270, 0, 0);
			button.transform.localScale = new Vector3(0.3f, 0.5f, 0.5f);
			MeshRenderer bmr = button.GetComponent<MeshRenderer>();
			bmr.shadowCastingMode = ShadowCastingMode.Off;
			bmr.receiveShadows = false;

			GameObject lccanvas = lcbutton.FindChild("Canvas");
			RectTransform crt = lccanvas.GetComponent<RectTransform>();
			crt.parent = LevelClearButton.transform;
			crt.localPosition = new Vector3(0, 0.112f, -0.51f);
			crt.anchoredPosition3D = new Vector3(0, 0.112f, -0.51f);
			crt.localScale = Vector3.one * 0.01f;
			crt.anchorMin = Vector2.zero;
			crt.anchorMax = Vector2.zero;
			crt.offsetMin = new Vector2(-100, -49.888f);
			crt.offsetMax = new Vector2(100, 50.112f);
			crt.pivot = Vector3.one * 0.5f;
			crt.sizeDelta = new Vector2(200, 100);
			GameObject lct = lccanvas.FindChild("Text (TMP)");
			lccanvas.RemoveComponentImmediate<TextLanguageScript>();
			lct.AddComponent<TMP_SpriteAnimator>();
			RectTransform lcrt = lct.GetComponent<RectTransform>();
			lcrt.parent = lccanvas.transform;
			lcrt.localPosition = new Vector3(0, -7.8f, 0);
			lcrt.anchoredPosition3D = new Vector3(0, -7.8f, 0);
			lcrt.localScale = Vector3.one * 0.91867f;
			lcrt.anchorMin = Vector2.one * 0.5f;
			lcrt.anchorMin = Vector2.one * 0.5f;
			lcrt.offsetMin = new Vector2(-100, -32.8f);
			lcrt.offsetMax = new Vector2(100, 17.2f);
			lcrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpug = lct.GetComponent<TextMeshProUGUI>();
			tmpug.text = "MOM";
			TMP_FontAsset atwriter3 = tmpug.font;
			Debug.LogError("atwriter3.atlasTexture => " + (atwriter3.atlasTexture == null ? "null" : atwriter3.atlasTexture.ToString()));
			tmpug.font = atwriter3;
			tmpug.SetField("m_sharedMaterial", atwriter3.material);
			tmpug.faceColor = new Color32(45, 45, 45, 255);
			tmpug.outlineColor = new Color32(178, 178, 178, 255);
			tmpug.outlineWidth = 0.112f;
			tmpug.enableAutoSizing = true;
			tmpug.extraPadding = false;
			tmpug.fontSizeMin = 18;
			tmpug.fontSizeMax = 72;
			tmpug.fontSize = 64.7f;
			tmpug.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpug.fontWeight = FontWeight.Regular;
			tmpug.alignment = TextAlignmentOptions.Center;
			GameObject lctt = lct.Instantiate("TimeText");
			RectTransform lctrt = lctt.GetComponent<RectTransform>();
			lctrt.parent = lccanvas.transform;
			lctrt.localPosition = new Vector3(0, 13.9f, 86.4f);
			lctrt.anchoredPosition3D = new Vector3(0, 13.9f, 86.4f);
			lctrt.localScale = Vector3.one * 0.1f;
			lctrt.anchorMin = Vector2.one * 0.5f;
			lctrt.anchorMax = Vector2.one * 0.5f;
			lctrt.offsetMin = new Vector2(-100, -11.1f);
			lctrt.offsetMax = new Vector2(100, 38.9f);
			lctrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpugt = lctt.GetComponent<TextMeshProUGUI>();
			tmpugt.text = "00:00.000";
			TMP_FontAsset atwriter1 = Get<TMP_FontAsset>("atwriter SDF 1");
			Debug.LogError("atwriter1.atlasTexture => " + (atwriter3.atlasTexture == null ? "null" : atwriter3.atlasTexture.ToString()));
			tmpugt.font = atwriter1;
			tmpugt.fontMaterial = atwriter1.material;
			tmpugt.fontSharedMaterial = atwriter1.material;
			tmpugt.faceColor = new Color32(175, 175, 175, 255);
			tmpugt.outlineColor = new Color32(55, 55, 55, 255);
			tmpugt.outlineWidth = 0.196f;
			tmpugt.enableAutoSizing = false;
			tmpugt.extraPadding = false;
			tmpugt.fontSizeMin = 18;
			tmpugt.fontSizeMax = 72;
			tmpugt.fontSize = 49.9f;
			tmpugt.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpugt.fontWeight = FontWeight.Regular;
			tmpugt.alignment = TextAlignmentOptions.Center;
			mlcbs.timeText = tmpugt;
			GameObject lcd = lctt.Instantiate("Deaths");
			RectTransform lcdrt = lcd.GetComponent<RectTransform>();
			lcdrt.parent = lctt.transform;
			lcdrt.localPosition = new Vector3(-38.2f, 206.8f, 0);
			lcdrt.anchoredPosition3D = new Vector3(-38.2f, 206.8f, 0);
			lcdrt.localScale = Vector3.one;
			lcdrt.offsetMin = new Vector2(-138.2f, 181.8f);
			lcdrt.offsetMax = new Vector2(61.8f, 231.8f);
			lcdrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpugd = lcd.GetComponent<TextMeshProUGUI>();
			tmpugd.text = "Deaths: 0";
			Debug.LogError("atwriter1.atlasTexture2 => " + (atwriter3.atlasTexture == null ? "null" : atwriter3.atlasTexture.ToString()));
			tmpugd.font = atwriter1;
			tmpugd.fontMaterial = atwriter1.material;
			tmpugd.fontSharedMaterial = atwriter1.material;
			tmpugd.faceColor = new Color32(175, 175, 175, 255);
			tmpugd.outlineColor = new Color32(55, 55, 55, 255);
			tmpugd.outlineWidth = 0.196f;
			tmpugd.enableAutoSizing = false;
			tmpugd.extraPadding = false;
			tmpugd.fontSizeMin = 18;
			tmpugd.fontSizeMax = 72;
			tmpugd.fontSize = 49.9f;
			tmpugd.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpugd.fontWeight = FontWeight.Regular;
			tmpugd.alignment = TextAlignmentOptions.Left;
			mlcbs.deathsText = tmpugd;

			mlcbs.deathsGlass.transform.parent = lctt.transform;
			RectTransform dge = mlcbs.deathsGlass.GetComponent<RectTransform>();
			dge.localPosition = new Vector3(-200.1f, 202.3f, 0);
			dge.anchoredPosition3D = new Vector3(-200.1f, 202.3f, 0);
			dge.localScale = Vector3.one * 0.8f;
			dge.offsetMin = new Vector2(-250.1f, 152.3f);
			dge.offsetMax = new Vector2(-150.1f, 252.3f);
			dge.sizeDelta = new Vector2Int(100, 100);

			GameObject lcb = lcd.Instantiate("Bubba Tokens");
			RectTransform lcbrt = lcb.GetComponent<RectTransform>();
			lcbrt.parent = lctt.transform;
			lcbrt.localPosition = new Vector3(-38.2f, 142.7f, 0);
			lcbrt.anchoredPosition3D = new Vector3(-38.2f, 142.7f, 0);
			lcbrt.localScale = Vector3.one;
			lcbrt.offsetMin = new Vector2(-138.2f, 117.7f);
			lcbrt.offsetMax = new Vector2(61.8f, 167.7f);
			lcbrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpugb = lcb.GetComponent<TextMeshProUGUI>();
			tmpugb.text = "Bubba:";
			Debug.LogError("atwriter1.atlasTexture3 => " + (atwriter3.atlasTexture == null ? "null" : atwriter3.atlasTexture.ToString()));
			tmpugb.font = atwriter1;
			tmpugb.fontMaterial = atwriter1.material;
			tmpugb.fontSharedMaterial = atwriter1.material;
			tmpugb.faceColor = new Color32(175, 175, 175, 255);
			tmpugb.outlineColor = new Color32(55, 55, 55, 255);
			tmpugb.outlineWidth = 0.196f;
			tmpugb.enableAutoSizing = false;
			tmpugb.extraPadding = false;
			tmpugb.fontSizeMin = 18;
			tmpugb.fontSizeMax = 72;
			tmpugb.fontSize = 49.9f;
			tmpugb.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpugb.fontWeight = FontWeight.Regular;
			tmpugb.alignment = TextAlignmentOptions.Left;
			mlcbs.bubbaText = tmpugb;

			GameObject bubba1 = Main.Bubba1;//lccanvas.FindChild("BubbaIcon");
			bubba1.name = "Bubba1";
			RectTransform birt1 = bubba1.GetComponent<RectTransform>();
			birt1.parent = lctt.transform;
			birt1.localPosition = new Vector3(70.7f, 143.6f, 0);
			birt1.anchoredPosition3D = new Vector3(70.7f, 143.6f, 0);
			birt1.localScale = Vector3.one * 59.52074f;
			birt1.anchorMin = Vector2.one * 0.5f;
			birt1.anchorMax = Vector2.one * 0.5f;
			birt1.pivot = Vector2.one * 0.5f;
			birt1.offsetMin = new Vector2(70.2f, 143.1f);
			birt1.offsetMax = new Vector2(71.2f, 144.1f);
			birt1.sizeDelta = Vector2.one;
			//bubba1.GetComponent<SpriteRenderer>().size = new Vector2(0.868f, 0.768f);
			////bubba1.GetComponent<SpriteRenderer>().bounds = new Bounds(bubba1.GetComponent<SpriteRenderer>().bounds.center, new Vector3(0.3f, 0.2f, 0.1f));
			//bubba1.GetComponent<SpriteRenderer>().enabled = false;
			GameObject bubba2 = bubba1.Instantiate("Bubba2");
			RectTransform birt2 = bubba2.GetComponent<RectTransform>();
			birt2.parent = lctt.transform;
			birt2.localPosition = new Vector3(132.9f, 143.6f, 0);
			birt2.anchoredPosition3D = new Vector3(132.9f, 143.6f, 0);
			birt2.localScale = Vector3.one * 59.52074f;
			birt2.anchorMin = Vector2.one * 0.5f;
			birt2.anchorMax = Vector2.one * 0.5f;
			birt2.pivot = Vector2.one * 0.5f;
			birt2.offsetMin = new Vector2(132.4f, 143.1f);
			birt2.offsetMax = new Vector2(133.4f, 144.1f);
			birt2.sizeDelta = Vector2.one;
			//bubba2.GetComponent<SpriteRenderer>().enabled = false;
			GameObject bubba3 = bubba1.Instantiate("Bubba3");
			RectTransform birt3 = bubba3.GetComponent<RectTransform>();
			birt3.parent = lctt.transform;
			birt3.localPosition = new Vector3(195.1f, 143.6f, 0);
			birt3.anchoredPosition3D = new Vector3(195.1f, 143.6f, 0);
			birt3.localScale = Vector3.one * 59.52074f;
			birt3.anchorMin = Vector2.one * 0.5f;
			birt3.anchorMax = Vector2.one * 0.5f;
			birt3.pivot = Vector2.one * 0.5f;
			birt3.offsetMin = new Vector2(194.6f, 143.1f);
			birt3.offsetMax = new Vector2(195.6f, 144.1f);
			birt3.sizeDelta = Vector2.one;
			//bubba3.GetComponent<SpriteRenderer>().enabled = false;
			//bubba1.GetComponent<SpriteRenderer>().enabled = true;
			//bubba2.GetComponent<SpriteRenderer>().enabled = true;
			//bubba3.GetComponent<SpriteRenderer>().enabled = true;

			mlcbs.bubbaSprites = new SpriteRenderer[3]
			{
				bubba1.GetComponent<SpriteRenderer>(),
				bubba2.GetComponent<SpriteRenderer>(),
				bubba3.GetComponent<SpriteRenderer>()
			};

			mlcbs.bubbaGlass.transform.parent = lctt.transform;
			RectTransform bge = mlcbs.bubbaGlass.GetComponent<RectTransform>();
			bge.localPosition = new Vector3(-200.1f, 140.1f, 0);
			bge.anchoredPosition3D = new Vector3(-200.1f, 140.1f, 0);
			bge.localScale = Vector3.one * 0.8f;
			bge.offsetMin = new Vector2(-250.1f, 90.1f);
			bge.offsetMax = new Vector2(-150.1f, 190.1f);
			bge.sizeDelta = new Vector2Int(100, 100);

			GameObject lcs = lcd.Instantiate("Stache Count");
			RectTransform lcsrt = lcs.GetComponent<RectTransform>();
			lcsrt.parent = lctt.transform;
			lcsrt.localPosition = new Vector3(-38.2f, 77.9f, 0);
			lcsrt.anchoredPosition3D = new Vector3(-38.2f, 77.9f, 0);
			lcsrt.localScale = Vector3.one;
			lcsrt.offsetMin = new Vector2(-138.2f, 52.9f);
			lcsrt.offsetMax = new Vector2(61.8f, 102.9f);
			lcsrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpugs = lcs.GetComponent<TextMeshProUGUI>();
			tmpugs.text = "Moustaches: 00%";
			Debug.LogError("atwriter1.atlasTexture4 => " + (atwriter3.atlasTexture == null ? "null" : atwriter3.atlasTexture.ToString()));
			tmpugs.font = atwriter1;
			tmpugs.fontMaterial = atwriter1.material;
			tmpugs.fontSharedMaterial = atwriter1.material;
			tmpugs.faceColor = new Color32(175, 175, 175, 255);
			tmpugs.outlineColor = new Color32(55, 55, 55, 255);
			tmpugs.outlineWidth = 0.196f;
			tmpugs.enableAutoSizing = false;
			tmpugs.extraPadding = false;
			tmpugs.fontSizeMin = 18;
			tmpugs.fontSizeMax = 72;
			tmpugs.fontSize = 49.9f;
			tmpugs.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpugs.fontWeight = FontWeight.Regular;
			tmpugs.alignment = TextAlignmentOptions.Left;
			mlcbs.stacheText = tmpugs;

			mlcbs.stacheGlass.transform.parent = lctt.transform;
			RectTransform sge = mlcbs.stacheGlass.GetComponent<RectTransform>();
			sge.localPosition = new Vector3(-200.1f, 70.4f, 0);
			sge.anchoredPosition3D = new Vector3(-200.1f, 70.4f, 0);
			sge.localScale = Vector3.one * 0.8f;
			sge.offsetMin = new Vector2(-250.1f, 20.4f);
			sge.offsetMax = new Vector2(-150.1f, 120.4f);
			sge.sizeDelta = new Vector2Int(100, 100);

			GameObject lccc = lcd.Instantiate("Case Closed Text");
			RectTransform lcccrt = lccc.GetComponent<RectTransform>();
			lcccrt.parent = lctt.transform;
			lcccrt.localPosition = new Vector3(-23, 130, 0);
			lcccrt.localEulerAngles = new Vector3(0, 0, 14.39776f);
			lcccrt.anchoredPosition3D = new Vector3(-23, 130, 0);
			lcccrt.localScale = Vector3.one * 2.1f;
			lcccrt.anchorMin = Vector2.one * 0.5f;
			lcccrt.anchorMax = Vector2.one * 0.5f;
			lcccrt.pivot = Vector2.one * 0.5f;
			lcccrt.offsetMin = new Vector2(-123, 105);
			lcccrt.offsetMax = new Vector2(77, 155);
			lcccrt.sizeDelta = new Vector2Int(200, 50);
			TextMeshProUGUI tmpugcc = lccc.GetComponent<TextMeshProUGUI>();
			tmpugcc.text = "";
			Debug.LogError("Main.atwriter_Red.atlasTexture => " + (Main.atwriter_Red.atlasTexture == null ? "null" : Main.atwriter_Red.atlasTexture.ToString()));
			tmpugcc.font = Main.atwriter_Red;//Get<TMP_FontAsset>("atwriter SDF Red")
			tmpugcc.fontMaterial = Main.atwriter_RedMat;
			tmpugcc.fontSharedMaterial = Main.atwriter_RedMat;//Get<Material>("atwriter SDF Material");
			tmpugcc.faceColor = new Color32(221, 28, 26, 255);
			tmpugcc.outlineColor = new Color32(55, 55, 55, 255);
			tmpugcc.outlineWidth = 0.196f;
			tmpugcc.enableAutoSizing = false;
			tmpugcc.extraPadding = false;
			tmpugcc.fontSizeMin = 18;
			tmpugcc.fontSizeMax = 72;
			tmpugcc.fontSize = 49.9f;
			tmpugcc.margin = new Vector4(11.1f, -19.0f, 12.4f, -11.1f);
			tmpugcc.fontWeight = FontWeight.Regular;
			tmpugcc.alignment = TextAlignmentOptions.Left;
			mlcbs.caseClosedText = tmpugcc;

			Debug.LogError("Main.Typo.atlasTexture => " + (Main.Typo.atlasTexture == null ? "null" : Main.Typo.atlasTexture.ToString()));
			tmpug.font = Main.Typo;//Get<TMP_FontAsset>("Typo SDF");
			tmpug.fontSharedMaterial = Main.TypoMat;//Get<Material>("Typo SDF Material");

			Console.Console.LogError("atwriter1.material == " + atwriter1.material.name + " [" + atwriter1.material.GetType() + "]");

			LevelClearButton.FindChild("Label").DestroyImmediate();
			LevelClearButton.FindChild("Spawnpoint").DestroyImmediate();
			lccanvas.FindChild("BubbaIcon").DestroyImmediate();
			lccanvas.FindChild("StacheIcon").DestroyImmediate();
			lccanvas.FindChild("SkullIcon").DestroyImmediate();

			LevelButtonScript lbs = ModdedLevelButton.GetComponent<LevelButtonScript>();
			ModdedLevelButtonScript mlbs = ModdedLevelButton.AddComponent<ModdedLevelButtonScript>();
			mlbs.bestTimeText = lbs.bestTimeText;
			mlbs.bubbaOutline = lbs.bubbaOutline;
			mlbs.bubbaRend = lbs.bubbaRend;
			mlbs.bubbaSprite = lbs.bubbaSprite;
			mlbs.buttonPlane = lbs.buttonPlane;
			mlbs.completion = lbs.completion;
			mlbs.currentLanguage = lbs.currentLanguage;
			mlbs.levelEnum = Level.MAIN_MENU;
			mlbs.level = 0;
			mlbs.levelAnim = lbs.levelAnim;
			mlbs.levelDataLoaded = lbs.levelDataLoaded;
			mlbs.levelDataText = lbs.levelDataText;
			mlbs.levelName = "LevelSelect";
			mlbs.levelTimer = lbs.levelTimer;
			mlbs.levelTimerMax = lbs.levelTimerMax;
			mlbs.rb = lbs.rb;
			mlbs.skullOutline = lbs.skullOutline;
			mlbs.skullRend = lbs.skullRend;
			mlbs.skullSprite = lbs.skullSprite;
			mlbs.spawnPoint = lbs.spawnPoint;
			mlbs.stacheOutline = lbs.stacheOutline;
			mlbs.stacheRend = lbs.stacheRend;
			mlbs.stacheSprite = lbs.stacheSprite;
			mlbs.startPos = lbs.startPos;
			lbs.DestroyImmediate();
			GameObject llct = LevelButton.FindChild("Label").FindChild("Canvas").FindChild("Text (TMP)");
			llct.name = "LevelName";
			llct.GetComponent<TextMeshProUGUI>().text = "Vanilla Level";
			GameObject mlct = ModdedLevelButton.FindChild("Label").FindChild("Canvas").FindChild("Text (TMP)");
			mlct.name = "LevelName";
			mlct.GetComponent<TextMeshProUGUI>().text = "Modded Level";
			mlbs.levelNameText = mlct.GetComponent<TextMeshProUGUI>();
			mlbs.levelNameTextLanguage = mlct.GetComponent<TextLanguageScript>();
			var moon = GetInstInactive("Moon", go => go.transform.parent.name != "Moon");
			moon.name = "Moon";
			var grav = GetInstInactive("GravOverride", go => go.transform.parent?.name == "MoonEmpty");
			var grav1 = GetInstInactive("GravOverride (1)", go => go.transform.parent?.name == "InumoreEmpty");
			grav.name = "GravOverride";
			grav1.name = "GravOverride (1)";
			Moon = CreateEmpty("MoonWithGravity");
			Moon.SetActive(false);
			Vector3 mepos = Get<GameObject>("MoonEmpty").transform.position;
			Moon.transform.localPosition = mepos;
			moon.transform.SetParent(Moon.transform, true);
			grav.transform.SetParent(Moon.transform, true);
			grav1.transform.SetParent(Moon.transform, true);
			moon.transform.localPosition += mepos;
			grav.transform.localPosition += mepos;
			grav1.transform.localPosition += mepos;
			Moon.transform.position = Vector3.zero;
			moon.SetActive(true);
			grav.SetActive(true);
			grav1.SetActive(true);
			Debug.LogError("NukeButton");
			NukeButton = GetInstInactive("NukeButton");
			NukeButton.name = "NukeButton";
			NukeButton.Prefabitize();
			Debug.LogError("GenericButton");
			GenericButton = GetInstInactive("DeleteButton");
			GenericButton.name = "GenericButton";
			GenericButton.RemoveComponentImmediate<GenericButtonScript>();
			GenericButton.AddComponent<GenericButtonScript>();
			GenericButton.Prefabitize();
			Debug.LogError("Spawnpoint");
			Spawnpoint = GetInstInactive("Spawnpoint");
			Spawnpoint.name = "Spawnpoint";
			Spawnpoint.Prefabitize();
			Debug.LogError("SandwichPlatform");
			Sandwich = GetInstInactive("SandwichPlatform");
			Sandwich.name = "SandwichPlatform";
			Sandwich.Prefabitize();
			Debug.LogError("GravOverride");
			GravityOverride = GetInstInactive("GravOverride");
			GravityOverride.name = "GravOverride";
			GravityOverride.Prefabitize();
			Debug.LogError("Tako");
			Tako = GetInstInactive("Tako");
			Tako.name = "Tako";
			Tako.Prefabitize();
			Debug.LogError("VFX_sparkle_small");
			VFX_sparkle_small = Main.game.LoadAndPrefabitize("VFX_sparkle_small", true);
			ParticleScaler small = VFX_sparkle_small.GetOrAddComponent<ParticleScaler>();
			small.scale = 0.25f;
			small.speed = 1.75f;
			small.parts = VFX_sparkle_small.GetComponentsInChildren<ParticleSystem>().ToList();
			Debug.LogError("VFX_sparkle_small_pink");
			VFX_sparkle_small_pink = Main.game.LoadAndPrefabitize("VFX_sparkle_small_pink", true);
			ParticleScaler small_pink = VFX_sparkle_small_pink.GetOrAddComponent<ParticleScaler>();
			small_pink.scale = 0.25f;
			small_pink.speed = 0.75f;
			small_pink.parts = VFX_sparkle_small_pink.GetComponentsInChildren<ParticleSystem>().ToList();
			Debug.LogError("VFX_sparkle_big");
			VFX_sparkle_big = Main.game.LoadAndPrefabitize("VFX_sparkle_big", true);
			ParticleScaler big = VFX_sparkle_big.GetOrAddComponent<ParticleScaler>();
			big.scale = 0.5f;
			big.speed = 1.75f;
			big.parts = VFX_sparkle_big.GetComponentsInChildren<ParticleSystem>().ToList();
			Debug.LogError("BubbaToken");
			BubbaToken = Main.game.LoadAndPrefabitize("BubbaToken", true);
			ModdedTokenScript token = BubbaToken.GetOrAddComponent<ModdedTokenScript>();
			token.tokenNum = 0;
			token.clip = Main.game.LoadAsset<AudioClip>("bubbaToken3");
			token.tokenTransform = BubbaToken.transform.GetChild(0);
			token.vfxPrefab = VFX_sparkle_big;

			Shader SixSide = Shader.Find("Skybox/6 Sided");
			Skybox = new Material(SixSide);
			Skybox.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 1));
			Skybox.SetFloat("_Exposure", 1);
			Skybox.SetFloat("_Rotation", 0);
			Skybox.SetTexture("_FrontTex", StreamExtensions.CreateTexture2DFromImage("skyft"));
			Skybox.SetTexture("_BackTex", StreamExtensions.CreateTexture2DFromImage("skybk"));
			Skybox.SetTexture("_LeftTex", StreamExtensions.CreateTexture2DFromImage("skylf"));
			Skybox.SetTexture("_RightTex", StreamExtensions.CreateTexture2DFromImage("skyrt"));
			Skybox.SetTexture("_UpTex", StreamExtensions.CreateTexture2DFromImage("skyup"));
			Skybox.SetTexture("_DownTex", StreamExtensions.CreateTexture2DFromImage("skydn"));

			//Skybox2 = new Material(SixSide);
			//Skybox2.SetColor("_Tint", new Color(0.5735294f, 0.5735294f, 0.5735294f, 0.5019608f));
			//Skybox2.SetFloat("_Exposure", 1);
			//Skybox2.SetFloat("_Rotation", 243);
			//Skybox2.SetTexture("_FrontTex", StreamExtensions.CreateTexture2DFromImage("skybox_Front"));
			//Skybox2.SetTexture("_BackTex", StreamExtensions.CreateTexture2DFromImage("skybox_Back"));
			//Skybox2.SetTexture("_LeftTex", StreamExtensions.CreateTexture2DFromImage("skybox_Right"));
			//Skybox2.SetTexture("_RightTex", StreamExtensions.CreateTexture2DFromImage("skybox_Left"));
			//Skybox2.SetTexture("_UpTex", StreamExtensions.CreateTexture2DFromImage("skybox_Top"));
			//Skybox2.SetTexture("_DownTex", StreamExtensions.CreateTexture2DFromImage("skybox_Bottom"));

			GetMainStuff();
			GetPlayer();
			//SceneUtils.QuickLoad(1, GetMainStuff);//GetMainStuff(SceneManager.GetActiveScene());

			//DumpUtils.DumpObject(Block, "SAObjects2");
			//DumpUtils.DumpObject(Button, "SAObjects2");
			//DumpUtils.DumpObject(Carrot, "SAObjects2");
			//DumpUtils.DumpObject(CloudSolid, "SAObjects2");
			//DumpUtils.DumpObject(CloudSpawner, "SAObjects2");
			//DumpUtils.DumpObject(Desk, "SAObjects2");
			//DumpUtils.DumpObject(Folder, "SAObjects2");
			//DumpUtils.DumpObject(LevelButton, "SAObjects2");
			//DumpUtils.DumpObject(Moon, "SAObjects2");
			//DumpUtils.DumpObject(NukeButton, "SAObjects2");
			//DumpUtils.DumpObject(GenericButton, "SAObjects2");
			//DumpUtils.DumpObject(Spawnpoint, "SAObjects2");
			//foreach (GameObject gameObject in GetAll<GameObject>())
			//{
			//	DumpUtils.DumpObject(gameObject, "SAObjects");
			//}

			//Mesh cheeseMesh = new ObjImporter().ImportFile("Cheese");

			//GameObject cheese = new GameObject("Cheese");
			//cheese.Prefabitize();
			//cheese.SetActive(false);
			//MeshFilter filter = cheese.AddComponent<MeshFilter>();
			//filter.sharedMesh = cheeseMesh;
			//MeshRenderer renderer = cheese.AddComponent<MeshRenderer>();
			//Material cheeseMat = new Material(Shader.Find("Standard")) { name = "Cheese" };
			//cheeseMat.SetColor("_Color", new Color32(byte.MaxValue, 166, byte.MinValue, byte.MaxValue));
			//renderer.material = cheeseMat;
			//MeshCollider collider = cheese.AddComponent<MeshCollider>();
			//collider.convex = true;
			//collider.sharedMesh = cheeseMesh;
			//collider.isTrigger = false;
			//Cheese = cheese;
		}
#endif

		[System.Obsolete("Use UnityObjectUtils.GetActiveRootGameObjects instead")]
		public static GameObject[] GetAllRootGameObjects() => UnityObjectUtils.GetActiveRootGameObjects();

		[System.Obsolete("Use UnityObjectUtils.GetActiveRootGameObject instead")]
		public static GameObject GetRootGameObject(string name) => UnityObjectUtils.GetActiveRootGameObject(name);

		/// <summary>
		/// Gets an object of a type by its name and a custom condition.
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		/// <param name="name">Name to search for</param>
		/// <param name="predicate">Other condition</param>
		/// <returns>Object found or null if nothing is found</returns>
		public static T Get<T>(string name, System.Predicate<T> predicate) where T : Object
		{
			foreach (T found in Resources.FindObjectsOfTypeAll<T>())
			{
				if (found.name.Equals(name) && predicate(found))
					return found;
			}

			return null;
		}

		/// <summary>
		/// Gets an object of a type by its name
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		/// <param name="name">Name to search for</param>
		/// <returns>Object found or null if nothing is found</returns>
		public static T Get<T>(string name) where T : Object
		{
			foreach (T found in Resources.FindObjectsOfTypeAll<T>())
			{
				if (found.name.Equals(name))
					return found;
			}

			return null;
		}

		/// <summary>
		/// Gets an object of a type by its name
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <param name="type">Type to search</param>
		/// <returns>Object found or null if nothing is found</returns>
		public static Object Get(string name, System.Type type)
		{
			foreach (Object found in Resources.FindObjectsOfTypeAll(type))
			{
				if (found.name.Equals(name))
					return found;
			}

			return null;
		}

		/// <summary>
		/// Gets all objects of a type
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		public static List<T> GetAll<T>() where T : Object
		{
			return new List<T>(Resources.FindObjectsOfTypeAll<T>());
		}

		/// <summary>
		/// Gets all objects of a type
		/// </summary>
		/// <param name="type">Type to search</param>
		public static List<Object> GetAll(System.Type type)
		{
			if (!type.IsSubclassOf(typeof(Object)))
				return null;
			return new List<Object>(Resources.FindObjectsOfTypeAll(type));
		}

		/// <summary>
		/// Gets an instance of a gameobject by its name and a custom condition.
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <param name="predicate">Other condition</param>
		/// <returns>Instance of gameobject found or null if nothing is found</returns>
		public static GameObject GetInstInactive(string name, System.Predicate<GameObject> predicate)
		{
			foreach (GameObject found in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (found.name.Equals(name) && predicate(found))
					return GameObjectUtils.InstantiateInactive(found);
			}

			return null;
		}

		/// <summary>
		/// Gets an instance of a gameobject by its name
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <returns>Instance of gameobject found or null if nothing is found</returns>
		public static GameObject GetInstInactive(string name)
		{
			foreach (GameObject found in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (found.name.Equals(name))
					return GameObjectUtils.InstantiateInactive(found);
			}

			return null;
		}

		/// <summary>
		/// Gets an instance of an object of a type by its name and a custom condition.
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		/// <param name="name">Name to search for</param>
		/// <param name="predicate">Other condition</param>
		/// <returns>Instance of object found or null if nothing is found</returns>
		public static T GetInst<T>(string name, System.Predicate<T> predicate) where T : Object
		{
			foreach (T found in Resources.FindObjectsOfTypeAll<T>())
			{
				if (found.name.Equals(name) && predicate(found))
					return Object.Instantiate(found);
			}

			return null;
		}

		/// <summary>
		/// Gets an instance of an object of a type by its name
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		/// <param name="name">Name to search for</param>
		/// <returns>Instance of object found or null if nothing is found</returns>
		public static T GetInst<T>(string name) where T : Object
		{
			foreach (T found in Resources.FindObjectsOfTypeAll<T>())
			{
				if (found.name.Equals(name))
					return Object.Instantiate(found);
			}

			return null;
		}

		/// <summary>
		/// Gets an instance of an object of a type by its name
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <param name="type">Type to search</param>
		/// <returns>Instance of object found or null if nothing is found</returns>
		public static Object GetInst(string name, System.Type type)
		{
			foreach (Object found in Resources.FindObjectsOfTypeAll(type))
			{
				if (found.name.Equals(name))
					return Object.Instantiate(found);
			}

			return null;
		}

		/// <summary>
		/// Gets an object of a type by its name in the world
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		/// <param name="name">Name to search for</param>
		/// <returns>Object found or null if nothing is found</returns>
		public static T GetWorld<T>(string name) where T : Object
		{
			foreach (T found in Object.FindObjectsOfType<T>())
			{
				if (found.name.Equals(name))
					return found;
			}

			return null;
		}

		/// <summary>
		/// Gets an object of a type by its name in the world
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <param name="type">Type to search</param>
		/// <returns>Object found or null if nothing is found</returns>
		public static Object GetWorld(string name, System.Type type)
		{
			foreach (Object found in Object.FindObjectsOfType(type))
			{
				if (found.name.Equals(name))
					return found;
			}

			return null;
		}

		/// <summary>
		/// Gets all objects of a type in the world
		/// </summary>
		/// <typeparam name="T">Type to search</typeparam>
		public static List<T> GetAllWorld<T>() where T : Object
		{
			return new List<T>(Object.FindObjectsOfType<T>());
		}
	}
}
