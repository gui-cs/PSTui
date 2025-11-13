// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Text;
using Terminal.Gui.Views;

namespace OutGridView.Cmdlet;

internal sealed class GridViewDataSource : IListDataSource, IDisposable
{
    public List<GridViewRow> GridViewRowList { get; set; }

    public int Count => GridViewRowList.Count;
    
    public int Length => GridViewRowList.Count;
    
    public bool SuspendCollectionChangedEvent { get; set; }
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public GridViewDataSource(List<GridViewRow> itemList)
    {
        GridViewRowList = itemList;
    }

    public void Render(ListView listView, bool selected, int item, int col, int line, int width, int start = 0)
    {
        listView.Move(col, line);

        var driver = Application.Driver;
        var row = GridViewRowList[item];
        driver!.AddStr(row.DisplayString);
        
    }

    public bool IsMarked(int item) => GridViewRowList[item].IsMarked;

    public void SetMark(int item, bool value)
    {
        var oldValue = GridViewRowList[item].IsMarked;
        GridViewRowList[item].IsMarked = value;
        var args = new RowMarkedEventArgs
        {
            Row = GridViewRowList[item],
            OldValue = oldValue
        };
        MarkChanged?.Invoke(this, args);
    }

    public sealed class RowMarkedEventArgs : EventArgs
    {
        public required GridViewRow Row { get; set; }
        public bool OldValue { get; set; }
    }

    public event EventHandler<RowMarkedEventArgs>? MarkChanged;

    public IList ToList()
    {
        return GridViewRowList;
    }
       
    public void Dispose()
    {
        // No resources to dispose currently
    }

}
