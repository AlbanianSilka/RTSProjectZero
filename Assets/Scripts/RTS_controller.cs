using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Interfaces;

public class RTS_controller : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaTransform;

    private Vector2 startPosition;

    public ISelectable CurrentSelected
    {
        get => _currentSelected;
        private set
        {
            _currentSelected = value;
            UI_controller.showSpellButtons(_currentSelected);
        }
    }
    public ISelectable _currentSelected;

    public List<UnitRTS> selectedUnitRTSList { get; private set; }
    public RTS_building selectedBuilding { get; private set; }
    public Canvas middleSection;
    public Canvas rightSection;
    public GameObject progressButtonPrefab;
    public GameObject workerButtonPrefab;
    public Player owner;
    public buildings_manager BuildingManager;

    private void Start()
    {
        if (!(owner is BotPlayer))
        {
            rightSection = Instantiate(rightSection);
            middleSection = Instantiate(middleSection);
            UI_controller.rtsController = this;
            UI_controller.rightContainer = rightSection;
            UI_controller.spellBoxes = rightSection.GetComponentsInChildren<spell_box>(includeInactive: true).ToList();
            UI_controller.progressBoxContainer = middleSection;
            UI_controller.progressBoxes = middleSection.GetComponentsInChildren<progress_box>(includeInactive: true).ToList();
        }
    }

    private void Awake()
    {
        selectedUnitRTSList = new List<UnitRTS>();
        selectionAreaTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!(owner is BotPlayer))
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectionAreaTransform.gameObject.SetActive(true);
                startPosition = Input.mousePosition;

            }

            if (Input.GetMouseButton(0))
            {
                Vector2 currentMousePosition = Input.mousePosition;

                Vector2 lowerLeft = new Vector2(
                    Mathf.Min(startPosition.x, currentMousePosition.x),
                    Mathf.Min(startPosition.y, currentMousePosition.y));

                Vector2 upperRight = new Vector2(
                    Mathf.Max(startPosition.x, currentMousePosition.x),
                    Mathf.Max(startPosition.y, currentMousePosition.y));

                selectionAreaTransform.offsetMin = lowerLeft;
                selectionAreaTransform.offsetMax = upperRight;
            }

            // Check if user unpressed left mouse button and if clicked object was not a UI element
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
            {
                if (!BuildingManager.isPlacingBuilding)
                {
                    selectionAreaTransform.gameObject.SetActive(false);
                    Collider2D[] collArray = Physics2D.OverlapAreaAll(Camera.main.ScreenToWorldPoint(startPosition),
                        Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    CurrentSelected = null;
                    selectedBuilding = null;
                    selectedUnitRTSList.Clear();

                    bool noUnits = true;

                    if (collArray.Length == 0)
                    {
                        CurrentSelected = null;
                        return;
                    }

                    foreach (Collider2D obj in collArray)
                    {
                        if (CurrentSelected == null && obj.TryGetComponent<ISelectable>(out var selected))
                        {
                            CurrentSelected = selected;
                        }

                        UnitRTS unitRTS = obj.GetComponent<UnitRTS>();
                        if (unitRTS != null && unitRTS.owner.team == owner.team)
                        {
                            noUnits = false;
                            selectedUnitRTSList.Add(unitRTS);
                        }
                        else if (obj.CompareTag("Building"))
                        {
                            selectedBuilding = obj.GetComponent<RTS_building>();
                        }
                    }

                    if (noUnits && selectedBuilding != null)
                    {
                        if (selectedBuilding.finished)
                        {
                            middleSection.gameObject.SetActive(true);
                            if (selectedBuilding.GetComponent<GoldenMine>() != null)
                            {
                                UI_controller.handleMineMiddle(selectedBuilding.GetComponent<GoldenMine>().workers, workerButtonPrefab);
                            }
                            else
                            {
                                UI_controller.handleMiddleSection(selectedBuilding.unitsQueue, progressButtonPrefab);
                            }
                        }
                    }
                    else
                    {
                        selectedBuilding = null;
                        middleSection.enabled = false;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector2 raycastOrigin = new Vector2(clickPosition.x, clickPosition.y);
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.zero);

                if (hit.collider == null)
                {
                    MoveSelectedUnits(clickPosition, selectedUnitRTSList);
                }
            }
        }
    }

    public void MoveSelectedUnits(Vector3 clickPosition, List<UnitRTS> selectedUnits)
    {
        clickPosition.z = 0f;
        List<Vector3> targetPositionList = GetPositionListAround(clickPosition, new float[] { 1f, 2f, 3f }, new int[] { 5, 10, 20 });
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
}
