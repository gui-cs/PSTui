// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;

// TODO: switch to System.Text.Json

namespace Microsoft.PowerShell.OutGridView.Models;

/// <summary>
///     Represents a column in a data table with metadata about its label, type, and property accessor.
/// </summary>
/// <param name="label">The display label for the column.</param>
/// <param name="propertyScriptAccessor">The script accessor used to retrieve the property value.</param>
public class DataTableColumn(string label, string propertyScriptAccessor)
{
    /// <summary>
    ///     Gets the runtime type of the column based on the <see cref="StringType" /> property.
    /// </summary>
    [JsonIgnore]
    public Type? Type => Type.GetType(StringType!);

    /// <summary>
    ///     Gets the display label for the column.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    ///     Gets or sets the serializable string representation of the column's type.
    /// </summary>
    public string? StringType { get; set; }

    /// <summary>
    ///     Gets the script accessor used to retrieve the property value for this column.
    /// </summary>
    public string PropertyScriptAccessor { get; } = propertyScriptAccessor;

    /// <summary>
    ///     Determines whether the specified object is equal to the current column.
    ///     Two columns are considered equal if they have the same label and property script accessor.
    /// </summary>
    /// <param name="obj">The object to compare with the current column.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified object is equal to the current column; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        var b = obj as DataTableColumn;
        return b?.Label == Label && b.PropertyScriptAccessor == PropertyScriptAccessor;
    }

    /// <summary>
    ///     Returns the hash code for this column based on its label and property script accessor.
    /// </summary>
    /// <returns>A hash code for the current column.</returns>
    public override int GetHashCode() => Label.GetHashCode() + PropertyScriptAccessor.GetHashCode();

    /// <summary>
    ///     Returns a Base64-encoded string representation of the column for safe embedding in XAML.
    /// </summary>
    /// <returns>A Base64-encoded string containing the label and property script accessor.</returns>
    public override string ToString() =>
        // Needs to be encoded to embed safely in XAML
        Convert.ToBase64String(Encoding.UTF8.GetBytes(Label + PropertyScriptAccessor));
}