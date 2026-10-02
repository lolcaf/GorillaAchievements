using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace GorillaAchievements.Patches;

public static class HarmonyPatches
{
    // For information on how to use Harmony, view this documentation:
    // https://harmony.pardeike.net/articles/intro.html

    [HarmonyPatch] // detect infection tags
    public class GorillaTagManagerPatch
    {
        [HarmonyPatch(typeof(GorillaTagManager), nameof(GorillaTagManager.LocalTag))]
        [HarmonyPostfix]
        public static void LocalTagPostfix(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool bodyHit, bool leftHand)
        {
            if (taggingPlayer == NetworkSystem.Instance.LocalPlayer)
            {
                Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Infection Time"));
                if (Plugin.Instance.taggedYou.Contains(taggedPlayer))
                {
                    Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Revenge Tag"));
                }
            }

            if (taggedPlayer == NetworkSystem.Instance.LocalPlayer)
            {
                Plugin.Instance.taggedYou.Add(taggingPlayer);
                Plugin.Log.WriteLine("Added to taggedYou: " + taggingPlayer.SanitizedNickName);
            }
        }

        [HarmonyPatch(typeof(GorillaTagManager), nameof(GorillaTagManager.InfrequentUpdate))]
        [HarmonyPostfix]
        public static void InfrequentUpdatePostfix(GorillaTagManager __instance)
        {
            if (__instance == null)
                return;

            if (__instance.currentInfected.Count == NetworkSystem.Instance.RoomPlayerCount - 1 && !__instance.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
            {
                Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Last Laugh"));
            }

            if (__instance.currentInfected.Count == 1 && (__instance.currentInfected.Contains(NetworkSystem.Instance.LocalPlayer) || __instance.currentIt == NetworkSystem.Instance.LocalPlayer))
            {
                Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Patient Zero"));
            }
        }
    }

    [HarmonyPatch(typeof(CosmeticsController), nameof(CosmeticsController.PurchaseItem))] // detect cosmetic purchases
    public class PurchaseDetectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Shopping Spree"));
        }
    }

    [HarmonyPatch(typeof(GRElevatorManager), nameof(GRElevatorManager.ElevatorButtonPressed))]
    public class ElevatorDetectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.Instance.AwardAchievement(Plugin.FindAchievement("Going Up"));
        }
    }

    /// Here is where the actual patching starts! You don't need to mess with any of this.

    private static HarmonyLib.Harmony? _harmonyInstance;

    /// <summary>
    ///     The current instance of Harmony that is patching the assembly.
    ///     If there is no Harmony instance, it will create one and return it.
    ///     You do not need to touch this section
    /// </summary>
    private static HarmonyLib.Harmony HarmonyInstance
    {
        get
        {
            _harmonyInstance ??= new HarmonyLib.Harmony(Constants.Guid);
            return _harmonyInstance;
        }
    }

    /// <summary>
    /// Patch the assembly.
    /// </summary>
    public static void Patch()
    {
        HarmonyInstance.PatchAll();
    }

    /// <summary>
    /// Unpatch the assembly.
    /// </summary>
    public static void Unpatch()
    {
        HarmonyInstance.UnpatchSelf();
    }
}
