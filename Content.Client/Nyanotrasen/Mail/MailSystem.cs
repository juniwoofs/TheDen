// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <sidneymaatman@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Mail;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Mail;

/// <summary>
///     Display a cool stamp on the parcel based on the job of the recipient.
/// </summary>
/// <remarks>
///     GenericVisualizer is not powerful enough to handle setting a string on
///     visual data then directly relaying that string to a layer's state.
///     I.e. there is nothing like a regex capture group for visual data.
///     Hence why this system exists.
///     To do this with GenericVisualizer would require a separate condition
///     for every job value, which would be extra mess to maintain.
///     It would look something like this, multipled a couple dozen times.
///     enum.MailVisuals.JobIcon:
///     enum.MailVisualLayers.JobStamp:
///     StationEngineer:
///     state: StationEngineer
///     SecurityOfficer:
///     state: SecurityOfficer
/// </remarks>
public sealed class MailJobVisualizerSystem : VisualizerSystem<MailComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SpriteSystem _stateManager = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    protected override void OnAppearanceChange(EntityUid uid, MailComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _appearanceSystem.TryGetData(uid, MailVisuals.JobIcon, out string job);

        if (string.IsNullOrEmpty(job))
            job = "JobIconUnknown";

        if (!_prototypeManager.TryIndex<JobIconPrototype>(job, out var icon))
        {
            args.Sprite.LayerSetTexture(MailVisualLayers.JobStamp, _spriteSystem.Frame0(_prototypeManager.Index("JobIconUnknown")));
            return;
        }

        args.Sprite.LayerSetTexture(MailVisualLayers.JobStamp, _spriteSystem.Frame0(icon.Icon));
    }
}

public enum MailVisualLayers : byte
{
    Icon,
    Lock,
    FragileStamp,
    JobStamp,
    PriorityTape,
    Breakage
}
