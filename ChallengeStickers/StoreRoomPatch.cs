using ChallengeStickers;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[HarmonyPatch(typeof(StoreRoomFunction))]
    internal class StoreRoomPatches
    {
        private static StoreRoomFunction storeFunc;

        private static PriceTag priceTagPre;

        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        private static void OnInitialize(StoreRoomFunction __instance, RoomController room, ref PriceTag ___mapTag, ref SceneObject ___storeData)
        {
            if (___storeData == null) return;

            storeFunc = __instance;
            priceTagPre = ___mapTag;

            

            PriceTag StickerChallengePackTag = null;

            StickerChallengePackTag = CreatePriceTag("150");
            StickerChallengePackTag.transform.localPosition = new Vector3(35f, 2.65f, 23f);
            var a =CreatePickup<ChallengeStickerPackPickup>(StickerChallengePackTag, 150, new Vector3(35f, 5f, 23f));
            
        }

        private static T CreatePickup<T>(PriceTag tag, int price, Vector3 pos) where T : BasePickup
        {
            Pickup pickupComp = GameObject.Instantiate(BasePlugin.PickupPre, storeFunc.Room.objectObject.transform);

            T pickup = pickupComp.gameObject.AddComponent<T>();
            pickup.name = "Pickup";
            pickup.Initialize(pickup.GetComponentInChildren<SpriteRenderer>(), tag, price);
            pickup.transform.position = pos;
          

            pickup.onPickupPurchasing += delegate ()
             {
                 if (BuyingItem(pickup.Price, tag, out int ytpCollected))
                 {
                     pickup.OnPurchasing(ytpCollected);
                 } else if (!(bool)storeFunc.ReflectionGetVariable("open"))
                 {
                     pickup.OnStealing();
                     if (!(bool)storeFunc.ReflectionGetVariable("alarmStarted") && pickup.RaiseAlarmDuringRobbery)
                         storeFunc.ReflectionInvoke("SetOffAlarm", new object[] { });
                 }
             };

            GameObject.Destroy(pickupComp);

            return pickup;
        }

        private static PriceTag CreatePriceTag(string text)
        {
            PriceTag priceTag = GameObject.Instantiate(priceTagPre);
            priceTag.transform.SetParent(storeFunc.Room.objectObject.transform);
            priceTag.SetText(text);
            return priceTag;
        }

        public static void PlayJohnnyUnafforable(StoreRoomFunction func = null)
        {
            if (func == null) func = storeFunc;
            if (func == null) return;

            PropagatedAudioManagerAnimator audMan = 
                (PropagatedAudioManagerAnimator)func.ReflectionGetVariable("johnnyAudioManager");
            if (!audMan.QueuedUp)
            {
                audMan.QueueRandomAudio((SoundObject[])func.ReflectionGetVariable("audUnafforable"));
            }
        }
        
        public static void PlayJohnnyBuy(StoreRoomFunction func = null)
        {
            if (func == null) func = storeFunc;
            if (func == null) return;

            PropagatedAudioManagerAnimator audMan = 
                (PropagatedAudioManagerAnimator)func.ReflectionGetVariable("johnnyAudioManager");
            if (!audMan.QueuedUp)
            {
                audMan.QueueRandomAudio((SoundObject[])func.ReflectionGetVariable("audBuy"));
            }

            storeFunc.ReflectionSetVariable("itemPurchased", true);
            storeFunc.ReflectionSetVariable("playerLeft", false);
        }

        public static bool BuyingItem(int price, PriceTag priceTag, out int ytpCollected)
        {
            ytpCollected = 0;
            if ((bool)storeFunc.ReflectionGetVariable("open"))
            {
                PropagatedAudioManagerAnimator audMan = (PropagatedAudioManagerAnimator)storeFunc.ReflectionGetVariable("johnnyAudioManager");
                bool isPossibleToBuy = Singleton<CoreGameManager>.Instance.GetPoints(0) >= price;

                if (!isPossibleToBuy && !Singleton<CoreGameManager>.Instance.johnnyHelped && Math.Abs(price - Singleton<CoreGameManager>.Instance.GetPoints(0)) <= 100)
                {
                    price = Singleton<CoreGameManager>.Instance.GetPoints(0);
                    Singleton<CoreGameManager>.Instance.johnnyHelped = true;
                    audMan.FlushQueue(true);
                    audMan.QueueAudio((SoundObject)storeFunc.ReflectionGetVariable("audHelp"));
                } else if (!isPossibleToBuy)
                {
                    if (!audMan.QueuedUp)
                    {
                        audMan.QueueRandomAudio((SoundObject[])storeFunc.ReflectionGetVariable("audUnafforable"));
                    }
                    return false;
                }

                ytpCollected = price;

                Singleton<CoreGameManager>.Instance.AddPoints(-price, 0, true);

                priceTag.SetText(Singleton<LocalizationManager>.Instance.GetLocalizedText("TAG_Sale"));

                storeFunc.ReflectionSetVariable("itemPurchased", true);
                storeFunc.ReflectionSetVariable("playerLeft", false);
                
                return true;
            }
            return false;
        }

        public class BasePickup : MonoBehaviour, IClickable<int>
    {
        [SerializeField]
        protected SpriteRenderer renderer;

        protected PriceTag priceTag;

        protected int price;

        protected float nonClickableTime;

        protected bool purchasable;

        protected string desc;

        public virtual bool RaiseAlarmDuringRobbery => true; //Invokes SetOffAlarm()

        public bool Purchasable => purchasable;

        public int Price => price;

        public Action onPickupClick;

        public Action onPickupPurchasing;

        public void Initialize(SpriteRenderer renderer, PriceTag priceTag, int price)
        {
            this.renderer = renderer;
            this.priceTag = priceTag;
            this.price = price;
            purchasable = true;
            OnCreationPost();
        }

        public virtual void OnStealing()
        {

        }

        public virtual void OnPurchasing(int spentYTPs)
        {
            SetSaleState(false);
        }

        protected virtual void OnCreationPost()
        {

        }

        private void Update()
        {
            if (nonClickableTime > 0f) nonClickableTime -= Time.deltaTime;
            VirtualUpdate();
        }

        protected virtual void VirtualUpdate()
        {

        }

        public void Clicked(int player)
        {
            if (!ClickableHidden())
            {
                VirtualClicked(player);
                onPickupClick?.Invoke();
                if (Purchasable) onPickupPurchasing?.Invoke();
            }
        }

        protected virtual void VirtualClicked(int player)
        {

        }

        public void SetSaleState(bool active, bool markAsSoldIfNot = true)
        {
            if (active)
            {
                purchasable = true;
                if (priceTag != null) SetPrice(price);
                renderer.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                purchasable = false;
                if (priceTag != null) priceTag.SetText(Singleton<LocalizationManager>.Instance.GetLocalizedText(
                    markAsSoldIfNot ? "TAG_Sold" : "Adv_Tag_Out"));
                if (renderer != null) renderer.color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

        protected void SetPrice(int price)
        {
            this.price = price;
            if (priceTag != null) priceTag.SetText(price.ToString());
        }

        public virtual bool ClickableHidden()
        {
            return nonClickableTime > 0f;
        }

        public virtual bool ClickableRequiresNormalHeight()
        {
            return true;
        }

        public virtual void ClickableSighted(int player)
        {
            if (desc != null) Singleton<CoreGameManager>.Instance.GetHud(player).SetTooltip(desc);
        }

        public virtual void ClickableUnsighted(int player)
        {
            if (desc != null) Singleton<CoreGameManager>.Instance.GetHud(player).CloseTooltip();
        }

    }

    public class ChallengeStickerPackPickup : BasePickup
    {
        protected override void OnCreationPost()
        {
            SetSaleState(true,false);
            desc = "desc_challenge_sticker_pack";
            var a =renderer.gameObject.AddComponent<ChallengeStickerPackAnimator>();
            a.spriteRenderer = renderer;
            a.sprites = BasePlugin.Instance.ChallengeStickerPacketSprites;
            a.framesPerSecond = 12f;

        }
        public override void OnPurchasing(int spentYTPs)
        {
            base.OnPurchasing(spentYTPs);
            BasePlugin.Instance.GiveChallengeStickers(BasePlugin.Instance.ChallengeStickerPackStickers, 2, true, false);
            
        }

    }
}