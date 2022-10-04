//Ajax Loading display

$(document).ready(function () {
    $("#loading").ajaxStart(function () {
        $(this).show();
    });

    $("#loading").ajaxStop(function () {
        $(this).hide();
    });

});

//Ajax function by jQuery
function sendAJAXRequest(req_type, uri, div) {
    $('#file_content').empty();
    $('#file_created_result').empty();

    // Get form values
    var model = $("#model").val();
    var member = $("#member").val();
    var dtg = $("#dtg").val();
    var lid = $("#lid").val();
    var tau1 = $("#tau1").val();
    var tau2 = $("#tau2").val();
    var method = $("#method").val();
    var node = $("#node").val();
    var account = $("#account").val();
    var nickname = $("#nickname").val();
    var adjust = $("#adjust").val();
    var batch = $("#batch").val();
    var jobid = $("#jobid").val();
    var cronmode = $("#cronmode").val();
    var keyw = $("#keyw").val();
    var parameter = $("#parameter").val();

    div = '#' + div;


    // start Remove this conditional structure or edit its code blocks so that they're not all the same

    //if (req_type == 'post') {
    //    //target_url = uri + '?timeStamp=' + new Date().getTime();
    //    target_url = uri;
    //    // to avoid browsers loading the past data from cache
    //} else {
    //    target_url = uri;
    //}

    target_url = uri; 

    // end Remove this conditional structure or edit its code blocks so that they're not all the same

    $.ajax({
        url: target_url,
        type: 'POST',
        dataType: 'html',
        data: {ModelName: model,
            MemberName: member,
            Dtg: dtg,
            Lid: lid,
            Tau1: tau1,
            Tau2: tau2,
            Method: method,
            Node: node,
            Account: account,
            Nickname: nickname,
            Adjust: adjust,
            Batch: batch,
            JobId: jobid,
            CronMode: cronmode,
            Keyword: keyw,
            Parameter: parameter
        },
        error: function () {
            alert("Syntax error on " + uri);
        },
        success: function (response) {
            $(div).html(response);
        }

    });

}


//Ajax function by jQuery
function sendAJAXRequest2(req_type, uri, div, par1, par2, par3) {
    // Get form values
    div = '#' + div;

    if (req_type == 'post') {
        target_url = uri + '?timeStamp=' + new Date().getTime();
        // to avoid browsers loading the past data from cache
    } else {
        target_url = uri;
    }

    $.ajax({
        url: target_url,
        type: 'POST',
        dataType: 'html',
        data: {passwd: par2,
            command_id: par1,
            command: par3
        },
        error: function () {
            alert("Syntax error on " + uri);
        },
        success: function (response) {
            $(div).html(response);
        }

    });

}

function executeCmd(id, pwd, cmd) {
    sendAJAXRequest2('post', './command/Exe', 'show', id, pwd, cmd);
}

function executeCmd(id, pwd, cmd, path) {
    sendAJAXRequest2('post', path + 'command/Exe', 'show', id, pwd, cmd);
}

function senfe(o, a, b, c, d) {
    var t = document.getElementById(o).getElementsByTagName("tr");
    for (var i = 1; i < t.length; i++) {
        t[i].style.backgroundColor = (t[i].sectionRowIndex % 2 == 0) ? a : b;
        /*t[i].onclick=function(){
         if(this.x!="1"){
         this.x="1";
         this.style.backgroundColor=d;
         }else{
         this.x="0";
         this.style.backgroundColor=(this.sectionRowIndex%2==0)?a:b;
         }
         }*/
        t[i].onmouseover = function () {
            if (this.x != "1")
                this.style.backgroundColor = c;
        }
        t[i].onmouseout = function () {
            if (this.x != "1")
                this.style.backgroundColor = (this.sectionRowIndex % 2 == 0) ? a : b;
        }
    }
}

function clear_nickname_options() {
    var html = '<option>-----</option>';
    $('#nickname').html(html);
    return;
}

function build_nickname_options() {
    var model = $('#model').val();
    var member = $('#member').val();
    var target_url = '/ModelEnquire/GetNicknameItems';

    $.ajax({
        url: target_url,
        type: 'POST',
        dataType: 'json',
        data: {
            modelName: model,
            memberName: member
        },
        error: function (response) {
            alert("Syntax error on " + target_url);
        },
        success: function (respones) {
            var html = '<option>-----</option>';
            $.each(respones.data, function (key, item) {
                html += `<option val="${item}">${item}</option>`;
                $('#nickname').html(html);
            });

            return;
        }
    });
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}

function show_delay_dialog(show_url, delete_url) {
    $.ajax({
        url: show_url,
        type: 'post',
        dataType: 'json',
        success: function (data) {
            if(data.length !== 0){
                build_delay_dialog(data, delete_url);
                play_alert_audio();
            }
        }
    });
}

function build_delay_dialog(delay_data, delete_url) {
    var position_value = 0;
    $.each(delay_data, function (key, value) {
        position_value = position_value + 35;
        $("<div id='messagebox_" + key + "'></div>")
            .dialog({
                "width": "400",
                "position": {
                    my: "center+" + position_value + " center+" + position_value,
                    at: "center-35% center-25%",
                    of: window
                },
                "title": value['model_name'] + '_' + value['member_name'] + '（' + value['nickname'] + '） Maybe Delay',
                "buttons": {
                    "OK": function () {
                        delete_delay_data(value['id'], delete_url);
                        $(this).dialog("close");
                    }
                }
            })
            .dialogExtend({
                "closable": false,
                "minimizable": true,
                "collapsable": true,
                "dblclick": "collapse",
                "titlebar": "transparent",
                "icons": {
                    "maximize": "ui-icon-circle-plus",
                },
            });

        if(value['run_type'] === 'Default') {
            value['run_type'] = '';
        }

        var message = "DTG & RUN：" + value['dtg'] + ' ' + value['run_type'];
        message += "<br>Job Start Time : " + value['model_start_time'];
        message += "<br>Shell / Stage Name：" + value['shell_name'] + ' / ' + value['batch_name'];
        message += "<br>Shell Estimated End Time : " + value['predicted_end_time'];

        $('#messagebox_' + key).append(message);
    })
}

function delete_delay_data(delay_id, delete_url) {
    $.ajax({
        url: delete_url,
        type: 'post',
        data: {
            "id": delay_id
        },
        dataType: 'html',
        success: function (result) {
        }
    });
}

function show_reject_log_dialog(show_url, delete_url) {
    $.ajax({
        url: show_url,
        type: 'post',
        dataType: 'html',
        success: function (reject_log_data) {
            if (reject_log_data !== "status : normal") {
                var data = reject_log_data.replace(/\n/g, "<br>");
                build_reject_dialog(data, delete_url);
                play_alert_audio();
            }
        }
    });
}

function build_reject_dialog(data, delete_url) {
    $("<div id='error_message'></div>")
            .dialog({
                "maxHeight": "300",
                "maxWidth": "400",
                "minWidth": "200",
                "position": {
                    at: "center center-10%",
                    of: window
                },
                "title": "Model Error Message",
                "buttons": {
                    "OK": function () {
                        delete_reject_dialog(delete_url);
                        $(this).dialog("close");
                    }
                }
            })
            .dialogExtend({
                "closable": false,
                "minimizable": true,
                "collapsable": true,
                "dblclick": "collapse",
                "titlebar": "transparent",
                "icons": {
                    "maximize": "ui-icon-circle-plus"
                }
            });
    $('#error_message').html(data);
}

function delete_reject_dialog(delete_url) {
    $.ajax({
        url: delete_url,
        type: 'post',
        dataType: 'json',
        success: function (result) {
        }
    });
}

function play_alert_audio() {
    if($('audio #reject_alert').length !== 0) {
        return;
    }

    var html = "<audio id='reject_alert' src='./media/alert2.mp3' preload='auto'></audio>";
    $("body").append(html);
    document.getElementById('reject_alert').play();
}

function sendTyphoonDataRequest(req_type, uri, div, data_count) {
    if(uri==="typhoon_preview.php"){
        $('#file_created_result').empty();
    }

    // Get form values
    var dtg = $("#dtg").val();
    var typoon_num = $('#num').val();
    var typhoon_data = {};

    for (var i=1; i <= data_count*typoon_num; i++){
        typhoon_data['entry'+i] = $('#typhoon_data_form input[name="entry'+i+'"]').val();
    }

    div = '#' + div;

    if (req_type == 'post') {
        target_url = uri + '?timeStamp=' + new Date().getTime();
        // to avoid browsers loading the past data from cache
    } else {
        target_url = uri;
    }

    $.ajax({
        url: target_url,
        type: 'POST',
        dataType: 'html',
        data: {
            p_dtg: dtg,
            p_typhoon_num: typoon_num,
            p_typhoon_data: JSON.stringify(typhoon_data),
            p_every_data_count: data_count
        },
        error: function () {
            alert("Syntax error on " + uri);
        },
        success: function (response) {
            $(div).html(response);
        }

    });

}