using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Runtime;

using DragNDropListView;

namespace Sample
{
    [Activity(Label = "DragNDrop ListView Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class SampleActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var data = new JavaList<IDictionary<string, object>>();
            for (var i = 0; i < 10; ++i)
                data.Add(new JavaDictionary<string, object>
                    {
                        {"text", "Test " + i},
                        {"icon", Android.Resource.Drawable.ButtonStarBigOn},
                    });


            var listView = FindViewById<DragNDropListView.DragNDropListView>(Android.Resource.Id.List);
            listView.Adapter = new DragNDropSimpleAdapter(
                this, data, Resource.Layout.dnd_list_item, 
                new[] { "text", "icon"}, 
                new[] { Resource.Id.text, Resource.Id.handler }, 
                Resource.Id.handler);
        }
    }
}

