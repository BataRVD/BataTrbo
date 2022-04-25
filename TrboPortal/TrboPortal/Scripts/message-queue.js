import * as api from './api.js';

var $table = $('#messageQueueTable')
var $remove = $('#remove')
var selections = []

// Initial data load.
jQuery(document).ready(function (_$) {
    initTable();
    autoRefresh();
});


window.clearBataTrboQueue=function clearBataTrboQueue() {
    $('#clearBataTrboQueue').prop("disabled", true)
    api.performApiCall(`system/clearQueue`, 'GET', null,
        (response) => {
            console.log(`Cleared BataTrbo Queue!`)
            $('#clearBataTrboQueue').prop("disabled", false)
        },
        (response) => {
            $('#clearBataTrboQueue').prop("disabled", false)
            Promise.resolve(response.text())
                .then(JSON.parse)
                .then(resp_data => api.showGenericErrors(`Failed to clear BataTrboQueue`, api.getValidationErrors(resp_data)));
        });
}


window.clearInternalQueue = function clearInternalQueue() {
    $('#clearInternalQueue').prop("disabled", true)
    api.performApiCall(`system/clearTrboNetQueue`, 'GET', null,
        (response) => {
            console.log(`Cleared BataTrbo Queue!`)
            $('#clearInternalQueue').prop("disabled", false)
        },
        (response) => {
            $('#clearInternalQueue').prop("disabled", false)
            Promise.resolve(response.text())
                .then(JSON.parse)
                .then(resp_data => api.showGenericErrors(`Failed to clear Internal TrboNet Queue`, api.getValidationErrors(resp_data)));
        });
}

function autoRefresh() {
    $table.bootstrapTable('refresh');
    setTimeout(function () { autoRefresh(); }, 2000);
}

function getIdSelections() {
    return $.map($table.bootstrapTable('getSelections'), function (row) {
        return row.id
    })
}

window.detailFormatter = function detailFormatter(index, row) {
    var html = []
    $.each(row, function (key, value) {
        html.push('<p><b>' + key + ':</b> ' + value + '</p>')
    })
    return html.join('')
}

function initTable() {
    $table.bootstrapTable('destroy').bootstrapTable({
        // Defines the URL to get table data from.
        url: `${getApiBaseUrl()}/system/queue`,
        height: 800,
        locale: 'en-US',
        columns: [{
            title: 'Timestamp',
            field: 'Timestamp',
            align: 'center',
            valign: 'middle',
            sortable: true,
        }, {
            title: 'Radio ID',
            field: 'RadioID',
            align: 'center',
            sortable: true,
            }],
        responseHandler: function (res) {
            $("#queueCounter").html(`Location requests sent: ${res.LocationRequestCounter}</br>Location responses received: ${res.LocationResponseCounter}</br>Internal Queue Size: ${res.InternalTrboNetQueueSize}`);
            return res.MessageQueueItems;
            
        }
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