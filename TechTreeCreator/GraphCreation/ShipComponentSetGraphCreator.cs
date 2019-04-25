using System;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public class ShipComponentSetGraphCreator : EntityCreator<ShipComponentSetDescription> {
        
        private HashSet<string> excludes = new HashSet<string>();
        public ShipComponentSetGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) : base(localisationApiHelper, cwParserHelper) {
        }

        protected override ShipComponentSetDescription Construct(CWNode node) {
            // keys are always things like utility_component_template

            var key = node.GetKeyValue("key");

            if (key == null) {
                throw new Exception("Could not find Key keyvalueproperty for node " + node);
            }
            return new ShipComponentSetDescription(key);
        }

        protected override void SetVariables(ShipComponentSetDescription result, CWNode node) {
            //do nothing
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.ComponentSets;
        }

        protected override bool ShouldInclude(ShipComponentSetDescription component) {
            return true;
        }
    }
}