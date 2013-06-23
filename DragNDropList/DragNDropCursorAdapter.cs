/*
 * Copyright 2012 Terlici Ltd.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.Content;
using Android.Database;
using Android.Views;

namespace DragNDropListView
{
    public class DragNDropCursorAdapter : Android.Support.V4.Widget.SimpleCursorAdapter, IDragNDropAdapter
    {
        private int[] _position;
        public DragNDropCursorAdapter(Context context, int layout, ICursor c, string[] @from, int[] to, int handler) 
            : base(context, layout, c, @from, to)
        {
            DragHandler = handler;
            Setup();
        }

        private void Setup()
        {
            var c = Cursor;

            if (null == c || !c.MoveToFirst()) return;

            _position = new int[c.Count];

            for (var i = 0; i < _position.Length; ++i) _position[i] = i;
        }

        public override ICursor SwapCursor(ICursor c)
        {
            var cursor = base.SwapCursor(c);

            _position = null;
            Setup();

            return cursor;
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

        public void OnItemDrag(DragNDropListView parent, View view, int position, long id)
        { }

        public void OnItemDrop(DragNDropListView parent, View view, int startPosition, int endPosition, long id)
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