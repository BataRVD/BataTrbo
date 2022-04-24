import * as api from './api.js';

var $table = $('#messageQueueTable')
var $remove = $('#remove')
var selections = []

// Initial data load.
jQuery(document).ready(function (_$) {
    initTable();
    autoRefresh();
});

function autoRefresh() {
    $table.bootstrapTable('refresh');
    setTimeout(function () { autoRefresh(); }, 2000);
}

function getIdSelections() {
    return $.map($table.bootstrapTable('getSelections'), function (row) {
        return row.id
    })
}

function responseHandler(res) {
    $.each(res.rows, function (i, row) {
        row.state = $.inArray(row.id, selections) !== -1
    })
    return res
}

function detailFormatter(index, row) {
    var html = []
    $.each(row, function (key, value) {
        html.push('<p><b>' + key + ':</b> ' + value + '</p>')
    })
    return html.join('')
}

function operateFormatter(value, row, index) {
    return [
        '<a class="request_gps" href="javascript:void(0)" title="Request GPS">',
        '<i class="fa fa-location-arrow"></i>',
        '</a>  ',
        '<a class="remove" href="javascript:void(0)" title="Remove">',
        '<i class="fa fa-trash"></i>',
        '</a>'
    ].join('')
}

window.operateEvents = {
    'click .request_gps': function (e, value, row, index) {
        alert('Requesting GPS for: ' + JSON.stringify(row))
    },
    'click .remove': function (e, value, row, index) {
        alert(`Delete clicked for: ${row['radioId']}`)
    },
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
        }]
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