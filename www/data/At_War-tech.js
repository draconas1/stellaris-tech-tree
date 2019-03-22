AtWar-GraphDataTech = {
  "nodes": [
    {
      "id": "eac_aw_tech_advanced_carrier",
      "label": "Advanced Carrier Designs",
      "group": "Engineering",
      "title": "<b>Advanced Carrier Designs</b><br/><i>Adds new strike craft focused ship sections.\\n</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>20000",
      "level": 16,
      "image": "images/technologies/eac_aw_tech_advanced_carrier.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_strike_craft_3",
        "tech_battleships"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_aw_tech_light_carrier",
      "label": "Cruiser Light Carrier Designs",
      "group": "Engineering",
      "title": "<b>Cruiser Light Carrier Designs</b><br/><i>Adds new strike craft focused ship sections to cruisers.\\n</i><br/><b>Tier: </b>3<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>12000",
      "level": 13,
      "image": "images/technologies/eac_aw_tech_light_carrier.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_strike_craft_2",
        "tech_cruisers"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_aw_tech_missile_cruiser",
      "label": "Missile Cruiser Designs",
      "group": "Engineering",
      "title": "<b>Missile Cruiser Designs</b><br/><i>Adds more missile / torpedo focused ship sections.\\n</i><br/><b>Tier: </b>3<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>12000",
      "level": 13,
      "image": "images/technologies/eac_aw_tech_missile_cruiser.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_missiles_4",
        "tech_cruisers"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_aw_tech_repeatable_starbase_defense_platform_capacity_add",
      "label": "Starport Defense Algorithms",
      "group": "Engineering",
      "title": "<b>Starport Defense Algorithms</b><br/><i>Improved control algorithms increase the number of defense platforms that can be reliably controlled.</i><br/><b>Tier: </b>5<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>50000<br/><b>Attributes: </b>Repeatable",
      "color": {
        "border": "#0078CE"
      },
      "borderWidth": 1,
      "level": 19,
      "image": "images/technologies/eac_aw_tech_repeatable_starbase_defense_platform_capacity_add.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_4"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_large_gun_citadel",
      "label": "Large Gun Citadels",
      "group": "Engineering",
      "title": "<b>Large Gun Citadels</b><br/><i>A citadel core with large gun mounts.</i><br/><b>Tier: </b>3<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>0",
      "level": 13,
      "image": "images/technologies/eac_tech_large_gun_citadel.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_4"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_1",
      "label": "Patrol Craft I",
      "group": "Engineering",
      "title": "<b>Patrol Craft I</b><br/><i>A modified corvette hull without an FTL drive. Built and maintained at the Planetary Defense Force Base.</i><br/><b>Tier: </b>0<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>0<br/><b>Attributes: </b>Starter",
      "color": {
        "border": "#00CE56"
      },
      "borderWidth": 1,
      "level": 5,
      "image": "images/technologies/eac_tech_patrol_craft_1.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_corvettes"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_2",
      "label": "Patrol Craft II",
      "group": "Engineering",
      "title": "<b>Patrol Craft II</b><br/><i>An improved modified corvette hull without an FTL drive. Built and maintained at the Planetary Defense Force Base.\\n§ROriginal Patrol Craft I designs will no longer be used.§!\\nAdds §G+2§! ship building jobs to the Planetary Defense Force Base.</i><br/><b>Tier: </b>3<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>10000",
      "level": 13,
      "image": "images/technologies/eac_tech_patrol_craft_2.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_1",
        "tech_cruisers"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_build_speed_1",
      "label": "Standardized Patrol Craft II Patterns",
      "group": "Engineering",
      "title": "<b>Standardized Patrol Craft II Patterns</b><br/><i>Establishing new standards for the modeling and construction of Patrol Craft II greatly improves the efficiency of the production pipeline. Decreases base Patrol Craft II construction time by 25%</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>20000",
      "level": 15,
      "image": "images/technologies/eac_tech_patrol_craft_build_speed_1.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_2"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_build_speed_2",
      "label": "Improved Patrol Craft II Assembly Lines",
      "group": "Engineering",
      "title": "<b>Improved Patrol Craft II Assembly Lines</b><br/><i>Establishing new standards for the assembly lines of Patrol Craft II greatly improves the efficiency of the production pipeline. Decreases base Patrol Craft II construction time by 50%</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>16000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 16,
      "image": "images/technologies/eac_tech_patrol_craft_build_speed_2.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_build_speed_1"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_hull_1",
      "label": "Improved Patrol Craft I Hulls I",
      "group": "Engineering",
      "title": "<b>Improved Patrol Craft I Hulls I</b><br/><i>Advances in building techniques will allow for the construction of sturdier and more durable Patrol Craft I. Increases Patrol Craft I cost by £alloys 25</i><br/><b>Tier: </b>1<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>3000",
      "level": 7,
      "image": "images/technologies/eac_tech_patrol_craft_hull_1.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_corvette_hull_1"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_hull_2",
      "label": "Improved Patrol Craft I Hulls II",
      "group": "Engineering",
      "title": "<b>Improved Patrol Craft I Hulls II</b><br/><i>Advances in building techniques will allow for the construction of sturdier and more durable Patrol Craft I. Increases Patrol Craft I cost by £alloys 25</i><br/><b>Tier: </b>2<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>4000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 9,
      "image": "images/technologies/eac_tech_patrol_craft_hull_2.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_hull_1"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_hull_3",
      "label": "Improved Patrol Craft II Hulls I",
      "group": "Engineering",
      "title": "<b>Improved Patrol Craft II Hulls I</b><br/><i>Advances in building techniques will allow for the construction of sturdier and more durable Patrol Craft II. Increases Patrol Craft II cost by £alloys 25</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>24000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 15,
      "image": "images/technologies/eac_tech_patrol_craft_hull_3.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_2"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_patrol_craft_hull_4",
      "label": "Improved eac_pdf_patrol_craft2 Hulls II",
      "group": "Engineering",
      "title": "<b>Improved eac_pdf_patrol_craft2 Hulls II</b><br/><i>Advances in building techniques will allow for the construction of sturdier and more durable Patrol Craft II. Increases Patrol Craft II cost by £alloys 25</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>16000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 16,
      "image": "images/technologies/eac_tech_patrol_craft_hull_4.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_patrol_craft_hull_3"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_planetary_defense_force",
      "label": "Planetary Defense Force",
      "group": "Engineering",
      "title": "<b>Planetary Defense Force</b><br/><i>A terrestrial fleet capable of entering space to deal with orbital and system hostiles. The fleet uses a modified corvette hull without an FTL drive.</i><br/><b>Tier: </b>0<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>0<br/><b>Attributes: </b>Starter",
      "color": {
        "border": "#00CE56"
      },
      "borderWidth": 1,
      "level": 5,
      "image": "images/technologies/eac_tech_planetary_defense_force.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_corvettes"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_space_defense_station_heavy_1",
      "label": "Miniaturized Defense Platform Components",
      "group": "Engineering",
      "title": "<b>Miniaturized Defense Platform Components</b><br/><i>Through component miniaturization, thicker hulls and additional turret points can be added to create a Heavy Defense Platform in the same shape and size as the current Defense Platform.</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>16000",
      "level": 15,
      "image": "images/technologies/eac_tech_space_defense_station_heavy_1.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_4"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_space_defense_station_heavy_hull_1",
      "label": "Improved Heavy Defense Platform Hulls",
      "group": "Engineering",
      "title": "<b>Improved Heavy Defense Platform Hulls</b><br/><i>A reinforced framework and blast shields with enhanced impact absorption will ensure the structural integrity of the heavy platform even under immense pressure.</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>20000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 16,
      "image": "images/technologies/eac_tech_space_defense_station_heavy_hull_1.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_space_defense_station_heavy_1"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "eac_tech_space_defense_station_heavy_hull_2",
      "label": "Advanced Heavy Defense Platform Hulls",
      "group": "Engineering",
      "title": "<b>Advanced Heavy Defense Platform Hulls</b><br/><i>The latest heavy platform hulls have optimized structural integrity fields and improved bulkheads.</i><br/><b>Tier: </b>4<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>24000<br/><b>Attributes: </b>Rare",
      "color": {
        "border": "#8900CE"
      },
      "borderWidth": 1,
      "level": 17,
      "image": "images/technologies/eac_tech_space_defense_station_heavy_hull_2.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "eac_tech_space_defense_station_heavy_hull_1"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "Physics-root",
      "label": "physics",
      "group": "Physics",
      "level": 0,
      "image": "images/technologies/Physics-root.png",
      "hasImage": true,
      "nodeType": "tech",
      "categories": [
        "particles",
        "field_manipulation",
        "computing"
      ]
    },
    {
      "id": "Society-root",
      "label": "society",
      "group": "Society",
      "level": 0,
      "image": "images/technologies/Society-root.png",
      "hasImage": true,
      "nodeType": "tech",
      "categories": [
        "biology",
        "statecraft",
        "new_worlds",
        "psionics",
        "military_theory"
      ]
    },
    {
      "id": "Engineering-root",
      "label": "engineering",
      "group": "Engineering",
      "level": 0,
      "image": "images/technologies/Engineering-root.png",
      "hasImage": true,
      "nodeType": "tech",
      "categories": [
        "industry",
        "propulsion",
        "materials",
        "voidcraft"
      ]
    }
  ],
  "edges": [
    {
      "from": "tech_starbase_4",
      "to": "eac_tech_large_gun_citadel",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_space_defense_station_heavy_hull_1",
      "to": "eac_tech_space_defense_station_heavy_hull_2",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_space_defense_station_heavy_1",
      "to": "eac_tech_space_defense_station_heavy_hull_1",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_starbase_4",
      "to": "eac_tech_space_defense_station_heavy_1",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_starbase_4",
      "to": "eac_aw_tech_repeatable_starbase_defense_platform_capacity_add",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_build_speed_1",
      "to": "eac_tech_patrol_craft_build_speed_2",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_2",
      "to": "eac_tech_patrol_craft_build_speed_1",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_hull_3",
      "to": "eac_tech_patrol_craft_hull_4",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_2",
      "to": "eac_tech_patrol_craft_hull_3",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_hull_1",
      "to": "eac_tech_patrol_craft_hull_2",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_corvette_hull_1",
      "to": "eac_tech_patrol_craft_hull_1",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "eac_tech_patrol_craft_1",
      "to": "eac_tech_patrol_craft_2",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_cruisers",
      "to": "eac_tech_patrol_craft_2",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_corvettes",
      "to": "eac_tech_patrol_craft_1",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_corvettes",
      "to": "eac_tech_planetary_defense_force",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_strike_craft_2",
      "to": "eac_aw_tech_light_carrier",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_cruisers",
      "to": "eac_aw_tech_light_carrier",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_strike_craft_3",
      "to": "eac_aw_tech_advanced_carrier",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_battleships",
      "to": "eac_aw_tech_advanced_carrier",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_missiles_4",
      "to": "eac_aw_tech_missile_cruiser",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_cruisers",
      "to": "eac_aw_tech_missile_cruiser",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    }
  ]
}
