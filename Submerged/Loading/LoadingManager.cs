using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Submerged.Loading;

[RegisterInIl2Cpp]
public sealed class LoadingManager(nint ptr) : MonoBehaviour(ptr)
{
    public static readonly List<string> loadingers = [];

    public GameObject loadingScreen;

    public void Update()
    {
        if (AccountManager.InstanceExists && !loadingScreen)
        {
            loadingScreen = Instantiate(AccountManager.Instance.waitingText, transform, true);
            loadingScreen.GetComponentInChildren<TimeOutableScreen>().Destroy();
        }

        if (loadingers.Any()) return;
        loadingScreen.SetActive(false);
        enabled = false;
    }

    public static void RegisterLoading(string name)
    {
        if (loadingers.Contains(name)) return;
        Info($"Loading {name}");
        loadingers?.Add(name);
    }

    public static void DoneLoading(string name)
    {
        if (!loadingers.Contains(name)) return;
        Info($"Loaded {name}");
        loadingers?.Remove(name);
    }
}
