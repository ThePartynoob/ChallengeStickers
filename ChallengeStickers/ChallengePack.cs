using System;
using System.Collections.Generic;
using System.Linq;
using ChallengeStickers;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using UnityEngine;

internal class ChallengeStickerPackAnimator : MonoBehaviour
{
    internal Sprite[] sprites;
    internal float framesPerSecond = 12f;
    internal SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;
    void Start()
    {
        spriteRenderer.sprite = sprites[0];
    
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / framesPerSecond)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[currentFrame];
        }
    }



}

public class ITM_ChallengeStickerPack : Item
{
    public override bool Use(PlayerManager pm)
    {
        BasePlugin.Instance.GiveChallengeStickers(BasePlugin.Instance.ChallengeStickerPackStickers,2,true,false);
        Destroy(base.gameObject);
        return true;
    }
}