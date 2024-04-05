using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class GoldenMine : RTS_building
{
    public Deposit attachedDeposit;
    public List<Peasant> workers;

    private Dictionary<Peasant, Coroutine> workTimers = new Dictionary<Peasant, Coroutine>();

    public GoldenMine()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 25 },
            { ResourceType.Wood, 25 }
        };
    }

    public void AddWorkerToMine(Peasant addedWorker)
    {
        // max number of workers in one mine is currently 5
        if(workers.Count < 5)
        {
            workers.Add(addedWorker);
            StartWorkTimer(addedWorker);
        }
    }

    public void removeWorkerByIndex(int workerIndex)
    {
        Peasant worker = workers[workerIndex];
        StopWorkTimer(worker);
    }

    private void RemoveWorker(Peasant peasant)
    {
        workers.Remove(peasant);
        StopWorkTimer(peasant);
    }

    private void StartWorkTimer(Peasant peasant)
    {
        Coroutine timerCoroutine = StartCoroutine(WorkTimer(peasant));
        workTimers.Add(peasant, timerCoroutine);
    }

    private void StopWorkTimer(Peasant peasant)
    {
        if (workTimers.ContainsKey(peasant))
        {
            Coroutine timerCoroutine = workTimers[peasant];
            StopCoroutine(timerCoroutine);
            workTimers.Remove(peasant);
            peasant.gameObject.SetActive(true);
            workers.Remove(peasant);
            UI_controller.handleMineMiddle(workers, rtsController.workerButtonPrefab);
        }
    }

    private IEnumerator WorkTimer(Peasant peasant)
    {
        while (peasant.carriedResource.amount < peasant.maxCarryCapacity)
        {
            yield return new WaitForSeconds(5f);
            peasant.carriedResource.amount += 5;

            if (peasant.carriedResource.amount >= peasant.maxCarryCapacity)
            {
                RemoveWorker(peasant);
                yield break; 
            }
        }
    }

    protected override void Die()
    {
        base.Die();

        if (attachedDeposit != null)
        {
            attachedDeposit.gameObject.SetActive(true);
        }
    }
}
