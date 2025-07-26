using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class progress_box : MonoBehaviour
{
    private Coroutine progressRoutine;

    public int boxIndex;
    public float maxTrainingTime;
    public healthbar_manager _progressBar;

    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;

    private void Awake()
    {
        Clear();
    }

    public void Setup(RTS_controller controller, SpellSO spell, int boxIndex)
    {
        _button.onClick.RemoveAllListeners();
        RTS_building building = controller._currentSelected as RTS_building;
        if(building is null)
        {
            return;
        }

        this.boxIndex = boxIndex;

        // Stop any old progress coroutine
        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }

        List<UnitRTS> unitsQueue = (building is GoldenMine mine)
                    ? mine.workers.ConvertAll(w => (UnitRTS)w)
                    : building.unitsQueue;

        if (spell == null || building == null || boxIndex >= unitsQueue.Count)
        {
            Clear();
            return;
        }

        if (spell is IIndexedSpell indexedSpell)
        {
            indexedSpell.buttonIndex = boxIndex;
        }

        spell.icon = unitsQueue[boxIndex].unitIcon;
        _icon.color = Color.white;
        _icon.sprite = spell.icon;
        _button.onClick.AddListener(() => spell.Cast(controller));

        if(building is GoldenMine)
        {
            return;
        }

        if (boxIndex == 0)
        {
            maxTrainingTime = unitsQueue[0].spawnTime;
            _progressBar.gameObject.SetActive(true);
            progressRoutine = StartCoroutine(UpdateProgressBar(building));
        }
        else
        {
            _progressBar.gameObject.SetActive(false);
        }
    }

    private void Clear()
    {
        _icon.sprite = null;
        _icon.color = Color.clear;
        _progressBar.gameObject.SetActive(false);
    }

    private IEnumerator UpdateProgressBar(RTS_building building)
    {
        while (building.remainingSpawnTime <= 0f)
        {
            yield return null;
        }

        while (building.remainingSpawnTime > 0f)
        {
            float currentTime = maxTrainingTime - building.remainingSpawnTime;
            _progressBar.updateHealthBar(currentTime, maxTrainingTime);
            yield return null; // wait for next frame
        }

        // reset after done
        _progressBar.updateHealthBar(maxTrainingTime, maxTrainingTime);
    }
}
