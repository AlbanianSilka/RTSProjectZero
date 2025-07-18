using System;
using System.Collections.Generic;

public abstract class Spell
{
    public string SpellName { get; protected set; }
    public abstract void Cast(RTS_controller controller, List<UnitRTS> units);
}
