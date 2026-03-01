#pragma warning disable IDE0305 // Simplify collection initialization

using System.Collections.Generic;

namespace ObenFind
{
    internal class Result(Storage storage)
    {
        public Storage Storage { get; set; } = storage;
        public List<Item> Items { get; set; } = [];
        public List<(Item item, string text)> ItemTuples { get; set; } = [];
    }
}
