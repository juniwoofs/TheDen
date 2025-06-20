// SPDX-FileCopyrightText: 2022 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Chat.Managers;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// This handles narcolepsy, causing the affected to fall asleep uncontrollably at a random interval.
/// </summary>
public sealed class NarcolepsySystem : EntitySystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string StatusEffectKey = "ForcedSleep"; // Same one used by N2O and other sleep chems.

    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NarcolepsyComponent, ComponentStartup>(SetupNarcolepsy);
        SubscribeLocalEvent<NarcolepsyComponent, SleepStateChangedEvent>(OnSleepChanged);
    }

    private void SetupNarcolepsy(EntityUid uid, NarcolepsyComponent component, ComponentStartup args)
    {
        PrepareNextIncident((uid, component));
    }

    private void OnSleepChanged(Entity<NarcolepsyComponent> ent, ref SleepStateChangedEvent args)
    {
        // When falling asleep while an incident is nigh, force it to happen immediately.
        if (args.FellAsleep)
        {
            if (ent.Comp.NextIncidentTime < ent.Comp.TimeBeforeWarning)
                StartIncident(ent);
            return;
        }

        // When waking up after sleeping for at least the minimum time of an incident, reset the incident timer and show a popup.
        if (args.TimeSlept is null || args.TimeSlept.Value.TotalSeconds < ent.Comp.DurationOfIncident.X)
            return;

        ShowRandomPopup(ent, ent.Comp.WakeupLocaleBase, ent.Comp.WakeupLocaleCount);
        PrepareNextIncident(ent);
    }

    public void AdjustNarcolepsyTimer(EntityUid uid, float setTime, NarcolepsyComponent? narcolepsy = null)
    {
        if (!Resolve(uid, ref narcolepsy, false) || narcolepsy.NextIncidentTime > setTime)
            return;

        narcolepsy.NextIncidentTime = setTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NarcolepsyComponent>();
        while (query.MoveNext(out var uid, out var narcolepsy))
        {
            if (HasComp<SleepingComponent>(uid))
                continue;

            narcolepsy.NextIncidentTime -= frameTime;
            if (narcolepsy.NextIncidentTime <= narcolepsy.TimeBeforeWarning && narcolepsy.NextIncidentTime < narcolepsy.LastWarningRollTime - 1f)
            {
                // Roll for showing a popup. There should really be a class for doing this.
                narcolepsy.LastWarningRollTime = narcolepsy.NextIncidentTime;
                if (_random.Prob(narcolepsy.WarningChancePerSecond))
                {
                    ShowRandomPopup(uid, narcolepsy.WarningLocaleBase, narcolepsy.WakeupLocaleCount);
                    narcolepsy.LastWarningRollTime = 0f; // Do not show any more popups for the upcoming incident
                }
            }

            if (narcolepsy.NextIncidentTime >= 0)
                continue;

            StartIncident((uid, narcolepsy));
        }
    }

    public void StartIncident(Entity<NarcolepsyComponent> ent)
    {
        var duration = _random.NextFloat(ent.Comp.DurationOfIncident.X, ent.Comp.DurationOfIncident.Y);
        PrepareNextIncident(ent, duration);

        _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent, StatusEffectKey, TimeSpan.FromSeconds(duration), false);
    }

    private void PrepareNextIncident(Entity<NarcolepsyComponent> ent, float startingFrom = 0f)
    {
        var time = _random.NextFloat(ent.Comp.TimeBetweenIncidents.X, ent.Comp.TimeBetweenIncidents.Y);
        ent.Comp.NextIncidentTime = startingFrom + time;
        ent.Comp.LastWarningRollTime = float.MaxValue;
    }

    private void ShowRandomPopup(EntityUid uid, string prefix, int count)
    {
        if (count <= 0 || !TryComp<ActorComponent>(uid, out var actor))
            return;

        var popup = Loc.GetString($"{prefix}-{_random.Next(1, count + 1)}");
        _popups.PopupEntity(popup, uid, uid, PopupType.MediumCaution);
        // This should use ChatChannel.Visual, but it's not displayed on the client.
        _chatMan.ChatMessageToOne(ChatChannel.Notifications, popup, popup, uid, false,
            actor.PlayerSession.Channel, Color.IndianRed);
    }
}
