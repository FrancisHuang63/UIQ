$(function () {
    get_model_name();

    $(document).on('change', '#model', function () {
        var model_name = $('#model').val();
        $.ajax({
            url: '/ModelEnquire/GetMemberItems',
            data: {
                'modelName': model_name
            },
            type: 'post',
            dataType: 'json',
            success: function (respones) {
                var html = '<option>-----</optin>';
                if (model_name !== '-----') {
                    $.each(respones.data, function (key, value) {
                        html += "<option value='" + value + "'>" + value + "</optin>";
                    });
                }
                $('#member').html(html);
                $('#member').trigger("change");
            }
        });
    });

    $(document).on('change', '#member', function () {
        var model_name = $('#model').val();
        var member_name = $('#member').val();
        $.ajax({
            url: '/ModelEnquire/GetNicknameItems',
            data: {
                'modelName': model_name,
                'memberName': member_name
            },
            type: 'post',
            dataType: 'json',
            success: function (respones) {
                var html = '<option>-----</optin>';
                if (member_name !== '-----') {
                    $.each(respones.data, function (key, value) {
                        html += "<option value='" + value + "'>" + value + "</optin>";
                    });
                }
                $('#nickname').html(html);
                $('#nickname').trigger("change");
            }
        });
    });

    $(document).on('click', '#enquire', function () {
        $('#result').initial_jtable({
            config: create_batch_history_table,
            filter: {
                'modelName': $('#model').val(),
                'memberName': $('#member').val(),
                'nickname': $('#nickname').val()
            },
            text: false
        });
    });

    $('#enquire').trigger('click');

    function get_model_name() {
        $.ajax({
            url: '/ModelEnquire/GetModelItems',
            type: 'post',
            dataType: 'json',
            success: function (respones) {
                var html = '';
                $.each(respones.data, function (key, value) {
                    html += "<option value='" + value + "'>" + value+ "</optin>";
                });
                $('#model').append(html);
            }
        });
    }

    function create_batch_history_table() {
        return{
            sorting: false,
            pageSize: 20,
            pageSizes: [10,20,25,50,100],
            actions: {
                listAction: '/ModelEnquire/GetModelTimeTableData'
            },
            fields: {
                model: {
                    title: 'Model',
                    width: '3%'
                },
                member: {
                    title: 'Member',
                    width: '3%'
                },
                nickname: {
                    title: 'Nickname',
                    width: '4%'
                },
                run_type: {
                    title: 'Run Type',
                    width: '5%'
                },
                batch_time: {
                    title: 'Batch Time (setup/total/min)',
                    width: '6%'
                },
                avg_execution_time: {
                    title: 'Batch Time (online/total/min)',
                    width: '6%'
                },
                cron_mode: {
                    title: 'Cron mode',
                    width: '6%'
                },
                typhoon_mode: {
                    title: 'Typhoon mode',
                    width: '6%'
                },
                round: {
                    title: 'Run',
                    width: '3%'
                },
                functions: {
                    title: 'Stage Detail',
                    width: '1%',
                    sorting: false,
                    edit: false,
                    create: false,
                    display: function (model_data) {
                        return $('#result').initial_jtable_child({
                            config: get_batch_detail,
                            filter: function () {
                                return{
                                    'modelName': model_data.record.model,
                                    'memberName': model_data.record.member,
                                    'nickname': model_data.record.nickname,
                                    'runType': model_data.record.run_type,
                                    'typhoonMode': model_data.record.typhoon_mode,
                                    'cronMode': model_data.record.cron_mode,
                                    'round': model_data.record.round
                                };
                            }
                        });
                    }
                },
                shell: {
                    title: 'Shell Detail',
                    width: '1%',
                    sorting: false,
                    edit: false,
                    create: false,
                    display: function (batch_data) {
                        return $('#result').initial_jtable_child({
                            config: get_shell_detail,
                            filter: function () {
                                return{
                                    'modelName': batch_data.record.model,
                                    'memberName': batch_data.record.member,
                                    'nickname': batch_data.record.nickname,
                                    'runType': batch_data.record.run_type,
                                    'typhoonMode': batch_data.record.typhoon_mode,
                                    'cronMode': batch_data.record.cron_mode,
                                    'round': batch_data.record.round
                                };
                            },
                            text: false
                        });
                    }
                }
            }
        };
    }

    function get_batch_detail() {
        return{
            paging: false,
            sorting: false,
            actions: {
                listAction: '/ModelEnquire/GetBatchDetailTableData'
            },
            fields: {
                batch_name: {
                    title: 'Batch Name',
                    width: '40%'
                },
                setting_time: {
                    title: 'Batch Time (setup/accumulate/min)',
                    width: '30%'
                },
                history_time: {
                    title: 'Batch Time (online/accumulate/min)',
                    width: '30%'
                }
            }
        };
    }

    function get_shell_detail() {
        return{
            paging: false,
            sorting: false,
            actions: {
                listAction: '/ModelEnquire/GetShellDetailTableData'
            },
            fields: {
                batch_name: {
                    title: 'Batch Name'
                },
                shell_name: {
                    title: 'Shell Name'
                },
                avg_time_min: {
                    title: 'Shell Avg Time (min)'
                }
            }
        };
    }

    if (typeof String.prototype.trim !== 'function') {
        String.prototype.trim = function () {
            return this.replace(/^\s+|\s+$/g, '');
        };
    }
});


