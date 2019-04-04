using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.Directories;
using NetExtensions.Collection;
using Serilog;

namespace TechTreeCreator.DTO {
    public class ModEntityData<T> : IEnumerable<KeyValuePair<string, T>> where T : Entity {
        private readonly Dictionary<string, T> entities;
        private readonly StellarisDirectoryHelper directoryHelper;
        private readonly ModEntityData<T> previous;

        public ModEntityData(StellarisDirectoryHelper directoryHelper, ModEntityData<T> previous = null) {
            this.directoryHelper = directoryHelper;
            this.previous = previous;
            entities = new Dictionary<string, T>();
            Links = new HashSet<Link>();
        }

        public bool ContainsEntity(string key) {
            return entities.ContainsKey(key);
        }

        public bool ContainsEntityInTree(string key) {
            return ContainsEntity(key) || (previous != null && previous.ContainsEntityInTree(key));
        }

        public T this[string key] {
            get => entities.ContainsKey(key) || previous == null ? entities[key] : previous[key];
            set => entities[key] = value;
        }

        public ModEntityData<T> FindCoreGameData() {
            return directoryHelper.ModName == StellarisDirectoryHelper.StellarisCoreRootDirectory ? this : previous?.FindByModName(StellarisDirectoryHelper.StellarisCoreRootDirectory);
        }

        public ModEntityData<T> FindByModName(string name) {
            return directoryHelper.ModName == name ? this : previous?.FindByModName(name);
        }

        public IList<ModEntityData<T>> FindByModGroup(string groupName) {
            if (previous == null) {
                var result = new List<ModEntityData<T>>();
                if (directoryHelper.ModGroup == groupName) {
                    result.Add(this);
                }

                return result;
            }
            else {
                var result = previous.FindByModGroup(groupName);
                if (directoryHelper.ModGroup == groupName) {
                    result.Insert(0, this);
                }

                return result;
            }
        }

        public void ApplyToChain(Action<Dictionary<string, T>, ISet<Link>> action) {
            action(entities, Links);
            previous?.ApplyToChain(action);
        }

        public IEnumerable<T> Entities => entities.Values;

        public IEnumerable<T> AllEntities => AllEntitiesByKey.Values;

        public ISet<Link> Links { get; }

        public ISet<Link> AllLinks {
            get {
                if (previous == null) {
                    return new HashSet<Link>(Links);
                }
                else {
                    var previousAllLinks = previous.AllLinks;
                    previousAllLinks.AddRange(Links);
                    return previousAllLinks;
                }
            }
        }

        public ISet<Link> AllLinksForModGroup(string modGroup) {
            return modGroup != null ? AllLinks.Where(x => x.To.ModGroup == modGroup).ToHashSet() : AllLinks;
        }

        public Dictionary<string, T> AllEntitiesByKey {
            get {
                var list = GetAllEntities();
                list.Reverse();
                var result = new Dictionary<string, T>();
                foreach (var entitiesByKey in list) {
                    foreach (var (_, entity) in entitiesByKey) {
                        if (entities.ContainsKey(entity.Id)) {
                            Log.Logger.Debug("File {file} contained node {key} which overwrites previous node from {previousFile}", entity.FilePath, entity.Id, entities[entity.Id].FilePath);
                            entities[entity.Id] = entity;
                        }
                    }
                }

                return result;
            }
        }
        
        public IEnumerable<T> AllEntitiesForModGroup(string modGroup) {
            return modGroup != null ? AllEntitiesByKey.Values.Where(x => x.ModGroup == modGroup).ToList() : AllEntitiesByKey.Values.ToList();
        }

        private List<IEnumerable<KeyValuePair<string, T>>> GetAllEntities() {
            if (previous == null) {
                return new List<IEnumerable<KeyValuePair<string, T>>>() {entities};
            }
            else {
                var list = previous.GetAllEntities();
                list.Add(entities);
                return list;
            }
        }
        
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
            return entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}