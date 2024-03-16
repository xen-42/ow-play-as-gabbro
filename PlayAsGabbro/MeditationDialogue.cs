using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayAsGabbro;

internal static class MeditationDialogue
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
    }
}
