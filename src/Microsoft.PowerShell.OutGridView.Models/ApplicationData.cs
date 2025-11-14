// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.OutGridView.Models;

/// <summary>
///     Represents the application configuration and state data for the Out-ConsoleGridView component.
/// </summary>
public class ApplicationData
{
    /// <summary>
    ///     Gets or sets the title displayed in the Out-GridView window.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     Gets or sets the output mode that determines how items can be selected and returned.
    /// </summary>
    public OutputModeOption OutputMode { get; set; }

    /// <summary>
    ///     Gets or sets the filter text to apply to the data table.
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use minimal UI mode.
    /// </summary>
    public bool MinUI { get; set; }

    /// <summary>
    ///     Gets or sets the data table containing the columns and rows to display.
    /// </summary>
    public DataTable? DataTable { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use the .NET driver for rendering.
    /// </summary>
    public bool UseNetDriver { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether verbose output is enabled.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether debug output is enabled.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    ///     Gets or sets the version of the module.
    /// </summary>
    public string? ModuleVersion { get; set; }
}