using System;
using System.Collections.Generic;
using System.Linq;
using NetExtensions.Collection;
using NetExtensions.Object;
using Newtonsoft.Json.Serialization;

namespace TechTreeCreator.DTO {
    public enum TechArea {
        Physics,
        Society,
        Engineering
    }

    public enum TechFlag {
        Starter,
        Dangerous,
        Rare,
        Repeatable,
        NonTechDependency
    }

    public interface IHasCost {
        IDictionary<string, double> Cost { get; }
        IDictionary<string, double> Upkeep { get; }
    }
    
    public interface IGestaltAvailability {
        bool? Machines { get; set; }
        
        bool? Gestalt { get; set; }
    }

    public class Tech : Entity, IGestaltAvailability {
        public TechArea Area { get; set; }

        public int? Tier { get; set; }

        // ReSharper disable once PossibleInvalidOperationException
        public int TierValue => Tier.Value;

        public int? BaseCost { get; set; }

        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<TechFlag> Flags { get; set; }
        
        public bool? Machines { get; set; }
        
        public bool? Gestalt { get; set; }

        public Tech(string id) : base(id) {
        }
    }

    public class Building : Entity, IHasCost, IGestaltAvailability {
        public int? BaseBuildTime { get; set; }
        public string Category { get; set; }

        public IDictionary<string, double> Cost { get; }
        public IDictionary<string, double> Upkeep { get; }
        public IDictionary<string, double> Produces { get; }
        
        public bool? Machines { get; set; }
        
        public bool? Gestalt { get; set; }

        public Building(string id) : base(id) {
            Cost = new Dictionary<string, double>();
            Upkeep = new Dictionary<string, double>();
            Produces = new Dictionary<string, double>();
        }
    }

    public class ShipComponent : Entity, IHasCost {
        public string Size { get; set; }
        public string ComponentSet { get; set; }
        
        public string ComponentSetName { get; set; }
        public string ComponentSetDescription { get; set; }
        
        public Dictionary<string, string> Properties { get; }
        
        public IDictionary<string, double> Cost { get; }
        public IDictionary<string, double> Upkeep { get; }
        
        public ShipComponent(string id) : base(id) {
            Cost = new Dictionary<string, double>();
            Upkeep = new Dictionary<string, double>();
            Properties = new Dictionary<string, string>();
        }
    }
    
    public class ShipComponentSet : Entity {
        public IList<ShipComponent> ShipComponents { get; }
        
        public ShipComponentSet(string id) : base(id) {
            ShipComponents = new List<ShipComponent>();
        }

        public ShipComponentSet(string id, ShipComponent component) : base(component) {
            Id = id;
            Name = component.ComponentSetName ?? component.Name;
            Description = component.ComponentSetDescription ?? component.Description;
            ShipComponents = new List<ShipComponent>();
        }
    }

    public class ShipComponentSetDescription : Entity {
        public ShipComponentSetDescription(string id) : base(id) {
        }
    }

    public class Decision : Entity {
        public IDictionary<string, double> Cost { get; }
        
        public String CustomTooltip { get; set; }
        public Decision(string id) : base(id) {
            Cost = new Dictionary<string, double>();
        }
    }

    public abstract class Entity {
        public string Id { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public String ExtraName { get; set; }
        public String ExtraDesc { get; set; }

        private string icon;

        public string Icon {
            get => icon ?? Id;
            set => icon = value;
        }

        public string FilePath { get; set; }

        public string Mod { get; set; }

        public string ModGroup { get; set; }

        public IList<Tech> Prerequisites { get; set; }
        public IEnumerable<string> PrerequisiteIds { get; set; }
        
        public IDictionary<string, string> Modifiers { get; }

        // ReSharper disable once InconsistentNaming
        public string DLC { get; set; }

        public bool IconFound { get; set; }

        protected Entity(string id) {
            Id = id;
            Modifiers = new Dictionary<string, string>();
        }
        
        protected Entity(Entity other) {
            Id = other.Id;
            Name = other.Name;
            Description = other.Description;
            icon = other.icon;
            FilePath = other.FilePath;
            Mod = other.Mod;
            ModGroup = other.ModGroup;
            Prerequisites = new List<Tech>(other.Prerequisites);
            PrerequisiteIds = new List<string>(other.PrerequisiteIds);
            IconFound = other.IconFound;
            Modifiers = other.Modifiers;
        }
    }

    public class Link {
        private string id;

        public string Id {
            get {
                if (id == null) {
                    return From?.Id + "->" + To?.Id;
                }
                else {
                    return id;
                }
            }
            set => id = value;
        }

        public Entity From { get; set; }
        public Entity To { get; set; }
    }

    public class TechsAndDependencies {
        public IDictionary<string, Tech> Techs { get; set; }
        public ISet<Link> Prerequisites { get; set; }
    }

    public class ObjectsDependantOnTechs {
        //also add to the clone method at the bottom!
        private ModEntityData<Building> buildings;
        private ModEntityData<ShipComponent> shipComponents;
        private ModEntityData<ShipComponentSetDescription> shipComponentDescriptions;
        private ModEntityData<Decision> decisions;
        private ISet<string> modGroups = new HashSet<string>();

        private void FixImages<T>(ModEntityData<T> mods) where T: Entity
        {
            if (mods == null) return;
            // because the image copying only get the most recent version of the entity, make sure that the image flag is set on all
            // relevant for the vanilla graph display
            var liveEntity = mods.AllEntitiesByKey;
            mods.ApplyToChain((entities, links) =>
            {
                foreach (var entity in entities.Values)
                {
                    entity.IconFound = liveEntity[entity.Id].IconFound;
                }
            }); 
        }

        public void FixImages()
        {
            FixImages(buildings);
            FixImages(shipComponents);
            FixImages(shipComponentDescriptions);
            FixImages(decisions);
        }
        
        public ModEntityData<Building> Buildings {
            get => buildings;
            set {
                buildings = value; 
                modGroups.AddRange(value.AllEntities.Select(x => x.ModGroup));
            }
        }
        
        public ModEntityData<Decision> Decisions {
            get => decisions;
            set {
                decisions = value; 
                modGroups.AddRange(value.AllEntities.Select(x => x.ModGroup));
            }
        }
        
        public ModEntityData<ShipComponent> ShipComponents => shipComponents;

        public ObjectsDependantOnTechs SetShipComponents(ModEntityData<ShipComponent> shipComponents, ModEntityData<ShipComponentSetDescription> componentSets) {
            this.shipComponents = shipComponents;
            this.shipComponentDescriptions = componentSets;
            if (shipComponents != null && componentSets != null) {
                ShipComponentsSets = shipComponents.Transform<ShipComponentSet>((modName, entities, links) => MergeComponents(modName, entities, links, componentSets));
            }

            return this;
        }
        
        
        public ModEntityData<ShipComponentSet> ShipComponentsSets {
            get;
            private set;
        }
        
        private Tuple<IDictionary<string, ShipComponentSet>, ISet<Link>> MergeComponents(string modName, IDictionary<string, ShipComponent> shipComponents, ISet<Link> links, ModEntityData<ShipComponentSetDescription> componentSetDescriptors) {
            Dictionary<string, ShipComponentSet> result = new Dictionary<string, ShipComponentSet>();
            Dictionary<string, ShipComponentSet> nodeIdToComponentIdLookup = new Dictionary<string, ShipComponentSet>();

            var templateNames = componentSetDescriptors.FindByModName(modName);
            var componentSetTemplateIds = templateNames != null ? templateNames.AllEntities.Select(x => x.Id).ToHashSet() : new HashSet<string>();


            var nonMergedComponents = new HashSet<string>() {"ftl_components", "sensor_components"};

            foreach (var shipComponent in shipComponents.Values) {
                var componentId = shipComponent.ComponentSet ?? shipComponent.Id;
                // do not merge certain utilities
                if (nonMergedComponents.Contains(componentId)) {
                    componentId = shipComponent.Id;
                }
                
                ShipComponentSet shipComponentSet = result.ComputeIfAbsent(componentId, id => new ShipComponentSet(id, shipComponent));
                shipComponentSet.ShipComponents.Add(shipComponent);
                nodeIdToComponentIdLookup[shipComponent.Id] = shipComponentSet;
            }

            // combat comptuers handled separately
            if (result.TryGetValue("combat_computers", out var combatComputers)) {
                foreach (var computersShipComponent in combatComputers.ShipComponents) {
                    int lastUnderscoreIndex = computersShipComponent.Id.LastIndexOf("_", StringComparison.Ordinal);
                    var computerLevel = computersShipComponent.Id.Substring(lastUnderscoreIndex);
                    string key = combatComputers.Id + computerLevel;
                    ShipComponentSet shipComponentSet = result.ComputeIfAbsent(key, id => new ShipComponentSet(id, computersShipComponent));
                    shipComponentSet.ShipComponents.Add(computersShipComponent);
                    nodeIdToComponentIdLookup[computersShipComponent.Id] = shipComponentSet;
                }

                result.Remove("combat_computers");
            }
            
            
            // reactors handled separately
            if (result.TryGetValue("power_core", out var powerCores)) {
                foreach (var powerCoresShipComponent in powerCores.ShipComponents) {
                    
                    // try to use the actual component set for this
                    string componentSetKey = componentSetTemplateIds.FirstOrDefault(x => powerCoresShipComponent.Id.Contains(x));

                    string key;
                    if (componentSetKey != null) {
                        // add the modname such that eahc mod gets its own entries, otherwise all power cores for a given level get eaten by the last mod that touched them
                        key = modName + componentSetKey;
                    }
                    else {
                        // fallback works for core but not well for mods
                        int firstUnderScoreIndex = powerCoresShipComponent.Id.IndexOf("_", StringComparison.Ordinal);
                        var computerLevel = powerCoresShipComponent.Id.Substring(firstUnderScoreIndex);
                        if (powerCoresShipComponent.Id.StartsWith("ION_CANNON")) {
                            int firstUnderScoreIndex2 = computerLevel.IndexOf("_", 1, StringComparison.Ordinal);
                            computerLevel = computerLevel.Substring(firstUnderScoreIndex2);
                        }

                        key = powerCores.Id + computerLevel;
                    }

                    ShipComponentSet shipComponentSet = result.ComputeIfAbsent(key, id => new ShipComponentSet(id, powerCoresShipComponent));
                    
                    shipComponentSet.ShipComponents.Add(powerCoresShipComponent);
                    nodeIdToComponentIdLookup[powerCoresShipComponent.Id] = shipComponentSet;
                }
                result.Remove("power_core");
            }
            
            // thrusters handled separately
            if (result.TryGetValue("thruster_components", out var thrusters)) {
                foreach (var computersShipComponent in thrusters.ShipComponents) {
                    int lastUnderscoreIndex = computersShipComponent.Id.LastIndexOf("_", StringComparison.Ordinal);
                    var computerLevel = computersShipComponent.Id.Substring(lastUnderscoreIndex);
                    string key = thrusters.Id + computerLevel;
                    ShipComponentSet shipComponentSet = result.ComputeIfAbsent(key, id => new ShipComponentSet(id, computersShipComponent));
                    shipComponentSet.ShipComponents.Add(computersShipComponent);
                    nodeIdToComponentIdLookup[computersShipComponent.Id] = shipComponentSet;
                }
                result.Remove("thruster_components");
            }

            //now need to remake links
            HashSet<Link> newLinks = links.Select(x => new Link() {From = x.From, To = nodeIdToComponentIdLookup[x.To.Id]}).ToHashSet(IEqualityComparerExtensions.Create<Link>(x => x.From.Id, x => x.To.Id));
            return new Tuple<IDictionary<string, ShipComponentSet>, ISet<Link>>(result, newLinks);
        }

        public ISet<string> ModGroups => modGroups;

        

        public ObjectsDependantOnTechs CopyOnlyCore() {
            return new ObjectsDependantOnTechs() {
                Buildings = this.Buildings?.FindCoreGameData() ?? new ModEntityData<Building>(),
                Decisions = this.Decisions?.FindCoreGameData() ?? new ModEntityData<Decision>(),
            }.SetShipComponents(this.ShipComponents?.FindCoreGameData(), this.shipComponentDescriptions?.FindCoreGameData());
            
        }
    }
}