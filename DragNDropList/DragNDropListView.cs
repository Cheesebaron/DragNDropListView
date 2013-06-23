/*
 * Copyright 2013 Tomasz Cielecki @Cheesebaron
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

using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DragNDropListView
{
    public class DragNDropListViewDragEventArgs: EventArgs
    {
        public DragNDropListView ListView { get; set; }
        public View View { get; set; }
        public int Position { get; set; }
        public long Id { get; set; }
    }

    public class DragNDropListViewDropEventArgs : EventArgs
    {
        public DragNDropListView ListView { get; set; }
        public View View { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public long Id { get; set; }
    }

    public delegate void DragNDropListViewDragEventHandler(DragNDropListViewDragEventArgs args);
    public delegate void DragNDropListViewDropEventHandler(DragNDropListViewDropEventArgs args);

    public class DragNDropListView : ListView
    {
        public event DragNDropListViewDragEventHandler ItemDrag;
        public event DragNDropListViewDropEventHandler ItemDrop;

        public interface IOnItemDragNDropListener
        {
            void OnItemDrag(DragNDropListView parent, View view, int position, long id);
            void OnItemDrop(DragNDropListView parent, View view, int startPosition, int endPosition, long id);
        }

        public bool Dragging { get; private set; }

        public ImageView DragView { get; private set; }

        private int _startPosition = InvalidPosition;
        private int _dragPointOffset;
        private int _dragHandler = 0;

        private IOnItemDragNDropListener _dragNDropListener;

        public DragNDropListView(Context context) 
            : this(context, null)
        { }

        public DragNDropListView(Context context, IAttributeSet attrs) 
            : this(context, attrs, 0)
        { }

        public DragNDropListView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
        { }

        public void SetOnItemDragNDropListener(IOnItemDragNDropListener listener)
        {
            _dragNDropListener = listener;
        }

        public void SetDragNDropAdapter(IDragNDropAdapter adapter)
        {
            _dragHandler = adapter.DragHandler;
            Adapter = adapter;
        }

        public bool IsDrag(MotionEvent ev)
        {
            if (Dragging) return true;
            if (_dragHandler == 0) return false;

            var x = (int)ev.GetX();
            var y = (int)ev.GetY();

            var startPosition = PointToPosition(x, y);

            if (startPosition == InvalidPosition) return false;

            var childPosition = startPosition - FirstVisiblePosition;
            var parent = GetChildAt(childPosition);
            var handler = parent.FindViewById(_dragHandler);

            if (handler == null) return false;

            var top = parent.Top + handler.Top;
            var bottom = top + handler.Height;
            var left = parent.Left + handler.Left;
            var right = left + handler.Width;

            return left <= x && x <= right && top <= y && y <= bottom;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var action = e.Action;
            var x = (int) e.GetX();
            var y = (int) e.GetY();

            if (action == MotionEventActions.Down && IsDrag(e)) Dragging = true;

            if (Dragging) return base.OnTouchEvent(e);

            switch (action)
            {
                case MotionEventActions.Down:
                    _startPosition = PointToPosition(x, y);

                    if (_startPosition != InvalidPosition)
                    {
                        var childPosition = _startPosition - FirstVisiblePosition;
                        _dragPointOffset = y - GetChildAt(childPosition).Top;
                        _dragPointOffset -= (int) e.RawY - y;

                        StartDrag(childPosition, y);
                        Drag(0, y);
                    }
                    break;

                case MotionEventActions.Move:
                    Drag(0, y);
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                default:
                    Dragging = false;

                    if (_startPosition != InvalidPosition)
                        StopDrag(_startPosition - FirstVisiblePosition, PointToPosition(x, y));
                    break;
            }

            return true;
        }

        private void StartDrag(int childPosition, int y)
        {
            var item = GetChildAt(childPosition);

            if (null == item) return;

            var id = GetItemIdAtPosition(_startPosition);
            if (null != _dragNDropListener)
                _dragNDropListener.OnItemDrag(this, item, _startPosition, id);

            ((IDragNDropAdapter)Adapter).OnItemDrag(this, item, _startPosition, id);

            if (null != ItemDrag)
                ItemDrag(new DragNDropListViewDragEventArgs{Id = id, ListView = this, View = item, Position = _startPosition});

            item.DrawingCacheEnabled = true;

            using (var bitmap = Bitmap.CreateBitmap(item.DrawingCache))
            {
                var windowParams = new WindowManagerLayoutParams
                    {
                        Gravity = GravityFlags.Top,
                        X = 0,
                        Y = y - _dragPointOffset,
                        Height = ViewGroup.LayoutParams.WrapContent,
                        Width = ViewGroup.LayoutParams.WrapContent,
                        Flags = WindowManagerFlags.NotFocusable
                                | WindowManagerFlags.NotTouchable
                                | WindowManagerFlags.KeepScreenOn
                                | WindowManagerFlags.LayoutInScreen
                                | WindowManagerFlags.LayoutNoLimits,
                        Format = Format.Translucent,
                        WindowAnimations = 0
                    };

                var v = new ImageView(Context);
                v.SetImageBitmap(bitmap);

                var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                windowManager.AddView(v, windowParams);
                DragView = v;

                item.Visibility = ViewStates.Invisible;
                item.Invalidate();
            }
        }

        private void StopDrag(int childIndex, int endPosition)
        {
            if (null == DragView) return;

            var item = GetChildAt(childIndex);

            if (endPosition != InvalidPosition)
            {
                var id = GetItemIdAtPosition(_startPosition);

                if (null != _dragNDropListener)
                    _dragNDropListener.OnItemDrop(this, item, _startPosition, endPosition, id);

                ((IDragNDropAdapter)Adapter).OnItemDrop(this, item, _startPosition, endPosition, id);

                if (null != ItemDrop)
                    ItemDrop(new DragNDropListViewDropEventArgs{ Id = id, ListView = this, View = item, StartPosition = _startPosition, EndPosition = endPosition });
            }

            DragView.Visibility = ViewStates.Gone;
            var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.RemoveView(DragView);

            DragView.SetImageDrawable(null);
            DragView = null;

            item.DrawingCacheEnabled = false;
            item.DestroyDrawingCache();

            item.Visibility = ViewStates.Visible;

            _startPosition = InvalidPosition;

            InvalidateViews();
        }

        private void Drag(int x, int y)
        {
            if (null == DragView) return;

            var layoutParams = (WindowManagerLayoutParams) DragView.LayoutParameters;
            layoutParams.X = x;
            layoutParams.Y = y - _dragPointOffset;

            var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.UpdateViewLayout(DragView, layoutParams);
        }
    }
}