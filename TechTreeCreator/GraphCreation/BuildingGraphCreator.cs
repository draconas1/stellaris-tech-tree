using System;
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
            result.BaseBuildTime = (node.GetKeyValue("base_buildtime") ?? "0").ToInt();
            result.Category = node.GetKeyValue("category");
            
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = value.Value.ToInt()));
                cwNode.ActOnNodes("upkeep", costNode => costNode.KeyValues.ForEach(value => result.Upkeep[value.Key] = value.Value.ToInt()));
                cwNode.ActOnNodes("produces", costNode => costNode.KeyValues.ForEach(value => result.Produces[value.Key] = value.Value.ToInt()));
            });
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.Buildings;
        }
    }
}