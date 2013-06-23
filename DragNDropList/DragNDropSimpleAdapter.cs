using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DragNDropListView
{
    public class DragNDropSimpleAdapter : SimpleAdapter, IDragNDropAdapter
    {
        private readonly int[] _position;

        public DragNDropSimpleAdapter(Context context, JavaList<IDictionary<string, object>> data, int resource, string[] @from, int[] to, int handler) 
            : base(context, data, resource, @from, to)
        {

            DragHandler = handler;
            _position = new int[data.Count];

            for (var i = 0; i < data.Count; ++i) _position[i] = i;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return base.GetDropDownView(_position[position], convertView, parent);
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return base.GetItem(_position[position]);
        }

        public override int GetItemViewType(int position)
        {
            return base.GetItemViewType(_position[position]);
        }

        public override long GetItemId(int position)
        {
            return base.GetItemId(_position[position]);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return base.GetView(_position[position], convertView, parent);
        }

        public override bool IsEnabled(int position)
        {
            return base.IsEnabled(_position[position]);
        }

        public void OnItemDrag(global::DragNDropListView.DragNDropListView parent, View view, int position, long id)
        { }

        public void OnItemDrop(global::DragNDropListView.DragNDropListView parent, View view, int startPosition, int endPosition, long id)
        {
            var position = _position[startPosition];

            if (startPosition < endPosition)
                for (var i = startPosition; i < endPosition; ++i)
                    _position[i] = _position[i + 1];
            else if (endPosition < startPosition)
                for (var i = startPosition; i > endPosition; --i)
                    _position[i] = _position[i - 1];

            _position[endPosition] = position;
        }

        public int DragHandler { get; private set; }
    }
}