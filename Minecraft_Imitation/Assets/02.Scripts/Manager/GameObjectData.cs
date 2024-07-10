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
    public BlockData(BlockKind blockKind, BlockType blockType, float strength, Sound.AudioClipName brockBreakSound, Sound.AudioClipName brockBrokenSound, ObjectParticleData.ParticleKind objectParticle) : base(ObjectKind.Block)
    {
        this.blockKind = blockKind;
        this.blockType = blockType;
        this.strength = strength;
        this.brockBreakSound = brockBreakSound;
        this.brockBrokenSound = brockBrokenSound;
        this.objectParticle = objectParticle;
    }
    public BlockData(BlockData blockData) : base(ObjectKind.Block)
    {
        this.blockKind = blockData.blockKind;
        this.blockType = blockData.blockType;
        this.strength = blockData.strength;
        this.brockBreakSound = blockData.brockBreakSound;
        this.brockBrokenSound = blockData.brockBrokenSound;
        this.objectParticle = blockData.objectParticle;
    }
    public enum BlockKind
    {
        None,
        Dirt,
        Wood,
        Water,
        Stone,
        WoodenPlank
    }
    public enum BlockType
    {
        Knife,
        Ax,
        Shovel,
        Pick,
        Hoe,
        Bucket
    }

    public BlockKind blockKind;
    public BlockType blockType;
    public float strength;
    public Sound.AudioClipName brockBreakSound;
    public Sound.AudioClipName brockBrokenSound;
    public Material material;
    public ObjectParticleData.ParticleKind objectParticle; // 블럭 파괴시 나오는 아이템
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
    public enum ParticleKind // 플레이어가 먹거나 뱉을 수 있는 오브젝트 목록
    {
        None,
    //  Block ------------------------------------------------------------------------
        Dirt,
        Wood,
        Water,
        Stone,
        WoodenPlank,

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

public class CombinationData
{
    public CombinationData(ParticleKind result, int count, List<ParticleKind> particleKinds)
    {
        this.result = result;
        this.count = count;
        int y = 1;
        int check = 0;
        for (int i = 0; i < particleKinds.Count; i++)
        {
            check++;
            if (particleKinds[i] != ParticleKind.None)
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
            if (particleKinds[i] != ObjectParticleData.ParticleKind.None)
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
            particleKinds.Add(ObjectParticleData.ParticleKind.None);
        }


        this.particleKinds = particleKinds.ToArray();
    }

    public ParticleKind result { get; private set; }
    public ParticleKind[] particleKinds { get; private set; }= new ParticleKind[9];
    public int count { get; private set; }
    public int x { get; private set; } = 0;
    public int y { get; private set; } = 0;
}