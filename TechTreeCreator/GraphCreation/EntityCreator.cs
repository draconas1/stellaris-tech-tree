using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using FSharpx.Collections;
using Serilog;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation {
    public abstract class EntityCreator<T> where T : Entity {
        public ILocalisationApiHelper LocalisationApiHelper { get; }
        public ICWParserHelper CWParserHelper { get; }

        /// <summary>
        /// List of file names (exact) that will be skipped when parsing.  Defaults to  "00_tier.txt", "00_category.txt" 
        /// </summary>
        public List<string> IgnoreFiles { get; set; }
        /// <summary>
        /// File mask used for finding files.  defaults to "*.txt"
        /// </summary>
        public string ParseFileMask { get; set; }
        
        public bool AbortOnFailure { get; set; }
        
        protected EntityCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) {
            LocalisationApiHelper = localisationApiHelper;
            CWParserHelper = cwParserHelper;
            ParseFileMask = StellarisDirectoryHelper.TextMask;
            AbortOnFailure = false;
        }

        protected abstract T Construct(CWNode node);

        protected abstract void SetVariables(T entity, CWNode node);

        protected abstract string GetDirectory(StellarisDirectoryHelper directoryHelper);

        protected virtual bool ShouldInclude(T entity) {
            return true;
        }

        public ModEntityData<T> ProcessDirectoryHelper(ModEntityData<T> previous, StellarisDirectoryHelper directoryHelper, ModEntityData<Tech> techs) {
            var directoryPath = GetDirectory(directoryHelper);
            if (Directory.Exists(directoryPath)) {
                var result = new ModEntityData<T>(directoryHelper, previous);
                var techFiles = DirectoryWalker.FindFilesInDirectoryTree(directoryPath, ParseFileMask, IgnoreFiles);
                Log.Logger.Debug("Directory {directory} produced files {files}", directoryPath, techFiles);
                var parsedTechFiles = CWParserHelper.ParseParadoxFiles(techFiles.Select(x => x.FullName).ToList(), true);
                foreach (var (file, cwNode) in parsedTechFiles) {
                    Log.Logger.Debug("Processing file {file}", file);
                    // top level nodes are files, so we process the immediate children of each file, which is the individual techs.
                    foreach (var node in cwNode.Nodes) {
                        try {
                            var entity = Construct(node);
                            Initialise(entity, file, directoryHelper.ModName, directoryHelper.ModGroup, node);
                            SetVariables(entity, node);
                            if (ShouldInclude(entity)) {
                                result[entity.Id] = entity;
                            }
                            else {
                                Log.Logger.Debug("File {file} contained node {key} was processed, but failed the include filter so is discarded", file, entity.Id);
                            }
                        }
                        catch (Exception e) {
                            if (AbortOnFailure) {
                                throw new Exception($"Error Processing node {node.Key} in file: {file}", e);
                            }
                            Log.Logger.Error(e, "Error Processing node {node} in file: {file}", node.Key, file);
                        }
                    }
                }
                
                // special case for managing techs
                // techs are their own tech lookup.
                var latestTechData = result as ModEntityData<Tech>;
                if (latestTechData != null) {
                    AddLinks(result, latestTechData);
                }
                else {
                    AddLinks(result, techs);
                }

                return result;
            }
            Log.Logger.Debug("{mod} did not have {directory}", directoryHelper.ModName, directoryPath.Replace(directoryHelper.Root, ""));
            return previous;
         }


         private void AddLinks(ModEntityData<T> data, ModEntityData<Tech> techs) {
             foreach (var entity in data.Entities) {
                 // it is possible that the pre-reqs for a something do not exist
                 // in this we do not add them to the populated list, but leave them in the ids list
                 var prereqs = new List<Tech>();
                 foreach (var prerequisiteId in entity.PrerequisiteIds) {
                     if (techs.ContainsEntityInTree(prerequisiteId)) {
                         var prereq = techs[prerequisiteId];
                         prereqs.Add(prereq);
                         data.Links.Add(new Link() {From = prereq, To = entity});
                     }
                     else {
                         Log.Logger.Warning("Could not find prerequisite {prerequisite} for {entityType} {entityId}", prerequisiteId, entity.GetType().Name, entity.Id);
                     }
                 }

                 entity.Prerequisites = prereqs;
             }
         }
        
        private void Initialise(Entity entity, string filePath, string modName, string modGroup, CWNode node) {
            // not always using the node key, so use the id that was specified for the entity and trust that I got that right
            entity.Name = LocalisationApiHelper.GetName(entity.Id);
            entity.Description = LocalisationApiHelper.GetDescription(entity.Id);
            entity.FilePath = filePath;
            entity.Mod = modName;
            entity.ModGroup = modGroup;
            
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
    }
}