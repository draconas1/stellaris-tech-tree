GraphOptions = {
    nodes: {
        shape: 'box',
        font: {
            size: 26
        },
        color: {
            highlight: {
                background: 'red'
            }
        },
        borderWidth: 0,
        borderWidthSelected: 2,
        shapeProperties: {
            useBorderWithImage: true,
            borderRadius: 6
        }
    },
    edges: {
        smooth: {
            type: 'cubicBezier',
            forceDirection: 'horizontal',
            roundness: 0.4
        },
        dashes: true
    },
    layout: {
        hierarchical: {
            direction: "LR",
            sortMethod: 'directed',
            levelSeparation: 600,
            nodeSpacing: 120,
            treeSpacing: 100,
            blockShifting: true,
            edgeMinimization: true,
            parentCentralization: true
        }
    },
    groups: {
        Physics: {
            color: {
                background: '#427df4'
            }
        },
        Society: {
            color: {
                background: '#80d17d'
            }
        },
        Engineering: {
            color: {
                background: '#e5a649'
            }
        }
    },
    interaction: {
        dragNodes: true,
        navigationButtons: true,
        keyboard: true
    },
    physics: {
        enabled: false
    }
};