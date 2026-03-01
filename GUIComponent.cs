using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObenFind
{
    internal partial class GUIComponent : MonoBehaviour
    {
        readonly Stopwatch sw = Stopwatch.StartNew();
        long elapsed;

        readonly int windowId = nameof(ObenFind).GetHashCode();
        Rect windowRect;
        Vector2 scrollPos;

        bool removeVisitedStorages = true;
        bool focusPending;

        void Awake()
        {
            Patches.OnEnteredExternally += Storage_EnterExternally;
        }

        void OnEnable()
        {
            RefreshStorages();
            focusPending = true;
        }

        void OnDisable()
        {
            sw.Reset();
        }

        void OnGUI()
        {
            if (GameController.instance == null)
            {
                this.enabled = false;
                return;
            }

            sw.Restart();
            GUI.backgroundColor = Color.Lerp(Color.black, Color.red, elapsed / 1000);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, string.Empty);
            GUI.backgroundColor = Color.white;
            elapsed = sw.ElapsedMilliseconds;
        }

        void WindowFunction(int id)
        {
            removeVisitedStorages = GUILayout.Toggle(removeVisitedStorages, "Unmark on opened");
            Show_FilterBox();
            Show_Results();
            HandleFocus();

            if (!GUI.changed)
                GUI.DragWindow();
        }

        void HandleFocus()
        {
            if (focusPending)
            {
                if (GUI.GetNameOfFocusedControl() == "Filter")
                    focusPending = false;
                else
                    GUI.FocusControl("Filter");
            }
        }

        void Show_FilterBox()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.SetNextControlName("Filter");
                string oldString = SearchString;
                SearchString = GUILayout.TextField(SearchString, GUILayout.Width(225));
                if (SearchString != oldString)
                    Search();

                if (!string.IsNullOrEmpty(SearchString) && GUILayout.Button("X", GUILayout.Width(25)))
                {
                    ClearRenderers();
                    SearchResults.Clear();
                    SearchString = string.Empty;
                }
            }
        }

        void Show_Results()
        {
            using (var scope = new GUILayout.ScrollViewScope(scrollPos, GUILayout.Width(250), GUILayout.Height(250)))
            {
                scrollPos = scope.scrollPosition;

                HashSet<string> itemtitles = [];

                var results = SearchResults
                    .Where(result => result.Storage != null)
                    .SelectMany(result => result.ItemTuples)
                    .DistinctBy(tuple => tuple.item.Title)
                    .OrderBy(tuple => tuple.item.Title);

                foreach (var (item, text) in results)
                {
                    if (GUILayout.Button(text, GUI.skin.label))
                    {
                        SearchString = item.Title;
                        GUI.FocusControl(null);
                        Search();
                        return;
                    }
                }
            }
        }
    }
}
