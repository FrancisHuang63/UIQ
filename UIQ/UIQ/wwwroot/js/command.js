//Ajax Loading display

$(document).ready(function () {
    $("#loading").ajaxStart(function () {
        $(this).show();
    });

    $("#loading").ajaxStop(function () {
        $(this).hide();
    });

    $('#parameter_modal').on('shown.bs.modal', function () {
        $("#parameters").focus();
    });

    $("#modal_execute").on('click',function(){
        if (confirm("Execute this command?")){
            $('#modal_close').trigger('click');
            $('#show').empty();

            excuteCmdWithParameters();
        }
    });
});

function showParameterModal(command_id, group){
    $('#selected_cmd_id').text(command_id);

    var match_result=check_urlstring_by_regex('(Add|Edit)');
    var command = $('#content').val();

    var match_add_result = check_urlstring_by_regex('(Add)');
    var match_edit_result = check_urlstring_by_regex('(Edit)');
    if(match_add_result && match_edit_result){
        var check_time_result = checkExeTimeFormat();

        if(check_time_result !== "OK"){
            alert(check_time_result);
            return;
        }
    }

    if (match_result != null && !command){
        alert('Please input "Content"!');
        return;
    }

    $('#parameters').val('');
    $('#password').val('');

    if (group !== 'ADM'){
        $('#pwd_form_group').show();
    }

    $('#parameter_modal').modal({
        show: true
    });


    if(match_result != null){
        var parameter_example = $('#example').val();
        $('label[for="parameters"]').html('Parameter (ex. ' + parameter_example + ')');

        return;
    }

    $.ajax({
        url: '/MaintainTools/getCommandInfo',
        type: 'POST',
        data:{
            commandId: command_id
        },
        dataType: 'json',
        success: function(cmd_data){
            $('label[for="parameters"]').html('Parameter (ex.' + cmd_data.command_example + ')');
        }
    });
}

function excuteCmdWithParameters(){
    var command_id = $('#selected_cmd_id').text();
    var parameters = $('#parameters').val();
    var pwd = $('#password').val();

    var match_add_result = check_urlstring_by_regex('(Add)');
    var match_edit_result = check_urlstring_by_regex('(Edit)');
    var exe_url = '/MaintainTools/CommandExecute/?timeStamp=' + new Date().getTime();
    var time_url = '/MaintainTools/CalculateCommandExecuteTime/?timeStamp=' + new Date().getTime();
    if(match_add_result != null || match_edit_result != null){
        var ajax_data = {
            commandId: command_id,
            parameters: parameters,
            passwd: pwd,
            command: $('#content').val(),
            execTime: $('#exec_time').val()
        };
    }else{
        var ajax_data = {
            command_id: command_id,
            parameters: parameters,
            passwd: pwd,
            command: ''
        };
    }

    showCommandInfo(exe_url, time_url, ajax_data);
}

function check_urlstring_by_regex(url_slug_regex){
    var url=window.location.href;
    var match_result = url.match(url_slug_regex);

    return match_result;
}

function checkExeTimeFormat(){
    var match_add_result = check_urlstring_by_regex('(Add)');
    var match_edit_result = check_urlstring_by_regex('(Edit)');

    if(match_add_result != null || match_edit_result != null){
        var exec_time = $('#exec_time').val();

        if(!/^([\d]+)$/.test(exec_time)){
            return('The format of execution time is wrong!');
        }

        return 'OK';
    }
}

function showCommandInfo(exe_url, time_url, ajax_data){
    $.ajax({
        url: time_url,
        type: 'POST',
        data: ajax_data,
        dataType: 'html',
        error: function () {
            alert("Syntax error on command/calculate_time");
        },
        success: function (response) {
            var html =  $('<div/>').html(response).text();

            $('#show').html(html);
            exeCommand(exe_url, ajax_data);
        }
    });
}

function exeCommand(exe_url, ajax_data) {
    $.ajax({
        url: exe_url,
        type: 'POST',
        data: ajax_data,
        dataType: 'html',
        error: function () {
            alert("Syntax error on command/exe");
        },
        success: function (response) {
            $('#show').append(response);
        }
    });
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}
