//Ajax Loading display
$(document).ready(function () {
    $("#loading").ajaxStart(function () {
        $(this).show();
    });

    $("#loading").ajaxStop(function () {
        $(this).hide();
    });

});

function htmlEncode(s) {
    return s.replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/'/g, '&#39;')
        .replace(/"/g, '&#34;');
}

function htmlDecode(str) {
    let div = document.createElement("div");

    return div.html(str).text();
}

//Ajax function by jQuery
function sendAJAXRequest(req_type, uri, callBackFunction) {
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

    target_url = uri;

    $.ajax({
        url: target_url,
        type: req_type,
        dataType: 'json',
        data: {
            ModelName: model,
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
        success: callBackFunction
    });
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

function clearSelectOption(elementId) {
    $(`#${elementId}`).html('<option value="">-----</option>')
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
            var html = '<option value="">-----</option>';
            $.each(respones.data, function (key, item) {
                html += `<option val="${htmlEncode(item)}">${htmlEncode(item)}</option>`;
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
        success: function (response) {
            if (response.success) {
                if (response.data.length !== 0) {
                    build_delay_dialog(response.data, delete_url);
                    play_alert_audio();
                }
            }
            else {
                alert(htmlEncode(response.message));
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
                        document.getElementById('alert_audio').pause();
                        document.getElementById('alert_audio').currentTime = 0;
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

        if (value['run_type'] === 'Default') {
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
        dataType: 'json',
        success: function (result) {
        }
    });
}

function show_reject_log_dialog(show_url, delete_url) {
    $.ajax({
        url: show_url,
        type: 'post',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                if (response.data !== "status : normal") {
                    build_reject_dialog(htmlEncode(response.data), delete_url);
                    play_reject_audio();
                }
            }
            else {
                alert(htmlEncode(response.message));
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
                    document.getElementById('reject_audio').pause();
                    document.getElementById('reject_audio').currentTime = 0;
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
    data = data.replace(/\n/g, "<br>");
    $('#error_message').html(htmlEncode(data));
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
    if ($('audio #alert_audio').length !== 0) {
        return;
    }

    var html = "<audio id='alert_audio' src='/media/delaymedia.mp4' preload='auto'></audio>";
    $("body").append(html);
    document.getElementById('alert_audio').play();
}

function play_reject_audio() {
    if ($('audio #reject_audio').length !== 0) {
        return;
    }

    var html = "<audio id='reject_audio' src='/media/alert2.mp4' preload='auto'></audio>";
    $("body").append(html);
    document.getElementById('reject_audio').play();
}

function padLeft(str, len, paddingStr) {
    str = '' + str;
    if (str.length >= len) {
        return str;
    } else {
        return padLeft(paddingStr + str, len, paddingStr);
    }
}

function padRight(str, len, paddingStr) {
    str = '' + str;
    if (str.length >= len) {
        return str;
    } else {
        return padRight(str + paddingStr, len, paddingStr);
    }
}

Array.prototype.groupBy = function (prop) {
    return this.reduce(function (groups, item) {
        const val = item[prop]
        groups[val] = groups[val] || []
        groups[val].push(item)
        return groups
    }, {})
};