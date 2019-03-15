using System.Collections.Generic;
using System.Diagnostics;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using Microsoft.FSharp.Core;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public abstract class EntityCreator {
        private readonly ILocalisationApiHelper localisationApiHelper;

        protected  ILocalisationApiHelper LocalisationApiHelper => localisationApiHelper;

        protected EntityCreator(ILocalisationApiHelper localisationApiHelper) {
            this.localisationApiHelper = localisationApiHelper;
        }

        protected void Initialise(Entity entity, string filePath, CWNode node) {
            entity.Name = localisationApiHelper.GetName(node.Key);
            entity.Description = localisationApiHelper.GetDescription(node.Key);
            entity.FilePath = filePath;
            
            // if icon has been defined
            if (node.GetKeyValue("icon") != null)
            {
                entity.Icon = node.GetKeyValue("icon");
            }
            
            // if its a DLC item
            node.ActOnNodes("potential", potentialNode => {
                entity.DLC = potentialNode.GetKeyValue("host_has_dlc");
            });
            
            node.ActOnNodes("prerequisites", cwNode => entity.PrerequisiteIds = cwNode.Values, () => entity.PrerequisiteIds = new string[]{});
        }

        protected ISet<Link> PopulateTechDependenciesAndReturnLinks(IEnumerable<Entity> entities, IDictionary<string, Tech> techs) {
            var links = new HashSet<Link>();
            // populate prerequisites
            foreach (var entity in entities) {
                // it is possible that the pre-reqs for a something do not exist
                // in this we do not add them to the populated list, but leave them in the ids list
                var prereqs = new List<Tech>();
                foreach (var prerequisiteId in entity.PrerequisiteIds) {
                    if (techs.TryGetValue(prerequisiteId, out var prereq)) {
                        prereqs.Add(prereq);
                        links.Add(new Link() {From = prereq, To = entity});
                    }
                    else {
                        Debug.WriteLine("Could not find prerequisite {0} for {1} {2}", prerequisiteId, entity.GetType().Name, entity.Id);
                    }
                }

                entity.Prerequisites = prereqs;
            }

            return links;
        }
    }
}