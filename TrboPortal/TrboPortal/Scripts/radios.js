import * as api from './api.js';

// Prevent full page refresh on submit. We're using a JavaScript method SubmitClicked instead.
// https://stackoverflow.com/a/19454346
$("#radioEditForm").submit(function (e) {
    e.preventDefault();
});


/**
 * Posts validated form data to server.
 * */
window.deleteRadio = function deleteRadio(radioId) {
    const data = '';

    api.performApiCall(`radio?radioIds=${radioId}`, 'DELETE', data,
        (response) => {
            console.log(`Delete ${radioId} successfull!`)
            console.log(response);
            refreshData();
        },
        (response) => {
            Promise.resolve(response.text())
                .then(JSON.parse)
                .then(resp_data => api.showGenericErrors(`Unable to delete ${radioId}`, api.getValidationErrors(resp_data)));
        });
}

window.refreshGps = function refreshGps(radioId) {
    const data = null;

    api.performApiCall(`gps/update?radioIds=${radioId}`, 'GET', data,
        (response) => {
            console.log(`Refresh ${radioId} successfull!`)
            console.log(response);
            refreshData();
        },
        (response) => {
            Promise.resolve(response.text())
                .then(JSON.parse)
                .then(response_text => api.showGenericErrors(`Failed to refresh GPS of ${radioId}`, api.getResponseErrors(response, response_text)));
        });
}

window.toggleRadioEditForm = function toggleRadioEditForm() {
    console.log("toggleRadioEditForm");
    $("#radioEditContainer").toggle();
    document.getElementById('radioEditForm').reset();
}

window.showRadioEditForm = function showRadioEditForm(show = true) {
    if (show) {
        $("#radioEditContainer").show();
    } else {
        $("#radioEditContainer").hide();
    }
    document.getElementById('radioEditForm').reset();
}

window.editRadio = function editRadio(radioId, name, GpsMode, requestInterval) {
    showRadioEditForm();
    $('#radioId').val(radioId);
    $('#name').val(name);
    $('#gpsMode').val(GpsMode);
    $('#requestInterval').val(requestInterval);
}

window.SubmitClicked = function SubmitClicked() {
    console.log("SubmitClicked");
    patchRadio();
}

function refreshData() {
    console.log('Refresh called');
    $('#radiosTable').bootstrapTable('refresh');
}

function patchRadio() {
    // Disable input before posting
    $('#submit').prop('disabled', true);

    //TODO Validation
    const data = getJsonFromForm();

    api.performApiCall("radio", 'PATCH', data,
        (response) => {
            $('#submit').prop('disabled', false);
            console.log("Patch successfull!");
            refreshData();
            showRadioEditForm(false);
        },
        (response) => {
            $('#submit').prop('disabled', false);
            Promise.resolve(response.text())
                .then(resp_data => api.showGenericErrors(`Unable to save Radio Settings of ${radioId}.`, api.getResponseErrors(resp_data)));
        });
}

function getJsonFromForm() {
    return [{
        radioId: $('#radioId').val(),
        name: $('#name').val(),
        GpsMode: $('#gpsMode :selected').val(),
        requestInterval: $('#requestInterval').val()
    }];
}

