GigastructuresGraphDataDependants = {
  "ModGroup": "Gigastructures",
  "nodes": [
    {
      "id": "building_shroud_capacitor",
      "label": "Shroud Capacitor",
      "group": "Building",
      "title": "<b>Shroud Capacitor</b><br/><i>A massive psionic facility forcefully extracting Shroud Energy and redistributing it on the planet's surface, greatly boosting telepathic abilities and industries.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>1000<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 1000 Minerals, 50 Alloys<br/><b>Upkeep:</b> 10 Unity, 10 Energy Credits<br/><b>Produces:</b> 5 Physics Research",
      "level": 19,
      "image": "images/buildings/building_shroud_capacitor.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_shroud_container"
      ]
    },
    {
      "id": "building_iodizium_research",
      "label": "Iodizium Research Facility",
      "group": "Building",
      "title": "<b>Iodizium Research Facility</b><br/><i>This building conducts various experiments around Iodizium Crystals in order to study them and understand their incredibly odd nature.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>350<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 1000 Minerals<br/><b>Upkeep:</b> 5 Minerals, 1 Iodizium Crystals<br/><b>Produces:</b> 10 Physics Research, 10 Engineering Research, 10 Society Research",
      "level": 21,
      "image": "images/buildings/building_iodizium_research.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_fusion_disruption"
      ]
    },
    {
      "id": "building_iodizium_plant",
      "label": "Iodizium Power Plant",
      "group": "Building",
      "title": "<b>Iodizium Power Plant</b><br/><i>A power plant powered by the exotic, physics-defying Iodizium Crystals. The crystals' strange nature makes them incredibly potent energy generators.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>350<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 1000 Minerals<br/><b>Upkeep:</b> 5 Minerals, 1 Iodizium Crystals<br/><b>Produces:</b> 20 Energy Credits",
      "level": 21,
      "image": "images/buildings/building_iodizium_plant.png",
      "hasImage": true,
      "nodeType": "building",
      "prerequisites": [
        "tech_fusion_disruption"
      ]
    },
    {
      "id": "building_space_dust_sifter",
      "label": "Space Dust Processing Facility",
      "group": "Building",
      "title": "<b>Space Dust Processing Facility</b><br/><i>Space dust frequently passes through the Orbital Elysium. This sprawling facility can gather this dust and refine it into small amounts of something usable.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>350<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 600 Minerals<br/><b>Upkeep:</b> 3.5 Energy Credits<br/><b>Produces:</b> 1 Volatile Motes",
      "level": 21,
      "image": "images/buildings/building_space_dust_sifter.png",
      "hasImage": false,
      "nodeType": "building",
      "prerequisites": [
        "tech_orbital_elysium"
      ]
    },
    {
      "id": "building_gasgiant_floater_harvester",
      "label": "Floater Analysis Complex",
      "group": "Building",
      "title": "<b>Floater Analysis Complex</b><br/><i>Facilities dedicated to herding and studying the peculiar airborne lifeforms found on this Gas Giant.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>400<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 500 Minerals<br/><b>Upkeep:</b> 3 Energy Credits<br/><b>Produces:</b> 10 Society Research",
      "level": 17,
      "image": "images/buildings/building_gasgiant_floater_harvester.png",
      "hasImage": false,
      "nodeType": "building",
      "prerequisites": [
        "tech_gas_giant_colony"
      ]
    },
    {
      "id": "building_gasgiant_mote_harvester",
      "label": "Atmospheric Mote Extractor",
      "group": "Building",
      "title": "<b>Atmospheric Mote Extractor</b><br/><i>A facility which exploits massive mote clouds drifting through this Gas Giant's atmosphere.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>400<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 500 Minerals<br/><b>Upkeep:</b> 3 Energy Credits<br/><b>Produces:</b> 1.5 Volatile Motes",
      "level": 17,
      "image": "images/buildings/building_gasgiant_mote_harvester.png",
      "hasImage": false,
      "nodeType": "building",
      "prerequisites": [
        "tech_gas_giant_colony"
      ]
    },
    {
      "id": "building_gas_fusion_plant",
      "label": "Atmospheric Fusion Facility",
      "group": "Building",
      "title": "<b>Atmospheric Fusion Facility</b><br/><i>This building fuses light elements, such as Hydrogen and Helium harvested from this planet's atmosphere, into heavier elements that can be used by our industries.</i><br/><b>Mod: </b><i>Gigastructural Engineering & More (2.2)</i><br/><b>Build Time: </b>400<br/><b>Category: </b>Manufacturing<br/><b>Cost:</b> 600 Minerals<br/><b>Upkeep:</b> 3 Energy Credits<br/><b>Produces:</b> 7.5 Minerals",
      "level": 17,
      "image": "images/buildings/building_gas_fusion_plant.png",
      "hasImage": false,
      "nodeType": "building",
      "prerequisites": [
        "tech_gas_giant_colony"
      ]
    }
  ],
  "edges": [
    {
      "from": "tech_shroud_container",
      "to": "building_shroud_capacitor",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_fusion_disruption",
      "to": "building_iodizium_research",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_fusion_disruption",
      "to": "building_iodizium_plant",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_orbital_elysium",
      "to": "building_space_dust_sifter",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_gas_giant_colony",
      "to": "building_gasgiant_floater_harvester",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_gas_giant_colony",
      "to": "building_gasgiant_mote_harvester",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_gas_giant_colony",
      "to": "building_gas_fusion_plant",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    }
  ]
}
