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
    public class ShipComponentGraphCreator : EntityCreator<ShipComponent> {
        
        private HashSet<string> excludes = new HashSet<string>();
        public ShipComponentGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper) : base(localisationApiHelper, cwParserHelper) {
            excludes.Add("STARBASE_COMBAT_COMPUTER_1");
            excludes.Add("STARBASE_COMBAT_COMPUTER_2");
            excludes.Add("STARBASE_COMBAT_COMPUTER_3");
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
            node.ActOnKeyValues("power", power => result.Properties["Power"] = power);
            node.ActOnKeyValues("sensor_range", power => result.Properties["Sensor Range"] = power);
            node.ActOnKeyValues("hyperlane_range", power => result.Properties["Hyperlane Detection Range"] = power);
            node.ActOnNodes("ship_modifier", modifierNode =>  AddModifiers(result, modifierNode));
            
            result.ComponentSet = node.GetKeyValue("component_set");

            if (result.ComponentSet != null) {
                result.ComponentSetName = LocalisationApiHelper.GetName(result.ComponentSet);
                result.ComponentSetDescription = LocalisationApiHelper.GetDescription(result.ComponentSet);
            }

            if (result.Icon.StartsWith("GFX_")) {
                result.Icon = result.Icon.Replace("GFX_", "");
            }

            // because afterburners decide to pluralise, for shits and giggles.
            if (result.Icon.Contains("ship_part_afterburner")) {
                result.Icon = result.Icon.Replace("ship_part_afterburner", "ship_part_afterburners");
            }

            // because computer icons have "_role_" for shits and giggles
            if (result.Icon.Contains("ship_part_computer_")) {
                result.Icon = "computers/" + (!result.Icon.Contains("ship_part_computer_default") ? result.Icon.Replace("ship_part_computer_", "ship_part_computer_role_") : result.Icon);
            }

            CWNode classRestriction = node.GetNode("class_restriction");
            // most power cores restricted by ship class
            if (classRestriction != null) {
                var classesThatUsePowerCore = classRestriction.Values.Select(x => LocalisationApiHelper.GetName(x)).StringJoin(" & ");
                result.Name = classesThatUsePowerCore + " " + result.Name;
            }
            else {
                // ion cannon by size
                node.ActOnNodes("size_restriction", sizeNode => {
                    var sizesThatUsePowerCore = sizeNode.Values.Select(x => LocalisationApiHelper.GetName(x)).StringJoin(" & ");
                    result.Name = sizesThatUsePowerCore + " " + result.Name;
                });
            }
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.ComponentTemplates;
        }

        protected override bool ShouldInclude(ShipComponent component) {
            return component.PrerequisiteIds.Any() && !excludes.Contains(component.Id ) ;
        }
    }
}