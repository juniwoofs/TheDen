// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Threading;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.StationEvents.Events
{
    [UsedImplicitly]
    public sealed class PowerGridCheckRule : StationEventSystem<PowerGridCheckRuleComponent>
    {
        [Dependency] private readonly ApcSystem _apcSystem = default!;

        protected override void Started(EntityUid uid, PowerGridCheckRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);

            if (!TryGetRandomStation(out var chosenStation))
                return;

            component.AffectedStation = chosenStation.Value;

            var query = AllEntityQuery<ApcComponent, TransformComponent>();
            while (query.MoveNext(out var apcUid ,out var apc, out var transform))
            {
                if (apc.MainBreakerEnabled && CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == chosenStation)
                    component.Powered.Add(apcUid);
            }

            RobustRandom.Shuffle(component.Powered);

            component.NumberPerSecond = Math.Max(1, (int)(component.Powered.Count / component.SecondsUntilOff)); // Number of APCs to turn off every second. At least one.
        }

        protected override void Ended(EntityUid uid, PowerGridCheckRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, component, gameRule, args);

            foreach (var entity in component.Unpowered)
            {
                if (Deleted(entity))
                    continue;

                if (TryComp(entity, out ApcComponent? apcComponent))
                {
                    if(!apcComponent.MainBreakerEnabled)
                        _apcSystem.ApcToggleBreaker(entity, apcComponent);
                }
            }

            component.Unpowered.Clear();
        }

        protected override void ActiveTick(EntityUid uid, PowerGridCheckRuleComponent component, GameRuleComponent gameRule, float frameTime)
        {
            base.ActiveTick(uid, component, gameRule, frameTime);

            var updates = 0;
            component.FrameTimeAccumulator += frameTime;
            if (component.FrameTimeAccumulator > component.UpdateRate)
            {
                updates = (int) (component.FrameTimeAccumulator / component.UpdateRate);
                component.FrameTimeAccumulator -= component.UpdateRate * updates;
            }

            for (var i = 0; i < updates; i++)
            {
                if (component.Powered.Count == 0)
                    break;

                var selected = component.Powered.Pop();
                if (Deleted(selected))
                    continue;
                if (TryComp<ApcComponent>(selected, out var apcComponent))
                {
                    if (apcComponent.MainBreakerEnabled)
                        _apcSystem.ApcToggleBreaker(selected, apcComponent);
                }
                component.Unpowered.Add(selected);
            }
        }
    }
}
