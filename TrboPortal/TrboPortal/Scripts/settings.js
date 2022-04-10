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
    const request = new Request("/TrboPortal/v1/system/settings");
    api.performApiCall(request, 'GET', null,
        (response) => response.json().then(json => {
            processGetResponse(json);
            $('#preloader').hide();
        }),
        (response) => showApiErrorWarning("#warning-generic", response));
}

/**
 * Expose SubmitClicked() to cshtml (to handle the dual-state Edit/Submit button click)
 * */
window.SubmitClicked = function SubmitClicked() {
    if (inEditMode) {
        // We're going to post, set submit text to back 'Edit', disable and post.
        $('#submit').html("Edit");
        $('#submit').prop('disabled', true);
        Post();
    } else {
        // We're going to edit, change submit text to 'Submit'
        $('#submit').html("Submit");
    }
    inEditMode = !inEditMode
    EnterEditMode(inEditMode);
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
    $("#SettingsForm input").prop("readonly", !invokeEditMode);
    $("#SettingsForm select").prop("disabled", !invokeEditMode);
}

/**
 * Posts validated form data to server.
 * */
function Post() {
    //TODO Validation
    const data = getJsonFromForm();

    const request = new Request("/TrboPortal/v1/system/settings");
    api.performApiCall(request, 'PATCH', data,
        (response) => {
            console.log("Patch successfull!")
            console.log(response);
            //Refresh to get latest data from server.
            RefreshData();
        },
        (response) => showApiErrorWarning("#warning-generic", response));
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
            Host: $('#CiaBataSettings-Host').val()
        },
    };
}

function setFormFromJson(json) {
    $('#ServerInterval').val(json['ServerInterval'])
    $('#DefaultGpsMode').val(json['DefaultGpsMode'])
    $('#DefaultInterval').val(json['DefaultInterval'])
    $('#TurboNetSettings-Host').val(json['TurboNetSettings']['Host'])
    $('#TurboNetSettings-Port').val(json['TurboNetSettings']['Port'])
    $('#TurboNetSettings-User').val(json['TurboNetSettings']['User'])
    $('#TurboNetSettings-Password').val(json['TurboNetSettings']['Password'])
    $('#CiaBataSettings-Host').val(json['CiaBataSettings']['Host'])
}

/**
 * Parse API error message and display into warningElement for 5 seconds
 */
function showApiErrorWarning(warningElement, response) {
    api.parseErrorMessagePromise(response).then((msg) => {
        $(warningElement).html('<i class="fas fa-exclamation-triangle"></i> ' + msg);
        $(warningElement).dequeue();
        $(warningElement).show(400);
        $(warningElement).delay(5000).hide(400).promise().done(function () {
            $(warningElement).text("");
        });
    });
}
