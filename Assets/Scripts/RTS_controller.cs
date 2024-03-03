using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RTS_controller : MonoBehaviour
{
    [SerializeField] private Transform selectionAreaTransform;

    private Vector3 startPosition;
    private buildings_manager building_manager;

    public List<UnitRTS> selectedUnitRTSList { get; private set; }

    private void Start()
    {
        HideSpellButtons();
    }

    private void Awake()
    {
        selectedUnitRTSList = new List<UnitRTS>();
        selectionAreaTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionAreaTransform.gameObject.SetActive(true);
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 lowerLeft = new Vector3(
                Mathf.Min(startPosition.x, currentMousePosition.x),
                Mathf.Min(startPosition.y, currentMousePosition.y));

            Vector3 upperRight = new Vector3(
                Mathf.Max(startPosition.x, currentMousePosition.x),
                Mathf.Max(startPosition.y, currentMousePosition.y));

            selectionAreaTransform.position = lowerLeft;
            selectionAreaTransform.localScale = upperRight - lowerLeft;
        }

        // Check if user unpressed left mouse button and if clicked object was not a UI element
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            if (!buildings_manager.isPlacingBuilding)
            {
                selectionAreaTransform.gameObject.SetActive(false);
                Collider2D[] collArray = Physics2D.OverlapAreaAll(startPosition, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                selectedUnitRTSList.Clear();
                HideSpellButtons();

                foreach (Collider2D obj in collArray)
                {
                    UnitRTS unitRTS = obj.GetComponent<UnitRTS>();
                    if (unitRTS != null)
                    {
                        selectedUnitRTSList.Add(unitRTS);
                    }

                }

                UI_controller.showSpellButtons(selectedUnitRTSList);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            MoveSelectedUnits(clickPosition, selectedUnitRTSList);
        }
    }

    public void MoveSelectedUnits(Vector3 clickPosition, List<UnitRTS> selectedUnits)
    {
        clickPosition.z = 0f;
        List<Vector3> targetPositionList = GetPositionListAround(clickPosition, new float[] { 1f, 2f, 3f}, new int[] {5, 10, 20});
        int targetPositionListIndex = 0;

        int unitCount = selectedUnits.Count;
        for (int i = 0; i < unitCount; i++)
        {
            UnitRTS unitRTS = selectedUnits[i];
            unitRTS.MoveTo(targetPositionList[targetPositionListIndex]);
            targetPositionListIndex = (targetPositionListIndex + 1) % targetPositionList.Count;
        }
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float[] ringDistanceArray, int[] ringPositionCountArray)
    {
        List<Vector3> positionList = new List<Vector3>();
        positionList.Add(startPosition);
        for (int i=0; i<ringDistanceArray.Length; i++)
        {
            positionList.AddRange(GetPositionListAround(startPosition, ringDistanceArray[i], ringPositionCountArray[i]));
        }
        return positionList;
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float distance, int positionCount)
    {
        List<Vector3> positionList = new List<Vector3>();
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (360f / positionCount);
            Vector3 dir = ApplyRotationToVector(new Vector3(1, 0), angle);
            Vector3 position = startPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }

    private Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

    private void HideSpellButtons()
    {
        GameObject[] spellButtons = GameObject.FindGameObjectsWithTag("SpellBtn");
        foreach (GameObject spellButton in spellButtons)
        {
            spellButton.SetActive(false);
        }
    }
}
