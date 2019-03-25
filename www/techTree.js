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


  const nodeBuilder = [];
  const allNodesBuilder = [];
  const edgeListing = [];

  modSelectors.forEach(x => {

    const variables = modToVariables[x.value];
    if (x.checked) {
      variables.forEach(varName =>  {
        nodeBuilder.push(...(window[varName].nodes));
        edgeListing.push(...(window[varName].edges));
      })
    }
    variables.forEach(varName => allNodesBuilder.push(...(window[varName].nodes)));
  });

  const nodeListing = _.uniqBy(nodeBuilder, 'id');
  const allNodeListing = _.uniqBy(allNodesBuilder, 'id');

  // initalise
  const techAreaFilter = document.getElementById("techAreaFilterBox").value;
  let activeNodes;
  if (techAreaFilter === undefined || techAreaFilter === '' || techAreaFilter === 'All') {
    activeNodes = nodeListing;
  } else {
    //lodash filter with a shorthand for propertyname matches value.
    activeNodes = _.filter(nodeListing, ['group', techAreaFilter]);
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

  const includePrerequisites = document.getElementById("includeDependenciesCheckbox").checked;
  if (includePrerequisites === true) {
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

