$(document).ready(function () {
    $('input[name="IsNewModelName"]').on('change', function () {
        var selected_model = $('input[name="IsNewModelName"]:checked').val();
        if (selected_model === 'False') {
            $('#model_select').trigger('change');
        } else {
            var model_position = $('input[name="New_Model_Position"]').val();
            $('#model_position').val(model_position);
        }
    });


    $('#model_select').on('change', function () {
        var model_position = $('#model_select option:selected').attr('data-position');
        $('#model_position').val(model_position);
        $('#memberModelId').val($(this).val());
    });

    $(document).on('click', '#addCheckPoint', function () {
        var dialog_id = htmlEncode($(this).closest('div')[0].id);
        var batch_id = htmlEncode($(this).parents('tr').children('th[id="batch_id"]').data('batch_id'));
        var batch_name = htmlEncode($(this).parents('tr').children('th[id="batch_name"]').data('batch_name'));
        var batch_index = htmlEncode($(this).parents('div.check_point_dialog').attr('id').split("_")[3]);
        add_new_check_point(batch_id, dialog_id, $(this), batch_name, batch_index);
    });

    $(document).on('click', '#delCheckPoint', function () {
        var batch_row_index = $(this).parents('tbody').prev('thead');
        var tr = $(this).parents('tr');
        var target = tr[1];
        var target_check_row_num = $(target).parents('.check_point_dialog').children();

        $(target).remove();
    });

});

$(document).on('click', '#showCheckPoint', function () {
    let batch_row_index = $(this).closest('tr.batchItem').index();
    //batch_row_index = batch_row_index.substr(batch_row_index.indexOf("_") + 1);
    let batch_name = $(this).closest('tr.batchItem').find('[name="Batch.Batch_Name"]').val();
    let batch_id = parseInt($(this).closest('tr.batchItem').find('[name="Batch.Batch_Id"]').val());

    if (!batch_name) {
        alert('Batch name is empty! Please set batch_name!');
        return;
    }
    show_dialog(batch_id, batch_name, batch_row_index);

    $('#typhoon_model').trigger('change');
});

$(document).on('change', '#typhoon_model', function () {
    if ($(this).is(':checked')) {
        $('.typhoon').show();
    } else {
        $('.typhoon').hide();
    }
});

function show_dialog(batch_id, batch_name, batch_row_index) {
    var dialog_title = 'Batch: ' + batch_name;
    var new_dialog_id = "check_point_dialog_" + batch_row_index;
    if ($('body #' + new_dialog_id).length === 0) {
        //var $dialog = $("#check_point_dialog").clone().prop("id", new_dialog_id);
        //$('body').append($dialog);
        let dialog = `<div class="check_point_dialog" id="${new_dialog_id}" title="Check Points" style="display:none;">
					    <table width="100%" border="1" aria-describedby="check_point_dialog">
						    <thead>
							    <tr>
								    <th style="display:none;" id="batch_id"></th>
								    <th id="batch_name" width=65%></th>
								    <th id="add_check_btn" width=35%>
									    <a type="button" id="addCheckPoint" class="btn_default">
										    <img src="../../images/add_icons.png" alt="??????"/>
										    Add Check Point
									    </a>
								    </th>
							    </tr>
						    </thead>
						    <tbody></tbody>
					    </table>
				    </div>`;
        $('body').append(dialog);
    }

    if ($(".ui-dialog #" + new_dialog_id).length === 0) {
        new_dialog = initial_dialog(new_dialog_id, batch_row_index);
        new_dialog.closest('div.ui-dialog').appendTo('#setting');
    }

    $('#' + new_dialog_id + ' #batch_name').text(dialog_title);
    $('#' + new_dialog_id + ' #batch_name').attr('data-batch_row_index', batch_row_index);
    $('#' + new_dialog_id + ' #batch_id').data('batch_id', batch_id);
    $('#' + new_dialog_id + ' #batch_name').data('batch_name', batch_name);

    $('#' + new_dialog_id).dialog('open');
}

function initial_dialog(dialog_id, batch_row_index) {
    var shell_data = new Array();
    var target_dialog = '#' + dialog_id;
    var new_dialog = $(target_dialog).dialog({
        modal: true,
        autoOpen: false,
        minWidth: 600,
        buttons: {
            "close": function () {
                var total_length = $(this).parent().find('input[autocomplete="off"]').length;
                for (var i = 0; i < total_length; i++) {
                    shell_data[i] = $('#batch_' + batch_row_index + '_checkStep_' + i + '_flexselect').val();
                }
                var no_repetition_lenght = $.unique(shell_data).length;
                if (total_length > no_repetition_lenght) {
                    if (confirm('These shell are repeated\nAre you sure to add ?')) {
                        $(this).dialog("close");
                    }
                } else {
                    $(this).dialog("close");
                }
            }
        },
        create: function () {
            if ($(target_dialog + " #batch_" + batch_row_index + "_RowIndex").length === 0) {
                $(target_dialog).append($('<input>', {
                    id: "batch_" + batch_row_index + "_RowIndex",
                    type: "hidden",
                    name: "batch_" + batch_row_index + "_RowIndex",
                    value: batch_row_index
                }));
            }

            if ($(target_dialog + " #batch_" + batch_row_index + "_checkRowNum").length === 0) {
                $(target_dialog).append($('<input>', {
                    id: "batch_" + batch_row_index + "_checkRowNum",
                    type: "hidden",
                    name: "batch_" + batch_row_index + "_checkRowNum",
                    value: 0
                }));
            }
        }
    });

    return new_dialog;
}

function add_new_check_point(batchId, dialog_id, item, batch_name, batch_index) {
    var model_id = item.parents().find('select#model_select').val();
    var member_name = item.parents().find('input[name="Member.Member_Name"]').val();
    var member_account = item.parents().find('input[name="Member.Account"]').val();
    var run_type = item.parents().find('[name="Batch.Batch_Type"]').val();
    var round = item.parents().find('[name="Batch.Batch_Dtg"]').val();

    $.ajax({
        url: '/MaintainTools/GetShell',
        type: 'post',
        data: {
            "model_id": model_id,
            "member_name": member_name,
            "member_account": member_account,
            "batch_name": batch_name,
            "run_type": run_type,
            "round": round
        },
        dataType: 'json',
        success: function (shell_info) {
            var shell = unset_repeat_shell(shell_info);
            var member_id = $('#memberId').val();
            var batch_row_index = dialog_id.split("_")[3];
            var id_prefix = htmlEncode("batch_" + batch_row_index + '_');
            var new_check_point_number = parseInt($('#' + dialog_id + ' #' + id_prefix + 'checkRowNum').val());

            var html = '<tr class="checkPointItem"><td id="check_' + new_check_point_number + '" colspan="2"><table width="100%"><tr>';
            html += `<input type="hidden" name="CheckPoint.Batch_Id" value="${batchId}">`;
            html += `<input type="hidden" name="CheckPoint.Batch_Name" value="${batch_name}">`;
            html += '<td>Shell Name<font color="red">*</font></td>';
            html += '<td><select id="' + id_prefix + 'checkStep_' + new_check_point_number + '" >';
            $.each(shell, function (key, value) {
                html += '<option value = "' + value + '">' + value + '</option>';
            });
            html += '</select> (Ex: Wn2d.ksh 0036)';

            if (member_id) {
                html += '<img id="delCheckPoint" src="../../images/delete_icons.png" alt="??????" align="right"/></td></tr>';
            } else {
                html += '<img id="delCheckPoint" src="../images/delete_icons.png" alt="??????" align="right"/></td></tr>';
            }

            html += '<tr><td>Check Time(min)</td>';
            html += '<td><input id="' + id_prefix + 'checkTime_' + new_check_point_number + '" name="' + id_prefix + 'checkTime_' + new_check_point_number + '" type="text" value="" disabled> (Ex: 20)</td></tr>';
            html += '<tr class="typhoon" style="display: none;"><td>Typhoon Time(min)</td>';
            html += '<td><input id="' + id_prefix + 'typhoonTime_' + new_check_point_number + '" name="' + id_prefix + 'typhoonTime_' + new_check_point_number + '" type="text" value="" disabled> (Ex: 20)</td></tr>';
            html += '<tr><td>Tolerance Time(min)</td>';
            html += '<td><input id="' + id_prefix + 'checkToleranceTime_' + new_check_point_number + '" name="CheckPoint.Tolerance_Time" type="text" value=""> (Ex: 20)</td></tr>';
            html += '</table></td></tr>';

            $('#' + dialog_id + ' #' + id_prefix + 'checkRowNum').val(parseInt(new_check_point_number) + 1);
            $('#' + dialog_id + ' tbody:eq(0)').append(html);
            $('#typhoon_model').trigger('change');
            set_new_shell_time(shell_info, id_prefix, new_check_point_number, round);

            $("#check_" + new_check_point_number + " #" + id_prefix + "checkStep_" + new_check_point_number).flexselect({
                allowMismatch: true,
                inputNameTransform: function (name) {
                    return 'CheckPoint.Shell_Name';
                }
            });
        }
    });
}

function set_new_shell_time(shell_info, id_prefix, new_check_point_number, round) {
    id_prefix = htmlEncode(id_prefix);
    new_check_point_number = parseInt(new_check_point_number);

    $(document).mousemove('#check_' + new_check_point_number + ' #' + id_prefix + 'checkStep_' + new_check_point_number + '_flexselect', function () {
        var shell_name = $('#check_' + new_check_point_number + ' #' + id_prefix + 'checkStep_' + new_check_point_number + '_flexselect').val();
        var check_time = get_shell_avg_time("0", shell_name, shell_info);
        var typhoon_time = get_shell_avg_time("1", shell_name, shell_info);
        $('#' + id_prefix + 'checkTime_' + new_check_point_number).val(Math.ceil(check_time));
        $('#' + id_prefix + 'typhoonTime_' + new_check_point_number).val(Math.ceil(typhoon_time));
    });
}

function set_old_check_dialog(batchId, batch_type, batch_dtg, item, batch_name, batch_row_index, check_point_info) {
    var dialog_title = 'Batch: ' + batch_name;
    var dialog_id = htmlEncode("check_point_dialog_" + batch_row_index);
    //var $dialog = $("#check_point_dialog").clone().prop("id", dialog_id);
    let dialog = $`<div class="check_point_dialog" id="${dialog_id}" title="Check Points" style="display:none;">
					<table width="100%" border="1" aria-describedby="check_point_dialog">
						<thead>
							<tr>
								<th style="display:none;" id="batch_id"></th>
								<th id="batch_name" width=65%></th>
								<th id="add_check_btn" width=35%>
									<a type="button" id="addCheckPoint" class="btn_default">
										<img src="~/images/add_icons.png" alt="??????"/>
										Add Check Point
									</a>
								</th>
							</tr>
						</thead>
						<tbody></tbody>
					</table>
				</div>`;
    $('body').append(dialog);

    var check_row_index = 0;
    var id_prefix = "batch_" + batch_row_index + '_';

    $.each(check_point_info, function (key, value) {
        add_old_check_point(batchId, batch_type, batch_dtg, item, batch_row_index, dialog_id, check_row_index, value);
        check_row_index = check_row_index + 1;
    });

    $('#batch_' + batch_row_index + ' #showCheckPoint').trigger('click');

    $('#' + dialog_id).dialog('close');

}

function add_old_check_point(batchId, batch_type, batch_dtg, item, batch_row_index, dialog_id, check_row_index, check_point_info) {
    var id_prefix = "batch_" + batch_row_index + '_';
    var model_id = item.parents().find('select#model_select').val();
    var member_name = item.parents().find('input[name="Member.Member_Name"]').val();
    var member_account = item.parents().find('input[name="Member.Account"]').val();
    var batch_name = item.val();
    var run_type = batch_type.val();
    var round = batch_dtg.val();
    
    $.ajax({
        url: '/MaintainTools/GetUnselectedShell',
        type: 'post',
        data: {
            "shell_name": check_point_info.shell_name,
            "model_id": mo_id,
            "member_name": mb_name,
            "member_account": acnt,
            "batch_name": batch_name,
            "run_type": run_type,
            "round": round
        },
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                let shell_info = response.data;
                var shell = unset_repeat_shell(shell_info);
                var html = '<tr class="checkPointItem"><td id="check_' + check_row_index + '" colspan="2"><table width="100%"><tr>';
                html += `<input type="hidden" name="CheckPoint.Batch_Id" value="${batchId}">`;
                html += `<input type="hidden" name="CheckPoint.Batch_Name" value="${batch_name}">`;
                html += '<td>Shell Name<font color="red">*</font></td>';
                html += '<td><select id="' + id_prefix + 'checkStep_' + check_row_index + '">';
                html += '<option value="' + check_point_info.shell_name + '" selected="selected">' + check_point_info.shell_name + '</option>';
                $.each(shell, function (key, value) {
                    html += '<option value = "' + value + '">' + value + '</option>';
                });
                html += '</select> (Ex: Wn2d.ksh 0036)';
                html += '<img id="delCheckPoint" src="../../images/delete_icons.png" alt="??????" align="right"/></td></tr>';
                html += '<tr><td>Check Time(min)</td>';
                html += '<td><input id="' + id_prefix + 'checkTime_' + check_row_index + '" name="' + id_prefix + 'checkTime_' + check_row_index + '" type="text" value="" disabled> (Ex: 20)</td></tr>';
                html += '<tr class="typhoon" style="display: none;"><td>Typhoon Time(min)</td>';
                html += '<td><input id="' + id_prefix + 'typhoonTime_' + check_row_index + '" name="' + id_prefix + 'typhoonTime_' + check_row_index + '" type="text" value="" disabled> (Ex: 20)</td></tr>';
                html += '<tr><td>Tolerance Time(min)</td>';
                html += '<td><input id="' + id_prefix + 'checkToleranceTime_' + check_row_index + '" name="CheckPoint.Tolerance_Time" type="text" value="' + check_point_info.tolerance_time + '"> (Ex: 20)</td></tr>';
                html += '</table></td></tr>';

                var target_batch_row_index = $('#' + dialog_id + ' #batch_' + batch_row_index + '_RowIndex');
                var target_check_row_num = $('#' + dialog_id + ' #' + id_prefix + 'checkRowNum');
                if (target_batch_row_index.legth === 0) {
                    var batch_row_index_html = '<input id="batch_' + batch_row_index + '_RowIndex" type="hidden" name="batch_' + batch_row_index + '_RowIndex" value="' + batch_row_index + '">';
                    $('#' + dialog_id).append(batch_row_index_html);
                }
                if (target_check_row_num.length === 0) {
                    var check_row_num_html = '<input id="batch_' + batch_row_index + '_checkRowNum" type="hidden" name="batch_' + batch_row_index + '_checkRowNum" value="0">';
                    $('#' + dialog_id).append(check_row_num_html);
                }

                $('#' + dialog_id + ' #' + id_prefix + 'checkRowNum').val(parseInt(check_row_index) + 1);
                $('#' + dialog_id + ' tbody:eq(0)').append(html);

                var check_time = get_avg_time("0", check_point_info.avg_time);
                $('#' + id_prefix + 'checkTime_' + check_row_index).val(Math.ceil(check_time));

                var typhoon_time = get_avg_time("1", check_point_info.avg_time);
                $('#' + id_prefix + 'typhoonTime_' + check_row_index).val(typhoon_time);

                var typhoon_time_value = $('#' + id_prefix + 'typhoonTime_' + check_row_index).val();
                var check_time_value = $('#' + id_prefix + 'checkTime_' + check_row_index).val();
                var shell_name = $('#' + id_prefix + 'checkStep_' + check_row_index).val();
                set_old_shell_time(shell_info, shell_name, id_prefix, check_row_index, typhoon_time_value, check_time_value);

                $("#" + id_prefix + "checkStep_" + check_row_index).flexselect({
                    allowMismatch: true,
                    inputNameTransform: function () {
                        return 'CheckPoint.Shell_Name';
                    }
                });
            }
            else {
                alert('Error: ' + htmlEncode(response.message));
            }
        }
    });
}

function set_old_shell_time(shell_info, shell_name, id_prefix, check_row_index, typhoon_time, check_time) {
    $(document).on('change', '#' + id_prefix + 'checkStep_' + check_row_index, function () {
        var result = $.grep(shell_info, function (e) {
            return e.shell_name === $('#' + id_prefix + 'checkStep_' + check_row_index).val();
        });
        if (result.length !== 0) {
            var unselect_check_time = get_shell_avg_time('0', $('#' + id_prefix + 'checkStep_' + check_row_index).val(), shell_info);
            var unselect_typhoon_time = get_shell_avg_time('1', $('#' + id_prefix + 'checkStep_' + check_row_index).val(), shell_info);
            $('#' + id_prefix + 'checkTime_' + check_row_index).val(Math.ceil(unselect_check_time));
            $('#' + id_prefix + 'typhoonTime_' + check_row_index).val(Math.ceil(unselect_typhoon_time));
        } else {
            $('#' + id_prefix + 'checkTime_' + check_row_index).val(check_time);
            $('#' + id_prefix + 'typhoonTime_' + check_row_index).val(typhoon_time);
        }
    });
}

function get_shell_avg_time(mode, shell_name, shell_info) {
    var row = 0;
    var shell_avg_time = 0;
    $.each(shell_info, function (key, value) {
        if (value['sh_name'] === shell_name && value['typhoon_mode'] === mode) {
            row++;
            shell_avg_time += value['avg_execution_time'] * 1;
        }
    });

    var output = $.isNumeric(shell_avg_time / row / 60) ? shell_avg_time / row / 60 : 0;

    return output;
}

function unset_repeat_shell(shell_info) {
    var output = {};
    var length = shell_info.length;
    for (var key = 0; key < length; key++) {
        if (output[shell_info[key].shell_name])
            continue;
        output[shell_info[key].shell_name] = shell_info[key].shell_name;
    }

    return output;
}

function get_avg_time(mode, shell_data) {
    var avg_time = 0;
    $.each(shell_data, function (key, value) {
        if (value.typhoon_mode === mode) {
            avg_time = value.avg_execution_time
        }
    });

    return avg_time;
}
