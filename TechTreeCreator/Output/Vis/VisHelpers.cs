using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CWToolsHelpers.Localisation;
using NetExtensions.Object;
using Serilog;
using TechTreeCreator.DTO;

namespace TechTreeCreator.Output.Vis {
    public static class VisHelpers {
        
        private static readonly Regex IconStringRx = new Regex(@"£.+£", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ColourCodeRx = new Regex(@"§.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static void SetBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 1;
        }
        
        public static  VisNode CreateNode(Entity entity, string imagesRelativePath, string nodeType)
        {
            var sanitisedName = ColourCodeRx.Replace(IconStringRx.Replace(entity.Name, ""), "");
            var sanitisedDescription =  ColourCodeRx.Replace(IconStringRx.Replace(entity.Description, ""), "");
            var result = new VisNode {
                id = entity.Id,
                label = sanitisedName,
                title = $"<b>{sanitisedName}</b> ({entity.Id})",
                image = $"{imagesRelativePath}/{entity.Id}.png",
                hasImage = entity.IconFound,
                nodeType = nodeType
            };
            result.title = result.title + $"<br/><i>{sanitisedDescription}</i>";

            if (entity.Mod != "Stellaris") {
                result.title = result.title + $"<br/><b>Mod: </b>{entity.Mod}";
            } 
            
            if (entity.DLC != null) {
                result.title = result.title + $"<br/><i>Requires the {entity.DLC} DLC</i>";
            }
            

            return result;
        }

        // fucking capitalisation
        private static string GetPotentialLocalisationKeys(ILocalisationApiHelper localisation, string rootKey) {
            var result = new List<string>();
            result.Add(rootKey);
            result.Add(rootKey.ToUpperInvariant());
            result.Add("mod_" + rootKey);
            result.Add("MOD_" + rootKey.ToUpperInvariant());

            foreach (var key in result) {
                if (localisation.HasValueForKey(key)) {
                    return localisation.GetName(key);
                }
            }

            return rootKey;
        }
        
        public static void AddModifiersToNode(ILocalisationApiHelper localisation, VisNode node, 
            IDictionary<string, string> modifiers, 
            Entity source,
            bool localiseKeys = true) {
            foreach (var modifierNodeKeyValue in modifiers) {
                string key = localiseKeys ? GetPotentialLocalisationKeys(localisation, modifierNodeKeyValue.Key) : modifierNodeKeyValue.Key;
                string prefix = "";
                string suffix = "";
                string value = modifierNodeKeyValue.Value;
                try {
                    if (modifierNodeKeyValue.Key.ToUpperInvariant().EndsWith("ADD")) {
                        double intValue = value.ToDouble();
                        prefix = intValue >= 0 ? "+" : "";
                    }

                    if (modifierNodeKeyValue.Key.ToUpperInvariant().EndsWith("MULT")) {
                        int percentageValue = (int) (value.ToDouble() * 100);
                        prefix = percentageValue >= 0 ? "+" : "";
                        suffix = "%";
                        value = percentageValue.ToString(CultureInfo.InvariantCulture);
                    }
                }
                catch (FormatException e) {
                    Log.Logger.Error(e, "Error parsing {nodeId} from {filePath}", node.id, source.FilePath);
                }

                node.title = $"{node.title}<br/><b>{key}:</b> {prefix}{value}{suffix}";
            }
        }
        
        public static string CreateCostString<T>(ILocalisationApiHelper localisation, string resourceType, IDictionary<string, T> costs) {
            if (costs.Any()) {
                string costString = $"<br/><b>{resourceType}:</b>";
                foreach (var (key, value) in costs) {
                    costString = $"{costString} {value} {localisation.GetName(key)},";
                }

                return costString.Remove(costString.Length - 1);
                ;
            }
            else {
                return "";
            }
        }

        public static void SetLevel(VisNode node, Entity entity, IDictionary<string, VisNode> prereqTechNodeLookup) {
            // find the highest prerequisite tech level and then add 1 to it to ensure it is rendered in a sensible place.
            var highestLevelOfPrerequisiteTechs = entity.Prerequisites.Select(x => prereqTechNodeLookup[x.Id].level).Max();
            if (!highestLevelOfPrerequisiteTechs.HasValue) {
                throw new Exception(entity.Name + " Had no prerequiste levels: " + entity.FilePath);
            }

            node.level = highestLevelOfPrerequisiteTechs + 1;
        }

        public static void SetGestaltAvailability(VisNode node, Entity entity) {
            if (entity is IGestaltAvailability iga) {
                if (iga.Machines.HasValue) {
                    node.title = node.title + "<br/>" + (!iga.Machines.Value ? "Not for machine intelligence" : "Machine intelligence");
                }
            
                if (iga.Gestalt.HasValue) {
                    node.title = node.title + "<br/>" + (!iga.Gestalt.Value ? "Not for gestalt consciousness" : "Gestalt consciousness");
                }
            }
        }

        public static string CreateRelativePath(string fullPath, string relativeTo) {
            var relativePath = fullPath.Replace(relativeTo, "");
            relativePath = relativePath.Replace(@"\", "/");
            if (relativePath.StartsWith("/")) {
                relativePath = relativePath.Remove(0, 1);
            }

            return relativePath;
        }

        public static VisEdge MarshalLink(Link node)
        {
            return new VisEdge
            {
                from = node.From.Id,
                to = node.To.Id,
                arrows = "to",
                dashes = true,
                color = new VisColor
                {
                    color = "grey"
                }
            };
        }
    }
}