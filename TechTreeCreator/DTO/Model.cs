using System.Collections.Generic;

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
        public Building(string id) : base(id) {
        }
    } 

    public abstract class Entity {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private string icon;
        public string Icon { get => icon ?? Id;
            set => icon = value;
        }
        
        public IEnumerable<Tech> Prerequisites { get; set; }
        public IEnumerable<string> PrerequisiteIds { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string DLC { get; set; }

        public bool IconFound { get; set; }

        protected Entity(string id) {
            Id = id;
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
        public Dictionary<string, Tech> Techs { get; set; }
        public ISet<Link> Prerequisites { get; set; }
    }

    public class ObjectsDependantOnTechs {
        public Dictionary<string, Building> Buildings { get; set; }
        public ISet<Link> Prerequisites { get; set; }
    }
    
}