using HarmonyLib;

namespace ObenFind
{
    internal static class Patches
    {
        public delegate void StorageEvent(Storage storage);
        public static event StorageEvent OnEnteredExternally;

        [HarmonyPatch(typeof(Storage), nameof(Storage.EnterExternally)), HarmonyPostfix]
        static void Storage_EnterExternally(Storage __instance)
            => OnEnteredExternally?.Invoke(__instance);
    }
}
