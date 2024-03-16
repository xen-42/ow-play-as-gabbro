using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayAsGabbro;

internal static class DialogueConditionHandler
{
    public static void Setup()
    {
        GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
    }

    private static void OnExitConversation()
    {
        if (DialogueConditionManager.SharedInstance.GetConditionState("HatchlingMeditate"))
        {
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
        }

        if (DialogueConditionManager.SharedInstance.GetConditionState("Cheater"))
        {
            Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
        }

        // In case they go straight to the radio then walk into the trigger
        if (TimeLoop._loopCount == 1 && PlayerData.GetPersistentCondition("SpokeToRadio"))
        {
            GameObject.Find("StatueIsland_Body/Sector_StatueIsland/RadioRemoteDialogueTrigger")?.gameObject?.SetActive(false);
        }
    }
}
