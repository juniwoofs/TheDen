// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Gets ammo for this NPC's selected gun; either active hand or itself.
/// </summary>
public sealed partial class GunAmmoPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("minPercent")]
    public float MinPercent = 0f;

    [DataField("maxPercent")]
    public float MaxPercent = 1f;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var gunSystem = _entManager.System<GunSystem>();

        if (!gunSystem.TryGetGun(owner, out var gunUid, out _))
        {
            return false;
        }

        var ammoEv = new GetAmmoCountEvent();
        _entManager.EventBus.RaiseLocalEvent(gunUid, ref ammoEv);
        float percent;

        if (ammoEv.Capacity == 0)
            percent = 0f;
        else
            percent = ammoEv.Count / (float) ammoEv.Capacity;

        percent = System.Math.Clamp(percent, 0f, 1f);

        if (MaxPercent < percent)
            return false;

        if (MinPercent > percent)
            return false;

        return true;
    }
}
