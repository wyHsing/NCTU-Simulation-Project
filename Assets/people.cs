using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class people : MonoBehaviour
{
    public Transform goal;
    public Transform robot;
    private NavMeshAgent man;

    public float wanderRadius;
    public float wanderTimer;
    private float timer;
    private bool flag;
    // Use this for initialization
    void Start()
    {
        man = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        flag = false;
    }

    // Update is called once per frame
    void Update()
    {
        //man.destination = goal.position;
        if(flag == false)
        {
            timer += Time.deltaTime;
            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                man.SetDestination(newPos);
                timer = 0;
            }
            if (Vector3.Distance(man.transform.position, robot.position) <= 10.00)
            {
                flag = true;
                man.destination = goal.position;
            }
        }else
        {
            man.destination = goal.position;
        }
        
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}
