// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Contains layout and dimension details for rendering the grid view in the console.
/// </summary>
internal sealed class GridViewDetails
{
    /// <summary>
    ///     Gets or sets the width of each column in the grid view.
    /// </summary>
    public int[]? ListViewColumnWidths { get; set; }

    /// <summary>
    ///     Gets or sets the offset where the header should start, accounting for space needed for checkboxes.
    /// </summary>
    public int ListViewOffset { get; set; }

    /// <summary>
    ///     Gets or sets the usable width available on the screen after subtracting space needed for UI elements such as spaces
    ///     between columns.
    /// </summary>
    public int UsableWidth { get; set; }
}