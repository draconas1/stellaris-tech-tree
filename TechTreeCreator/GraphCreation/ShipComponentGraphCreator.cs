using System;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public class ShipComponentGraphCreator : EntityCreator<ShipComponent> {
        public ShipComponentGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) : base(localisationApiHelper, cwParserHelper) {
        }

        protected override ShipComponent Construct(CWNode node) {
            // keys are always things like utility_component_template

            var key = node.GetKeyValue("key");

            if (key == null) {
                throw new Exception("Could not find Key keyvalueproperty for node " + node);
            }
            return new ShipComponent(key);
        }

        protected override void SetVariables(ShipComponent result, CWNode node) {
            result.Size = node.GetKeyValue("size");
            result.Power = node.GetKeyValueOrDefault("power", "0").ToInt();
            result.ComponentSet = node.GetKeyValue("component_set");
            
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = value.Value.ToDouble()));
                cwNode.ActOnNodes("upkeep", costNode => costNode.KeyValues.ForEach(value => result.Upkeep[value.Key] = value.Value.ToDouble()));
            });
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.ComponentTemplates;
        }

        protected override bool ShouldInclude(ShipComponent building) {
            return building.PrerequisiteIds.Any();
        }
    }
}