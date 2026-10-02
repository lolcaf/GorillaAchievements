using GorillaAchievements.Classes;
using UnityEngine;

namespace GorillaAchievements.MonoBehaviors;

[RequireComponent(typeof(Collider))]
public class AwardTrigger : MonoBehaviour
{
    public Achievement achievement;

    public static AwardTrigger Create(Vector3 position, Achievement achievement)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "AwardTrigger";
        obj.GetComponent<Collider>().isTrigger = true;
        obj.transform.position = position;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (Constants.DebugMode)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                color = new Color(1, 0, 0, 0.5f)
            };
        }
        else
            renderer.enabled = false;

        AwardTrigger trigger = obj.AddComponent<AwardTrigger>();
        trigger.achievement = achievement;
        return trigger;
    }

    private void Start()
    {
        gameObject.layer = 18;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (achievement == null || Plugin.AchievementOwned(achievement))
            return;

        if (other.gameObject == GorillaTagger.Instance.rightHandTriggerCollider || other.gameObject == GorillaTagger.Instance.leftHandTriggerCollider)
        {
            Plugin.Instance.AwardAchievement(achievement);
        }
    }
}
