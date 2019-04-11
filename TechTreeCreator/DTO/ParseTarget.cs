using System.Collections.Generic;
using System.Linq;

namespace TechTreeCreator {
    public enum ParseTarget {
        Technologies,
        Buildings,
        ShipComponents
    }


    public static class ParseTargetExtensions {

        public static IEnumerable<ParseTarget> WithoutTechs(this IEnumerable<ParseTarget> targets) {
            return targets.Where(x => x != ParseTarget.Technologies);
        }
        
        public static string ImagesDirectory(this ParseTarget target) {
            switch (target) {
                case ParseTarget.Technologies: return "technologies";
                case ParseTarget.Buildings: return "buildings";
                case ParseTarget.ShipComponents: return "shipComponents";
            }

            return null;
        }
    }
}