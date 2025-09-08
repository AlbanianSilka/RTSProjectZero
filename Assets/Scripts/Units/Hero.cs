using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Interfaces;
using Data;
using static Resource;

public class Hero : UnitRTS {
    protected virtual float respawnTime => 10f;
}