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
    public BlockData(ObjectKind objectKind, BlockKind blockKind, BlockType blockType, float strength) : base(objectKind)
    {
        this.blockKind = blockKind;
        this.blockType = blockType;
        this.strength = strength;
    }

    public enum BlockKind
    {
        Dirt,
        Wood,
        Water,
        Stone
    }
    public enum BlockType
    {
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe
    }

    public BlockKind blockKind;
    public BlockType blockType;
    public float strength;
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
        Pig,
        chicken,
        Creeper,
    }

    public MobKind mobKind;
}



