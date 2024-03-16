using HarmonyLib;
using UnityEngine;

namespace PlayAsGabbro;

[HarmonyPatch(typeof(PlayerSpawner))]
internal static class PlayerSpawnerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerSpawner.OnResetSimulation))]
    public static bool PlayerSpawner_OnResetSimulation(PlayerSpawner __instance)
    {
        var transform = GameObject.Find("StatueIsland_Body").transform;
        PlayerSpawner._localResetPos = transform.InverseTransformPoint(__instance._playerBody.transform.position);
        PlayerSpawner._localResetRotation = Quaternion.Inverse(transform.rotation) * __instance._playerBody.transform.rotation;
        PlayerSpawner._resetCamaDegreesY = __instance._cameraController.GetDegreesY();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerSpawner.OnResumeSimulation))]
    public static bool PlayerSpawner_OnResumeSimulation(PlayerSpawner __instance)
    {
        OWRigidbody attachedOWRigidbody = GameObject.Find("StatueIsland_Body").GetAttachedOWRigidbody(false);
        __instance._playerBody.transform.position = attachedOWRigidbody.transform.TransformPoint(PlayerSpawner._localResetPos);
        __instance._playerBody.transform.rotation = attachedOWRigidbody.transform.rotation * PlayerSpawner._localResetRotation;
        __instance._cameraController.SetDegreesY(PlayerSpawner._resetCamaDegreesY);
        __instance._playerBody.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(attachedOWRigidbody);
        __instance.FindPlanetSpawns();

        // We getting warp bugged, what is this NH? (yes)
        GameObject.Destroy(GameObject.Find("StreamingGroup_TH").gameObject);

        return false;
    }
}
