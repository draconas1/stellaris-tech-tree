function createOptions() {

    const options = _.cloneDeep(GraphOptions);
    const showImagesCheckbox = document.getElementById("showImages");
    if (showImagesCheckbox.checked === true) {
        options.nodes.shape = 'image';
        options.nodes.borderWidth = 2;
		options.nodes.font = {
			vadjust : -15
		};
        options.groups.Physics.color.border = '#427df4';
        options.groups.Society.color.border = '#80d17d';
        options.groups.Engineering.color.border = '#e5a649';
    }

    return options;
}

function searchForNode(event) {
    let toFind = this.value;
    if (toFind.length > 3) {
        toFind = toFind.toUpperCase();
        let found = [];

        Object.values(allNodes).forEach(node => {
            if (node.label.toUpperCase().indexOf(toFind) > -1) {
                found.push(node.id);
            }
        });

        if (found.length > 0) {
            network.selectNodes(found);
            network.fit({
                nodes: found,
                animation: true
            });

            PathFunctions.highlightDependencyGraph({
                nodes : found
            });
        }
    }
    return false;
}

async function createNetwork() {
    const techAreaFilter = document.getElementById("filterBox").value;
    if (techAreaFilter === undefined || techAreaFilter === '' || techAreaFilter === 'All') {
        nodesDataset = new vis.DataSet(GraphData.nodes);
    } else {
        //lodash filter with a shorthand for propertyname matches value.
        let newNodes = _.filter(GraphData.nodes, ['group', techAreaFilter]);
        nodesDataset = new vis.DataSet(newNodes);
    }


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

    network.on("click", PathFunctions.highlightDependencyGraph);
    if (document.getElementById("showImages").checked === false) {
        let promiseResults = await ImageFunctions.loadAllImages();
        network.on("afterDrawing", ctx =>  ImageFunctions.addImagesToTextNodes(ctx, promiseResults));
    }

    return false;
}