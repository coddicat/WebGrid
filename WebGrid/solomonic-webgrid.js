/*!
 * Solomonik Web Grid JavaScript Library v1.0.0
 * http://solomon-k.com/
 *
 * Copyright 2015, Rodion Shlomo Solomonik 
 * Released under the MIT license
 *
 * Date: 2014-06-11
 */


if (!$solomonic) {
    var $solomonic = {};
}

if (!$solomonic.webGrid) {
    $solomonic.webGrid = {};
}

if (!$solomonic.waitText) {
    $solomonic.waitText = "Loading. Please wait...";
}

if (!$solomonic.waitDiv) {
    $solomonic.waitDiv = function () {
        return "<div class='progress progress-striped active'><div class='progress-bar progress-bar-success' style='width: 100%;'>" + $solomonic.waitText + "</div></div>";
    };
}

$solomonic.webGrid.selectAll = function (elm) {
    var obj = $(elm);
    var table = obj.closest('table.solomonic-table');
    var value = obj.is(':checked');
    table.find('input[solomonic-selectRow]:checkbox').prop('checked', value);
};

$solomonic.webGrid.selectRow = function (elm) {
    var obj = $(elm);
    var table = obj.closest('table.solomonic-table');

    if (table.find('input[solomonic-selectRow]:checkbox').not(':checked').length > 0) {
        table.find('input[solomonic-selectAll]:checkbox').prop('checked', false);
    } else {
        table.find('input[solomonic-selectAll]:checkbox').prop('checked', true);
    }
};

$solomonic.webGrid.toPage = function (elm) {
    var obj = $(elm);
    var form = obj.closest('form');
    form.find('input[name="WebGridSettings.WebGridPagerSettings.CurrentPage"]').val(obj.attr('solomonic-topage'));

    $solomonic.webGrid.settingChanged(elm, true);
};

$solomonic.webGrid.sortBy = function (elm) {
    var obj = $(elm);
    var desc = false;
    if (obj.is("[solomonic-sort-descending]"))
        desc = true;

    var form = obj.closest('form');
    form.find('input[name="WebGridSettings.WebSort.ColumnName"]').val(obj.attr('solomonic-sortby'));
    form.find('input[name="WebGridSettings.WebSort.Descending"]').val(desc);

    $solomonic.webGrid.settingChanged(elm, true);
};

$solomonic.webGrid.settingChanged = function (elm, doit) {
    var autorefresh = $(elm).closest('table.solomonic-table').attr('solomonic-autorefresh');
    var form = $(elm).closest('form');
    form.find('input[name="WebGridSettings.IsChanged"]').val(true);

    if (autorefresh == 'true' || autorefresh == 'True' || doit) {
        form.submit();
    }
};

$solomonic.webGrid.Refresh = function (table) {
    var obj = $(table);

    obj.find('input[solomonic-selectAll]:checkbox').click(function () { $solomonic.webGrid.selectAll(this); });
    obj.find('input[solomonic-selectRow]:checkbox').click(function () { $solomonic.webGrid.selectRow(this); });

    obj.find('a[solomonic-topage]').click(function () {
        $solomonic.webGrid.toPage(this);
        return false;
    });

    obj.find('a[solomonic-sortby]').click(function () {
        $solomonic.webGrid.sortBy(this);
        return false;
    });

    obj.find('a[solomonic-searchrow]').click(function () {
        $solomonic.webGrid.settingChanged(this);
        return false;
    });

    obj.find('button.solomonic-save-changes').click(function () {
        var form = $(this).closest('form');
        form.find('input[name="SaveChanges"]').val(true);
        form.submit();
        return false;
    });

    obj.find('button.solomonic-refresh').click(function () {
        var form = $(this).closest('form');
        form.submit();
        return false;
    });

    obj.find('select[solomonic-itemsperpage]').change(function () {
        $solomonic.webGrid.settingChanged(this);
    });

    obj.find('select[solomonic-dropdownfilter]').change(function () {
        $solomonic.webGrid.settingChanged(this);
    });

    obj.find('.solomonic-table-input').on('change', $solomonic.webGrid.input_onChange);
    obj.find('.solomonic-table-input').on('change', 'input', $solomonic.webGrid.input_onChange);
    obj.find('.solomonic-table-input').on('change', 'textarea', $solomonic.webGrid.input_onChange);
    obj.find('.solomonic-table-input').on('change', 'select', $solomonic.webGrid.input_onChange);

    obj.find('.solomonic-table-main-div').on('scroll', function () {
        //obj.find('.solomonic-inner-head-table').width(obj.find('.solomonic-inner-table').width());
        var offset = $(this).find('.solomonic-inner-table').offset();
        if (offset) {
            obj.find('.solomonic-inner-head-table').offset({ left: offset.left });
        }
    });

    var interval = setInterval(function () {
        if (obj.is(':visible')) {
            obj.find('.solomonic-inner-head-table').width(obj.find('.solomonic-inner-table').width());
        } else {
            clearInterval(interval);
        }
    }, 100);
};

$solomonic.webGrid.input_onChange = function (event) {
    var elm = event.currentTarget;
    var obj = $(elm);
    obj.closest('tr.dataRow').find('input[name$=Changed]').val(true);
    obj.closest('form').find('.solomonic-save-changes').removeAttr('disabled');
};

$solomonic.webGrid.ajaxOnFailure = function (elm, xhr, status, error) {
    var updateTarget = $(elm).attr('data-ajax-update');
    var refresh = $("<button class='btn btn-default btn-danger'>Retry</button>");
    var mainDiv = $(updateTarget).find('.solomonic-table-main-div');
    var innerDiv = $("<div style='display:table-cell; vertical-align:middle'></div>");
    innerDiv.height(mainDiv.height());
    innerDiv.width(mainDiv.width());
    innerDiv.html(xhr.responseText);
    innerDiv.append(refresh);
    mainDiv.html("").append(innerDiv);
    refresh.on('click', function () {
        $(elm).submit();
    });
};

$solomonic.webGrid.ajaxOnBegin = function (elm) {
    var updateTarget = $(elm).attr('data-ajax-update');
    var obj = $(updateTarget);
    var mainDiv = obj.find('.solomonic-table-main-div');
    if (mainDiv[0]) {
        var innerDiv = $("<div style='display:table-cell; vertical-align:middle'></div>");
        innerDiv.height(mainDiv.height());
        innerDiv.width(mainDiv.width());
        innerDiv.html($solomonic.waitDiv());
        mainDiv.html("").append(innerDiv);
    } else
        obj.html($solomonic.waitDiv());

};
