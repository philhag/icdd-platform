
// xBim Import
//import '@xbim/viewer';
import { Viewer, Product, State, ViewType, RenderingMode, ProductType, NavigationCube } from "@xbim/viewer";
window.selected = [] /// Array for Selected Item Model Numbers
window.formerSelected = []
window.modelIds = [] /// Array for ModelIds
window.models = []; /// Array with Names of the different loaded Models
window.modelMap = new Map();
window.viewData = new Map();

var check = Viewer.check();
if (check.noErrors) {
    window.viewer = new Viewer("xBIM-viewer");
    $(function () {
        window.viewer.on("loaded",
            (args) => {
                if (!(args.model in window.modelIds)) {
                    window.modelIds.push(args.model);
                }
                window.viewer.show(ViewType.DEFAULT);
                window.viewer.start();
            });
        const cube = new NavigationCube();
        window.viewer.show(ViewType.DEFAULT);
        window.viewer.addPlugin(cube);
    });

    window.viewer.on('pick',
        function (args) {
            if (args == null || args.id == null) {
                return;
            }
            //you can use ID for funny things like hiding or
            //recolouring which will be covered in one of the next tutorials
            var id = args.id;
            var modelId = args.model;
            var guid = window.getGuid(modelId, id);

            let selected = selectCheck(id);
            appSelectElem(id, selected, false);
        });

    window.viewer.on('dblclick',
        function (args) {
            if (args == null || args.id == null) {
                return;
            }
            window.viewer.zoomTo(args.id);
        });

    window.viewer.on('contextmenu',
        function (args) {
            if (document.getElementById("contextMenu").style.display == "block") {
                hideMenu();
            } else {
                var menu = document.getElementById("contextMenu")
                menu.style.display = 'block';
                menu.style.left = args.event.layerX + "px";
                menu.style.top = args.event.layerY + "px";
            }
            document.getElementById("contextMenuCopyGuid").onclick = function () { copyElementGuid(args); };
            addQueryButtonFunction(args);
        });

    document.onclick = hideMenu;
    function hideMenu() {
        document.getElementById("contextMenu")
            .style.display = "none"
    }
    window.addQueryButtonFunction = function (args) {
        let guid;
        for (let i = 1; i <= modelMap.size; i++) {
            if (viewData.get(modelMap.get(i)).objectMap[args.id]) {
                guid = viewData.get(modelMap.get(i)).objectMap[args.id]
            }
        }
        let query = `SELECT ?documentName ?identifier
WHERE
{
    ?linkIdentifier ls:identifier \'` + guid + `\' .
    ?linkIdentifier lse:linked ?other .
    ?other ls:identifier ?identifier .
    ?linkElement ls:hasIdentifier ?other.
    ?linkElement ls:hasDocument ?document.
    ?document ct:name ?documentName.
}
`
        document.getElementById("contextMenuGenerateSPARQL").onclick = function () { loadSPARQL(window.project, window.containerMeta, window.containerVersion, query) };
    }

    window.copyElementGuid = function (args) {
        var modelId = 0;
        var loadedModelId = 0;
        for (let i = 1; i <= modelMap.size; i++) {
            if (viewData.get(modelMap.get(i)).objectMap[args.id]) {
                modelId = modelMap.get(i);
                navigator.clipboard.writeText(viewData.get(modelMap.get(i)).objectMap[args.id]).then(function () { console.log("Copied") }, function (err) { console.error(err) });
                toastr.info("GUID " + viewData.get(modelMap.get(i)).objectMap[args.id] + " from Model " + args.model + " copied to clipboard.");
            }
        }
    }
    //Returns true if the label was in selected, false otherwise
    window.selectCheck = function (label) {
        label = parseInt(label);
        if (!(selected.includes(label))) {
            selected.push(label);
            window.viewer.setState(State.HIGHLIGHTED, [label]);
            return false;
        }
        else {
            selected = selected.filter(elem => elem !== label);
            window.viewer.setState(State.UNSTYLED, [label]);
            return true;
        }
    }

    window.appSelectElem = function (elem, selected, zoom = true) {
        //Select in Viewer already done through highlighting from selectCheck function
        //Select in Content View:
        elem = parseInt(elem);
        var elemGuid = undefined;
        for (let value of window.viewData.values()) {
            let map = value.objectMap;
            elemGuid = map[elem] !== undefined ? map[elem] : undefined;
            if (elemGuid != undefined)
                break;
        }
        let button;
        $(".guidButton").each(function () {
            let guid = this.innerText;
            for (let value of window.viewData.values()) {
                let map = value.objectMap;
                if (map[elem] == guid) {
                    button = this;
                    return;
                    
                }
            }
        });
        if (selected) {
            $(button).removeAttr("selected");
            $(button).find("span").addClass("oi-eye");
            $(button).find("span").removeClass("oi-circle-check text-success");
        }
        else {
            $(button).attr("selected", true);
            $(button).find("span").removeClass("oi-eye");
            $(button).find("span").addClass("oi-circle-check text-success");
        }

        //Select in Query:
        let row;
        $(".guidRow").each(function (i, obj) {
            let guid = this.innerText;
            for (let value of window.viewData.values()) {
                let map = value.objectMap;
                if (map[elem] == guid) {
                    row = obj;
                    if (selected) {
                        $(row).removeAttr("selected");
                        $(row).removeClass("bg-rub-green text-success");
                        $(row).children().last().remove();
                    }
                    else {
                        $(row).attr("selected", true);
                        $(row).addClass("bg-rub-green text-success");
                        $(row).append('<span class="oi oi-circle-check text-success px-2"></span>');
                    }
                    return;

                }
            }
        });

        if (!selected && zoom)
            window.viewer.zoomTo(parseInt(elem));
        selectTable(elemGuid, elem, selected)

    }

    window.selectTable = function(guid, label, selected){
        if (selected)
            $("#label-" + label).remove();
        else
            $("#selectedGuidTable-" + modelMap.get(getModelByLabel(label)) + " > tbody").append($("<tr id='label-" + label + "'><td>" + label + "</td><td>" + guid + "</td></tr>"));
    }

    window.isolateSelected = function (obj) {
        let labels = [];
        viewData.forEach(elem => labels.push(Object.keys(elem.objectMap)));
        labels.flat();
        if (obj.checked) {
            viewer.renderingMode = RenderingMode.XRAY_ULTRA;
            window.viewer.addState(State.XRAYVISIBLE, selected);
        }
        else {
            viewer.renderingMode = RenderingMode.NORMAL;
            window.viewer.removeState(State.XRAYVISIBLE, selected);
        }
    }


    window.hideModel = function (id, checkbox) {
        let labels = Object.keys(viewData.get(modelMap.get(parseInt(id) + 1)).objectMap);
        if (!checkbox.checked) {
            window.viewer.setState(State.HIDDEN, labels, getModelByLabel(labels[0]));
            window.viewer.zoomTo();
            selected.forEach(element => appSelectElem(element, true));
            selected = [];
        }
        else
            window.viewer.setState(State.UNSTYLED, labels, getModelByLabel(labels[0]));
    };

    window.loadModel = function (model, guid) {
        window.viewer.load(model);
        window.viewer.start();
        $("#load-" + guid).attr("disabled", "");
        $("#model-id-" + guid).html("null");
    };

    window.getGuid = function (modelId, entityLabel) {
        if (window.viewData == undefined || window.modelMap == undefined)
            return null;
        if (modelId == undefined || entityLabel == undefined)
            return null;

        var contentId = window.modelMap.get(modelId);

        if (contentId == undefined)
            return null;
        var viewerJson = window.viewData.get(contentId);
        return viewerJson.objectMap[entityLabel];
    }

    window.selectAndZoom = function (contentid, guid) {
        $("#btnIfcView").trigger("click");// Select ifc viewer tab

        var viewerJson = window.viewData.get(contentid);
        if (viewerJson.objectMap == undefined)
            return;
        var label = getKeyByValue(viewerJson.objectMap, guid);
        let selected = selectCheck(label);
        appSelectElem(label, selected);
       
    }

    window.resetCam = function () {
        window.viewer.show(ViewType.DEFAULT);
    }

    window.getModelByContentId = function (contentid) {
        if (contentid == undefined || window.modelMap == undefined)
            return null;
        return getKeyByValue(modelMap, contentid);
    }

    window.getLabel = function (modelId, guid) {
        if (window.viewData == undefined || window.modelMap == undefined)
            return null;
        if (modelId == undefined || guid == undefined)
            return null;

        var contentId = window.modelMap.get(modelId);

        if (contentId == undefined)
            return null;
        var viewerJson = window.viewData.get(contentId);
        return getKeyByValue(viewerJson.objectMap, guid);
    }

    function getKeyByValue(object, value) {
        return Object.keys(object).find(key => object[key] === value);
    }

    window.getMap = function (modelId) {
        if (window.viewData == undefined || window.modelMap == undefined)
            return null;
        if (modelId == undefined)
            return null;

        var contentId = window.modelMap.get(modelId);

        if (contentId == undefined)
            return null;
        var viewerJson = window.viewData.get(contentId);
        return viewerJson.objectMap;

    }

    window.setNavigationMode = function (obj) {
        viewer.navigationMode = obj.value.toLowerCase();
    }

    window.resetCamera = function () {
        for (let j = 0; j <= models.length; j++) {
            for (let i = 0; i <= selected.length; i++) {
                appSelectElem(parseInt(selected[i]), true, false);
                window.viewer.setState(State.UNSTYLED, [selected[i]], models[j]);
            }
        }
        selected = [];
        window.viewer.show(ViewType.DEFAULT);
    }

    window.loadModelWithMapping = function (containerid, containerversion, contentid, projectid) {
        
            $("#model-id-" + contentid).html('<div class="spinner-border text-rub-green" role="status"><span class= "visually-hidden">Loading...</span></div>');
            $.ajax({
                type: "GET", //rest Type
                dataType: 'json',
                url: BASE_URL + "/Partials/Viewer",
                data: {
                    id: containerid,
                    contentId: contentid,
                    projId: projectid,
                    containerVersion: containerversion
                },
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    var json = JSON.parse(result);

                    if (window.viewData == undefined) {
                        window.viewData = new Map();
                    }
                    window.viewData.set(contentid, json);

                    var blob = baseToBlob(json.wexbim);
                    viewer.loadAsync(blob);
                    viewer.start();
                    var modelId = window.modelIds.length + 1;

                    if (window.modelMap == undefined) {
                        window.modelMap = new Map();
                    }
                    window.modelMap.set(modelId, contentid);

                    $("#model-id-" + contentid).html(contentid + "<br>");
                    $("#div-loading-model").hide();
                }
            });
        
    };

    window.getLast = (arr = null, n = null) => {
        if (arr == null) return void 0;
        if (n === null) return arr[arr.length - 1];
        return arr.slice(Math.max(arr.length - n, 0));
    };

    window.resetModel = function (model) {
        let mapKeys = Object.keys(mapping);
        for (let i = 0; i < mapKeys.length; i++) {
            window.viewer.setState(State.UNSTYLED, [parseInt(mapKeys[i])], model);
        }
    }


    window.getModelByLabel = function (label) {
        for (let i = 0; i <= modelIds.length; i++) {
            if (window.viewer.isProductInModel(label, modelIds[i])) {
                return modelIds[i];
            }
        }
    }


    window.baseToBlob = function (data) {

        var byteString = atob(data);

        // write the bytes of the string to an ArrayBuffer
        var ab = new ArrayBuffer(byteString.length);
        var ia = new Uint8Array(ab);
        for (var i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        return new Blob([ia]);
    }
    console.log("The 'viewer' bundle containing 'window.viewer' has been loaded!");
}
else {
    for (var i in check.errors) {
        console.log("The 'viewer' bundle containing 'window.viewer' could not been loaded!");
        var error = check.errors[i];
        console.log(error);
    }
}
