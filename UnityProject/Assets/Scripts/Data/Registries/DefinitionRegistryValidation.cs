using System;
using System.Collections.Generic;
namespace MobileGame.Foundation
{
    public static class DefinitionRegistryValidation
    {
        public static void Add<T>(IEnumerable<T> entries, IDictionary<string, ThingDefinition> all) where T : ThingDefinition
        {
            if(entries == null) throw new ArgumentException("Missing registry entries.");
            foreach(var entry in entries)
            {
                if(entry == null) throw new ArgumentException("Null SO registry entry.");
                entry.Validate();
                if(all.ContainsKey(entry.DefinitionId)) throw new ArgumentException("Duplicate registry ID: " + entry.DefinitionId);
                all.Add(entry.DefinitionId,entry);
            }
        }
    }
}
