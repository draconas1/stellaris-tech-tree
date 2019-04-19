using System;
using System.Collections.Generic;
using System.Linq;
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

        public ObjectsDependantOnTechs CreateDependantGraph(IList<ParseTarget> parseTargetsWithoutTechs) {
            var result = new ObjectsDependantOnTechs();

            foreach (var target in parseTargetsWithoutTechs) {
                switch (target) {
                    case ParseTarget.Buildings: {
                        var creator = new BuildingGraphCreator(localisationApiHelper, cwParserHelper);
                        result.Buildings = ProcessDependant(creator, target);
                        break;
                    }
                    
                    case ParseTarget.ShipComponents: {
                        var creator = new ShipComponentGraphCreator(localisationApiHelper, cwParserHelper);
                        result.ShipComponents = ProcessDependant(creator, target);
                        
                        
                        
                        break;
                    }
                    
                    default: throw new Exception("unknown target: " + target);
                }
            }

            return result;
        }


        private ModEntityData<T> ProcessDependant<T>(EntityCreator<T> creator, ParseTarget parseTarget) where T : Entity {
            ModEntityData<T> entities = null;
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                entities = creator.ProcessDirectoryHelper(entities, modDirectoryHelper, techsAndDependencies);
            }

            entities?.ApplyToChain((ents, links) => {
                var invalidEntities = ents.Where(x => !x.Value.Prerequisites.Any()).Select(x => x.Value).ToList();
                foreach (var invalidEntity in invalidEntities) {
                    Log.Logger.Warning("Removing {entityId} from {file} dependant entities as we were unable to locate its specified pre-requisite techs", invalidEntity.Id,
                        invalidEntity.FilePath);
                    ents.Remove(invalidEntity.Id);
                    var invalidLinks = links.Where(x => x.To.Id == invalidEntity.Id).ToList();
                    links.RemoveAll(invalidLinks);
                }
            });
            
            if (entities != null) {
                Log.Logger.Debug("Processed {entityCount} {parseTarget} with {linkCount} links", entities.EntityCount, parseTarget, entities.LinkCount);                            
            }
            else {
                Log.Logger.Warning("{parseTarget} had no items in any of the sources");     
            }

            return entities;
        }

    }
}
