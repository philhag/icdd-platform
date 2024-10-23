var leftDirectedLinkElements = [];
var rightDirectedLinkElements = [];
var rightDirected1NLinkElements = [];

window.isValidForm = function (linksetid, side, linktype) {
    var formInputs = $("#" + side + "Form" + linktype + "-" + linksetid).find(":input");
    var firstInvalid = "";
    var isURI = true;
    for (var i = 0; i < formInputs.length; i++) {
        if (formInputs[i].value == '' && formInputs[i].hasAttribute('required')) {
            firstInvalid = formInputs[i].id;
            document.getElementById(formInputs[i].id).reportValidity();
            break;
        }
        if (formInputs[i].type == "url" && formInputs[i].hasAttribute('required')) {
            try {
                url = new URL(formInputs[i].value);
            } catch (_) {
                isURI = false;
            }
            if (!isURI) {
                document.getElementById(formInputs[i].id).setCustomValidity('Please enter a valid URI.');
                document.getElementById(formInputs[i].id).reportValidity();
            }
            break;
        }
    }
    return (firstInvalid == "" && isURI);
}

window.CreateLinkElement = function (linksetid, side, linktype, multiple) {
    var newIdentifier = {};
    var elements = document.getElementById(side + "Elements" + linktype + "-" + linksetid);
    var doc = document.getElementById(side + "Document" + linktype + "-" + linksetid);
    var idType = document.getElementById(side + "IdType" + linktype + "-" + linksetid).value;
    var identifier = document.getElementById(side + "Identifier" + linktype + "-" + linksetid).value;
    var identifierField = document.getElementById(side + "IdentifierField" + linktype + "-" + linksetid).value;
    var uri = document.getElementById(side + "Uri" + linktype + "-" + linksetid).value;
    var queryExpression = document.getElementById(side + "QueryExpression" + linktype + "-" + linksetid).value;
    var queryLanguage = document.getElementById(side + "QueryLanguage" + linktype + "-" + linksetid).value;

    var newId = uuidv4()

    if (idType == "1") {
        newIdentifier["type"] = idType;
        newIdentifier["identifier"] = identifier;
        newIdentifier["identifierField"] = identifierField;

        if (multiple) {
            var newLi = document.createElement("li");
            newLi.id = newId;
            newLi.innerHTML = doc.children[doc.selectedIndex].textContent + " [" + identifier + ", " + identifierField + "]" + "<button class=\"btn btn-sm btn-link tab-clicker\" onclick=\"DeleteLinkElement(this.parentNode.parentNode, this.parentNode)\"><span class=\"oi oi-x\"></span></button>";
            elements.appendChild(newLi);
        }
    } else if (idType == "2") {
        newIdentifier["type"] = idType;
        newIdentifier["uri"] = uri;

        if (multiple) {
            var newLi = document.createElement("li");
            newLi.id = newId;
            newLi.innerHTML = doc.children[doc.selectedIndex].textContent + " [" + uri + "]" + "<button class=\"btn btn-sm btn-link tab-clicker\" onclick=\"DeleteLinkElement(this.parentNode.parentNode, this.parentNode)\"><span class=\"oi oi-x\"></span></button>";
            elements.appendChild(newLi);
        }
    } else if (idType == "3") {
        newIdentifier["type"] = idType;
        newIdentifier["queryExpression"] = queryExpression;
        newIdentifier["queryLanguage"] = queryLanguage;

        if (multiple) {
            var newLi = document.createElement("li");
            newLi.id = newId;
            newLi.innerHTML = doc.children[doc.selectedIndex].textContent + " [" + queryExpression + ", " + queryLanguage + "]" + "<button class=\"btn btn-sm btn-link tab-clicker\" onclick=\"DeleteLinkElement(this.parentNode.parentNode, this.parentNode)\"><span class=\"oi oi-x\"></span></button>";
            elements.appendChild(newLi);
        }
    } else if (idType == "0" && multiple) {
        var newLi = document.createElement("li");
        newLi.id = newId;
        newLi.innerHTML = doc.children[doc.selectedIndex].textContent + " [-]" + "<button class=\"btn btn-sm btn-link tab-clicker\" onclick=\"DeleteLinkElement(this.parentNode.parentNode, this.parentNode)\"><span class=\"oi oi-x\"></span></button>";
        elements.appendChild(newLi);
    }

    var newLinkElem = {
        ["tempId"]: newId,
        ["hasDocument"]: doc.value,
        ["hasIdentifier"]: newIdentifier
    };
    return newLinkElem;
}

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

window.DeleteLinkElement = function (linkElementParent, linkElement) {
    linkElementParent.removeChild(linkElement);

    if (linkElementParent.id.startsWith("rightElementsDir1N")) {
        let index = 0;
        for (var i = 0, len = rightDirected1NLinkElements.length; i < len; i++) {
            if (rightDirected1NLinkElements[i]['tempId'] == linkElement.id) {
                index = i;
                break;
            }
        }
        rightDirected1NLinkElements.splice(index, 1);
    }
    else if (linkElementParent.id.startsWith("leftElementsDir")) {
        let index = 0;
        for (var i = 0, len = leftDirectedLinkElements.length; i < len; i++) {
            if (leftDirectedLinkElements[i]['tempId'] == linkElement.id) {
                index = i;
                break;
            }
        }
        leftDirectedLinkElements.splice(index, 1);
    }
    else if (linkElementParent.id.startsWith("rightElementsDir")) {
        let index = 0;
        for (var i = 0, len = rightDirectedLinkElements.length; i < len; i++) {
            if (rightDirectedLinkElements[i]['tempId'] == linkElement.id) {
                index = i;
                break;
            }
        }
        rightDirectedLinkElements.splice(index, 1);
    }
}

window.AddLinkElement = function (linksetid, side, linktype) {
    var isValid = isValidForm(linksetid, side, linktype);
    if (isValid) {
        var newLinkElem = CreateLinkElement(linksetid, side, linktype, true);

        if (side == "left" && linktype == "Dir")
            leftDirectedLinkElements.push(newLinkElem);
        else if (side == "right" && linktype == "Dir")
            rightDirectedLinkElements.push(newLinkElem);
        else if (side == "right" && linktype == "Dir1N")
            rightDirected1NLinkElements.push(newLinkElem);
    }
}

window.GetLinkSpecialization = function (linksetid, linktype) {
    return document.getElementById("spec" + linktype + "-" + linksetid).value;
}

window.PostBinaryLink = function (projectId, containerId, containerVersion, linksetId) {
    var isValidLeft = isValidForm(linksetId, "left", "Bin");
    var isValidRight = isValidForm(linksetId, "right", "Bin");
    if (isValidLeft && isValidRight) {
        var leftBinaryElement = CreateLinkElement(linksetId, "left", "Bin", false);
        var rightBinaryElement = CreateLinkElement(linksetId, "right", "Bin", false);

        var allLinkElements = {
            ["leftElement"]: leftBinaryElement,
            ["rightElement"]: rightBinaryElement
        };

        var jsonLinkElements = JSON.stringify(allLinkElements);

        $.ajax({
            type: 'GET',
            url: BASE_URL + '/Container/AddBinaryLink',
            data: {
                projectId: projectId,
                containerId: containerId,
                containerVersion: containerVersion,
                linksetId: linksetId,
                jsonLinkElements: jsonLinkElements
            },
            async: true,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(`#modal-binary-${linksetId}`).modal('hide');
                window.loadLinkset(projectId, containerId, containerVersion, linksetId);
                window.toastr.success("Binary link created!", "Linkset");
            },
            error: function (error) {
                toastr.error("Could not create link, please try again.");
            }
        });
    }
}

window.PostDirectedLink = function (projectId, containerId, containerVersion, linksetId) {

    var allLinkElements = {
        ["leftElements"]: leftDirectedLinkElements,
        ["rightElements"]: rightDirectedLinkElements
    };

    var jsonLinkElements = JSON.stringify(allLinkElements);

    $.ajax({
        type: 'GET',
        url: BASE_URL + '/Container/AddDirectedLink',
        data: {
            projectId: projectId,
            containerId: containerId,
            containerVersion: containerVersion,
            linksetId: linksetId,
            jsonLinkElements: jsonLinkElements
        },
        async: true,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            $(`#modal-directed-${linksetId}`).modal('hide');
            window.loadLinkset(projectId, containerId, containerVersion, linksetId);
            window.toastr.success("Directed link created!", "Linkset");
        },
        error: function (error) {
            toastr.error("Could not create link, please try again.");
        }
    });
}



window.PostDirectedBinaryLink = function (projectId, containerId, containerVersion, linksetId) {
    var isValidLeft = isValidForm(linksetId, "left", "DirBin");
    var isValidRight = isValidForm(linksetId, "right", "DirBin");
    if (isValidLeft && isValidRight) {
        var leftBinaryElement = CreateLinkElement(linksetId, "left", "DirBin", false);
        var rightBinaryElement = CreateLinkElement(linksetId, "right", "DirBin", false);
        var specialization = GetLinkSpecialization(linksetId, "DirBin");

        var allLinkElements = {
            ["leftElement"]: leftBinaryElement,
            ["rightElement"]: rightBinaryElement,
            ["specialization"]: specialization
        };

        var jsonLinkElements = JSON.stringify(allLinkElements);

        $.ajax({
            type: 'GET',
            url: BASE_URL + '/Container/AddDirectedBinaryLink',
            data: {
                projectId: projectId,
                containerId: containerId,
                containerVersion: containerVersion,
                linksetId: linksetId,
                jsonLinkElements: jsonLinkElements
            },
            async: true,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(`#modal-directedbinary-${linksetId}`).modal('hide');
                window.loadLinkset(projectId, containerId, containerVersion, linksetId);
                window.toastr.success("Directed-binary link created!", "Linkset");
            },
            error: function (error) {
                toastr.error("Could not create link, please try again.");
            }
        });
    }
}

window.PostDirected1NLink = function (projectId, containerId, containerVersion, linksetId) {
    var isValid = isValidForm(linksetId, "left", "Dir1N");
    if (isValid) {
        var leftDirected1NElement = CreateLinkElement(linksetId, "left", "Dir1N", false);
        var specialization = GetLinkSpecialization(linksetId, "Dir1N");

        var allLinkElements = {
            ["leftElement"]: leftDirected1NElement,
            ["rightElements"]: rightDirected1NLinkElements,
            ["specialization"]: specialization
        };

        var jsonLinkElements = JSON.stringify(allLinkElements);

        $.ajax({
            type: 'GET',
            url: BASE_URL + '/Container/AddDirected1ToNLink',
            data: {
                projectId: projectId,
                containerId: containerId,
                containerVersion: containerVersion,
                linksetId: linksetId,
                jsonLinkElements: jsonLinkElements
            },
            async: true,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $(`#modal-directed1ton-${linksetId}`).modal('hide');
                window.loadLinkset(projectId, containerId, containerVersion, linksetId);
                window.toastr.success("Directed 1-to-n link created!", "Linkset");
            },
            error: function (error) {
                toastr.error("Could not create link, please try again.");
            }
        });
    }
}

window.ResetForms = function (linksetid) {
    leftDirectedLinkElements = [];
    rightDirectedLinkElements = [];
    rightDirected1NLinkElements = [];

    $("#leftElementsDir-" + linksetid).html("");
    $("#rightElementsDir-" + linksetid).html("");
    $("#rightElementsDir1N-" + linksetid).html("");

    var textInputs = $(".idReset").find(":input[type=text], :input[type=url]");
    for (var i = 0; i < textInputs.length; i++) {
        textInputs[i].value = "";
        textInputs[i].innerText = "";
    }

    var selectInputs = $(".idReset").find("select");
    for (var i = 0; i < selectInputs.length; i++) {
        selectInputs[i].selectedIndex = 0;
    }

    var linktypes = ["Dir", "Dir1N", "Bin", "DirBin"];
    for (var i = 0; i < linktypes.length; i++) {
        toggleCheckLink("1", linksetid, "left", linktypes[i]);
        toggleCheckLink("1", linksetid, "right", linktypes[i]);
    }
}

window.toggleCheckLink = function (idType, linksetid, side, linktype) {
    if (idType == "1") {
        $("#" + side + "StringBased" + linktype + "-" + linksetid).show();
        $("#" + side + "Identifier" + linktype + "-" + linksetid).prop("required", true);
        $("#" + side + "IdentifierField" + linktype + "-" + linksetid).prop("required", true);

        $("#" + side + "UriBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Uri-" + linksetid).prop("required", false);

        $("#" + side + "QueryBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "QueryExpression" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "QueryLanguage" + linktype + "-" + linksetid).prop("required", false);

    } else if (idType == "2") {
        $("#" + side + "UriBased" + linktype + "-" + linksetid).show();
        $("#" + side + "Uri" + linktype + "-" + linksetid).prop("required", true);

        $("#" + side + "StringBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Identifier" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "IdentifierField" + linktype + "-" + linksetid).prop("required", false);

        $("#" + side + "QueryBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "QueryExpression" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "QueryLanguage" + linktype + "-" + linksetid).prop("required", false);

    } else if (idType == "3") {
        $("#" + side + "QueryBased" + linktype + "-" + linksetid).show();
        $("#" + side + "QueryExpression" + linktype + "-" + linksetid).prop("required", true);
        $("#" + side + "QueryLanguage" + linktype + "-" + linksetid).prop("required", true);

        $("#" + side + "StringBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Identifier" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "IdentifierField" + linktype + "-" + linksetid).prop("required", false);

        $("#" + side + "UriBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Uri" + linktype + "-" + linksetid).prop("required", false);
    } else {
        $("#" + side + "StringBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Identifier" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "IdentifierField" + linktype + "-" + linksetid).prop("required", false);

        $("#" + side + "UriBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "Uri" + linktype + "-" + linksetid).prop("required", false);

        $("#" + side + "QueryBased" + linktype + "-" + linksetid).hide();
        $("#" + side + "QueryExpression" + linktype + "-" + linksetid).prop("required", false);
        $("#" + side + "QueryLanguage" + linktype + "-" + linksetid).prop("required", false);
    }
}

window.updateLinkset = function (projectId, containerId, containerVersion, linksetId, containerInternalId) {

    var formInputs = $("#Form-" + linksetId + "-" + containerInternalId).find(":input");
    var isValid = true;

    for (var i = 0; i < formInputs.length; i++) {
        if (!formInputs[i].checkValidity()) {
            formInputs[i].reportValidity();
            isValid = false;
            break;
        }
    }

    if (isValid) {
        var jsonFormObject = $("#Form-" + linksetId + "-" + containerInternalId).serializeArray(), linksetMetadataUpdate = {};

        $(jsonFormObject).each(function (i, field) {
            linksetMetadataUpdate[field.name] = field.value;
        });
        console.log(JSON.stringify(jsonFormObject));

        $.ajax({
            type: "POST", //rest Type
            url: BASE_URL + "/Container/UpdateLinkset",
            data: {
                containerId: containerId,
                projectId: projectId,
                containerVersion: containerVersion,
                linksetId: linksetId,
                linksetMetadataUpdate: linksetMetadataUpdate
            },
            success: function (result) {
                window.loadLinkset(projectId, containerId, containerVersion, linksetId);
                window.loadTree(projectId, containerId, containerVersion);
                window.toastr.success("Linkset has been successfully updated!", "Document");
            },
            error: function (result) {
                window.loadLinkset(projectId, containerId, containerVersion, linksetId);
                window.loadTree(projectId, containerId, containerVersion);
                window.toastr.error("Error updating document!", "Document");
            }
        });
    }
}

window.deleteLink = function (projectId, containerId, containerVersion, linksetId, linkId) {

    $.ajax({
        type: "POST", //rest Type
        url: BASE_URL + "/Container/DeleteLink",
        data: {
            containerId: containerId,
            projectId: projectId,
            containerVersion: containerVersion,
            linksetId: linksetId,
            linkId: linkId
        },
        success: function (result) {
            $(`#modalDeleteLink-${linkId}`).modal('hide');
            window.loadLinkset(projectId, containerId, containerVersion, linksetId);
            window.toastr.success("Link deleted!", "Linkset");
        },
        error: function (result) {
            $(`#modalDeleteLink-${linkId}`).modal('hide');
            window.toastr.error("Link could not be deleted!", "Linkset");
        }
    });

};

window.deleteLinkset = function (projectId, containerId, containerVersion, linksetId) {
    $.ajax({
        type: 'GET',
        url: BASE_URL + '/Container/DeleteLinkset',
        data: {
            projectId: projectId,
            containerId: containerId,
            containerVersion: containerVersion,
            linksetId: linksetId
        },
        async: true,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            $(`#modalDeleteLinkset-${linksetId}`).modal('hide');
            window.loadIndex(projectId, containerId, containerVersion);
            window.loadTree(projectId, containerId, containerVersion);
            window.toastr.success("Linkset has been deleted", "Linkset");
        },
        error: function (error) {
            toastr.error("Could not delete linkset, please try again.");
        }
    });
}

window.importIFCGuid = function (linksetId, side, linkType, hasElement) {

    var ifcDocumentGuid = $("#" + side + "DocumentBin-" + linksetId).val();

    var ifcGuids = [];
    $("#selectedGuidTable-" + ifcDocumentGuid + " > tbody > tr").each(function () { ifcGuids.push($(this).find("td:last").text()) });

    if (ifcGuids.length != 1) {
        toastr.error("Please select exactly one ID in the viewer.");
    }
    else {
        let ifcGuid = ifcGuids[0];

        // If link element needs to be created
        if (hasElement) {
            var newIdentifier = {};
            var elements = document.getElementById(side + "Elements" + linkType + "-" + linksetId);

            newIdentifier["type"] = "1";
            newIdentifier["identifier"] = ifcGuid;
            newIdentifier["identifierField"] = "GUID";

            var documentIndexName = "";

            Array.from(document.querySelector("#" + side + "Document" + linkType + "-" + linksetId).options).forEach(function (elem) {
                if (elem.value == ifcDocumentGuid)
                    documentIndexName = elem.text;
            });

            var newLi = document.createElement("li");
            newLi.textContent = documentIndexName + " [" + newIdentifier["identifier"] + ", " + newIdentifier["identifierField"] + "]";
            elements.appendChild(newLi);

            var newLinkElem = {
                ["hasDocument"]: ifcDocumentGuid,
                ["hasIdentifier"]: newIdentifier
            };

            if (side == "left" && linkType == "Dir")
                leftDirectedLinkElements.push(newLinkElem);
            else if (side == "right" && linkType == "Dir")
                rightDirectedLinkElements.push(newLinkElem);
            else if (side == "right" && linkType == "Dir1N")
                rightDirected1NLinkElements.push(newLinkElem);
        }
        else {
            var idTypeSelectInput = document.getElementById(side + "IdType" + linkType + "-" + linksetId);
            idTypeSelectInput.value = "1";
            toggleCheckLink("1", linksetId, side, linkType);
            var documentSelectInput = document.getElementById(side + "Document" + linkType + "-" + linksetId);
            documentSelectInput.value = ifcDocumentGuid;
            var identifierInput = document.getElementById(side + "Identifier" + linkType + "-" + linksetId);
            identifierInput.value = ifcGuid;
            var identifierFieldInput = document.getElementById(side + "IdentifierField" + linkType + "-" + linksetId);
            identifierFieldInput.value = "GUID";
        }
    }
}

window.console.log('The \'Linkset\' bundle has been loaded!');