AtWarGraphDataDependants = {
  "ModGroup": "At War",
  "nodes": [
    {
      "id": "eac_pdf_base",
      "label": "Planetary Defense Force Base",
      "group": "Building",
      "title": "<b>Planetary Defense Force Base</b><br/><i>Home for the Planetary Defense Force. Use the planetary decision '§BPlanetary Defense Force§!' to interact with the base.</i><br/><b>Mod: </b>At War: Planetary Defense Force<br/><b>Build Time: </b>270<br/><b>Category: </b>Army<br/><b>Cost:</b> 400 Minerals<br/><b>Upkeep:</b> 6 Energy Credits, 2 Alloys",
      "level": 6,
      "image": "images/buildings/eac_pdf_base.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "eac_tech_planetary_defense_force"
      ]
    },
    {
      "id": "eac_building_planetary_rail_scattergun",
      "label": "Planetary Railgun Battery",
      "group": "Building",
      "title": "<b>Planetary Railgun Battery</b><br/><i>This emplacement fires an array of 20 projectiles using twin parallel electromagetic rails to rapidly accelerate them towards targets.\\n§HStats§!\\nAccuracy: §Y40%§!\\nHull Damage / projectile: §Y20 - 35§!\\nCooldown: §Y4§! £resource_time \\nAverage Damage / projectile: §Y2.75§! / £resource_time</i><br/><b>Mod: </b>At War: Planetary Cannons<br/><b>Build Time: </b>360<br/><b>Category: </b>Army<br/><b>Cost:</b> 600 Minerals, 100 Volatile Motes<br/><b>Upkeep:</b> 8 Energy Credits, 2 Volatile Motes",
      "level": 13,
      "image": "images/buildings/eac_building_planetary_rail_scattergun.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_mass_drivers_4"
      ]
    },
    {
      "id": "eac_building_planetary_coil_scattergun",
      "label": "Planetary Coilgun Battery",
      "group": "Building",
      "title": "<b>Planetary Coilgun Battery</b><br/><i>This emplacement fires an array of 20 projectiles using electromagnetic coils to rapidly accelerate them towards targets.\\n§HStats§!\\nAccuracy: §Y35%§!\\nHull Damage / projectile: §Y10 - 25§!\\nCooldown: §Y3§! £resource_time \\nAverage Damage / projectile: §Y2.00§! / £resource_time</i><br/><b>Mod: </b>At War: Planetary Cannons<br/><b>Build Time: </b>270<br/><b>Category: </b>Army<br/><b>Cost:</b> 400 Minerals, 50 Volatile Motes<br/><b>Upkeep:</b> 6 Energy Credits, 1 Volatile Motes",
      "level": 7,
      "image": "images/buildings/eac_building_planetary_coil_scattergun.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_mass_drivers_2"
      ]
    },
    {
      "id": "eac_building_planetary_tachyon_cannon",
      "label": "Planetary Tachyon Cannon",
      "group": "Building",
      "title": "<b>Planetary Tachyon Cannon</b><br/><i>This emplacement fires a tachyon beam of immense power.\\n§HStats§!\\n §W Accuracy: §! §Y65%§!\\nHull Damage: §Y1,000 - 5,000§!\\nCooldown: §Y10§! £resource_time \\nAverage Damage: §Y195.00§! / £resource_time</i><br/><b>Mod: </b>At War: Planetary Cannons<br/><b>Build Time: </b>450<br/><b>Category: </b>Army<br/><b>Cost:</b> 800 Minerals, 100 Rare Crystals<br/><b>Upkeep:</b> 10 Energy Credits, 3 Rare Crystals",
      "level": 18,
      "image": "images/buildings/eac_building_planetary_tachyon_cannon.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_energy_lance_2"
      ]
    },
    {
      "id": "eac_building_planetary_neutron_cannon",
      "label": "Planetary Neutron Cannon",
      "group": "Building",
      "title": "<b>Planetary Neutron Cannon</b><br/><i>This emplacement fires energy projectiles that consist of tightly concentrated neutrons capable of causing immense damage to the hull of enemy ships.\\n§HStats§!\\nAccuracy: §Y60%§!\\nHull Damage: §Y500 - 1,500§!\\nCooldown: §Y7§! £resource_time \\nAverage Damage: §Y85.71§! / £resource_time</i><br/><b>Mod: </b>At War: Planetary Cannons<br/><b>Build Time: </b>360<br/><b>Category: </b>Army<br/><b>Cost:</b> 600 Minerals, 75 Rare Crystals<br/><b>Upkeep:</b> 8 Energy Credits, 2 Rare Crystals",
      "level": 16,
      "image": "images/buildings/eac_building_planetary_neutron_cannon.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_energy_torpedoes_2"
      ]
    },
    {
      "id": "eac_building_planetary_proton_cannon",
      "label": "Planetary Proton Cannon",
      "group": "Building",
      "title": "<b>Planetary Proton Cannon</b><br/><i>This emplacement fires energy projectiles that consist of tightly concentrated protons capable of causing immense damage to the hull of enemy ships\\n§HStats§!\\nAccuracy: §Y50%§!\\nHull Damage: §Y100 - 500§!\\nCooldown: §Y5§! £resource_time \\nAverage Damage: §Y30.00§! / £resource_time</i><br/><b>Mod: </b>At War: Planetary Cannons<br/><b>Build Time: </b>270<br/><b>Category: </b>Army<br/><b>Cost:</b> 400 Minerals, 50 Rare Crystals<br/><b>Upkeep:</b> 6 Energy Credits, 1 Rare Crystals",
      "level": 14,
      "image": "images/buildings/eac_building_planetary_proton_cannon.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_energy_torpedoes_1"
      ]
    }
  ],
  "edges": [
    {
      "from": "eac_tech_planetary_defense_force",
      "to": "eac_pdf_base",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_mass_drivers_4",
      "to": "eac_building_planetary_rail_scattergun",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_mass_drivers_2",
      "to": "eac_building_planetary_coil_scattergun",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_energy_lance_2",
      "to": "eac_building_planetary_tachyon_cannon",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_energy_torpedoes_2",
      "to": "eac_building_planetary_neutron_cannon",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_energy_torpedoes_1",
      "to": "eac_building_planetary_proton_cannon",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    }
  ]
}
