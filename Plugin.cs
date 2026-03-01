using BepInEx;
using HarmonyLib;
using System;

namespace ObenFind;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static GUIComponent GUIComponent;
    internal static InputHandler InputHandler;

    private void Awake()
    {
        Console.WriteLine($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        GUIComponent = gameObject.AddComponent<GUIComponent>();
        GUIComponent.enabled = false;

        InputHandler = gameObject.AddComponent<InputHandler>();

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(Patches));
    }
}
