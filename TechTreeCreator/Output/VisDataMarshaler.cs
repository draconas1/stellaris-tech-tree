using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CWToolsHelpers.Directories;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using Serilog;
using TechTreeCreator.DTO;
using TechTreeCreator.Output.Vis;

namespace TechTreeCreator.Output {
    public class VisDataMarshaler {
        private readonly ILocalisationApiHelper localisationApi;
        private readonly OutputDirectoryHelper outputDirectoryHelper;
        private readonly TechsVisMarshaler techsVisMarshaler;

        public VisDataMarshaler(ILocalisationApiHelper localisationApi, OutputDirectoryHelper outputDirectoryHelper) {
            this.localisationApi = localisationApi;
            this.outputDirectoryHelper = outputDirectoryHelper;
            techsVisMarshaler = new TechsVisMarshaler(localisationApi, outputDirectoryHelper);
        }

        public IDictionary<string, VisData> CreateGroupedVisDependantData(IDictionary<string, VisData> techData, ObjectsDependantOnTechs objectsDependantOnTechs,
            IEnumerable<ParseTarget> parseTargets) {
            Dictionary<string, VisNode> prereqTechNodeLookup =
                techData.Values.Select(x => x.nodes).SelectMany(x => x).Distinct(IEqualityComparerExtensions.Create<VisNode>(x => x.id)).ToDictionary(x => x.id);
            var result = new Dictionary<string, VisData>();
            objectsDependantOnTechs.ModGroups.ForEach(x => result[x] = CreateDependantDataForModGroup(x, prereqTechNodeLookup, objectsDependantOnTechs, parseTargets));
            return result;
        }

        public IDictionary<string, VisData> CreateTechVisData(ModEntityData<Tech> techsAndDependencies, string imagesPath) {
            return techsVisMarshaler.CreateTechVisData(techsAndDependencies, imagesPath);
        }

        public VisData CreateRootNotes(ModEntityData<Tech> techsAndDependencies, string imagesPath) {
            return techsVisMarshaler.CreateRootNotes(techsAndDependencies, imagesPath);
        }

        private VisData CreateDependantDataForModGroup(string modGroup, IDictionary<string, VisNode> prereqTechNodeLookup, ObjectsDependantOnTechs objectsDependantOnTechs,
            IEnumerable<ParseTarget> parseTargets) {
            var result = new VisData() {
                ModGroup = modGroup,
            };

            foreach (var parseTarget in parseTargets.WithoutTechs()) {

                switch (parseTarget) {
                    case ParseTarget.Buildings: {
                        result.nodes.AddRange(objectsDependantOnTechs.Buildings.AllEntitiesForModGroup(modGroup)
                            .Select(x => MarshalBuilding(x, prereqTechNodeLookup, outputDirectoryHelper.GetImagesPath(parseTarget.ImagesDirectory()))));
                        result.edges.AddRange(objectsDependantOnTechs.Buildings.AllLinksForModGroup(modGroup).Select(VisHelpers.MarshalLink));
                        break;
                    }

                    case ParseTarget.ShipComponents: {

                        result.nodes.AddRange(objectsDependantOnTechs.ShipComponentsSets.AllEntitiesForModGroup(modGroup)
                            .Select(x => MarshallShipComponentSet(x, prereqTechNodeLookup, outputDirectoryHelper.GetImagesPath(parseTarget.ImagesDirectory()))));
                        ISet<Link> allLinksForModGroup = objectsDependantOnTechs.ShipComponentsSets.AllLinksForModGroup(modGroup);
                        result.edges.AddRange(allLinksForModGroup.Select(VisHelpers.MarshalLink));
                        break;
                    }
                    
                    case ParseTarget.Decisions: {
                        result.nodes.AddRange(objectsDependantOnTechs.Decisions.AllEntitiesForModGroup(modGroup)
                            .Select(x => MarshalDecision(x, prereqTechNodeLookup, outputDirectoryHelper.GetImagesPath(parseTarget.ImagesDirectory()))));
                        ISet<Link> allLinksForModGroup = objectsDependantOnTechs.Decisions.AllLinksForModGroup(modGroup);
                        result.edges.AddRange(allLinksForModGroup.Select(VisHelpers.MarshalLink));
                        break;
                    }
                    default:
                        throw new Exception(parseTarget.ToString());
                }
            }

            return result;
        }


        private VisNode MarshalBuilding(Building building, IDictionary<string, VisNode> prereqTechNodeLookup, string imagesPath) {
            var result = VisHelpers.CreateNode(building, CreateRelativePath(imagesPath), "building");
            result.prerequisites = building.PrerequisiteIds != null
                ? building.PrerequisiteIds.ToArray()
                : new string[] { };

            result.title = result.title + "<br/><b>Build Time: </b>" + building.BaseBuildTime;
            result.group = "Building";

            if (building.Category != null) {
                result.title = result.title + "<br/><b>Category: </b>" + localisationApi.GetName(building.Category);
            }

            result.title = result.title + AddBuildingResources("Cost", building.Cost);
            result.title = result.title + AddBuildingResources("Upkeep", building.Upkeep);
            result.title = result.title + AddBuildingResources("Produces", building.Produces);
            
            VisHelpers.AddModifiersToNode(localisationApi, result, building.Modifiers, building);
            
            VisHelpers.SetGestaltAvailability(result, building);

            // find the highest prerequisite tech level and then add 1 to it to ensure it is rendered in a sensible place.
            var highestLevelOfPrerequisiteTechs = building.Prerequisites.Select(x => prereqTechNodeLookup[x.Id].level).Max();
            if (!highestLevelOfPrerequisiteTechs.HasValue) {
                throw new Exception(building.Name + " Had no prerequiste levels: " + building.FilePath);
            }

            result.level = highestLevelOfPrerequisiteTechs + 1;


            return result;
        }

        private VisNode MarshalDecision(Decision decision, IDictionary<string, VisNode> prereqTechNodeLookup, string imagesPath) {
            var result = VisHelpers.CreateNode(decision, CreateRelativePath(imagesPath), "decision");
            result.prerequisites = decision.PrerequisiteIds != null
                ? decision.PrerequisiteIds.ToArray()
                : new string[] { };

            result.group = "Dependant";

            if (decision.CustomTooltip != null) {
                result.title = result.title + "<br/>" + decision.CustomTooltip;
            }

            result.title = result.title + AddBuildingResources("Cost", decision.Cost);
            VisHelpers.AddModifiersToNode(localisationApi, result, decision.Modifiers, decision);

            VisHelpers.SetLevel(result, decision, prereqTechNodeLookup);

            VisHelpers.SetGestaltAvailability(result, decision);
            return result;
        }
        
        private VisNode MarshallShipComponentSet(ShipComponentSet shipComponentSet, IDictionary<string, VisNode> prereqTechNodeLookup, string imagesPath) {
            var result = VisHelpers.CreateNode(shipComponentSet, CreateRelativePath(imagesPath), "shipComponent");
            result.prerequisites = shipComponentSet.PrerequisiteIds != null
                ? shipComponentSet.PrerequisiteIds.ToArray()
                : new string[] { };

            
            // so for utility slot components, the component description is the generic description of the slot
            // the description of the tech level component item is held by the tech in the unlock_desc field.
            // the description also begins with squggle formatted name which we try to format out
            foreach (var prerequisite in shipComponentSet.Prerequisites) {
                // Titan cannon unlocks with titan so gets titan tech description
                if (shipComponentSet.Id == "PERDITION_BEAM") {
                    continue;
                }
                
                if (prerequisite.ExtraDesc != null) {
                    var parts = prerequisite.ExtraDesc.Split(@"\n");
                    if (parts.Length == 1) {
                        result.title = result.title + $"<br/><i>{prerequisite.ExtraDesc}</i>";
                    }
                    else {
                        result.title = result.title + $"<br/><i>{parts[1]}</i>";
                        // removes most of the squiggle formatting
                        Regex rgx = new Regex("[^a-zA-Z0-9 ]");
                        result.label = rgx.Replace(parts[0], "");
                        // sqiggleH is a common format, so remove a starting H
                        if (result.label.StartsWith("H")) {
                            result.label = result.label.Substring(1);
                        }
                    }
                }
            }

            foreach (var shipComponent in shipComponentSet.ShipComponents) {
                result.title = result.title + $"<br/><br/><b>{shipComponent.Name}</b>";
                result.title = result.title + AddBuildingResources("Cost", shipComponent.Cost);
                result.title = result.title + AddBuildingResources("Upkeep", shipComponent.Upkeep);
                VisHelpers.AddModifiersToNode(localisationApi, result, shipComponent.Properties, shipComponent, false);
                VisHelpers.AddModifiersToNode(localisationApi, result, shipComponent.Modifiers, shipComponent);
            }

            result.group = "Dependant";

            // find the highest prerequisite tech level and then add 1 to it to ensure it is rendered in a sensible place.
            var highestLevelOfPrerequisiteTechs = shipComponentSet.Prerequisites.Select(x => prereqTechNodeLookup[x.Id].level).Max();
            if (!highestLevelOfPrerequisiteTechs.HasValue) {
                throw new Exception(shipComponentSet.Name + " Had no prerequiste levels: " + shipComponentSet.FilePath);
            }

            result.level = highestLevelOfPrerequisiteTechs + 1;

            return result;
        }
   
        private string CreateRelativePath(string imagesPath) {
            return VisHelpers.CreateRelativePath(imagesPath, outputDirectoryHelper.Root);
        }

        private string AddBuildingResources<T>(string resourceType, IDictionary<string, T> costs) {
            return VisHelpers.CreateCostString(localisationApi, resourceType, costs);
        }
    }
}