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
        private readonly ModEntityData<Tech> techsAndDependencies;

        public DependantsGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper, 
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            ModEntityData<Tech> techsAndDependencies)
        {
            this.localisationApiHelper = localisationApiHelper;
            this.cwParserHelper = cwParserHelper;
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers;
            this.techsAndDependencies = techsAndDependencies;
        }

        public ObjectsDependantOnTechs CreateDependantGraph() {
            var result = new ObjectsDependantOnTechs();
            var buildingGraphCreator = new BuildingGraphCreator(localisationApiHelper, cwParserHelper);
            var buildings = ProcessDependant(buildingGraphCreator);
            result.Buildings = buildings;
            return result;
        }


        private ModEntityData<T> ProcessDependant<T>(EntityCreator<T> creator) where T : Entity {
            ModEntityData<T> entities = null;
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                entities = creator.ProcessDirectoryHelper(entities, modDirectoryHelper, techsAndDependencies);
            }
            
            entities.ApplyToChain((ents, links) => {
                var invalidEntities = ents.Where(x => !x.Value.Prerequisites.Any()).Select(x => x.Value).ToList();
                foreach (var invalidEntity in invalidEntities) {
                    Log.Logger.Warning("Removing {entityId} from {file} dependant entities as we were unable to locate its specified pre-requisite techs", invalidEntity.Id, invalidEntity.FilePath);
                    ents.Remove(invalidEntity.Id);
                    var invalidLinks = links.Where(x => x.To.Id == invalidEntity.Id).ToList();
                    links.RemoveAll(invalidLinks);
                }
            });

            return entities;
        }

    }
}
