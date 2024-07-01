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
        ObjectParticle
    }

    public Sprite icon;
    public ObjectKind objectKind;

}

[System.Serializable]
public class BlockData : GameObjectData
{
    public BlockData(ObjectKind objectKind, BlockKind blockKind, BlockType blockType, float strength, Sound.AudioClipName brockBreakSound, Sound.AudioClipName brockBrokenSound) : base(ObjectKind.Block)
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
    public Material material;
}

[System.Serializable]
public class ItemData : GameObjectData
{
    public ItemData(ObjectKind objectKind, BlockType blockType) : base(ObjectKind.Item)
    {
        this.blockType = blockType;
    }

    public BlockType blockType;
}

[System.Serializable]
public class MobData : GameObjectData
{
    public MobData(MobKind mobKind) : base(ObjectKind.Mob)
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


public class ObjectParticleData : GameObjectData
{
    public ObjectParticleData(ObjectKind objectKind, ParticleKind particleKind) : base(ObjectKind.ObjectParticle)
    {
        this.particleKind = particleKind;
    }
    public enum ParticleKind // 플레이어가 먹거나 뱉을 수 있는 오브젝트 목록
    {
    //  Block ------------------------------------------------------------------------
        Dirt,
        Wood,
        Water,
        Stone,

    //  Item  ----------------------------------------------------------------------- 
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe
    }

    public ParticleKind particleKind;
}


