using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace PlayAsGabbro
{
    public class PlayAsGabbro : ModBehaviour
    {
        public static PlayAsGabbro Instance { get; private set; }

        public INewHorizons NewHorizons { get; private set; }

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

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            DialogueConditionHandler.Setup();

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                // If they skipped the radio on the first loop make sure to set the condition
                if (TimeLoop._loopCount > 1)
                {
                    PlayerData.SetPersistentCondition("SpokeToRadio", true);
                }

                if (loadScene == OWScene.SolarSystem || loadScene == OWScene.EyeOfTheUniverse)
                {
                    FireOnNextUpdate(SuitUpAsGabbro);
                }
                if (loadScene == OWScene.SolarSystem)
                {
                    // TODO: Only do this in the main solar system (NH compat)
                    StartCoroutine(SpawnCoroutine());
                }
            };
        }

        private void SuitUpAsGabbro()
        {
            Locator.GetPlayerSuit().SuitUp(false, true, true);
            SkinReplacer.ReplaceSkin(GameObject.Find("Player_Body"), true);
        }

        private IEnumerator SpawnCoroutine()
        {
            yield return new WaitForFixedUpdate();

            // First loop stuff before pairing
            if (TimeLoop._loopCount == 1 && !TimeLoop.IsTimeFlowing())
            {
                var firstLoopStuff = new GameObject("FirstLoopStuff");
                firstLoopStuff.AddComponent<GabbroMemoryUplinkFixer>();

                // Lock ship hatch until paired
                var hatch = GameObject.FindObjectOfType<HatchController>();
                hatch.CloseHatch();
                hatch.transform.parent.Find("TractorBeam").gameObject.SetActive(false);

                // Custom hornfels dialogue
                var hornfelsDialogue = GameObject.Find("TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Characters_Observatory/Villager_HEA_Hornfels/ConversationZone_Hornfels");
                hornfelsDialogue.gameObject.SetActive(false);

                var hornfels = hornfelsDialogue.transform.parent;
                var (dialogue, _) = NewHorizons.SpawnDialogue(this, Locator.GetAstroObject(AstroObject.Name.TimberHearth).gameObject, "planets/text/Hornfels.xml",
                    pathToAnimController: "Sector_TH/Sector_Village/Sector_Observatory/Characters_Observatory/Villager_HEA_Hornfels");
                dialogue.transform.parent = hornfels;
                dialogue.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            }

            // Repeat it else we get tossed around by physics and stuff
            for (int i = 0; i < 10; i++)
            {
                OnPlayerSpawn();

                yield return new WaitForFixedUpdate();
            }
        }

        private void CloseHatch()
        {

        }

        private void OnPlayerSpawn()
        {
            var gabbroIsland = GameObject.Find("GabbroIsland_Body");
            var statueIsland = GameObject.Find("StatueIsland_Body");
            var player = Locator.GetPlayerBody();
            var ship = Locator.GetShipBody();

            // Spawn on Gabbro island
            if (TimeLoop._loopCount > 1)
            {
                // Spawn player
                var worldPosition = gabbroIsland.transform.TransformPoint(new Vector3(38.325f, 18.537f, 6.619f));
                var worldRotation = gabbroIsland.transform.rotation;
                player.WarpToPositionRotation(worldPosition, worldRotation);
                player.SetVelocity(gabbroIsland.GetAttachedOWRigidbody().GetVelocity());
                var campfire = gabbroIsland.GetComponentInChildren<Campfire>();
                player.transform.LookAt(campfire.transform, gabbroIsland.transform.up);

                // Spawn ship
                var shipWorldPosition = gabbroIsland.transform.position + player.transform.forward * 80f;

                // Move ship position into the water
                var giantsDeep = Locator.GetAstroObject(AstroObject.Name.GiantsDeep);
                var relativePosition = giantsDeep.transform.InverseTransformPoint(shipWorldPosition);
                var inWaterRelativePosition = relativePosition.normalized * 500f;
                var finalShipWorldPosition = giantsDeep.transform.TransformPoint(inWaterRelativePosition);

                ship.WarpToPositionRotation(finalShipWorldPosition, worldRotation);
                ship.SetVelocity(gabbroIsland.GetAttachedOWRigidbody().GetVelocity());
            }
            // Spawn on statue island (first loop)
            else
            {
                // Spawn player
                var worldPosition = statueIsland.transform.TransformPoint(new Vector3(-15.76197f, 1.707366f, -73.72968f));
                var worldRotation = statueIsland.transform.rotation;

                // Check if we didn't just respawn after linking (PlayerSpawner will handle us in that case)
                if (!TimeLoop.IsTimeFlowing())
                {
                    // First spawn
                    player.WarpToPositionRotation(worldPosition, worldRotation);
                    player.SetVelocity(statueIsland.GetAttachedOWRigidbody().GetVelocity());
                }

                // Spawn ship
                var shipWorldPosition = statueIsland.transform.TransformPoint(new Vector3(-27.47425f, -0.6602894f, -86.83144f)) + statueIsland.transform.up;
                var shipWorldRotation = statueIsland.transform.rotation;

                ship.WarpToPositionRotation(shipWorldPosition, shipWorldRotation);
                ship.SetVelocity(statueIsland.GetAttachedOWRigidbody().GetVelocity());
            }

            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }
        }
    }
}