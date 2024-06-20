using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BlockData;
using static GameObjectData;
using static ItemData;

[System.Serializable]
public class GameObjectData
{
    public GameObjectData(ObjectKind objectKind)
    {
        this.objectKind = objectKind;
    }

    public enum ObjectKind
    {
        None,
        Block,
        Item,
        Mob,
        Particle
    }

    public Sprite icon;
    public ObjectKind objectKind;

}

[System.Serializable]
public class BlockData : GameObjectData
{
    public BlockData(ObjectKind objectKind, BlockKind blockKind, BlockType blockType, float strength, Sound.AudioClipName brockBreakSound, Sound.AudioClipName brockBrokenSound) : base(objectKind)
    {
        this.blockKind = blockKind;
        this.blockType = blockType;
        this.strength = strength;
        this.brockBreakSound = brockBreakSound;
        this.brockBrokenSound = brockBrokenSound;
    }

    public enum BlockKind
    {
        None,
        Dirt,
        Wood,
        Water,
        Stone
    }
    public enum BlockType
    {
        None,
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe
    }

    public BlockKind blockKind;
    public BlockType blockType;
    public float strength;
    public Sound.AudioClipName brockBreakSound;
    public Sound.AudioClipName brockBrokenSound;
}

[System.Serializable]
public class ItemData : GameObjectData
{
    public ItemData(ObjectKind objectKind, ItemKind itemKind) : base(objectKind)
    {
        this.itemKind = itemKind;
    }

    public enum ItemKind
    {
        None,
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe
    }

    public ItemKind itemKind;
}

[System.Serializable]
public class MobData : GameObjectData
{
    public MobData(ObjectKind objectKind, MobKind mobKind) : base(objectKind)
    {
        this.mobKind = mobKind;
    }
    public enum MobKind
    {
        None,
        Pig,
        chicken,
        Creeper,
    }

    public MobKind mobKind;
}



