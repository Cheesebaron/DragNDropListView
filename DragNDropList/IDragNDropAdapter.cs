using Android.Widget;

namespace DragNDropListView
{
    public interface IDragNDropAdapter : IListAdapter, DragNDropListView.IOnItemDragNDropListener
    {
        int DragHandler { get; }
    }
}