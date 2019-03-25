using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CWTools.Process;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using Serilog;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation
{
    public class DependantsGraphCreator
    {
        private readonly ILocalisationApiHelper localisationApiHelper;
        private readonly ICWParserHelper cwParserHelper;
        private readonly StellarisDirectoryHelper stellarisDirectoryHelper;
        private readonly IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers;
        private readonly TechsAndDependencies techsAndDependencies;

        public DependantsGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper, 
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            TechsAndDependencies techsAndDependencies)
        {
            this.localisationApiHelper = localisationApiHelper;
            this.cwParserHelper = cwParserHelper;
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers;
            this.techsAndDependencies = techsAndDependencies;
        }

        public ObjectsDependantOnTechs CreateDependantGraph() {
            var result = new ObjectsDependantOnTechs() { Prerequisites = new HashSet<Link>()};
            var buildingGraphCreator = new BuildingGraphCreator(localisationApiHelper, cwParserHelper);
            var (buildings, buildingLinks) = ProcessDependant(buildingGraphCreator);
            result.Buildings = buildings;
            result.Prerequisites.AddRange(buildingLinks);
            return result;
        }


        private Tuple<IDictionary<string, T>, ISet<Link>> ProcessDependant<T>(EntityCreator<T> creator) where T : Entity {
            var entities = new Dictionary<string, T>();
           
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                creator.ProcessDirectoryHelper(entities, modDirectoryHelper);
            }

            var links = creator.PopulateTechDependenciesAndReturnLinks(entities.Values, techsAndDependencies.Techs);

            var invalidEntities = entities.Where(x => !x.Value.Prerequisites.Any()).Select(x => x.Key).ToList();
            foreach (var invalidEntityKey in invalidEntities) {
                Log.Logger.Warning("Removing {0} from dependant entities as we were unable to locate its specified pre-requisite techs", invalidEntityKey);
                entities.Remove(invalidEntityKey);
                var invalidLinks = links.Where(x => x.To.Id == invalidEntityKey).ToList();
                links.RemoveAll(invalidLinks);
            }

            return new Tuple<IDictionary<string, T>, ISet<Link>>(entities, links);
        }



    }
}
