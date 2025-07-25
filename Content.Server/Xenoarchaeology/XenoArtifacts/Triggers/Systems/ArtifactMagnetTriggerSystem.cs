// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Server.Salvage;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Clothing;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

/// <summary>
/// This handles artifacts that are activated by magnets, both salvage and magboots.
/// </summary>
public sealed class ArtifactMagnetTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;

    private readonly List<EntityUid> _toActivate = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SalvageMagnetActivatedEvent>(OnMagnetActivated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!EntityQuery<ArtifactMagnetTriggerComponent>().Any())
            return;

        _toActivate.Clear();

        //assume that there's more magboots than artifacts
        var query = EntityQueryEnumerator<MagbootsComponent, TransformComponent, ItemToggleComponent>();
        while (query.MoveNext(out _, out var magboot, out var magXform, out var toggle))
        {
            if (!toggle.Activated)
                continue;

            var artiQuery = EntityQueryEnumerator<ArtifactMagnetTriggerComponent, TransformComponent>();
            while (artiQuery.MoveNext(out var artifactUid, out var trigger, out var xform))
            {
                if (!magXform.Coordinates.TryDistance(EntityManager, xform.Coordinates, out var distance))
                    continue;

                if (distance > trigger.MagbootRange)
                    continue;

                _toActivate.Add(artifactUid);
            }
        }

        foreach (var a in _toActivate)
        {
            _artifact.TryActivateArtifact(a);
        }
    }

    private void OnMagnetActivated(ref SalvageMagnetActivatedEvent ev)
    {
        var magXform = Transform(ev.Magnet);

        var query = EntityQueryEnumerator<ArtifactMagnetTriggerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var artifact, out var xform))
        {
            if (!magXform.Coordinates.TryDistance(EntityManager, xform.Coordinates, out var distance))
                continue;

            if (distance > artifact.Range)
                continue;

            _toActivate.Add(uid);
        }

        foreach (var a in _toActivate)
        {
            _artifact.TryActivateArtifact(a);
        }
    }
}
