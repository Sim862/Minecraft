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
        ObjectParticleData.ParticleKind a = DataManager.instance.GetCombinationData(new List<ObjectParticleData.ParticleKind>() { 
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.Wood,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
            ObjectParticleData.ParticleKind.None,
        });

        print(a);
    }

  
}
