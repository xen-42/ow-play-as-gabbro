using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;

namespace PlayAsGabbro
{
    public class PlayAsGabbro : ModBehaviour
    {
        public static PlayAsGabbro Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public static void Log(string message) => Instance.ModHelper.Console.WriteLine(message);
        public static void FireOnNextUpdate(Action action) => Instance.ModHelper.Events.Unity.FireOnNextUpdate(action);

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(PlayAsGabbro)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene == OWScene.SolarSystem || loadScene == OWScene.EyeOfTheUniverse)
                {
                    FireOnNextUpdate(() =>
                    {
                        Locator.GetPlayerSuit().SuitUp(false, true, true);
                        SkinReplacer.ReplaceSkin(GameObject.Find("Player_Body"), true);
                        
                        // Spawn player
                        var island = GameObject.Find("GabbroIsland_Body");
                        var player = Locator.GetPlayerBody();

                        var worldPosition = island.transform.TransformPoint(new Vector3(38.325f, 18.537f, 6.619f));
                        var worldRotation = island.transform.rotation;
                        player.WarpToPositionRotation(worldPosition, worldRotation);
                        player.SetVelocity(island.GetAttachedOWRigidbody().GetVelocity());
                        var campfire = island.GetComponentInChildren<Campfire>();
                        player.transform.LookAt(campfire.transform, island.transform.up);

                        // Spawn ship
                        var ship = Locator.GetShipBody();
                        var shipWorldPosition = island.transform.position + player.transform.forward * 80f;

                        // Move position into the water
                        var giantsDeep = Locator.GetAstroObject(AstroObject.Name.GiantsDeep);
                        var relativePosition = giantsDeep.transform.InverseTransformPoint(shipWorldPosition);
                        var inWaterRelativePosition = relativePosition.normalized * 500f;
                        var finalShipWorldPosition = giantsDeep.transform.TransformPoint(inWaterRelativePosition);

                        ship.WarpToPositionRotation(finalShipWorldPosition, worldRotation);
                        ship.SetVelocity(island.GetAttachedOWRigidbody().GetVelocity());

                        if (!Physics.autoSyncTransforms)
                        {
                            Physics.SyncTransforms();
                        }
                    });
                }
            };
        }
    }
}