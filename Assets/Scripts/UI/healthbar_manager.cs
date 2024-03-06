using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthbar_manager : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera camera_follow;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offeset;

    public void updateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }

    private void Update()
    {
        // setting the position of a health bar for game object, even if it is moving
        transform.rotation = camera_follow.transform.rotation;
        // (better to always make it 0/1.25/0 ((XYZ)) for a buildings)
        transform.position = target.position + offeset;
    }
}
