// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Represents a single row in the grid view, including its display text, marked state, and original position.
/// </summary>
public class GridViewRow
{
    /// <summary>
    ///     Gets or sets the formatted string to display for this row in the grid view.
    /// </summary>
    public string? DisplayString { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this row is marked (selected) by the user.
    /// </summary>
    public bool IsMarked { get; set; }

    /// <summary>
    ///     Gets or sets the original index of this row in the source data before any filtering or sorting.
    /// </summary>
    public int OriginalIndex { get; set; }

    /// <summary>
    ///     Returns the display string representation of this row.
    /// </summary>
    /// <returns>The <see cref="DisplayString" /> value.</returns>
    public override string ToString() => DisplayString ?? string.Empty;
}