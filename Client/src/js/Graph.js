window.initializeGraph = function (projectId, containerId, containerVersion, linksetId, linkId) {
    $.get(BASE_URL + "/Partials/Graph",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, linksetId: linksetId, linkId: linkId },
        function (data) {
            var dt = JSON.parse(data);
            var container = document.getElementById('graph-container');
            var network = new window.vis.Network(container, dt, initializeOptions());

        });
};

window.initializeOptions = function () {
    return {
        edges: {
            color: {
                color: '#94c11c',
                highlight: '#003560'
            },
            arrows: "to",
            width: 2
        },

        nodes: {
            shape: 'box',
            color: {
                background: 'lightgray',
                border: '#94c11c',
                highlight: {
                    border: '#94c11c',
                    background: '#94c11c'
                }
            },
            font: { color: 'black' },
            borderWidth: 2,
            margin: {
                top: 10,
                bottom: 10,
                left: 10,
                right: 10
            },
            shadow: {
                enabled: true,
                color: 'rgba(0,0,0,0.5)',
                size: 10,
                x: 5,
                y: 5
            }
        },
        layout: {
            randomSeed: undefined,
            improvedLayout: true,
            clusterThreshold: 250,
            hierarchical: {
                enabled: true,
                levelSeparation: 300,
                nodeSpacing: 450,
                treeSpacing: 500,
                blockShifting: true,
                edgeMinimization: true,
                parentCentralization: true,
                direction: 'UD',        // UD, DU, LR, RL
                sortMethod: 'directed',  // hubsize, directed
                shakeTowards: 'leaves'  // roots, leaves
            }
        },
        interaction: {
            dragNodes: true,
            dragView: true,
            hideEdgesOnDrag: false,
            hideEdgesOnZoom: false,
            hideNodesOnDrag: false,
            hover: false,
            hoverConnectedEdges: true,
            keyboard: {
                enabled: false,
                speed: { x: 10, y: 10, zoom: 0.02 },
                bindToWindow: true,
                autoFocus: true,
            },
            multiselect: false,
            navigationButtons: true,
            selectable: true,
            selectConnectedEdges: true,
            tooltipDelay: 300,
            zoomSpeed: 1,
            zoomView: true
        }
    };
}

window.console.log('The \'Graph\' bundle has been loaded!');