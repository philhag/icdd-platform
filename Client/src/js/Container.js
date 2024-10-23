const toast = require("../../../wwwroot/lib/bootstrap/js/dist/toast");

$(function () {
    $("#divContainerTree").jstree({
        "core": {
            "animation": 0,
            "check_callback": true,
            "themes": { "stripes": true }
        }
    });

    $("#divContainerTree").on("activate_node.jstree", function (e, data) {
        if (data == undefined || data.node == undefined || data.node.id == undefined)
            return;
        if (data.node.id === "project") {
            window.location.replace(window.BASE_URL + "/Project/Details/" + data.node.data.project);
        }
        if (data.node.data.type === "other") {
            window.location.replace(window.BASE_URL + "/Project/" + data.node.data.project + "/Container/" + data.node.data.container + "/" + data.node.data.containerversion + "/Index");
        }
        if (data.node.id === "root") {
            loadIndex(data.node.data.project, data.node.data.container, data.node.data.containerversion);
        }
        if (data.node.id === "index-node") {
            loadIndex(data.node.data.project, data.node.data.container, data.node.data.containerversion);
        }
    });

    if (!(window.location.hash == undefined || window.location.hash == "") && window.view != undefined && window.view == "") {
        load(window.project, window.containerMeta, window.containerVersion, window.location.hash);
        window.view = window.location.hash;
    }
});

window.load = function (projectId, containerId, containerVersion, guid) {

    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');

    $.get(BASE_URL + "/Partials/Content",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, guid: guid },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/Properties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, guid: guid },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
};
window.loadTree = function (projectId, containerId, containerVersion) {
    $("#divContainerTree").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerTree").jstree(true).destroy();
    $.get(BASE_URL + "/Partials/TreeContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerTree`).html(data);
            $("#divContainerTree").jstree({
                "core": {
                    "animation": 0,
                    "check_callback": true,
                    "themes": { "stripes": true }
                }
            });
        });
};

window.loadSPARQL = function (projectId, containerId, containerVersion, query="") {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');

    $.get(BASE_URL + "/Partials/SPARQL",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerContent`).html(data);
            document.querySelectorAll(".queryBtnClass").forEach(item => {
                item.addEventListener('click',
                    event => {
                        var elem = event.target;
                        document.getElementById("sparql-query").value = elem.textContent;
                        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                            return new bootstrap.Tooltip(tooltipTriggerEl);
                        });
                    });
            });

            if (query != "") {
                document.getElementById("sparql-query").value = query;
            }
        });
    $.get(BASE_URL + "/Partials/SavedSPARQLqueries",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerProperties`).html(data);
            document.querySelectorAll(".queryBtnClass").forEach(item => {
                item.addEventListener('click',
                    event => {
                        var elem = event.target;
                        document.getElementById("sparql-query").value = elem.textContent;
                    });
            });
        });
    window.location.hash = "";
};

window.loadSHACL = function (projectId, containerId, containerVersion) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');

    $.get(BASE_URL + "/Partials/SHACL",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerContent`).html(data);
            document.querySelectorAll(".queryBtnClass").forEach(item => {
                item.addEventListener('click',
                    event => {
                        var elem = event.target;
                        document.getElementById("sparql-query").value = elem.textContent;
                        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                            return new bootstrap.Tooltip(tooltipTriggerEl);
                        });
                    });
            });
        });
    $.get(BASE_URL + "/Partials/SavedSHACLfiles",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerProperties`).html(data);
            document.querySelectorAll(".shaclBtnClass").forEach(item => {
                item.addEventListener('click',
                    event => {
                        var elemContent = event.currentTarget.getAttribute("data-filename");
                        var shapeData = document.getElementById("shaclInput-" + elemContent).textContent;
                        document.getElementById("shacl-shapes").value = shapeData;
                    });
            });
        });
    window.location.hash = "";
};

window.loadLink = function (projectId, containerId, containerVersion, linksetId, linkId) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/LinkContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, linksetId: linksetId, linkId: linkId },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/LinksetProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, linksetId: linksetId },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
    window.location.hash = linkId;
};

window.loadLinkset = function (projectId, containerId, containerVersion, linksetId) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/LinksetContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, linksetId: linksetId },
        function (data) {
            $(`#divContainerContent`).html(data);
            var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        });
    $.get(BASE_URL + "/Partials/LinksetProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, linksetId: linksetId },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
    window.location.hash = linksetId;
};

window.loadIndex = function (projectId, containerId, containerVersion) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/IndexContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/IndexProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
        function (data) {
            $(`#divContainerProperties`).html(data);

        });
    window.location.hash = "";
};

window.loadDocument = function (projectId, containerId, containerVersion, contentId) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/DocumentContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, contentId: contentId },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/DocumentProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, contentId: contentId },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
    window.location.hash = contentId;
};

window.loadOntology = function (projectId, containerId, containerVersion, ontologyName) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/OntologyContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, ontologyName: ontologyName },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/OntologyProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, ontologyName: ontologyName },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
    window.location.hash = "";
};

window.loadPayloadTriples = function (projectId, containerId, containerVersion, payloadName) {
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $.get(BASE_URL + "/Partials/PayloadTriplesContent",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, payloadName: payloadName },
        function (data) {
            $(`#divContainerContent`).html(data);

        });
    $.get(BASE_URL + "/Partials/PayloadTriplesProperties",
        { projectId: projectId, containerId: containerId, containerVersion: containerVersion, payloadName: payloadName },
        function (data) {
            $(`#divContainerProperties`).html(data);
        });
    window.location.hash = "";
};

window.queryContainer = function (containerId, containerVersion, projectId) {

    $("#query-status").html('<div class="spinner-border text-rub-green m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    var query = $.trim($("#sparql-query").val());
    var applyInference = $("#chkApplyInference").is(":checked");

    $.ajax({
        type: "GET", //rest Type
        dataType: "json",
        url: BASE_URL + "/Container/QueryContainer",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            applyInference: applyInference,
            query: query
        },
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            var json = JSON.parse(result);
            var str = JSON.stringify(json, undefined, 4);
            window.$("#sparql-results").html(str);
            console.log(json);
            if (json.head !== undefined) {
                window.$("#var-header").html("");
                window.$("#sparql-tabular-container").html("");
                let varArray = json.head.vars;
                json.head.vars.forEach(function (heading, index) {
                    window.$("#var-header").append(`<div class='col' id='col-${index}'>${heading}</div>`);
                });
                var size = 12 / varArray.length;

                json.results.bindings.forEach(function (heading, index) {
                    window.$("#sparql-tabular-container").append(`<div id="row-${index}" class="row m-0 p-0"></div>`);
                    varArray.forEach(function (variable, index2) {
                        if (variable === "ifcguid") {
                            window.$(`#row-${index}`).append(`<div class='col-${size} border fs-6 text-break guidRow' id='col-${index2}' onclick="queryGuids(this)" style="cursor: pointer;">${heading[variable].value}</div>`);
                        }
                        else {
                            window.$(`#row-${index}`).append(`<div class='col-${size} border fs-6 text-break' id='col-${index2}'>${heading[variable].value}</div>`);
                        }
                    });

                });
                document.getElementById("downloadCSV").disabled = false;
                document.getElementById("downloadCSV").onclick = function () { downloadCSV(json); };
                if (json.head.vars.includes("ifcguid")) {
                    document.getElementById("highlightGuidButton").disabled = false;
                    window.ifcguids = new Array();
                    json.results.bindings.forEach(function (heading, index) {
                        varArray.forEach(function (variable, index2) {
                            if (variable === "ifcguid") {
                                window.ifcguids.push(heading[variable].value);
                            }
                        });

                    });

                } else {
                    document.getElementById("highlightGuidButton").disabled = true;

                }

                for (let elem of window.selected) {
                    appSelectElem(parseInt(elem), false, false);
                }
                window.$("#query-status").html('<div class="oi oi-check text-rub-green" role="status"><span class= "visually-hidden">Success</span></div>');
                window.$("#query-results").html('<div class="px-3"> ' + json.results.bindings.length + ' results</div>');
                //add query to list
                $("#divContainerProperties").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');
                $.get(BASE_URL + "/Partials/SavedSPARQLqueries",
                    { projectId: projectId, containerId: containerId, containerVersion: containerVersion },
                    function (data) {
                        $(`#divContainerProperties`).html(data);
                        document.querySelectorAll(".queryBtnClass").forEach(item => {
                            item.addEventListener('click',
                                event => {
                                    var elem = event.target;
                                    document.getElementById("sparql-query").value = elem.textContent;
                                });
                        });
                    });

            } else {
                toastr.error("SPARQL query is not well formulated. Please try again.", "SPARQL Query Error");
                window.$("#query-status").html('<div class="oi oi-warning text-danger" role="status"><span class= "visually-hidden">Error</span></div>');

            }
        }
    });
}

window.queryContainerShacl = function (containerId, containerVersion, projectId) {

    $("#query-status-shacl").html('<div class="spinner-border text-rub-green m-3" role="status"><span class= "visually-hidden">Querying...</span></div>');
    var query = $.trim($("#shacl-shapes").val());
    var applyInference = $("#chkApplyInference").is(":checked");



    var queryFile = new FormData();
    queryFile.append('file', new File([new Blob([query])], "request.query.ttl"));
    queryFile.append('containerId', containerId);
    queryFile.append('containerVersion', containerVersion);
    queryFile.append('projectId', projectId);
    queryFile.append('applyInference', applyInference);
    $.ajax({
        type: "POST", //rest Type
        // dataType: "json",
        cache: false,
        contentType: false,
        processData: false,
        url: BASE_URL + "/Container/ShaclContainer",
        data: queryFile,
        async: true,
        // contentType: "application/json; charset=utf-8",
        success: function (result) {
            var json = JSON.parse(result);
            var str = JSON.stringify(json, undefined, 4);
            window.$("#validation-results-shacl-json").html(str);
            console.log(json);
            if (json.length > 0) {
                window.$("#validation-results-shacl-tabular").html("");
                if (json.find(c => c.ResultValue === "all valid") !== undefined) {
                    window.$("#query-results-shacl").html("Valid");
                    window.$("#query-status-shacl").html('<div class="oi oi-check text-rub-green px-2" role="status"><span class= "visually-hidden">Success</span></div>');
                    toastr.success("SHACL validation was successful", "SHACL Validation!");
                    return;
                } else {
                    window.$("#query-results-shacl").html("<div class='text-warning'>"+json.length + " results</div>");
                    window.$("#query-status-shacl").html('<div class="oi oi-warning text-warning px-2" role="status"><span class= "visually-hidden">Violation</span></div>');
                    json.forEach(function (result, index) {
                        toastr.warning("Violation during SHACL execution for Node: " + result.FocusNode, "SHACL Validation");
                        window.$("#validation-results-shacl-tabular").append(`<div id="row-${index}" class="row m-0 p-0"></div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.Severity}</div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.SourceShape}</div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.FocusNode}</div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.FocusPath}</div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.FocusValue}</div>`);
                        window.$(`#row-${index}`).append(`<div class='col-2 border fs-6 text-break' id='col-${index}'>${result.ValidationMessage}</div>`);
                    });
                }
               
            } else {
                toastr.error("Error during SHACL execution.", "SHACL Validation");
                window.$("#query-status").html('<div class="oi oi-warning text-danger" role="status"><span class= "visually-hidden">Error</span></div>');

            }
        }
    });
}

window.saveCSV = function (objArray) {
    let headers = [];
    let csv = "";
    objArray["head"].vars.forEach(function (element) { csv += element + ","; headers.push(element) });
    csv = csv.substring(0, csv.length - 1);
    csv += "\n";
    objArray["results"].bindings.forEach(function (result) {
        for (let i in headers) {
            csv += result[headers[i]].value + ",";
        }
        csv += "\n";
    });
    return csv;
}

window.downloadCSV = function (json) {
    let csv = saveCSV(json);
    let downloadLink = document.createElement("a");
    let blob = new Blob(["\ufeff", csv]);
    let url = URL.createObjectURL(blob);
    downloadLink.href = url;
    downloadLink.download = "query.csv";

    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}


window.addQuery = function (containerId, containerVersion, projectId) {
    var query = $.trim($("#sparql-query").val());
    var queryname = $.trim($("#sparql-query-name").val());
    var applyInference = $("#chkApplyInference").is(":checked");

    $.ajax({
        type: "POST", //rest Type
        url: BASE_URL + "/Container/PostContainerQuery",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            applyInference: applyInference,
            query: query,
            queryName: queryname
        },
        async: true,
        success: function (result) {
            $(`#modalAddQuery`).modal('hide');
            window.loadSPARQL(projectId, containerId, containerVersion);
            window.toastr.success("SPARQL query has been added!", "SPARQL Query");
        },
        error: function (result) {
            $(`#modalAddQuery`).modal('hide');
            toastr.error("SPARQL query is not well formulated or a duplicate. Please try again.", "SPARQL Query Error");
        }
    });
}

window.queryGuids = function (obj, zoom = true) {
    let guid = obj.innerText;
    for (let value of window.viewData.values()) { //For each Model Data
        let map = value.objectMap; //Label: GUID Map
        for (let elem in map) { //For each key value pair
            if (map[elem] == guid) {
                let selected = selectCheck(elem);
                appSelectElem(parseInt(elem), selected, zoom);
                return;
            }
        }
        toastr.info("GUID not found. Is the model in the viewer loaded?");
    }
}

window.queryGuidsFromJson = function (guidArray) {
    console.log(guidArray);
    window.resetCamera();
    guidArray.forEach(queryGuid);
};

window.queryGuid = function (guidString) {
    let guid = guidString;
    for (let value of window.viewData.values()) { //For each Model Data
        let map = value.objectMap; //Label: GUID Map
        for (let elem in map) {
            if (Object.prototype.hasOwnProperty.call(map, elem)) { //For each key value pair

                if (map[elem] === guid) {
                    let selected = selectCheck(elem);
                    appSelectElem(parseInt(elem), selected, true);
                    return;
                }
            }
        }
        toastr.info("GUIDs not found. Is the model in the viewer loaded?");
    }
}

function createGuid() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

window.toggleExplorer = function (visible) {
    if (visible) {
        $("#hExplorer").html("Explorer");
        $("#divExplorer").addClass("col-md-3");
        $("#divExplorer").removeClass("col-md-1");
        $("#divContent").addClass("col-md-5");
        $("#divContent").removeClass("col-md-6");
        $("#divProperties").addClass("col-md-4");
        $("#divProperties").removeClass("col-md-5");
    }
    else {
        $("#hExplorer").html("Expl...");
        $("#divExplorer").addClass("col-md-1");
        $("#divExplorer").removeClass("col-md-3");
        $("#divContent").addClass("col-md-6");
        $("#divContent").removeClass("col-md-5");
        $("#divProperties").addClass("col-md-5");
        $("#divProperties").removeClass("col-md-4");
    }
};

window.resizeViewer = function (obj) {

    if (obj.checked) {
        $("#viewer-container").css("width", "90%");
        $("#viewer-container").height(550);
        $("#xBIM-viewer").height(550);
        $("#xBIM-viewer").addClass("border-2 border-dark");

        $("#placeholder-container").css("width", "100%");
        $("#placeholder-container").css("height", "600px");
        $("#viewer-container").css("position", "fixed");
        $("#viewer-container").css("right", "25px");
        window.viewer.start();
        window.viewer.zoomTo();
    }
    else {
        $("#xBIM-viewer").height(400);
        $("#xBIM-viewer").removeClass("border-2 border-dark");
        $("#viewer-container").height(415);
        $("#viewer-container").css("width", "100%");
        $("#viewer-container").css("position", "");
        $("#placeholder-container").css("width", "0");
        $("#placeholder-container").css("height", "0");
        window.viewer.start();
        window.viewer.zoomTo();
    }
}

window.loadDbAsPayloadTriples = function (projectId, containerId, containerVersion, contentId) {
    $("#taskbar").append('<button id="task-' + contentId + '-triples" class="btn btn-light btn-sm shadow mx-1"><span class="spinner-border spinner-border-sm text-rub-blue small" role="status"></span><span class="text-rub-blue small mx-2">Database task in process</span><span class="oi oi-circle-x"></span></button>');
    $.ajax({
        type: "GET", //rest Type
        url: BASE_URL + "/Container/ConvertDbAsTriples",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            contentId: contentId
        },
        success: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.toastr.success("Converted " + result + " datasets.", "R2RML converter");
            $("#task-" + contentId + "-triples").remove();
        },
        error: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.toastr.error("Error during conversion!", "R2RML converter");
            $("#task-" + contentId + "-triples").remove();
        }
    });
}

window.deleteOntology = function (projectId, containerId, containerVersion, ontologyName) {
    $("#divContainerTree").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');

    $.ajax({
        type: "POST", //rest Type
        url: BASE_URL + "/Container/DeleteOntology",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            ontologyName: ontologyName
        },
        success: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.loadIndex(projectId, containerId, containerVersion);
            window.toastr.success(ontologyName + " deleted!", "Ontology Resources");
        },
        error: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.toastr.error("Error deleting ontology!", "Ontology Resources");
        }
    });
}

window.deletePayloadTriples = function (projectId, containerId, containerVersion, payloadName) {
    $("#divContainerTree").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerContent").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');
    $("#divContainerProperties").html('<div class="spinner-border text-rub-blue" role="status"><span class= "visually-hidden">Querying...</span></div>');

    $.ajax({
        type: "POST", //rest Type
        url: BASE_URL + "/Container/DeletePayloadTriples",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            payloadName: payloadName
        },
        success: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.loadIndex(projectId, containerId, containerVersion);
            window.toastr.success(payloadName + " deleted!", "Payload triples");
        },
        error: function (result) {
            window.loadTree(projectId, containerId, containerVersion);
            window.toastr.error("Error deleting payload!", "Payload triples");
        }
    });
}

window.updateContent = function (projectId, containerId, containerVersion, contentId, containerInternalId) {

    var formInputs = $("#Form-" + contentId + "-" + containerInternalId).find(":input");
    var isValid = true;

    for (var i = 0; i < formInputs.length; i++) {
        if (!formInputs[i].checkValidity()) {
            formInputs[i].reportValidity();
            isValid = false;
            break;
        }
    }

    if (isValid) {

        var jsonFormObject = $("#Form-" + contentId + "-" + containerInternalId).serializeArray(), contentMetadataUpdate = {};

        $(jsonFormObject).each(function (i, field) {
            contentMetadataUpdate[field.name] = field.value;
        });

        $.ajax({
            type: "POST", //rest Type
            url: BASE_URL + "/Container/UpdateContent",
            data: {
                containerId: containerId,
                projectId: projectId,
                containerVersion: containerVersion,
                contentId: contentId,
                contentMetadataUpdate: contentMetadataUpdate
            },
            success: function (result) {
                window.loadDocument(projectId, containerId, containerVersion, contentId);
                window.loadTree(projectId, containerId, containerVersion);
                window.toastr.success("Document has been successfully updated!", "Document");
            },
            error: function (result) {
                window.loadTree(projectId, containerId, containerVersion);
                window.loadDocument(projectId, containerId, containerVersion, contentId);
                window.toastr.error("Error updating document!", "Document");
            }
        });

    }
}

window.addRequestedDocument = function (projectId, containerId, containerVersion, contentId) {
    $("#newMimeTypeMessage-" + contentId).text("");
    $("#newFileExtMessage-" + contentId).text("");

    var formInputs = $("#reqDoc-" + contentId).find(":input");
    var isValid = true;

    for (var i = 0; i < formInputs.length; i++) {
        if (!formInputs[i].checkValidity()) {
            formInputs[i].reportValidity();
            isValid = false;
            break;
        }
    }

    if (isValid) {
        var formData = new FormData($("#reqDoc-" + contentId)[0]);
        var changeTypeCheck = $('#changeType').is(':checked');
        formData.append("changeTypeString", changeTypeCheck.toString());
        var formDataFile = formData.get("uploadFile");
        var mimeTypeInput = formData.get("newMimeType");
        var fileExtInput = formData.get("newFileExt");

        var checkMimeType = formDataFile.type != mimeTypeInput;
        var checkFileExt = formDataFile.name.split('.').pop() != fileExtInput;

        if (checkMimeType && checkFileExt) {
            $("#newMimeTypeMessage-" + contentId).text("Mime type does not match input file.");
            $("#newFileExtMessage-" + contentId).text("File extension does not match input file.");
        }
        else if (checkMimeType) {
            $("#newMimeTypeMessage-" + contentId).text("Mime type does not match input file.");
        }
        else if (checkFileExt) {
            $("#newFileExtMessage-" + contentId).text("File extension does not match input file.");
        }
        else {
            $.ajax({
                type: "POST", //rest Type
                url: BASE_URL + "/Container/AddRequestedDocument",
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    $('#modalAddRequestedDocument-' + contentId).modal('hide');
                    window.loadDocument(projectId, containerId, containerVersion, contentId);
                    window.loadTree(projectId, containerId, containerVersion);
                    window.toastr.success("Document has been successfully uploaded!", "Document");
                },
                error: function (result) {
                    window.loadTree(projectId, containerId, containerVersion);
                    window.loadDocument(projectId, containerId, containerVersion, contentId);
                    window.toastr.error("Error uploading document!", "Document");
                }
            });
        }
    }
}

window.suggestMimeTypeAndFileExt = function (event, contentId) {
    $("#newMimeTypeSuggestion-" + contentId).text("Suggested mime type: " + event.files[0].type);
    $("#newFileExtSuggestion-" + contentId).text("Suggested file extension: " + event.files[0].name.split('.').pop());
}

window.console.log("The 'Container' bundle has been loaded!");