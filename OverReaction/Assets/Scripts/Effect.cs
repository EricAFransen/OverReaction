using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Effect
{
    public double TimeRemaining;

    public Effect(double Duration)
    {
        TimeRemaining = Duration;
    }

    public abstract void Play();

    public abstract void Remove();
}