import * as api from './api.js';
var $table = $('#radiosTable');
var $remove = $('#remove');
var selections = [];

// Prevent full page refresh on submit. We're using a JavaScript method SubmitClicked instead.
// https://stackoverflow.com/a/19454346
$("#radioEditForm").submit(function (e) {
    e.preventDefault();
});

// Initial data load.
jQuery(document).ready(function (_$) {
    initTable();
});

/**
 * Toggles & Resets Radio Add/Update Form.
 * @param {any} show
 */

window.toggleRadioEditForm = function toggleRadioEditForm() {
    console.log("toggleRadioEditForm");
    $("#radioEditContainer").toggle();
    document.getElementById('radioEditForm').reset();
}

/**
 * Shows & Resets Radio Add/Update Form.
 * @param {any} show
 */
window.showRadioEditForm = function showRadioEditForm(show = true) {
    if (show) {
        $("#radioEditContainer").show();
    } else {
        $("#radioEditContainer").hide();
    }
    document.getElementById('radioEditForm').reset();
}

/**
 * Radio Add/Update submit handler
 */
window.SubmitClicked = function SubmitClicked() {
    console.log("SubmitClicked");
    patchRadio();
}

/**
 * BootstrapTable onClick handlers for 'Actions' column.
 */
window.tableOperationEvents = {
    'click .request_gps': function (e, value, row, index) {
        //alert('Requesting GPS for: ' + row['radioId'] + JSON.stringify(row))
        refreshGps(row['radioId']);
    },
    'click .edit': function (e, value, row, index) {
        editRadio(row['radioId'], row['name'], row['GpsMode'], row['requestInterval']);
    },
}

/**
 * Called by BootstrapTable to handle reponse data.
 * @param {any} res
 */
window.tableDataHandler = function tableDataHandler(res) {
    $.each(res.rows, function (i, row) {
        row.state = $.inArray(row.id, selections) !== -1
    })
    return res
}

/**
 * Detail formatter for BootstrapTable row.
 * Renders HTML to show when row is expaned.
 * @param {any} index Index of row element.
 * @param {any} row Data associated with subject row.
 */
window.tableDetailFormatter = function tableDetailFormatter(index, row) {
    var detailsToHide = ['state', 'radioId', 'Status', 'LastSeen', 'LastGpsRequested', 'LastGpsTimeStamp', 'GpsMeasurements'];
    var html = []
    html.push('<h3>Info</h3>')
    $.each(row, function (key, value) {
        if (!detailsToHide.includes(key)) {
            html.push('<p><b>' + key + ':</b> ' + value + '</p>')
        }
    })
    html.push(gpsMeasurementsFormatter(row['GpsMeasurements']));
    return html.join('')
}

/**
 * Handles onClick event of editRadio action on BootStrapTable row.
 * Populates and shows Radio Add/Update form.
 * @param {any} radioId
 * @param {any} name
 * @param {any} GpsMode
 * @param {any} requestInterval
 */
function editRadio(radioId, name, GpsMode, requestInterval) {
    showRadioEditForm();
    $('#radioId').val(radioId);
    $('#name').val(name);
    $('#gpsMode').val(GpsMode);
    $('#requestInterval').val(requestInterval);
}

/**
 * Renders column "Actions" of BootstrapTable containing clickable buttons for modifcation edits on said Radio's row.
 * @param {any} value ???
 * @param {any} row Actual row data
 * @param {any} index Index of row
 */
function operateFormatter(value, row, index) {
    return [
        '<a class="request_gps" href="javascript:void(0)" title="Request GPS"><i class="fa fa-location-arrow"></i>     </a>',
        '<a class="edit" href="javascript:void(0)" title="Edit"><i class="fa fa-pen"></i>     </a>',
        '<a class="remove" href="#genericModal" title="Remove" data-toggle="modal" data-target="#genericModal" data-radioid="' + row['radioId'] + '"><i class="fa fa-trash"></i>     </a>',
    ].join('')
}

/**
 * Renders part of DetailFormatter's view for expaned row containing table of most recent GPS measurements of said row.
 * @param {any} gpsMeasurements
 */
function gpsMeasurementsFormatter(gpsMeasurements) {
    var html = []
    if (gpsMeasurements == null || gpsMeasurements.length == 0) {
        return html;
    }
    html.push('<h3>GPS Measurements</h3>');
    html.push("<table><tr><th>TimeStamp</th><th>Latitude</th><th>Longitude</th></tr>");
    $.each(gpsMeasurements, function (index, m) {
        var row = `<tr><td>${m['Timestamp']}</td><td>${m['Latitude']}</td><td>${m['Longitude']}</td></tr>`;
        html.push(row);
    })
    html.push("</table>");
    return html;
}

/**
 * Renders friendly DateTime representation for use in BootstrapTable columns
 * @param {any} value DateTime as string of said row and column
 * @param {any} row all row data
 * @param {any} index index of row.
 */
function dateFormatter(value, row, index) {
    if (typeof (value) == 'undefined') {
        return 'N/A';
    }
    return moment(value).format("dddd HH:mm:ss");
}

/**
 * Trigger BootstrapTable refresh
 */
function refreshData() {
    console.log('Refresh called');
    $('#radiosTable').bootstrapTable('refresh');
}


/**
 * Invokes Radio DELETE to server for said radioId. 
 * */
function deleteRadio(radioId) {
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

/**
 * Invokes GPS Refresh on server for said radioId
 * @param {any} radioId
 */
function refreshGps(radioId) {
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

/**
 * Handles submit of Radio Add/Update form.
 */
function patchRadio() {
    // Disable input before posting
    $('#submit').prop('disabled', true);

    //TODO Validation
    const data = getPatchJsonData();

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

/**
 * Constructs data to send with PATCH radio from radio add/update form.
 */
function getPatchJsonData() {
    return [{
        radioId: $('#radioId').val(),
        name: $('#name').val(),
        GpsMode: $('#gpsMode :selected').val(),
        requestInterval: $('#requestInterval').val()
    }];
}


/** 
 * Returns a list of all selected RowIDs of BootstrapTable
 */
function getIdSelections() {
    return $.map($table.bootstrapTable('getSelections'), function (row) {
        return row.id
    })
}

/**
 * Render delete confirmation modal.
 */
$("#genericModal").on('show.bs.modal', function (e) {
    var data = $(e.relatedTarget).data();
    $('.title', this).text(data.radioid);
    $('.btn-ok', this).data('radioid', data.radioid);
});

/**
 * Handle modal delete confirmed click.
 */
$('#genericModal').on('click', '.btn-ok', function (e) {
    var radioid = $(this).data('radioid');
    deleteRadio(radioid);
});

/**
 * Initialize & Render BootstrapTable with all radios.
 */
function initTable() {
    $table.bootstrapTable('destroy').bootstrapTable({
        // Defines the URL to get table data from.
        url: `${getApiBaseUrl()}/radio`,
        height: 800,
        locale: 'en-US',
        columns: [
            [{
                field: 'state',
                checkbox: true,
                rowspan: 2,
                align: 'center',
                valign: 'middle'
            },
            {
                title: 'Radio ID',
                field: 'radioId',
                rowspan: 2,
                align: 'center',
                valign: 'middle',
                sortable: true,
            }, {
                title: 'Item Detail',
                colspan: 5,
                align: 'center'
            }],
            [{
                field: 'Status',
                title: 'Status',
                sortable: true,
                align: 'center'
            },
            {
                field: 'LastSeen',
                title: 'Last Seen',
                sortable: true,
                align: 'center',
                formatter: dateFormatter
            },
            {
                field: 'LastGpsRequested',
                title: 'Last GPS Request',
                align: 'center',
                clickToSelect: false,
                formatter: dateFormatter
            },
            {
                field: 'LastGpsTimeStamp',
                title: 'Last GPS Received',
                align: 'center',
                clickToSelect: false,
                formatter: dateFormatter
            },
            {
                field: 'operate',
                title: 'Actions',
                align: 'center',
                clickToSelect: false,
                events: window.tableOperationEvents,
                formatter: operateFormatter
            }]
        ]
    })
    $table.on('check.bs.table uncheck.bs.table ' +
        'check-all.bs.table uncheck-all.bs.table',
        function () {
            $remove.prop('disabled', !$table.bootstrapTable('getSelections').length)

            // save your data, here just save the current page
            selections = getIdSelections()
            // push or splice the selections if you want to save all data selections
        })
    $table.on('all.bs.table', function (e, name, args) {
        console.log(name, args)
    })

}
