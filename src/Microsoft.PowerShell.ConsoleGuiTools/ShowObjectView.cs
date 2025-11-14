// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;

using OutGridView.Models;

using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;

namespace OutGridView.Cmdlet;

internal sealed class ShowObjectView : Window, ITreeBuilder<object>
{
    private readonly TreeView<object> tree;
    private readonly RegexTreeViewTextFilter filter;
    private readonly Label filterErrorLabel;

    public bool SupportsCanExpand => true;
    private Shortcut? selectedStatusBarItem;
    private StatusBar? statusBar;

    public ShowObjectView(List<object> rootObjects, ApplicationData applicationData)
    {
        Title = applicationData.Title ?? "Show Object Tree";
        Width = Dim.Fill();
        Height = Dim.Fill(1);
        Modal = false;


        if (applicationData.MinUI)
        {
            BorderStyle = LineStyle.None;
            Title = string.Empty;
            X = -1;
            Height = Dim.Fill();
        }

        tree = new TreeView<object>
        {
            Y = applicationData.MinUI ? 0 : 2,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        tree.TreeBuilder = this;
        tree.AspectGetter = this.AspectGetter;
        tree.SelectionChanged += this.SelectionChanged;

        // In Terminal.Gui v2, the default keybindings are different
        // No need to clear ExpandAll keybinding

        this.filter = new RegexTreeViewTextFilter(this, tree);
        this.filter.Text = applicationData.Filter ?? string.Empty;
        tree.Filter = this.filter;

        if (rootObjects.Count > 0)
        {
            tree.AddObjects(rootObjects);
        }
        else
        {
            tree.AddObject("No Objects");
        }
        
        var shortcuts = new List<Shortcut>();

        string elementDescription = "objects";

        var types = rootObjects.Select(o => o.GetType()).Distinct().ToArray();
        if (types.Length == 1)
        {
            elementDescription = types[0].Name;
        }

        var lblFilter = new Label
        {
            Text = "Filter:",
            X = 1,
        };
        var tbFilter = new TextField
        {
            Text = applicationData.Filter ?? string.Empty,
            X = Pos.Right(lblFilter),
            Width = Dim.Fill(1)
        };
        tbFilter.CursorPosition = tbFilter.Text.Length;

        tbFilter.TextChanged += (sender, e) =>
        {
            filter.Text = tbFilter.Text?.ToString() ?? string.Empty;
        };


        filterErrorLabel = new Label
        {
            Text = string.Empty,
            X = Pos.Right(lblFilter) + 1,
            Y = Pos.Top(lblFilter) + 1,
            Width = Dim.Fill() - lblFilter.Text!.Length
        };

        if (!applicationData.MinUI)
        {
            Add(lblFilter);
            Add(tbFilter);
            Add(filterErrorLabel);
        }

        shortcuts.Add(new Shortcut(Key.Esc, "Close", () => Application.RequestStop()));

        var siCount = new Shortcut(Key.Empty, $"{rootObjects.Count} {elementDescription}", null);
        selectedStatusBarItem = new Shortcut(Key.Empty, string.Empty, null);
        shortcuts.Add(siCount);
        shortcuts.Add(selectedStatusBarItem);

        if (applicationData.Debug)
        {
            shortcuts.Add(new Shortcut(Key.Empty, $" v{applicationData.ModuleVersion}", null));
            shortcuts.Add(new Shortcut(Key.Empty,
            $"{Application.Driver} v{FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Application))!.Location).ProductVersion}", null));
        }

        statusBar = new StatusBar(shortcuts);
        statusBar.Visible = !applicationData.MinUI;
        Application.Top!.Add(statusBar);

    Add(tree);
}

    internal void SetRegexError(string error)
    {
        if (string.Equals(error, filterErrorLabel.Text?.ToString(), StringComparison.Ordinal))
        {
            return;
        }
        filterErrorLabel.Text = error;
        filterErrorLabel.SchemeName = "Error";
    }  

    private void SelectionChanged(object? sender, SelectionChangedEventArgs<object> e)
    {
        var selectedValue = e.NewValue;

        if (selectedValue is CachedMemberResult cmr)
        {
            selectedValue = cmr.Value;
        }

        if (selectedValue != null && selectedStatusBarItem != null)
        {
            selectedStatusBarItem.Title = selectedValue.GetType().Name;
        }
        else if (selectedStatusBarItem != null)
        {
            selectedStatusBarItem.Title = string.Empty;
        }

        statusBar?.SetNeedsDraw();
    }

    private string? AspectGetter(object toRender)
    {
        if (toRender is Process p)
        {
            return p.ProcessName;
        }
        if (toRender is null)
        {
            return "Null";
        }
        if (toRender is FileSystemInfo fsi && !IsRootObject(fsi))
        {
            return fsi.Name;
        }

        return toRender.ToString();
    }

    private bool IsRootObject(object o)
    {
        return tree.Objects.Contains(o);
    }

    public bool CanExpand(object toExpand)
    {
        if (toExpand is CachedMemberResult p)
        {
            return IsBasicType(p?.Value);
        }

        // Any complex object type can be expanded to reveal properties
        return IsBasicType(toExpand);
    }

    private static bool IsBasicType(object? value)
    {
        return value != null && value is not string && !value.GetType().IsValueType;
    }

    public IEnumerable<object> GetChildren(object forObject)
    {
        if (forObject == null || !this.CanExpand(forObject))
        {
            return Enumerable.Empty<object>();
        }

        if (forObject is CachedMemberResult p)
        {
            if (p.IsCollection)
            {
                return p.Elements ?? Enumerable.Empty<object>();
            }

            return GetChildren(p.Value);
        }

        if (forObject is CachedMemberResultElement e)
        {
            return GetChildren(e.Value);
        }

        List<object> children = new List<object>();

        foreach (var member in forObject.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public).OrderBy(m => m.Name))
        {
            if (member is PropertyInfo prop)
            {
                children.Add(new CachedMemberResult(forObject, prop));
            }
            if (member is FieldInfo field)
            {
                children.Add(new CachedMemberResult(forObject, field));
            }
        }

        try
        {
            children.AddRange(GetExtraChildren(forObject));
        }
        catch (Exception)
        {
            // Extra children unavailable, possibly security or IO exceptions enumerating children etc
        }

        return children;
    }

    private static IEnumerable<object> GetExtraChildren(object forObject)
    {
        if (forObject is DirectoryInfo dir)
        {
            foreach (var c in dir.EnumerateFileSystemInfos())
            {
                yield return c;
            }
        }
    }

    internal static void Run(List<PSObject> objects, ApplicationData applicationData)
    {
        // In Terminal.Gui v2, Application.Init() no longer accepts a driver parameter.
        // Instead, use Application.ForceDriver to specify the driver.
        if (applicationData.UseNetDriver)
        {
            Application.ForceDriver = "NetDriver";
        }
        Application.Init();
        Window? window = null;

        try
        {
            window = new ShowObjectView(objects.Select(p => p.BaseObject).ToList(), applicationData);
            Application.Top!.Add(window);
            Application.Run();
        }
        finally
        {
            Application.Shutdown();
            window?.Dispose();
        }
    }
}

sealed class CachedMemberResultElement
{
    public int Index;
    public object? Value;

    private readonly string representation;

    public CachedMemberResultElement(object? value, int index)
    {
        Index = index;
        Value = value;

        try
        {
            representation = Value?.ToString() ?? "Null";
        }
        catch (Exception)
        {
            Value = representation = "Unavailable";
        }
    }
    public override string ToString()
    {
        return $"[{Index}]: {representation}]";
    }
}

sealed class CachedMemberResult
{
    public MemberInfo Member;
    public object? Value;
    public object Parent;
    private readonly string representation;
    private List<CachedMemberResultElement>? valueAsList;


    public bool IsCollection => valueAsList != null;
    public IReadOnlyCollection<CachedMemberResultElement>? Elements => valueAsList?.AsReadOnly();

    public CachedMemberResult(object parent, MemberInfo mem)
    {
        Parent = parent;
        Member = mem;

        try
        {
            if (mem is PropertyInfo p)
            {
                Value = p.GetValue(parent);
            }
            else if (mem is FieldInfo f)
            {
                Value = f.GetValue(parent);
            }
            else
            {
                throw new NotSupportedException($"Unknown {nameof(MemberInfo)} Type");
            }

            representation = ValueToString();

        }
        catch (Exception)
        {
            Value = representation = "Unavailable";
        }
    }

    private string? ValueToString()
    {
        if (Value == null)
        {
            return "Null";
        }
        try
        {
            if (IsCollectionOfKnownTypeAndSize(out Type? elementType, out int size))
            {
                return $"{elementType!.Name}[{size}]";
            }
        }
        catch (Exception)
        {
            return Value?.ToString();
        }


        return Value?.ToString();
    }

    private bool IsCollectionOfKnownTypeAndSize(out Type? elementType, out int size)
    {
        elementType = null;
        size = 0;

        if (Value == null || Value is string)
        {

            return false;
        }

        if (Value is IEnumerable ienumerable)
        {
            var list = ienumerable.Cast<object>().ToList();

            var types = list.Where(v => v != null).Select(v => v!.GetType()).Distinct().ToArray();

            if (types.Length == 1)
            {
                elementType = types[0];
                size = list.Count;

                valueAsList = list.Select((e, i) => new CachedMemberResultElement(e, i)).ToList();
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return Member.Name + ": " + representation;
    }
}

sealed class RegexTreeViewTextFilter : ITreeViewFilter<object>
{
    private readonly ShowObjectView parent;
    readonly TreeView<object> _forTree;

    public RegexTreeViewTextFilter(ShowObjectView parent, TreeView<object> forTree)
    {
        this.parent = parent;
        _forTree = forTree ?? throw new ArgumentNullException(nameof(forTree));
    }

    private string text = string.Empty;

    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            RefreshTreeView();
        }
    }

    private void RefreshTreeView()
    {
        _forTree.InvalidateLineMap();
        _forTree.SetNeedsDraw();
    }

    public bool IsMatch(object model)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return true;
        }

        parent.SetRegexError(string.Empty);

        var modelText = _forTree.AspectGetter(model);
        try
        {
            return Regex.IsMatch(modelText ?? string.Empty, text, RegexOptions.IgnoreCase);
        }
        catch (RegexParseException e)
        {
            parent.SetRegexError(e.Message);
            return true;
        }
    }
}
