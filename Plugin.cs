using BepInEx;
using BepInEx.Bootstrap;
using GorillaAchievements.Classes;
using GorillaAchievements.MonoBehaviors;
using GorillaAchievements.Patches;
using GorillaAchievements.Utilities;
using GorillaAchievements.Internal;
using GorillaLocomotion;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GorillaAchievements;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    internal static GorillaLog Log = new();

    public static Plugin Instance;

    internal static Achievement[] allAchievements;

    public static List<Achievement> realAllAchievements { get; private set; } = new List<Achievement>();

    private float lastIntervalTime;

    private GameObject assetBundle;

    private GameObject headFollower;

    private GameObject bodyFollower;

    private GameObject audioHolder;

    private ParticleSystem fireworkVFX;

    private GameObject UIcanvas;

    private GameObject achievementUI;

    internal List<NetPlayer> taggedYou = new List<NetPlayer>();

    private Vector3 lastPlayerPos;

    private float distanceTraveled;

    private List<string> visitedLobbies = new List<string>();

    private List<GTZone> visitedZones = new List<GTZone>();

    private bool menuOpen;

    public static bool debugMode = false;

    private readonly string[] ghostCodes =
    [
        "DAISY09",
        "PBBV",
        "J3VU",
        "RUN",
        "HIDE",
        "TIPTOE",
        "WARNINGBOT",
        "SREN17",
        "SREN18",
        "ECHO",
        "TESTLOLCAF", // I need this because most ghost codes are full
    ];

    private readonly string[] larpNames = [
        "JMANCURLY",
        "ELLIOT",
        "ERIK1515",
        "K9",
        "TTTPIG",
        "LEMMING"
    ];

    private readonly GTZone[] classicZones = [
        GTZone.forest,
        GTZone.canyon,
        GTZone.city,
        GTZone.mountain,
        GTZone.beach,
        GTZone.cave,
    ];

    private void Start()
    {
        Instance = this;
        HarmonyPatches.Patch();
        GorillaTagger.OnPlayerSpawned(() => MethodUtilities.Attempt(OnPlayerSpawned));

        allAchievements =
        [
            A("Tallest Tree", "Reach the top of the tallest tree", Difficulty.Easy, new Vector3(-73.82f, 37.93f, -47.19f)), // Confirmed Working
            A("Zoomies", "Go super fast zoomy monke!", Difficulty.Medium, Vector3.zero), // Confirmed Working
            A("Infection Time", "Tag another monke in infection", Difficulty.Medium, Vector3.zero), // Confirmed Working
            A("Swimmer", "Swim in water", Difficulty.Easy, Vector3.zero), // Confirmed Working
            A("Shopping Spree", "Purchase a cosmetic", Difficulty.Rare, Vector3.zero), // Confirmed Working
            A("Rainy Day", "Experience the rain", Difficulty.Rare, Vector3.zero), // Confirmed Working
            A("Splash Zone", "Fall into water from a high place", Difficulty.Medium, Vector3.zero), // Confirmed Working
            A("Going Up", "Ride the elevator to another location", Difficulty.Easy, Vector3.zero), // Confirmed Working
            A("Patient Zero", "Be the first tagged in an infection lobby", Difficulty.Rare, Vector3.zero), // Confirmed Working
            A("Last Laugh", "Be the last survivor in an infection round", Difficulty.Hard, Vector3.zero),
            A("Ghost Hunter", "Enter a ghost code", Difficulty.Easy, Vector3.zero), // Confirmed Working
            A("Pro Modder", "Have 10+ mods installed", Difficulty.Medium, Vector3.zero), // Confirmed Working
            A("Supporter", "Have another one of my mods installed <3", Difficulty.Rare, Vector3.zero), // Confirmed Working
            A("Tourist", "Visit all the classic maps", Difficulty.Hard, Vector3.zero), // Confirmed Working
            A("Marathon", "Travel a long distance", Difficulty.Hard, Vector3.zero), // Confirmed Working
            A("Revenge Tag", "Tag someone who tagged you in infection", Difficulty.Medium, Vector3.zero),
            A("Larper", "Find someone impersonating a popular youtuber", Difficulty.Rare, Vector3.zero),
            A("It's Crowded Here", "Play in a lobby with 15+ players", Difficulty.Rare, Vector3.zero),
            A("Back Again", "Join a public lobby you were previously in", Difficulty.Rare, Vector3.zero), // Confirmed Working
            A("AAC", "Meet an AA Creator", Difficulty.Hard, Vector3.zero), // Confirmed Working
            A("Finger Painter", "Meet a Finger Painter", Difficulty.Extreme, Vector3.zero), // Probably working bc aac works
        ];
    }

    /// <summary>
    /// Check if an achievement is owned
    /// </summary>
    /// <param name="achievement">The achievement to check</param>
    /// <returns>bool representing whether or not it is owned</returns>
    public static bool AchievementOwned(Achievement achievement)
    {
        return achievement != null && SaveSystem.userSave.unlockedAchievements.Contains(achievement.name);
    }

    private void Update()
    {
        if (GTPlayer.Instance == null || GorillaTagger.Instance == null)
            return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            menuOpen = !menuOpen;
        }

        NotificationSystem.Tick();
        if (Time.time > lastIntervalTime)
            AchievementCheck();
    }

    private void OnGUI()
    {
        if (!menuOpen)
            return;

        GUI.Box(new Rect(10, 10, 250, 250), "Gorilla Achievements");

        if (GUI.Button(new Rect(55, 40, 150, 20), "Clear Save"))
        {
            SaveSystem.userSave.unlockedAchievements.Clear();
        }

        GUI.Label(new Rect(35, 70, 200, 20), $"You unlocked {SaveSystem.userSave.unlockedAchievements.Count} of {realAllAchievements.Count} achievements");
    }

    private void AchievementCheck()
    {
        lastIntervalTime = Time.time + 1f;

        if (GTPlayer.Instance?.RigidbodyVelocity.magnitude > 15)
            AwardAchievement(FindAchievement("Zoomies"));

        if (GTPlayer.Instance?.InWater == true)
        {
            if (GTPlayer.Instance.RigidbodyVelocity.y < -10 && AchievementOwned(FindAchievement("Swimmer")))
                AwardAchievement(FindAchievement("Splash Zone"));

            AwardAchievement(FindAchievement("Swimmer"));
        }

        if (BetterDayNightManager.instance?.CurrentWeather() == BetterDayNightManager.WeatherType.Raining)
            AwardAchievement(FindAchievement("Rainy Day"));

        AACCheck();
        FPCheck();

        distanceTraveled += Vector3.Distance(GTPlayer.Instance.transform.position, lastPlayerPos);
        if (distanceTraveled > 5000)
            AwardAchievement(FindAchievement("Marathon"));

        lastPlayerPos = GTPlayer.Instance.transform.position;
    }

    /// <summary>
    /// Use this method to create a new method
    /// </summary>
    /// <param name="name">The name you want your achievement to have</param>
    /// <param name="description">The description of your achievement</param>
    /// <param name="difficulty">The difficulty of the achievement</param>
    /// <param name="pos">The position to reach to award (Vector3.zero to not have a position)</param>
    /// <returns>An Achievement</returns>
    public static Achievement A(string name, string description, Difficulty difficulty, Vector3 pos)
    {
        Achievement ach = new Achievement
        {
            name = name,
            description = description,
            difficulty = difficulty
        };

        if (pos != Vector3.zero)
            AwardTrigger.Create(pos, ach);

        realAllAchievements.Add(ach);

        return ach;
    }

    /// <summary>
    /// Award an achievement (includes effects)
    /// </summary>
    /// <param name="achievement">The achievement to award</param>
    public void AwardAchievement(Achievement achievement)
    {
        if (achievement == null || AchievementOwned(achievement))
            return;

        SaveSystem.userSave.unlockedAchievements.Add(achievement.name);
        NotificationSystem.Send($"You got the {achievement.name} achievement! ({achievement.difficulty})", achievement);
    }

    /// <summary>
    /// Find an achievement by name (null if not found)
    /// </summary>
    /// <param name="name">The name of the achievement you wanna find</param>
    /// <returns>The achievement (null of not found)</returns>
    public static Achievement FindAchievement(string name)
    {
        foreach (Achievement ach in realAllAchievements)
        {
            if (ach.name == name)
                return ach;
        }
        Log.WriteLine("Failed to find achievement with name: " + name);
        return null;
    }

    // these only see the cosmetics they ARE WEARING
    private void AACCheck()
    {
        foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal))
        {
            foreach (GameObject cosmetic in rig.cosmetics.Where(c => c.name == "LBANI."))
            {
                AwardAchievement(FindAchievement("AAC"));
                return;
            }
        }
    }

    private void FPCheck()
    {
        foreach (VRRig rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal))
        {
            foreach (GameObject cosmetic in rig.cosmetics.Where(c => c.name == "LBADE."))
            {
                AwardAchievement(FindAchievement("Finger Painter"));
                return;
            }
        }
    }

    private void OnPlayerSpawned()
    {
        SaveSystem.LoadData();
        LoadAssetBundle();

        NetworkSystem.Instance.OnJoinedRoomEvent += OnJoinRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += OnRoomLeft;
        NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoin;
        ZoneManagement.OnZoneChange += OnZoneLoaded;

        CheckSupporter();

        if (Chainloader.PluginInfos.Count >= 10)
            AwardAchievement(FindAchievement("Pro Modder"));
    }

    internal void AwardEffects(bool rare, Achievement ach)
    {
        StartCoroutine(AwardEffectsRoutine(rare, ach));
    }

    private IEnumerator AwardEffectsRoutine(bool rare, Achievement ach)
    {
        if (rare)
        {
            fireworkVFX.Play();
            audioHolder.Find("Hard").GetComponent<AudioSource>().Play();
        }
        else
            audioHolder.Find("Easy").GetComponent<AudioSource>().Play();

        achievementUI.SetActive(true);
        achievementUI.Find("Title").GetComponent<TextMeshProUGUI>().text = $"{ach.name} ({ach.difficulty})";
        achievementUI.Find("Description").GetComponent<TextMeshProUGUI>().text = ach.description;
        yield return new WaitForSeconds(4);
        achievementUI.SetActive(false);
    }

    private void OnZoneLoaded(ZoneData[] zones)
    {
        if (AchievementOwned(FindAchievement("Tourist")))
            return;

        foreach (GTZone zone in ZoneManagement.instance.activeZones)
        {
            if (visitedZones.Contains(zone))
                continue;

            if (classicZones.Contains(zone))
            {
                visitedZones.Add(zone);
                Log.WriteLine($"Visited new zone: {zone}");
            }

            if (visitedZones.Count == classicZones.Length)
                AwardAchievement(FindAchievement("Tourist"));
        }
    }

    private void LoadAssetBundle()
    {
        assetBundle = Instantiate(AssetBundleUtilities.Load("GorillaAchievements.Resources.gachievements", "GAchievementBundle"));

        headFollower = assetBundle.Find("HeadFollower");
        headFollower.transform.SetParent(Camera.main.transform, false);

        bodyFollower = assetBundle.Find("BodyFollower");
        bodyFollower.transform.SetParent(GTPlayer.Instance.bodyCollider.transform, false);
        bodyFollower.transform.localScale = Vector3.one * 1.4f;

        audioHolder = assetBundle.Find("Audios");

        fireworkVFX = bodyFollower.Find("FireworkVFX").GetComponent<ParticleSystem>();

        UIcanvas = headFollower.Find("Canvas");
        //UIcanvas.transform.localScale = Vector3.one * 0.6f;
        //UIcanvas.transform.localPosition += new Vector3(0, 0, 0.4f);
        UIcanvas.Find("DebugText").gameObject.SetActive(Constants.DebugMode);

        achievementUI = UIcanvas.Find("Achievement");
    }

    private void CheckSupporter()
    {
        foreach (PluginInfo info in Chainloader.PluginInfos.Values)
        {
            if (info.Metadata.GUID.Contains("lolcaf") && info.Metadata.GUID != Constants.Guid)
            {
                AwardAchievement(FindAchievement("Supporter"));
                return;
            }
        }
    }

    private void OnJoinRoom()
    {
        if (ghostCodes.Contains(NetworkSystem.Instance.RoomName))
            AwardAchievement(FindAchievement("Ghost Hunter"));

        foreach (NetPlayer plr in NetworkSystem.Instance.AllNetPlayers)
        {
            if (larpNames.Contains(plr.SanitizedNickName))
                AwardAchievement(FindAchievement("Meet a larper"));
        }

        if (NetworkSystem.Instance.RoomPlayerCount >= 15)
            AwardAchievement(FindAchievement("It's Crowded Here"));

        if (PhotonNetwork.CurrentRoom.IsVisible)
        {
            if (visitedLobbies.Contains(NetworkSystem.Instance.RoomName))
                AwardAchievement(FindAchievement("Back Again"));
            else
                visitedLobbies.Add(NetworkSystem.Instance.RoomName);
        }
    }

    private void OnRoomLeft()
    {
        taggedYou.Clear();
    }

    private void OnPlayerJoin(NetPlayer plr)
    {
        if (larpNames.Contains(plr.SanitizedNickName))
            AwardAchievement(FindAchievement("Meet a larper"));

        if (NetworkSystem.Instance.RoomPlayerCount >= 15)
            AwardAchievement(FindAchievement("It's Crowded Here"));
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveData();
        HarmonyPatches.Unpatch();
        Log.Dispose();
    }
}
