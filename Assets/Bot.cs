using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive ds;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive>();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }
    void Flee(Vector3 location)
    {
        Vector3 fleeVector =location - transform.position ;
        agent.SetDestination(transform.position - fleeVector);

        // second option
        /* Vector3 fleeVector =   transform.position - location;
         agent.SetDestination(transform.position + fleeVector);*/
    } 

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        // between forward direction
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        // angle btw forward dirction to the agent and the dirction to the target
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));

        if((toTarget > 90 && relativeHeading < 20) || ds.currentSpeed< 0.01)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    Vector3 wanderTarget = Vector3.zero;

    void Wander()
    {
        float wanderRadius = 10;
        float wanderDistance = 20;
        float wanderJitter = 1;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                        0,
                                        Random.Range(-1.0f, 1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);

    }

    void Hide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpot().Length; i++)
        {
            // vector from cop to the tree
            Vector3 hideDir = World.Instance.GetHidingSpot()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpot()[i].transform.position + hideDir.normalized * 10;

            if(Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }
        // either this or seek can be use.
        agent.SetDestination(chosenSpot);
      //  Seek(chosenSpot);
    }

    void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpot()[0];

        for (int i = 0; i < World.Instance.GetHidingSpot().Length; i++)
        {
            // vector from cop to the tree
            Vector3 hideDir = World.Instance.GetHidingSpot()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpot()[i].transform.position + hideDir.normalized * 10;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpot()[i];
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, - chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);

        // either this or seek can be use.
        agent.SetDestination(info.point + chosenDir.normalized *2);
        //  Seek(info.point + chosenDir.normalized * 5);
    }

    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - transform.position;
        float lookAngle = Vector3.Angle(transform.forward, rayToTarget);
        if(lookAngle < 60 && Physics.Raycast(transform.position, rayToTarget, out raycastInfo))
        {
            if (raycastInfo.transform.gameObject.tag == "Cop")
                return true;
        }
        return false;
    }

    bool CanSeeMe()
    {
        Vector3 rayToTarget =  transform.position - target.transform.position;
        float lookAngle = Vector3.Angle(target.transform.forward, rayToTarget);

        if (lookAngle < 60)
            return true;

        return false;
    }

    bool coolDown = false;
    void BehaviourCoolDown()
    {
        coolDown = false;
    }

    bool TargetInRange()
    {
        if (Vector3.Distance(transform.position, target.transform.position) < 10)
            return true;
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        //Seek(target.transform.position);
        //Flee(target.transform.position);
        //Pursue();
        // Evade();
        // Wander();
        if (!coolDown)
        {
            if (!TargetInRange())
            {
                Wander();
            }
            if (CanSeeTarget() && CanSeeMe())
            {
                CleverHide();
                coolDown = true;
                Invoke("BehaviourCoolDown", 5);
            }
            else
                Pursue();
        }
        
            

    }
}
