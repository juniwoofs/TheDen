// SPDX-FileCopyrightText: 2023 Debug
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2024 Leon Friedrich
// SPDX-FileCopyrightText: 2024 MilenVolf
// SPDX-FileCopyrightText: 2024 VMSolidus
// SPDX-FileCopyrightText: 2025 Falcon
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Pair;

// Contains misc helper functions to make writing tests easier.
public sealed partial class TestPair
{
    /// <summary>
    /// Creates a map, a grid, and a tile, and gives back references to them.
    /// </summary>
    [MemberNotNull(nameof(TestMap))]
    public async Task<TestMapData> CreateTestMap(bool initialized = true, string tile = "Plating")
    {
        var mapData = new TestMapData();
        TestMap = mapData;
        await Server.WaitIdleAsync();
        var tileDefinitionManager = Server.ResolveDependency<ITileDefinitionManager>();

        TestMap = mapData;
        await Server.WaitPost(() =>
        {
            mapData.MapUid = Server.System<SharedMapSystem>().CreateMap(out mapData.MapId, runMapInit: initialized);
            mapData.Grid = Server.MapMan.CreateGridEntity(mapData.MapId);
            mapData.GridCoords = new EntityCoordinates(mapData.Grid, 0, 0);
            var plating = tileDefinitionManager[tile];
            var platingTile = new Tile(plating.TileId);
            Server.System<SharedMapSystem>().SetTile(mapData.Grid.Owner, mapData.Grid.Comp, mapData.GridCoords, platingTile);
            mapData.MapCoords = new MapCoordinates(0, 0, mapData.MapId);
            mapData.Tile = Server.System<SharedMapSystem>().GetAllTiles(mapData.Grid.Owner, mapData.Grid.Comp).First();
        });

        TestMap = mapData;
        if (!Settings.Connected)
            return mapData;

        await RunTicksSync(10);
        mapData.CMapUid = ToClientUid(mapData.MapUid);
        mapData.CGridUid = ToClientUid(mapData.Grid);
        mapData.CGridCoords = new EntityCoordinates(mapData.CGridUid, 0, 0);

        TestMap = mapData;
        return mapData;
    }

    /// <summary>
    /// Convert a client-side uid into a server-side uid
    /// </summary>
    public EntityUid ToServerUid(EntityUid uid) => ConvertUid(uid, Client, Server);

    /// <summary>
    /// Convert a server-side uid into a client-side uid
    /// </summary>
    public EntityUid ToClientUid(EntityUid uid) => ConvertUid(uid, Server, Client);

    private static EntityUid ConvertUid(
        EntityUid uid,
        RobustIntegrationTest.IntegrationInstance source,
        RobustIntegrationTest.IntegrationInstance destination)
    {
        if (!uid.IsValid())
            return EntityUid.Invalid;

        if (!source.EntMan.TryGetComponent<MetaDataComponent>(uid, out var meta))
        {
            Assert.Fail($"Failed to resolve MetaData while converting the EntityUid for entity {uid}");
            return EntityUid.Invalid;
        }

        if (!destination.EntMan.TryGetEntity(meta.NetEntity, out var otherUid))
        {
            Assert.Fail($"Failed to resolve net ID while converting the EntityUid entity {source.EntMan.ToPrettyString(uid)}");
            return EntityUid.Invalid;
        }

        return otherUid.Value;
    }

    /// <summary>
    /// Execute a command on the server and wait some number of ticks.
    /// </summary>
    public async Task WaitCommand(string cmd, int numTicks = 10)
    {
        await Server.ExecuteCommand(cmd);
        await RunTicksSync(numTicks);
    }

    /// <summary>
    /// Execute a command on the client and wait some number of ticks.
    /// </summary>
    public async Task WaitClientCommand(string cmd, int numTicks = 10)
    {
        await Client.ExecuteCommand(cmd);
        await RunTicksSync(numTicks);
    }

    /// <summary>
    /// Retrieve all entity prototypes that have some component.
    /// </summary>
    public List<(EntityPrototype, T)> GetPrototypesWithComponent<T>(
        HashSet<string>? ignored = null,
        bool ignoreAbstract = true,
        bool ignoreTestPrototypes = true)
        where T : IComponent, new()
    {
        if (!Server.ResolveDependency<IComponentFactory>().TryGetRegistration<T>(out var reg)
            && !Client.ResolveDependency<IComponentFactory>().TryGetRegistration<T>(out reg))
        {
            Assert.Fail($"Unknown component: {typeof(T).Name}");
            return new();
        }

        var id = reg.Name;
        var list = new List<(EntityPrototype, T)>();
        foreach (var proto in Server.ProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (ignored != null && ignored.Contains(proto.ID))
                continue;

            if (ignoreAbstract && proto.Abstract)
                continue;

            if (ignoreTestPrototypes && IsTestPrototype(proto))
                continue;

            if (proto.Components.TryGetComponent(id, out var cmp))
                list.Add((proto, (T)cmp));
        }

        return list;
    }

    /// <summary>
    /// Helper method for enabling or disabling a antag role
    /// </summary>
    public async Task SetAntagPref(ProtoId<AntagPrototype> id, bool value)
    {
        var prefMan = Server.ResolveDependency<IServerPreferencesManager>();

        var prefs = prefMan.GetPreferences(Client.User!.Value);
        // what even is the point of ICharacterProfile if we always cast it to HumanoidCharacterProfile to make it usable?
        var profile = (HumanoidCharacterProfile) prefs.SelectedCharacter;

        Assert.That(profile.AntagPreferences.Any(preference => preference == id), Is.EqualTo(!value));
        var newProfile = profile.WithAntagPreference(id, value);

        await Server.WaitPost(() =>
        {
            prefMan.SetProfile(Client.User.Value, prefs.SelectedCharacterIndex, newProfile).Wait();
        });

        // And why the fuck does it always create a new preference and profile object instead of just reusing them?
        var newPrefs = prefMan.GetPreferences(Client.User.Value);
        var newProf = (HumanoidCharacterProfile) newPrefs.SelectedCharacter;
        Assert.That(newProf.AntagPreferences.Any(preference => preference == id), Is.EqualTo(value));
    }
}
