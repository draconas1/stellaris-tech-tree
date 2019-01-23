using System;
using System.Collections.Generic;
using CWTools.Parser;

namespace TechTree.DTO {
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
        RequiresAcquisition
    }
    
    public class Tech : Entity {
        
        public TechArea Area { get; set; }
        public string Category { get; set; }
        public int? Tier { get; set; }
        public int? BaseCost { get; set; }
        
        public IEnumerable<TechFlag> Flags { get; set; }
        public IEnumerable<Tech> Prerequisites { get; set; }
        public IEnumerable<string> PrerequisiteIds { get; set; }

        public Tech(string id) : base(id) {
        }
    }

    public abstract class Entity {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Entity(string id) {
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
    
}