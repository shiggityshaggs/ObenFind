#pragma warning disable IDE0305 // Simplify collection initialization

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObenFind
{
    internal partial class GUIComponent
    {
        readonly bool debug = false;

        readonly List<Storage> Storages = [];
        List<Result> SearchResults = [];
        string SearchString = string.Empty;
        readonly HashSet<Outline> Outlines = [];
        readonly HashSet<MeshRenderer> Renderers = [];

        void Log(string msg)
        {
            if (debug)
                Console.WriteLine(msg);
        }

        void ClearRenderers()
        {
            foreach (var outline in Outlines.Where(outline => outline != null))
            {
                outline.enabled = false;
            }
            Outlines.Clear();

            foreach (var renderer in Renderers.Where(renderer => renderer != null))
            {
                renderer.allowOcclusionWhenDynamic = true;
            }
            Renderers.Clear();
        }

        void Search()
        {
            ClearRenderers();
            SearchResults.Clear();

            if (string.IsNullOrEmpty(SearchString))
                return;

            sw.Restart();

            SearchResults = Storages
                .Where(storage => storage != null)
                .Select(storage =>
                {
                    Result result = new(storage);

                    result.Items = storage.storageSlots
                    .Select(slot => slot?.itemReference?.Item)
                    .Where(item => item != null && (SearchString == " " || item.Title.ToLower().Contains(SearchString.ToLower())) )
                    .DistinctBy(item => item.Title)
                    .ToList();

                    result.ItemTuples = result.Items
                    .Select(item =>
                    {
                        var text = Helpers.ColorizedMatch(SearchString, item.Title, RichtextColor.cyan);
                        return (item, text);
                    })
                    .ToList();

                    return result;
                })
                .Where(result => result.Items.Any())
                .ToList();

            foreach (var result in SearchResults)
            {
                if (!result.Storage.TryGetComponent(out Outline outline))
                {
                    outline = result.Storage.gameObject.AddComponent<Outline>();
                }
                Outlines.Add(outline);
                outline.enabled = true;

                var renderers = outline.gameObject.GetComponentsInChildren<MeshRenderer>()
                    .Where(r => r && r.allowOcclusionWhenDynamic);

                foreach (var renderer in renderers)
                {
                    Renderers.Add(renderer);
                    renderer.allowOcclusionWhenDynamic = false;
                }
            }

            Log($"Search: {sw.ElapsedMilliseconds}ms");
        }

        void RefreshStorages()
        {
            sw.Restart();

            ClearRenderers();
            SearchResults.Clear();
            SearchString = string.Empty;

            var fromManagers = BuildingSystem.instance?.furnitureManagers?
                .Where(mgr => mgr != null && mgr.AllFurnitures != null)
                .SelectMany(mgr => mgr.AllFurnitures.SelectMany(fi => fi?.gameObject?.GetComponentsInChildren<Storage>()))
                .Where(storage => (storage?.StorageOwner?.ID ?? 0) == 0)
                .ToList();

            var fromSauna = TenementController.instance.general.prefabs.content.currentSauna != null ? TenementController.instance.general.prefabs.content.currentSauna
                .GetComponentsInChildren<Storage>() : [];

            var fromUpgrades = BuildingSystem.instance?.furnitureManagers?
                .Where(mgr => mgr != null && mgr.tenementApartmentPrefabs != null)
                .SelectMany(mgr => mgr.tenementApartmentPrefabs.GetComponentsInChildren<Storage>())
                .Where(storage => (storage?.StorageOwner?.ID ?? 0) == 0)
                .ToList();

            Storages.AddRange(fromManagers ?? []);
            Storages.AddRange(fromUpgrades ?? []);
            Storages.AddRange(fromSauna ?? []);

            Log($"Refresh: {sw.ElapsedMilliseconds}ms");
        }

        void RemoveStorageFromResults(Storage storage)
        {
            if (!removeVisitedStorages) return;

            sw.Restart();

            if (storage.TryGetComponent(out Outline outline)) outline.enabled = false;

            foreach (var renderer in storage.GetComponentsInChildren<MeshRenderer>())
            {
                if (Renderers.Contains(renderer))
                    renderer.allowOcclusionWhenDynamic = true;
            }

            var index = SearchResults.FindIndex(x => x.Storage == storage);
            SearchResults.RemoveAt(index);

            Log($"RemoveStorageFromResults: {sw.ElapsedMilliseconds}ms");
        }

        internal void Storage_EnterExternally(Storage storage)
        {
            if (!IsSearchResultStorage(storage)) return;

            var searchedIDs = SearchedItemIDs();

            var slots = storage.Slots.
                Select(controller => controller.slot)
                .Where(slot =>
                {
                    int id = slot?.slotcontrol?.itemStack?.itemId ?? -1;
                    return (id != -1 && searchedIDs.Contains(id)) ? slot : false;
                });

            foreach (var slot in slots)
            {
                slot.slotImage.color = Color.cyan;
            }

            RemoveStorageFromResults(storage);
        }

        bool IsSearchResultStorage(Storage storage) => SearchResults
            .Any(result => result.Storage == storage);

        List<int> SearchedItemIDs() => SearchResults
            .SelectMany(result => result.Items.Select(item => item.ID))
            .ToList();
    }
}
