using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static BlockData;
using static GameObjectData;
using static ObjectParticleData;
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
    private ObjectKind objectKind;

    public ObjectKind GetObjectKind()
    {
        return objectKind;
    }

}

[System.Serializable]
public class BlockData : GameObjectData
{
    public BlockData(BlockName blockKind, BlockType blockType, float strength, Sound.AudioClipName brockBreakSound, Sound.AudioClipName brockBrokenSound, ObjectParticleData.ParticleName objectParticle) : base(ObjectKind.Block)
    {
        this.blockName = blockKind;
        this.blockType = blockType;
        this.strength = strength;
        this.brockBreakSound = brockBreakSound;
        this.brockBrokenSound = brockBrokenSound;
        this.objectParticle = objectParticle;
    }
    public BlockData(BlockData blockData) : base(ObjectKind.Block)
    {
        this.blockName = blockData.blockName;
        this.blockType = blockData.blockType;
        this.strength = blockData.strength;
        this.brockBreakSound = blockData.brockBreakSound;
        this.brockBrokenSound = blockData.brockBrokenSound;
        this.objectParticle = blockData.objectParticle;
    }
    public enum BlockName
    {
        None,
        Dirt,
        Wood,
        Water,
        Stone,
        WoodenPlank,
        Leaves,
        CraftingTable,
    }
    public enum BlockType
    {
        None,
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe,
        Bucket
    }

    public BlockName blockName;
    public BlockType blockType;
    public float strength;
    public Sound.AudioClipName brockBreakSound;
    public Sound.AudioClipName brockBrokenSound;
    public Material material;
    public ObjectParticleData.ParticleName objectParticle; // 블럭 파괴시 나오는 아이템
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
        Spider,
        Skeleton_Arrow,
    }

    public MobKind mobKind;
}


public class ObjectParticleData : GameObjectData
{
    public ObjectParticleData(ParticleName particleName, ParticleType particleType) : base(ObjectKind.ObjectParticle)
    {
        this.particleName = particleName;
        this.particleType = particleType;
    }
    public enum ParticleName // 플레이어가 먹거나 뱉을 수 있는 오브젝트 목록
    {
        None,
    //  Block ------------------------------------------------------------------------
        Dirt,
        Wood,
        Water,
        Stone,
        WoodenPlank,
        Leaves,
        CraftingTable,


    //  Item  ----------------------------------------------------------------------- 
        Stick,
        Knife,
        Ax,
        Shovel,
        WoodenPickaxe, StonePickaxe, IronPickaxe, GoldenPickaxe, DiamondPickaxe,
        Pick,
        Hoe,
        Bow,
        Arrow,

        //  Food  -----------------------------------------------------------------------
        RawPorkchop,

    }

    public enum ParticleType
    {
        None,
        Block,
        Tool,
        Food,
    }

    public ParticleName particleName;
    public ParticleType particleType;
}

[System.Serializable]
public class BlockMaterial
{
    public BlockData.BlockName blockKind;
    public Material material;
}

[System.Serializable]
public class UIItem
{
    public ObjectParticleData.ParticleName particleKind;
    public Sprite icon;
}

public class CombinationData
{
    public CombinationData(ParticleName result, int count, List<ParticleName> particleKinds)
    {
        this.result = result;
        this.count = count;
        int y = 1;
        int check = 0;
        for (int i = 0; i < particleKinds.Count; i++)
        {
            check++;
            if (particleKinds[i] != ParticleName.None)
            {
                this.y = y;
                if (check > x)
                {
                    x = check;
                }
            }
            if (check >= 3)
            {
                check = 0;
                y++;
            }
        }
        // None아닌 데이터 제일 앞으로 오게 정렬
        for (int i = 0; i < particleKinds.Count; i++)
        {
            if (particleKinds[i] != ObjectParticleData.ParticleName.None)
            {
                break;
            }
            else
            {
                particleKinds.RemoveAt(i);
                i--;
            }
        }
        // Count가 9가 될때 까지 None 넣어줌
        while (particleKinds.Count < 9)
        {
            particleKinds.Add(ObjectParticleData.ParticleName.None);
        }


        this.particleKinds = particleKinds.ToArray();
    }

    public ParticleName result { get; private set; }
    public ParticleName[] particleKinds { get; private set; }= new ParticleName[9];
    public int count { get; private set; }
    public int x { get; private set; } = 0;
    public int y { get; private set; } = 0;
}