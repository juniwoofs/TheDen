// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Speech.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Random;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class OwoifyCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public string Command => "owoify";

    public string Description => "For when you need everything to be cat. Uses OwOAccent's formatting on the name and description of an entity.";

    public string Help => "owoify <id>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[0], out var targetId))
        {
            shell.WriteLine(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var nent = new NetEntity(targetId);

        if (!_entManager.TryGetEntity(nent, out var eUid))
        {
            return;
        }

        var meta = _entManager.GetComponent<MetaDataComponent>(eUid.Value);

        var owoSys = _entManager.System<OwOAccentSystem>();
        var metaDataSys = _entManager.System<MetaDataSystem>();

        metaDataSys.SetEntityName(eUid.Value, owoSys.Accentuate(meta.EntityName), meta);
        metaDataSys.SetEntityDescription(eUid.Value, owoSys.Accentuate(meta.EntityDescription), meta);
    }
}
