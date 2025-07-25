// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Threading.Tasks;
using Content.Server.EUI;
using Content.Shared.Administration.Notes;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Eui;
using Robust.Shared.Configuration;

namespace Content.Server.Administration.Notes;

public sealed class UserNotesEui : BaseEui
{
    [Dependency] private readonly IAdminNotesManager _notesMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private readonly bool _seeOwnNotes;

    public UserNotesEui()
    {
        IoCManager.InjectDependencies(this);
        _seeOwnNotes = _cfg.GetCVar(CCVars.SeeOwnNotes);

        if (!_seeOwnNotes)
        {
            Logger.WarningS("admin.notes", "User notes initialized when see_own_notes set to false");
        }
    }

    private Dictionary<(int, NoteType), SharedAdminNote> Notes { get; set; } = new();

    public override EuiStateBase GetNewState()
    {
        return new UserNotesEuiState(
            Notes
        );
    }

    public async Task UpdateNotes()
    {
        if (!_seeOwnNotes)
        {
            Logger.WarningS("admin.notes", $"User {Player.Name} with ID {Player.UserId} tried to update their own user notes when see_own_notes was set to false");
            return;
        }

        Notes = (await _notesMan.GetVisibleRemarks(Player.UserId)).Select(note => note.ToShared()).ToDictionary(note => (note.Id, note.NoteType));
        StateDirty();
    }
}
