using TechTreeCreator.DTO;

namespace TechTreeCreator.Output.Vis {
    public static class VisHelpers {
        public static void SetBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 1;
        }
        
        public static  VisNode CreateNode(Entity entity, string imagesRelativePath, string nodeType) {
            var result = new VisNode {
                id = entity.Id,
                label = entity.Name,
                title = $"<b>{entity.Name}</b> ({entity.Id})",
                image = $"{imagesRelativePath}/{entity.Id}.png",
                hasImage = entity.IconFound,
                nodeType = nodeType
            };
            result.title = result.title + $"<br/><i>{entity.Description}</i>";

            if (entity.Mod != "Stellaris") {
                result.title = result.title + $"<br/><b>Mod: </b>{entity.Mod}";
            } 
            
            if (entity.DLC != null) {
                result.title = result.title + $"<br/><i>Requires the {entity.DLC} DLC</i>";
            }
            

            return result;
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
                @from = node.From.Id,
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