using UnityEngine;
using System.Collections;
using Interfaces;

public class EnvironmentObject : MonoBehaviour, IBlocksBuildingPlacement
{
    public bool BlocksPlacement() => true;
}

