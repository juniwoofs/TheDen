// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Content.Shared.Access;
using Content.Shared.Access.Systems;

namespace Content.Client.Access.UI;

[GenerateTypedNameReferences]
public sealed partial class AccessLevelControl : GridContainer
{
    public readonly Dictionary<ProtoId<AccessLevelPrototype>, Button> ButtonsList = new();

    public AccessLevelControl()
    {
        RobustXamlLoader.Load(this);
    }

    public void Populate(List<ProtoId<AccessLevelPrototype>> accessLevels, IPrototypeManager prototypeManager)
    {
        foreach (var access in accessLevels)
        {
            if (!prototypeManager.TryIndex(access, out var accessLevel))
            {
                Logger.Error($"Unable to find accesslevel for {access}");
                continue;
            }

            var newButton = new Button
            {
                Text = accessLevel.GetAccessLevelName(),
                ToggleMode = true,
            };
            AddChild(newButton);
            ButtonsList.Add(accessLevel.ID, newButton);
        }
    }

    public void UpdateState(
        List<ProtoId<AccessLevelPrototype>> pressedList,
        List<ProtoId<AccessLevelPrototype>>? enabledList = null)
    {
        foreach (var (accessName, button) in ButtonsList)
        {
            button.Pressed = pressedList.Contains(accessName);
            button.Disabled = !(enabledList?.Contains(accessName) ?? true);
        }
    }
}
