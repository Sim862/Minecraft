using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


public class NewBehaviourScript : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        CombinationData a = DataManager.instance.GetCombinationData(new List<ObjectParticleData.ParticleName>() { 
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.Wood,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
            ObjectParticleData.ParticleName.None,
        });

    }

  
}
