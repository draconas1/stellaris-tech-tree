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

    public class Tech : Entity {
        public TechArea Area { get; set; }

        public int? Tier { get; set; }

        // ReSharper disable once PossibleInvalidOperationException
        public int TierValue => Tier.Value;

        public int? BaseCost { get; set; }

        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<TechFlag> Flags { get; set; }

        public Tech(string id) : base(id) {
        }
    }

    public class Building : Entity {
        public int? BaseBuildTime { get; set; }
        public string Category { get; set; }

        public IDictionary<string, double> Cost { get; }
        public IDictionary<string, double> Upkeep { get; }
        public IDictionary<string, double> Produces { get; }

        public Building(string id) : base(id) {
            Cost = new Dictionary<string, double>();
            Upkeep = new Dictionary<string, double>();
            Produces = new Dictionary<string, double>();
        }
    }

    public class ShipComponent : Entity {
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
        private ModEntityData<Decision> decisions;
        private ISet<string> modGroups = new HashSet<string>();

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
        
        public ModEntityData<ShipComponent> ShipComponents {
            get => shipComponents;
            set {
                shipComponents = value; 
                modGroups.AddRange(value.AllEntities.Select(x => x.ModGroup));
                ShipComponentsSets = shipComponents.Transform<ShipComponentSet>(MergeComponents);
            }
        }
        
        public ModEntityData<ShipComponentSet> ShipComponentsSets {
            get;
            private set;
        }
        
        private Tuple<IDictionary<string, ShipComponentSet>, ISet<Link>> MergeComponents(IDictionary<string, ShipComponent> shipComponents, ISet<Link> links) {
            Dictionary<string, ShipComponentSet> result = new Dictionary<string, ShipComponentSet>();
            Dictionary<string, ShipComponentSet> nodeIdToComponentIdLookup = new Dictionary<string, ShipComponentSet>();
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
                    int firstUnderScoreIndex = powerCoresShipComponent.Id.IndexOf("_", StringComparison.Ordinal);
                    var computerLevel = powerCoresShipComponent.Id.Substring(firstUnderScoreIndex);
                    if (powerCoresShipComponent.Id.StartsWith("ION_CANNON")) {
                        int firstUnderScoreIndex2 = computerLevel.IndexOf("_", 1, StringComparison.Ordinal);
                        computerLevel = computerLevel.Substring(firstUnderScoreIndex2);
                    }
                    string key = powerCores.Id + computerLevel;
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
                ShipComponents = this.ShipComponents?.FindCoreGameData() ?? new ModEntityData<ShipComponent>(),
                Decisions = this.Decisions?.FindCoreGameData() ?? new ModEntityData<Decision>(),
                                 
            };
            
        }
    }
}