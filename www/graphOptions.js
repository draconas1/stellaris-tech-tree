GraphOptions = {
  nodes: {
    shape: 'box',
    shapeProperties: {
      useBorderWithImage: true,
      borderRadius: 3
    },
    font: {
      size: 26,
      align: 'left'
    },
    color: {
      highlight: {
        background: 'red'
      }
    },
    borderWidth: 0,
    borderWidthSelected: 2,
    heightConstraint: {minimum: 46},
    margin: {
      left: 58,
      right: 36
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
    ModPhysics: {
      color: {
        background: '#97aef4'
      }
    },
    Society: {
      color: {
        background: '#6bd169'
      }
    },
    ModSociety: {
      color: {
        background: '#acd1b9'
      }
    },
    Engineering: {
      color: {
        background: '#e5a649'
      }
    },
    ModEngineering: {
      color: {
        background: '#e5dda1'
      }
    },
    Building: {
      shape: 'image',
      shapeProperties: {
        useBorderWithImage: false,
        useImageSize: true,
        borderRadius: 0
      },
      font: {
        size: 26,
        align: 'left',
        vadjust: -50
      },
    }
  },
  interaction: {
    dragNodes: true,
    navigationButtons: true,
    keyboard: false
  },
  physics: {
    enabled: false
  }
};