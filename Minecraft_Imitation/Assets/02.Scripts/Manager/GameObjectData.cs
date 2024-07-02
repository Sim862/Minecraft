using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BlockData;
using static GameObjectData;
using static ToolData;

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
        Tool,
        Mob,
        ObjectParticle
    }
    public ObjectKind objectKind;

}

[System.Serializable]
public class BlockData : GameObjectData
{
    public BlockData(BlockKind blockKind, BlockType blockType, float strength, Sound.AudioClipName brockBreakSound, Sound.AudioClipName brockBrokenSound) : base(ObjectKind.Block)
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
    public ObjectParticleData.ParticleKind objectParticle; // �� �ı��� ������ ������
}

[System.Serializable]
public class ToolData : GameObjectData
{
    public ToolData(BlockType blockType, float power) : base(ObjectKind.Tool)
    {
        this.blockType = blockType;
        this.power = power;
    }

    public BlockType blockType;
    public float power;
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
    public ObjectParticleData(ParticleKind particleKind) : base(ObjectKind.ObjectParticle)
    {
        this.particleKind = particleKind;
    }
    public enum ParticleKind // �÷��̾ �԰ų� ���� �� �ִ� ������Ʈ ���
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

[System.Serializable]
public class BlockMaterial
{
    public BlockData.BlockKind blockKind;
    public Material material;
}

[System.Serializable]
public class UIItem
{
    public ObjectParticleData.ParticleKind particleKind;
    public Sprite icon;
}

