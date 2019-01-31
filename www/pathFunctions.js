PathFunctions = {

    highlightDependencyGraph: function (params) {
        //reset first
        Object.values(allEdges).forEach(edge => {
            if (!PathFunctions.connectsToRoot(edge)) {
                const color = {};
                color.color = 'grey';
                edge.color = color;
                edge.width = 1;
                edge.dashes = true;
            }
        });

        // if something is selected:
        params.nodes.forEach(selectedNode => {
            // find and highlight the path
            const fromNodes = PathFunctions.findAllConnections(selectedNode, 'from').concat(selectedNode);
            const toNodes = PathFunctions.findAllConnections(selectedNode, 'to').concat(selectedNode);
            PathFunctions.highlightEdges(fromNodes);
            PathFunctions.highlightEdges(toNodes);
        });

        // transform the object into an array
        edgesDataset.update(Object.values(allEdges));
    },

    highlightEdges: function (nodePathIds) {
        nodePathIds.forEach(nodeId => {
            // can only get all the edges connected to our node, which includes edges to irrelevant nodes
            // //(e.g. other things that also have a pre-requisite of one of our prerequites)
            let connectedEdgeIds = network.getConnectedEdges(nodeId);
            connectedEdgeIds.forEach(edgeId => {
                let edge = allEdges[edgeId];
                // only highlight our edge if its from and to ends are on our path
                if (nodePathIds.indexOf(edge.from) > -1 && nodePathIds.indexOf(edge.to) > -1) {
                    PathFunctions.highlightEdge(edge);
                }
            });
        });
    },

    highlightEdge: function (edge) {
        if (!PathFunctions.connectsToRoot(edge)) {
            let color = {};
            color.color = '#000000';
            color.highlight = '#000000';
            edge.color = color;
            edge.width = 3;
            edge.dashes = false;
        }
    },

    // Always give a direction else it will stack overflow due to infinite loop.
    // due to not checking if its already visited, so forward and back expansion means it just keeps bouncing.
    // fortunately a tech tree cannot contain loops
    findAllConnections: function (node, direction) {
        let connectedNodes = network.getConnectedNodes(node, direction);
        if (connectedNodes.length > 0) {
            let allConnectedNodes = [].concat(connectedNodes);
            connectedNodes.forEach(connectedNode => {
                let connections = PathFunctions.findAllConnections(connectedNode, direction);
                allConnectedNodes = allConnectedNodes.concat(connections);
            });

            return allConnectedNodes;
        }
        return [];
    },

    // does the edge connect to one of the fake root nodes
    connectsToRoot: function (edge) {
        return edge.from === 'Engineering-root' || edge.from === 'Society-root' || edge.from === 'Physics-root';
    }
};

