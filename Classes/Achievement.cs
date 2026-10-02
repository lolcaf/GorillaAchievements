using System;

namespace GorillaAchievements.Classes;

[Serializable]
public class Achievement
{
    public string name = "Achievement";

    public string description = "Description";

    public Difficulty difficulty;

    public bool hidden = false;
}

public enum Difficulty
{
    None,

    Easy,

    Medium,

    Hard,

    Extreme,

    Common,

    Rare,
}