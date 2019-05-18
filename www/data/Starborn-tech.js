StarbornGraphDataTech = {
  "nodes": [
    {
      "id": "tech_starborn_expand_starting_habitat_galatic_wonders",
      "label": "Orbital City Enlargement",
      "group": "ModEngineering",
      "title": "<b>Orbital City Enlargement</b> (tech_starborn_expand_starting_habitat_galatic_wonders)<br/><i>What is the point of building Galatic Wonders if we cannot use that engineering knowledge to improve our Orbital City</i><br/><b>Mod: </b>starborn<br/><b>Tier: </b>5<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>32000",
      "level": 24,
      "image": "images/technologies/tech_starborn_expand_starting_habitat_galatic_wonders.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_5"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "tech_starborn_expand_starting_habitat_master_builders",
      "label": "Orbital City Enlargement",
      "group": "ModEngineering",
      "title": "<b>Orbital City Enlargement</b> (tech_starborn_expand_starting_habitat_master_builders)<br/><i>Our mastery of building and construction has opened up the possibility for ideas once discarded as impossible to be used to enlarge our Orbital City</i><br/><b>Mod: </b>starborn<br/><b>Tier: </b>5<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>32000",
      "level": 24,
      "image": "images/technologies/tech_starborn_expand_starting_habitat_master_builders.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_5"
      ],
      "categories": [
        "voidcraft"
      ]
    },
    {
      "id": "tech_starborn_expand_starting_habitat_voidborn",
      "label": "Orbital City Enlargement",
      "group": "ModEngineering",
      "title": "<b>Orbital City Enlargement</b> (tech_starborn_expand_starting_habitat_voidborn)<br/><i>As we have become Voidborne our engineers believe they can develop new ways to enlarge and improve our Orbital City</i><br/><b>Mod: </b>starborn<br/><b>Tier: </b>3<br/><b>Category: </b>Voidcraft<br/><b>Base cost: </b>12000",
      "level": 16,
      "image": "images/technologies/tech_starborn_expand_starting_habitat_voidborn.png",
      "hasImage": true,
      "nodeType": "tech",
      "prerequisites": [
        "tech_starbase_4"
      ],
      "categories": [
        "voidcraft"
      ]
    }
  ],
  "edges": [
    {
      "from": "tech_starbase_5",
      "to": "tech_starborn_expand_starting_habitat_galatic_wonders",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_starbase_5",
      "to": "tech_starborn_expand_starting_habitat_master_builders",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    },
    {
      "from": "tech_starbase_4",
      "to": "tech_starborn_expand_starting_habitat_voidborn",
      "arrows": "to",
      "color": {
        "color": "grey"
      },
      "dashes": true
    }
  ]
}
