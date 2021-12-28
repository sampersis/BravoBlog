// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// JavaScript for Post/Create and Edit Pages

let tagIndex = 0;
let tags = [];
function CreateTag() {

    let none = "none";
    let space = ' ';
    let hash = '#';

    // Get the value of CheckBox Tag Labels and the value of the Input field
    var firstTag = document.querySelector("#first-tag").value; console.log("firstTag: " + firstTag);
    var secondTag = document.querySelector("#second-tag").value; console.log("secondTag: " + secondTag);
    var thirdTag = document.querySelector("#third-tag").value; console.log("thirdTag: " + thirdTag);
    var fourthTag = document.querySelector("#fourth-tag").value; console.log("fourthTag: " + fourthTag);
    var fifthTag = document.querySelector("#fifth-tag").value; console.log("fifthTag: " + fifthTag);
    var tagStr = document.querySelector("#post-tag").value; console.log("tagStr: " + tagStr);

    // split the value of the input tag field if the words are seperated by space
    var tagString = tagStr.split(space); // console.log("tagString: " + tagString);
    tagStr = ""; // console.log("tagStr: " + tagStr);

    for (var i = 0; i < tagString.length; i++) {
        // Keep only alphanumeric characters
        tagString[i] = tagString[i].replace(/[^0-9a-z]/gi, ''); //console.log("tagString Length: " + tagString[i].length);
        if (tagString[i].length != 0) {
            tagStr += tagString[i].charAt(0).toUpperCase() + tagString[i].slice(1).toLowerCase();
        }
    }

    // Get and check whether the length of Tag is bigger than zero
    var tagStrLen = tagStr.length; // console.log("tagStringLen: " + tagStrLen);
    tagStr = hash + tagStr; // Add a # at the beginnig of the Tag

    for (var i = 0; i < tagIndex; i++) {
        // console.log("index: " + i + " tags: " + tags[i]);
        if (tags[i].toUpperCase() == tagStr.toUpperCase()) {
            alert("Dubplicate Tag!");
            tagStrLen = -1;
        }
    }

    if (tagStrLen > 0) {
        if (firstTag == none) {
            // console.log("firstTag check: " + firstTag);
            document.querySelector("#first-tag").value = tagStr; console.log("Inside 1st Tag: " + document.querySelector("#first-tag").value);
            document.querySelector("#first-tag").hidden = false;
            document.querySelector("#first-tag-input").hidden = false;
            document.querySelector("#remove-tag-btn").hidden = false;
            document.querySelector('#post-tag-count').innerText = 'Count (max. 50): 0';
        }
        else if (secondTag == none) {
            document.querySelector("#second-tag").value = tagStr; console.log("Inside 2nd Tag: " + document.querySelector("#second-tag").value);
            document.querySelector("#second-tag").hidden = false;
            document.querySelector("#second-tag-input").hidden = false;
            document.querySelector('#post-tag-count').innerText = 'Count (max. 50): 0';
        }
        else if (thirdTag == none) {
            document.querySelector("#third-tag").value = tagStr; console.log("Inside 3rd Tag: " + document.querySelector("#third-tag").value);
            document.querySelector("#third-tag").hidden = false;
            document.querySelector("#third-tag-input").hidden = false;
            document.querySelector('#post-tag-count').innerText = 'Count (max. 50): 0';
        }
        else if (fourthTag == none) {
            document.querySelector("#fourth-tag").value = tagStr; console.log("Inside 4th Tag: " + document.querySelector("#fourth-tag").value);
            document.querySelector("#fourth-tag").hidden = false;
            document.querySelector("#fourth-tag-input").hidden = false;
            document.querySelector('#post-tag-count').innerText = 'Count (max. 50): 0';
        }
        else if (fifthTag == none) {
            document.querySelector("#fifth-tag").value = tagStr; console.log("Inside 5th Tag: " + document.querySelector("#fifth-tag").value);
            document.querySelector("#fifth-tag").hidden = false;
            document.querySelector("#fifth-tag-input").hidden = false;
            document.querySelector('#post-tag-count').innerText = 'Count (max. 50): 0';
        }

        // Add to the list of existing tags
        if (tagIndex < 5) {
            // console.log("tagStr: " + tagStr);
            tags[tagIndex] = tagStr; // console.log("tags: " + tags[tagIndex]);
            tagIndex++; // console.log("Tag index: " + tagIndex);
        }
    }
    else if (tagStrLen == 0) {
        // An empty or only space string is not valid
        alert("Empty or whitespace tags cannot be created");
    }

    $("#tag-list input").each(function () {

        if ($(this).next().val() != none) {
            console.log("JQ: Tag value: " + $(this).next().val());
            $(this).show();
            $(this).next().show();
        }
    });

    document.querySelector("#post-tag").value = ""; // Reset the input where the tags are created
}

function RemoveTag() {

    // Reset the global values
    tags = [];
    tagIndex = 0;

    // Check number of checked checkbox
    var numberOfChecked = 0;
    var numberOfDelete = 0;

    $("#tag-list input").each(function () {

        if ($(this).is(":checked")) {
            numberOfChecked++;
        }
    });

    // If no tags were selected when the Remove Tag button were pressed then return an error message
    if (numberOfChecked == 0) {
        alert("No tag was selected!");
        return false;
    }
    else if (numberOfChecked > 0) { // Check which tags have been selected
        $("#tag-list input").each(function () {

            if ($(this).is(":checked")) {
                $(this).hide();
                $(this).prop('checked', false);
                $(this).next().hide();
                $(this).next().val("none");
            }
        });
    }

    // Find number of deleted tags
    $("#tag-list input").each(function () {
        if ($(this).next().val() == "none") { numberOfDelete++; }
    });

    //console.log("numberOfChecked: " + numberOfDelete);

    // If number of deleted tags are equal five (max number) then hide the Remove Button as well
    if (numberOfDelete == 5) {
        $("#remove-tag-btn").hide();
    }
}
