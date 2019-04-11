using System;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public class BuildingGraphCreator : EntityCreator<Building> {
        public BuildingGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) : base(localisationApiHelper, cwParserHelper) {
        }

        protected override Building Construct(CWNode node) {
            return new Building(node.Key);
        }

        protected override void SetVariables(Building result, CWNode node) {
            if (result.Icon.StartsWith("GFX_")) {
                result.Icon = result.Icon.Replace("GFX_", "");
            }
            
            result.BaseBuildTime = node.GetKeyValueOrDefault("base_buildtime", "0").ToInt();
            result.Category = node.GetKeyValue("category");
            
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = value.Value.ToDouble()));
                cwNode.ActOnNodes("upkeep", costNode => costNode.KeyValues.ForEach(value => result.Upkeep[value.Key] = value.Value.ToDouble()));
                cwNode.ActOnNodes("produces", costNode => costNode.KeyValues.ForEach(value => result.Produces[value.Key] = value.Value.ToDouble()));
            });
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.Buildings;
        }

        protected override bool ShouldInclude(Building building) {
            return building.PrerequisiteIds.Any();
        }
    }
}