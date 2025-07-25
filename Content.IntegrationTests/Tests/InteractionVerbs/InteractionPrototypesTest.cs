// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Client.Guidebook;
using Content.Server.Verbs;
using Content.Shared.InteractionVerbs;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.InteractionVerbs;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[TestOf(typeof(InteractionVerbPrototype))]
public sealed class InteractionPrototypesTest
{
    public const string TestMobProto = "MobHuman";

    [Test]
    public async Task ValidatePrototypeContents()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        await server.WaitIdleAsync();

        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        // TODO probably should test if an entity receives an abstract verb, but Iunno how
        foreach (var proto in protoMan.EnumeratePrototypes<InteractionVerbPrototype>())
        {
            Assert.That(proto.Abstract || proto.Action is not null, $"Non-abstract prototype {proto.ID} lacks an action!");
        }


        await pair.CleanReturnAsync();
    }
}
