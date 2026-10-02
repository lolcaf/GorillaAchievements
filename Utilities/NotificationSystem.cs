using GorillaAchievements.Classes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GorillaAchievements.Utilities;

internal static class NotificationSystem
{
    private class QueuedNotif
    {
        public string Text;

        public bool IncludesPC;

        public float DecayTime;

        public Achievement Achievement;
    }

    private static readonly Queue<QueuedNotif> waitingNotifs = new Queue<QueuedNotif>();

    private static GameObject lastNotificationText = null;

    internal static void Send(string text, Achievement ach, bool includesPC = true, float decayTime = 4)
    {
        waitingNotifs.Enqueue(new QueuedNotif
        {
            Text = text,
            IncludesPC = includesPC,
            DecayTime = decayTime,
            Achievement = ach
        });
    }

    internal static void Tick()
    {
        if (lastNotificationText != null)
            return;

        if (waitingNotifs.Count == 0)
            return;

        QueuedNotif next = waitingNotifs.Dequeue();
        Show(next.Text, next.Achievement, next.IncludesPC, next.DecayTime);
    }

    private static void Show(string text, Achievement ach, bool includesPC, float decayTime)
    {
        if (Camera.main == null)
            return;

        GameObject txt = new GameObject("NotificationText");
        txt.transform.SetParent(Camera.main.transform, false);
        txt.transform.localPosition = new Vector3(0f, -0.3f, 0.8f);
        txt.transform.localRotation = Quaternion.identity;
        txt.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        txt.layer = LayerMask.NameToLayer("FirstPersonOnly");

        lastNotificationText = txt;

        TextMeshPro tmp = txt.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 4;
        tmp.richText = true;
        tmp.isOverlay = true;

        if (includesPC)
            SendPC(text, decayTime);

        if (ach != null && ach.difficulty != Difficulty.None)
            Plugin.Instance.AwardEffects((int)ach.difficulty >= 2 && ach.difficulty != Difficulty.Common, ach);

        Object.Destroy(txt, decayTime);
    }

    internal static void SendPC(string text, float decayTime = 4)
    {
        GameObject shoulderCam = GameObject.Find("Shoulder Camera");

        if (shoulderCam == null)
        {
            Plugin.Log.WriteLine("Shoulder Camera Not Found!");
            return;
        }

        GameObject txt = new GameObject("PCNotificationText");
        txt.transform.SetParent(shoulderCam.transform, false);
        txt.transform.localPosition = new Vector3(0f, -0.3f, 0.8f);
        txt.transform.localRotation = Quaternion.identity;
        txt.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        txt.layer = LayerMask.NameToLayer("ThirdPersonOnly");

        TextMeshPro tmp = txt.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 4;
        tmp.richText = true;
        tmp.isOverlay = true;

        Object.Destroy(txt, decayTime);
    }
}
