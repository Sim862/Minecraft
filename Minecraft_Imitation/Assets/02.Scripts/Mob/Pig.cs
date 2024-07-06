using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pig : Mob
{

    public Transform temp;
    

    void Update()
    {
        if (init_test)
        {
            init_test = false;
            initEntitiy(c_x, c_z, b_x, b_y, b_z);
        }

        if (start_test)
        {
            start_test = false;
            UpdateHP(temp, -1, 3);
        }

        LifeCycle();
    }

    private void LifeCycle()
    {
        if (alive)
        {
            Fall();
            positionData = MapManager.instance.PositionToBlockData(transform.position);

            if (mobState == MobState.Idle)
            {
                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    AStar_Random();
                    SetWayPosition();
                }
            }
            else if (mobState == MobState.Hit)
            {

                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    Runaway();
                }
            }
            else if(mobState == MobState.Move)
            {
                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= -2)
                {
                    nextMovementTime = 5;
                    AStar_Random();
                    SetWayPosition();
                }
            }

            Movement();
        }
    }
}
