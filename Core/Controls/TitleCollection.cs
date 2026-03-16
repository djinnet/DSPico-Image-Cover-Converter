using System.Collections.ObjectModel;

namespace PicoLauncher.Core.Controls;

public class TitleCollection : Collection<string>
{
    public event EventHandler Changed;

    protected override void InsertItem(int index, string item)
    {
        base.InsertItem(index, item);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    protected override void SetItem(int index, string item)
    {
        base.SetItem(index, item);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

public class TitleCollectionEditor : System.ComponentModel.Design.CollectionEditor
{
    public TitleCollectionEditor() : base(typeof(TitleCollection))
    {
    }
    protected override Type CreateCollectionItemType()
    {
        return typeof(string);
    }
}
