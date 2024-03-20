using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;

public class RTS_building : MonoBehaviour
{
    public Sprite canBuild;
    public Sprite cannotBuild;
    public float maxHealth;
    public float health;
    public bool finished;
    public string team;
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();
    public List<UnitRTS> unitsQueue;
    public event Action<RTS_building> OnDeath;
    public GameObject progressButtonPrefab;

    protected RTS_controller rtsController;

    private bool makingUnit;
    private static List<GameObject> progressBoxes = new List<GameObject>();

    private void Start()
    {
        GameObject[] progressBoxObjects = GameObject.FindGameObjectsWithTag("ProgressBox");
        progressBoxes.AddRange(progressBoxObjects);
    }

    private void Awake()
    {
        rtsController = FindObjectOfType<RTS_controller>();
        healthBar = GetComponentInChildren<healthbar_manager>();
        makingUnit = false;
        InvokeRepeating("CheckAndSpawnUnits", 0f, 1f);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHealth);
        Debug.Log($"{this.name} was hit");

        if (health <= 0)
        {
            Die();
        }
    }

    public void AddUnitToQueue(UnitRTS unit)
    {
        // currently max queue is 7 unuts (or 7 upgrades but that's for future)

        if(unitsQueue.Count < 7)
        {
            unitsQueue.Add(unit);
            addNewProgessButton(unit);
        }
    }

    private void CheckAndSpawnUnits()
    {
        if (unitsQueue.Count > 0 && !makingUnit)
        {
            UnitRTS unit = unitsQueue[0];

            makingUnit = true;
            StartCoroutine(SpawnUnitAfterDelay(unit));
        }
    }

    private IEnumerator SpawnUnitAfterDelay(UnitRTS unit)
    {
        yield return new WaitForSeconds(unit.spawnTime);

        Transform spawnMarker = this.transform.Find("SpawnMarker");

        if (spawnMarker != null)
        {
            Vector2 spawnPosition = spawnMarker.transform.position;
            UnitRTS newUnit = Instantiate(unit, spawnPosition, Quaternion.identity);
            unitsQueue.RemoveAt(0);
            newUnit.team = this.team;
            makingUnit = false;
        }
        else
        {
            Debug.LogError("SpawnMarker not found!");
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);

        Destroy(gameObject);
    }

    // TODO: refactor this shit, I'll need to move this to UI_controller
    // TODO: 1) create method display all buttons according unitQueue
    // TODO: 2) create method to add new progress button if new unit added to queue
    // !!! all in UI_controller !!!
    private void addNewProgessButton(UnitRTS addedUnit)
    {
        GameObject middleCanvas = rtsController.middleSection.gameObject;

        if(middleCanvas != null)
        {
            int queueIndex = unitsQueue.Count - 1;
            GameObject progressButton = Instantiate(progressButtonPrefab);
            progressButton.transform.SetParent(middleCanvas.transform, false);
            progress_button buttonComponent = progressButton.GetComponent<progress_button>();
            if(buttonComponent != null)
            {
                buttonComponent.buttonIndex = queueIndex;
                Sprite unitIcon = addedUnit.unitIcon;
                Image newBtnImg = progressButton.GetComponent<Image>();
                newBtnImg.sprite = unitIcon;
                foreach (GameObject progressBox in progressBoxes)
                {
                    progress_box progressBoxComponent = progressBox.GetComponent<progress_box>();
                    if (progressBoxComponent != null && progressBoxComponent.boxIndex == buttonComponent.buttonIndex)
                    {
                        progressButton.transform.position = progressBox.transform.position;
                        progressButton.transform.localScale = progressBox.transform.localScale;
                        progressButton.SetActive(true);

                        // Exit the loop after finding the matching progress box
                        return;
                    }
                }
            } else
            {
                Debug.LogError("You forgot to attach 'spell_button' component to prefab");
            }
        }
        else
        {
            Debug.LogError("You probably forgot to add middle canvas to the scene");
        }
    }

    public void changeProgressButtonsOrder(int buttonIndex)
    {
        GameObject[] progressButtons = GameObject.FindGameObjectsWithTag("ProgressBtn");


    }
}
