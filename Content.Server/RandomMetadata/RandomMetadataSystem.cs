// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Blitz <73762869+BlitzTheSquishy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.RandomMetadata;

public sealed class RandomMetadataSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    private readonly List<(string, object)> _outputSegments = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomMetadataComponent, MapInitEvent>(OnMapInit);
    }

    // This is done on map init so that map-placed entities have it randomized each time the map loads, for fun.
    private void OnMapInit(EntityUid uid, RandomMetadataComponent component, MapInitEvent args)
    {
        var meta = MetaData(uid);

        if (component.NameSegments != null)
        {
            _metaData.SetEntityName(uid, GetRandomFromSegments(component.NameSegments, component.NameFormat), meta);
        }

        if (component.DescriptionSegments != null)
        {
            _metaData.SetEntityDescription(uid,
                GetRandomFromSegments(component.DescriptionSegments, component.DescriptionFormat), meta);
        }
    }

    /// <summary>
    /// Generates a random string from segments and a separator.
    /// </summary>
    /// <param name="segments">The segments that it will be generated from</param>
    /// <param name="format">The format string used to combine the segments.</param>
    /// <returns>The newly generated string</returns>
    [PublicAPI]
    public string GetRandomFromSegments(List<ProtoId<LocalizedDatasetPrototype>> segments, LocId format)
    {
        _outputSegments.Clear();
        for (var i = 0; i < segments.Count; ++i)
        {
            var localizedProto = _prototype.Index(segments[i]);
            _outputSegments.Add(($"part{i}", _random.Pick(localizedProto)));
        }

        return Loc.GetString(format, _outputSegments.ToArray());
    }
}
