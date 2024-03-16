using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayAsGabbro;

internal class GabbroMemoryUplinkFixer : MonoBehaviour
{
    private MemoryUplinkTrigger _statue;

    public void Start()
    {
        if (TimeLoop._loopCount > 1 || TimeLoop.IsTimeFlowing())
        {
            Component.Destroy(this);
            PlayAsGabbro.Log("Time loop is enabled!");
            return;
        }

        _statue = GameObject.Find("StatueIsland_Body/Sector_StatueIsland/NomaiStatueGabbro")?.GetComponentInChildren<MemoryUplinkTrigger>(true);
        if (_statue == null)
        {
            PlayAsGabbro.Log("Couldn't find the Statue Island statue!");
            Component.Destroy(this);
            return;
        }
    }

    public void Update()
    {
        if ((Locator.GetPlayerBody().transform.position - _statue.transform.position).magnitude < 5f && Locator.GetPlayerController().IsGrounded()) 
        {
            _statue._waitForPlayerGrounded = true;
            _statue.enabled = true;

            // Ok done bye bye!
            Component.Destroy(this);
        }
    }

}
