using System;
using UnityEngine;

namespace PlayAsGabbro;

/// <summary>
/// Taken from QSB skins which was taken from my skin implementation for OWO which was cannibalized from Half Life Overhaul
/// </summary>
public static class SkinReplacer
{
    private static AssetBundle _assetBundle;

    public const string PLAYER_PREFIX = "Traveller_Rig_v01:Traveller_";
    public const string PLAYER_SUFFIX = "_Jnt";

    private static readonly GameObject _gabbro = LoadPrefab("OW_Gabbro_Skin");

    private static readonly Func<string, string> _boneMap = (name) => name.Replace("gabbro_OW_V02:gabbro_rig_v01:", PLAYER_PREFIX);

    public static SkinnedMeshRenderer[] ReplaceSkin(GameObject playerBody, bool isSuited)
    {
        var skin = _gabbro;
        var map = _boneMap;

        // Returns the skinned mesh renderer so if you switch to a different skin you can destroy the old one
        var root = "Traveller_HEA_Player_v2";

        var child = isSuited ?
            "Traveller_Mesh_v01:Traveller_Geo" :
            "player_mesh_noSuit:Traveller_HEA_Player";

        var originalSkin = playerBody.transform.Find(root + "/" + child).gameObject;

        return Swap(originalSkin, skin, map);
    }

    /// <summary>
    /// Creates a copy of the skin and attaches all it's bones to the skeleton of the player
    /// boneMap maps from the bone name of the skin to the bone name of the original player prefab
    /// 
    /// Original is meant to be the actual game object of the skin we're replacing
    /// </summary>
    private static SkinnedMeshRenderer[] Swap(GameObject original, GameObject toCopy, Func<string, string> boneMap)
    {
        var newModel = GameObject.Instantiate(toCopy, original.transform.parent.transform);
        newModel.transform.localPosition = Vector3.zero;
        newModel.SetActive(true);

        // Disappear existing mesh renderers
        foreach (var skinnedMeshRenderer in original.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!skinnedMeshRenderer.name.Contains("Props_HEA_Jetpack"))
            {
                skinnedMeshRenderer.sharedMesh = null;

                var owRenderer = skinnedMeshRenderer.gameObject.GetComponent<OWRenderer>();
                if (owRenderer != null) owRenderer.enabled = false;

                var streamingMeshHandle = skinnedMeshRenderer.gameObject.GetComponent<StreamingMeshHandle>();
                if (streamingMeshHandle != null) GameObject.Destroy(streamingMeshHandle);
            }
        }

        var skinnedMeshRenderers = newModel.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            var bones = skinnedMeshRenderer.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                // Reparent the bone to the player skeleton
                var bone = bones[i];
                string matchingBone = boneMap(bone?.name);
                var newParent = original.transform.parent.SearchInChildren(matchingBone);
                if (newParent == null)
                {
                    // This should never happen in a release, this is just for testing with new models
                }
                else
                {
                    bone.parent = newParent;
                    bone.localPosition = Vector3.zero;
                    bone.localRotation = Quaternion.identity;

                    bone.localScale = Vector3.one * 7f;
                }
            }

            skinnedMeshRenderer.rootBone = original.transform.parent.SearchInChildren(PLAYER_PREFIX + "Trajectory" + PLAYER_SUFFIX);
            skinnedMeshRenderer.quality = SkinQuality.Bone4;
            skinnedMeshRenderer.updateWhenOffscreen = true;

            // Reparent the skinnedMeshRenderer to the original object.
            skinnedMeshRenderer.transform.parent = original.transform;
        }
        // Since we reparented everything to the player we don't need this anymore
        GameObject.Destroy(newModel);

        return skinnedMeshRenderers;
    }

    private static GameObject LoadPrefab(string name)
    {
        if (_assetBundle == null)
        {
            _assetBundle = PlayAsGabbro.Instance.ModHelper.Assets.LoadBundle($"planets/assets/skins");
        }

        var prefab = _assetBundle.LoadAsset<GameObject>($"Assets/Prefabs/{name}.prefab");

        return prefab;
    }
}