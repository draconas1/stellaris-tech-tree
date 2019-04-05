function createOptions() {

  const options = _.cloneDeep(GraphOptions);
  const showImagesCheckbox = document.getElementById("showImages");
  if (showImagesCheckbox.checked === true) {
    options.nodes.shape = 'image';
    options.nodes.borderWidth = 2;
    options.nodes.font = {
      vadjust: -15
    };
    options.groups.Physics.color.border = '#427df4';
    options.groups.Society.color.border = '#80d17d';
    options.groups.Engineering.color.border = '#e5a649';
  }

  return options;
}

function findNodesByLabel() {
  let toFind = this.value;
  if (toFind.length > 3) {
    toFind = toFind.toUpperCase();
    let found = [];

    Object.values(allNodes).forEach(node => {
      if (node.label.toUpperCase().indexOf(toFind) > -1) {
        found.push(node.id);
      }
    });

    highlightFoundNodes(found);
    unsetHighlightCategoryFilter();
  }
  return false;
}

function highlightFoundNodes(found) {
  if (found.length > 0) {
    network.selectNodes(found);
    network.fit({
      nodes: found,
      animation: true
    });

    PathFunctions.highlightDependencyGraph({
      nodes: found
    });
  }
}

function unsetHighlightCategoryFilter() {
  document.getElementById("categorySelectBox").options[0].selected = true;
}

function highlightCategory() {
  const toFind = document.getElementById("categorySelectBox").value;
  let found = [];

  Object.values(allNodes).forEach(node => {
    if (node.categories !== undefined && node.categories.includes(toFind)) {
      found.push(node.id);
    }
  });
  highlightFoundNodes(found);

  return false;
}


async function createNetwork() {
  // determine what files we are loading
  const modSelectors = document.getElementsByName("modSelector");
  const includePrerequisites = document.getElementById("includeDependenciesCheckbox").checked;

  const nodeBuilder = [];
  const allNodesBuilder = [];
  const edgeListing = [];

  // want to see if only core game is checked
  // if so we load the special "core game with no mods" datafiles
  // otherwise we load with mod files.
  let isOnlyStellarisChecked = false;
  for (let i = 0; i< modSelectors.length; i++) {
    const selectElement = modSelectors[i];
    // if it is the main game element
    // then if it is checked there is a chance that we are using the special files
    // (which may be disproved by a later mod)
    // if it is not checked we are definately not using the special files.
    if (selectElement.value === "Stellaris") {
      if (selectElement.checked) {
        isOnlyStellarisChecked = true;
      }
      else {
        break;
      }
    }
      // if the element is not the main game element
      // and it is checked, then we are not loading the special files
      // and should abort immidiately
    else {
      if (selectElement.checked) {
        isOnlyStellarisChecked = false;
        break;
      }
    }
  }

  if (isOnlyStellarisChecked) {
    const variables = modToVariables["Stellaris-No-Mods"];
    variables.forEach(varName =>  {
      nodeBuilder.push(...(window[varName].nodes));
      edgeListing.push(...(window[varName].edges));
      allNodesBuilder.push(...(window[varName].nodes));

      // window[varName].nodes.forEach(node => {
      //   if(!node.level) {
      //     console.log("Node " + node.id + " in variable " + varName + " has no group");
      //   }
      // })
    })
  }
  else {
    // get nodes and edges from files
    modSelectors.forEach(selectElement => {
      const variables = modToVariables[selectElement.value];
      if (selectElement.checked) {
        variables.forEach(varName =>  {
          nodeBuilder.push(...(window[varName].nodes));
          edgeListing.push(...(window[varName].edges));
        })
      }
      else {
        // push the edges in anyway if include pre-requistes is true
        if (includePrerequisites) {
          variables.forEach(varName =>  {
            edgeListing.push(...(window[varName].edges));
          })
        }
      }
      // if not including prerequistes then don't want to create a massive array of everything
      // so only populate this if including pre-reqs
      if (includePrerequisites) {
        variables.forEach(varName => allNodesBuilder.push(...(window[varName].nodes)));
      }
    });
  }
  // add the tech root nodes.
  nodeBuilder.push(...(TechRootNodesGraphDataTech.nodes));
  allNodesBuilder.push(...(TechRootNodesGraphDataTech.nodes));

  const nodeListing = _.uniqBy(nodeBuilder, 'id');

  // initalise
  const techAreaFilter = document.getElementById("techAreaFilterBox").value;
  let activeNodes;
  if (techAreaFilter === undefined || techAreaFilter === '' || techAreaFilter === 'All') {
    activeNodes = nodeListing;
  } else {
    //lodash filter with a shorthand for propertyname matches value.
    activeNodes = _.filter(nodeListing, function (node) {
      return node.group === techAreaFilter || node.group === "Mod" + techAreaFilter;
    });
  }
  const categoryFilter = document.getElementById("categoryFilterBox").value;
  if (categoryFilter === undefined || categoryFilter === '' || categoryFilter === 'All') {
    // no op on active nodes
  } else {
    //lodash filter with a shorthand for propertyname matches value.
    activeNodes = _.filter(activeNodes, function (node) {
      return node.categories !== undefined && node.categories.includes(categoryFilter)
    });
  }

  if (includePrerequisites) {
    // if including prereqs then when building the graph we may need to use the massive array of everything
    const allNodeListing = includePrerequisites ? _.uniqBy(allNodesBuilder, 'id') : nodeListing;
    let nodesAndDeps = _.keyBy(activeNodes, 'id');
    PathFunctions.addAllDependencyNodes(_.transform(activeNodes, function (result, node) {
        result.push(node.id);
      }),
      nodesAndDeps,
      _.keyBy(allNodeListing, 'id')
    );
    activeNodes = Object.values(nodesAndDeps);
  }
  unsetHighlightCategoryFilter();
  nodesDataset = new vis.DataSet(activeNodes);
  edgesDataset = new vis.DataSet(edgeListing);

  // create a network
  const container = document.getElementById('network');
  const data = {
    nodes: nodesDataset,
    edges: edgesDataset
  };
  const options = createOptions();

  network = new vis.Network(container, data, options);

  // get a JSON object
  allNodes = nodesDataset.get({returnType: "Object"});
  allEdges = edgesDataset.get({returnType: "Object"});

  network.on("click", function (params) {
    PathFunctions.highlightDependencyGraph(params);
    unsetHighlightCategoryFilter()
  });
  if (document.getElementById("showImages").checked === false) {
    let promiseResults = await ImageFunctions.loadAllImages();
    network.on("afterDrawing", ctx => ImageFunctions.addImagesToTextNodes(ctx, promiseResults));
  }
  return false;
}

