using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pig : Entity
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
        if (currHP > 0)
        {
            Fall();
            positionData = MapManager.instance.PositionToBlockData(transform.position);

            if (entityState == EntityState.Idle)
            {
                movementDelayTime -= Time.deltaTime;
                if (movementDelayTime <= 0)
                {
                    movementDelayTime = 100;
                    AStar_Random();
                    SetWayPosition();
                }
            }
            else if (entityState == EntityState.Hit)
            {

                movementDelayTime -= Time.deltaTime;
                if (movementDelayTime <= 0)
                {
                    movementDelayTime = 100;
                    Runaway();
                }
            }

            Movement();
        }
    }
}
