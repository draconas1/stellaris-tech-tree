using System;
using System.Collections.Generic;
using System.Linq;
using TechTreeCreator.DTO;

namespace TechTreeCreator {
    public enum ParseTarget {
        Technologies,
        Buildings,
        ShipComponents,
        Decisions
    }


    public static class ParseTargetExtensions {

        public static IEnumerable<ParseTarget> WithoutTechs(this IEnumerable<ParseTarget> targets) {
            return targets.Where(x => x != ParseTarget.Technologies);
        }
        
        public static string ImagesDirectory(this ParseTarget target) {
            switch (target) {
                case ParseTarget.Technologies: return "technologies";
                case ParseTarget.Buildings: return "buildings";
                case ParseTarget.ShipComponents: return "ship_parts";
                case ParseTarget.Decisions: return "decisions";
            }

            return null;
        }
        
        public static IEnumerable<Entity> Get(this ObjectsDependantOnTechs deps, ParseTarget parseTarget){
            switch (parseTarget) {
                case ParseTarget.Technologies: throw new InvalidOperationException("No techs in dependants");
                case ParseTarget.Buildings: return deps.Buildings.AllEntities;
                case ParseTarget.ShipComponents: return deps.ShipComponentsSets.AllEntities;
                case ParseTarget.Decisions: return deps.Decisions.AllEntities;
                default: throw new InvalidOperationException("Unknown type: " + parseTarget);
            }
        }
    }
}