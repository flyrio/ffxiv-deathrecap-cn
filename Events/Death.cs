using System;
using System.Collections.Generic;

namespace DeathRecap.Events;

public record Death {
    public uint PlayerId { get; internal init; }
    public string PlayerName { get; internal init; } = null!;
    public DateTime TimeOfDeath { get; internal init; }

    public List<CombatEvent> Events { get; internal init; } = null!;

    public string Title {
        get {
            var timeSpan = DateTime.Now.Subtract(TimeOfDeath);

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return $"{timeSpan.Seconds} 秒前";

            if (timeSpan <= TimeSpan.FromMinutes(60))
                return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} 分钟前" : "约 1 分钟前";

            return timeSpan.Hours > 1 ? $"{timeSpan.Hours} 小时前" : "约 1 小时前";
        }
    }
}
