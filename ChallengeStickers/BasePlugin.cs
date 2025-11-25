using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using UnityEngine;
namespace ChallengeStickers;


[BepInPlugin("Partynoob.ChallengeStickers", "Challenge Stickers", "1.1.0")]
public class BasePlugin : BaseUnityPlugin
{
    public static BasePlugin Instance;
    public static AssetManager ASM;

    string[] stickerEnumsToRegister = new string[]
    {
        "MoreMMProblems",
        "BiggerLevel",
        "LessTime",
        "MoreNotebooks",
        "BaldBackup",
        "LessItems",
        "MinusSlot",
        "SpeedUp"
    };

    public static Dictionary<string, Sticker> StickersEnum = new Dictionary<string, Sticker>();

    public WeightedSticker[] ChallengeStickerPackStickers;

    public static Pickup PickupPre;

    public Sprite[] ChallengeStickerPacketSprites;
    IEnumerator PreLoad()
    {
        yield return 3;
        yield return "Registering sticker enums";
        foreach (var enumName in stickerEnumsToRegister)
        {
            var stickerEnum = EnumExtensions.ExtendEnum<Sticker>(enumName);
            StickersEnum.Add(enumName, stickerEnum);
        }
        yield return "Loading sprites";
        ASM.Add<Sprite>("spr_sticker_moreproblems", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"onemoreproblemmathmachine.png"));
        ASM.Add<Sprite>("spr_sticker_biggerlevel", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"BiggerLevel.png"));
        ASM.Add<Sprite>("spr_sticker_lesstime", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"LessTime.png"));
        ASM.Add<Sprite>("spr_sticker_morenotebooks", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"MoreNotebooks.png"));
        ASM.Add<Sprite>("spr_sticker_baldbackup", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"BaldBackup.png"));
        ASM.Add<Sprite>("spr_sticker_lessitems", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"LessItems.png"));
        ASM.Add<Sprite>("spr_sticker_minusslot", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"MinusSlot.png"));
        ASM.Add<Sprite>("spr_sticker_speedup", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"SpeedUp.png"));
        

        ASM.Add<Sprite>("spr_challengestickerpacket_sheet", AssetLoader.SpriteFromMod(this, Vector2.one/2,16,"ChallengeStickerPacket_Spin_Sheet.png"));
        ChallengeStickerPacketSprites = AssetLoader.SpritesFromSpritesheet(4,3,50,Vector2.one/2,ASM.Get<Sprite>("spr_challengestickerpacket_sheet").texture);
        
        yield return "Loading stickers";
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_moreproblems"))
            .SetEnum(StickersEnum["MoreMMProblems"])
            .SetAsBonusSticker()
            .SetDuplicateOddsMultiplier(0.7f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_biggerlevel"))
            .SetEnum(StickersEnum["BiggerLevel"])
            .SetAsBonusSticker()
            .SetAsAffectingGenerator()
            .SetDuplicateOddsMultiplier(0.85f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_lesstime"))
            .SetEnum(StickersEnum["LessTime"])
            .SetAsBonusSticker()
            .SetDuplicateOddsMultiplier(0.9f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_morenotebooks"))
            .SetEnum(StickersEnum["MoreNotebooks"])
            .SetAsBonusSticker()
            .SetAsAffectingGenerator()
            .SetDuplicateOddsMultiplier(0.65f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_baldbackup"))
            .SetEnum(StickersEnum["BaldBackup"])
            .SetAsBonusSticker()
            .SetAsAffectingGenerator()
            .SetDuplicateOddsMultiplier(0.33f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_lessitems"))
            .SetEnum(StickersEnum["LessItems"])
            .SetAsBonusSticker()
            .SetAsAffectingGenerator()
            .SetDuplicateOddsMultiplier(0.5f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_minusslot"))
            .SetEnum(StickersEnum["MinusSlot"])
            .SetAsBonusSticker()
            .SetDuplicateOddsMultiplier(0.5f)
            .Build();
        new StickerBuilder<ExtendedStickerData>(this.Info)
            .SetSprite(ASM.Get<Sprite>("spr_sticker_speedup"))
            .SetEnum(StickersEnum["SpeedUp"])
            .SetAsBonusSticker()
            .SetDuplicateOddsMultiplier(0.5f)
            .Build();

        ASM.Add("ITM_Challengestickerpacket",new ItemBuilder(this.Info)
        .SetSprites(ChallengeStickerPacketSprites[0], ChallengeStickerPacketSprites[0])
        .SetShopPrice(150)
        .SetNameAndDescription("challengestickerpacket", "challengestickerpack_desc")
        .SetAsInstantUse()
        .SetAsNotOverridable()
        .SetEnum("ITM_Challengestickerpacket")
        .Build());
        PickupPre = Resources.FindObjectsOfTypeAll<Pickup>().First(x => x.gameObject.activeSelf);
    }

    void Awake()
    {
        ASM = new AssetManager();
        Instance = this;
        MTM101BaldAPI.Registers.LoadingEvents.RegisterOnAssetsLoaded(this.Info, PreLoad(), MTM101BaldAPI.Registers.LoadingEventOrder.Pre);
        MTM101BaldAPI.Registers.LoadingEvents.RegisterOnAssetsLoaded(this.Info, PostLoad(), MTM101BaldAPI.Registers.LoadingEventOrder.Post);
        new HarmonyLib.Harmony(this.Info.Metadata.GUID).PatchAll();
        MTM101BaldAPI.AssetTools.AssetLoader.LocalizationFromMod(this);
        

        ChallengeStickerPackStickers = new WeightedSticker[]
        {
            new WeightedSticker(StickersEnum["MoreMMProblems"],35),
            new WeightedSticker(StickersEnum["BiggerLevel"],65),
            new WeightedSticker(StickersEnum["LessTime"],90),
            new WeightedSticker(StickersEnum["MoreNotebooks"],140),
            new WeightedSticker(StickersEnum["BaldBackup"],20),
            new WeightedSticker(StickersEnum["LessItems"],75),
            new WeightedSticker(StickersEnum["MinusSlot"],50),
            new WeightedSticker(StickersEnum["SpeedUp"],40),
        };
        
    }

    IEnumerator PostLoad() {
        yield return 1;
        yield return "Modifying the store";
        RoomAsset StoreRoom = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => ((UnityEngine.Object)x).name == "Room_JohnnysStore");
        GameObject Counter = Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.name == "Counter");
        GameObject PickupPrefab = Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.name == "Pickup");

        StoreRoom.basicObjects.Add(new()
        {
            position = new Vector3(35,-1.6f,23),
            rotation = Quaternion.identity,
            prefab = Counter.transform
        });

       
    }


    public void GiveChallengeStickers(WeightedSticker[] potentialStickers, int amount, bool openNow, bool sticky)
    {
        List<WeightedSticker> _potentialStickersToAdd = new List<WeightedSticker>();
        StickerManager sm = Singleton<StickerManager>.Instance;
        foreach (WeightedSticker weightedSticker in ChallengeStickerPackStickers)
        {
            _potentialStickersToAdd.Add(new WeightedSticker(weightedSticker.selection, Mathf.RoundToInt((float)weightedSticker.weight * sm.GetStickerOddsMultiplier(weightedSticker.selection))));
        }

        for (int j = 0; j < amount; j++)
        {
            sm.stickerInventory.Add(new StickerStateData(_potentialStickersToAdd.RandomSelection(), 0, openNow, sticky));
            if (openNow)
            {
                Singleton<CoreGameManager>.Instance.GetHud(0).ShowCollectedSticker(sm.GetInventoryStickerSprite(sm.stickerInventory.Count - 1));
            }
        }   

    }
            
}


