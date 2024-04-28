import * as api from './api.js';

// Edit/ReadOnly Flag.
var inEditMode = false;

// Initial data load.
jQuery(document).ready(function (_$) {
    RefreshData();
});

// Prevent full page refresh on submit. We're using a JavaScript method SubmitClicked instead.
// https://stackoverflow.com/a/19454346
$("#SettingsForm").submit(function (e) {
    e.preventDefault();
});

/**
 * Perform GET request to get values from server
 * */
function RefreshData() {
    api.performApiCall("system/settings", 'GET', null,
        (response) => response.json().then(json => {
            processGetResponse(json);
            $('#preloader').hide();
        }),
        (response) => Promise.resolve(response.text())
            .then(JSON.parse)
            .then(resp_data => showApiErrorWarning("Unable to load data", api.getValidationErrors(resp_data)))
    );
}

/**
 * Expose SubmitClicked() to cshtml (to handle the dual-state Edit/Submit button click)
 * */
window.SubmitClicked = function SubmitClicked() {
    if (inEditMode) {
        Post();
    } else {
        EnterEditMode(true);
    }
}

/**
 * Process GET request response. 
 * Populate form and enable Edit/Submit button (as retrieving latest values from server is done).
 * @param {any} json
 */
function processGetResponse(json) {
    console.log("Initial Settings received!");
    console.log(json);
    // Populate form
    setFormFromJson(json);
    // Enable Edit button
    $('#submit').prop('disabled', false);
}

/**
 * Sets Form in ReadOnly or EditMode depending on invokeEditMode flag
 * @param {any} invokeEditMode
 */
function EnterEditMode(invokeEditMode) {
    inEditMode = invokeEditMode
    if (inEditMode){
        $('#submit').html("Submit");
    } else {
        $('#submit').html("Edit");
    }
    $("#SettingsForm input").prop("readonly", !invokeEditMode);
    $("#SettingsForm select").prop("disabled", !invokeEditMode);
}

/**
 * Posts validated form data to server.
 * */
function Post() {
    // Disable input before posting
    $('#submit').prop('disabled', true);

    //TODO Validation
    const data = getJsonFromForm();

    api.performApiCall("system/settings", 'PATCH', data,
        (response) => {
            console.log("Patch successfull!")
            console.log(response);
            //Refresh to get latest data from server.
            EnterEditMode(false);
            clearErrorWarning();
            RefreshData();
        },
        (response) => {
            $('#submit').prop('disabled', false);
            Promise.resolve(response.text())
                .then(resp_data => showApiErrorWarning("Unable to save SystemSettings.", api.getResponseErrors(resp_data)));
        });
}

function getJsonFromForm() {
    return {
        ServerInterval: $('#ServerInterval').val(),
        DefaultGpsMode: $('#DefaultGpsMode :selected').val(),
        DefaultInterval: $('#DefaultInterval').val(),
        TurboNetSettings: {
            Host: $('#TurboNetSettings-Host').val(),
            Port: $('#TurboNetSettings-Port').val(),
            User: $('#TurboNetSettings-User').val(),
            Password: $('#TurboNetSettings-Password').val(),
        },
        CiaBataSettings: {
            Host: $('#CiaBataSettings-Host').val(),
            Token: $('#CiaBataSettings-Token').val(),
            Edition: $('#CiaBataSettings-Edition').val()
        },
        GoogleMapsApiKey: $("#GoogleMapsApiKey").val()
    };
}

function setFormFromJson(json) {
    if (json == null || typeof(json) == 'undefined') {
        // Probably empty DB
        console.log("Json undefined, is this an empty db?");
        return;
    }
    $('#ServerInterval').val(json['ServerInterval'])
    $('#DefaultGpsMode').val(json['DefaultGpsMode'])
    $('#DefaultInterval').val(json['DefaultInterval'])
    $('#TurboNetSettings-Host').val(json['TurboNetSettings']['Host'])
    $('#TurboNetSettings-Port').val(json['TurboNetSettings']['Port'])
    $('#TurboNetSettings-User').val(json['TurboNetSettings']['User'])
    $('#TurboNetSettings-Password').val(json['TurboNetSettings']['Password'])
    $('#CiaBataSettings-Host').val(json['CiaBataSettings']['Host'])
    $('#CiaBataSettings-Token').val(json['CiaBataSettings']['Token'])
    $('#CiaBataSettings-Edition').val(json['CiaBataSettings']['Edition'])
    $('#GoogleMapsApiKey').val(json['GoogleMapsApiKey'])
}

function showApiErrorWarning(message, errors) {
    $('#alertMessage').html(message);
    $('#validationErrorList').html(errors.map(e => `<li>${e}</li>\r\n`))
    $('#validationErrorContainer').show();

}

function clearErrorWarning() {
    $('#alertMessage').html("");
    $('#validationErrorList').html("")
    $('#validationErrorContainer').hide();
}

