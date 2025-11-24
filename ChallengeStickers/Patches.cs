using System.Linq;
using ChallengeStickers;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using UnityEngine;
[HarmonyPatch]
class Patches
{
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MathMachine), "Start")]
    static void MathMachine(MathMachine __instance)
    {
        __instance.ReflectionSetVariable("totalProblems", 1 + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MoreMMProblems"]));
        

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CoreGameManager), "YtpMultiplier", MethodType.Getter)]
    static void YtpMultiplier(ref float __result)
    {
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MoreMMProblems"]) * 0.40f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BiggerLevel"]) * 0.5f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessTime"]) * 0.125f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MoreNotebooks"]) * 0.85f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BaldBackup"]) * 1f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessItems"]) * 0.15f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MinusSlot"]) * 0.3f;
        __result *= 1f + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["SpeedUp"]) * 0.35f;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("PrepareLevelGenerationData")]
    static void PrepareLevelGenerationData(LevelGenerationParameters ___levelObject)
    {
        for (int i = 0; i < Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BaldBackup"]); i++)
        {
            var baldi = Resources.FindObjectsOfTypeAll<Baldi>().First(x => x.gameObject.name == "Baldi_Main3" );
            ___levelObject.forcedNpcs = ___levelObject.forcedNpcs.AddToArray(baldi);
        }
            if (Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BiggerLevel"]) > 0)
            {
                float percentage = Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BiggerLevel"]) * 0.3f;
                ___levelObject.minSize = new IntVector2(Mathf.CeilToInt(___levelObject.minSize.x + (___levelObject.minSize.x * percentage)), Mathf.CeilToInt(___levelObject.minSize.z + (___levelObject.minSize.z * percentage)));
                ___levelObject.maxSize = new IntVector2(Mathf.CeilToInt(___levelObject.maxSize.x + (___levelObject.maxSize.x * percentage)), Mathf.CeilToInt(___levelObject.maxSize.z + (___levelObject.maxSize.z * percentage)));
                ___levelObject.maxPlots += Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BiggerLevel"]);
                ___levelObject.minPlots += Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["BiggerLevel"]);
            }
            if (Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessTime"]) > 0)
        {
            float percentage = Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessTime"]) * 0.1f;
            ___levelObject.timeLimit /= 1f + percentage;
        }
        
            if (Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MoreNotebooks"]) > 0)
            {
                int extraNotebooks = Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MoreNotebooks"]) *2;
                var c = ___levelObject.roomGroup.First(x => x.name == "Class");
                var newC = new RoomGroup();
                newC.name = c.name;
                newC.minRooms = c.minRooms + extraNotebooks;
                newC.maxRooms = c.maxRooms + extraNotebooks;
                newC.ceilingTexture = c.ceilingTexture;
                newC.floorTexture = c.floorTexture;
                newC.wallTexture = c.wallTexture;
                newC.potentialRooms = c.potentialRooms;
                newC.light = c.light;
                newC.stickToHallChance = c.stickToHallChance;
                var index = ___levelObject.roomGroup.ToList().FindIndex(x => x.name == "Class");
                ___levelObject.roomGroup[index] = newC;
            }
            if (Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessItems"]) > 0)
{
    var newList = ___levelObject.potentialItems.ToList();
    Debug.Log("Original item count: " + newList.Count);

    float stickerValue = Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["LessItems"]);
    int amountToRemove = Mathf.CeilToInt(newList.Count * 0.10f * stickerValue) - ___levelObject.forcedItems.Count;

    Debug.Log("Removing: " + amountToRemove + " items");

    for (int i = 0; i < amountToRemove; i++)
    {
        if (newList.Count == 0)
            break;

        int index = UnityEngine.Random.Range(0, newList.Count);
        newList.RemoveAt(index);
    }

    Debug.Log("New item count: " + newList.Count);
    ___levelObject.potentialItems = newList.ToArray();
}
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemManager), "UpdateTargetInventorySize")]
    static bool UTIS(ItemManager __instance)
    {
        __instance.ReflectionSetVariable("targetInventorySize", __instance.defaultInventorySize + Singleton<StickerManager>.Instance.StickerValue(Sticker.InventorySlot) - (int)__instance.ReflectionGetVariable("inventoryShrinks") - Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["MinusSlot"]));
        __instance.maxItem = Mathf.Max((int)__instance.ReflectionGetVariable("targetInventorySize"), Mathf.Max(__instance.TotalItemsInInventory, __instance.MaxSlotWithItem + 1)) - 1;
        Singleton<CoreGameManager>.Instance.GetHud(__instance.pm.playerNumber).UpdateInventorySize(Mathf.Max((int)__instance.ReflectionGetVariable("targetInventorySize"), Mathf.Max(__instance.TotalItemsInInventory, __instance.MaxSlotWithItem + 1)));
        __instance.UpdateSelect();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EnvironmentController), "BeginPlay")]
    static void EnvBeginPlay(EnvironmentController __instance)
    {
        if (Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["SpeedUp"]) > 0)
        {
            __instance.AddTimeScale(new(1 + Singleton<StickerManager>.Instance.StickerValue(BasePlugin.StickersEnum["SpeedUp"]) * 0.15f,1,1));
        }
    }

 

}