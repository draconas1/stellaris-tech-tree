using System;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public class DecisionGraphCreator : EntityCreator<Decision> {
        public DecisionGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) : base(localisationApiHelper, cwParserHelper) {
        }

        protected override Decision Construct(CWNode node) {
            return new Decision(node.Key);
        }

        protected override void SetVariables(Decision result, CWNode node) {
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = value.Value.ToDouble()));
            });
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.Decisions;
        }

        protected override bool ShouldInclude(Decision building) {
            return building.PrerequisiteIds.Any();
        }
    }
}