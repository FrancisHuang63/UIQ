$(function () {
    get_model_name();

    $(document).on('change', '#model', function () {
        var model_name = $('#model').val();
        $.ajax({
            url: '../batch_history/get_member_name',
            data: {
                'model_name': model_name
            },
            type: 'post',
            dataType: 'json',
            success: function (member_name) {
                var html = '<option>-----</optin>';
                if (model_name !== '-----') {
                    $.each(member_name, function (key, value) {
                        html += "<option value='" + value.member_name + "'>" + value.member_name + "</optin>";
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
            url: '../batch_history/get_nickname',
            data: {
                'model_name': model_name,
                'member_name': member_name
            },
            type: 'post',
            dataType: 'json',
            success: function (nickname) {
                var html = '<option>-----</optin>';
                if (member_name !== '-----') {
                    $.each(nickname, function (key, value) {
                        html += "<option value='" + value.nickname + "'>" + value.nickname + "</optin>";
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
                'model_name': $('#model').val(),
                'member_name': $('#member').val(),
                'nickname': $('#nickname').val()
            },
            text: false
        });
    });

    $('#enquire').trigger('click');

    function get_model_name() {
        $.ajax({
            url: '../batch_history/get_model_name',
            type: 'post',
            dataType: 'json',
            success: function (model_name) {
                var html = '';
                $.each(model_name, function (key, value) {
                    html += "<option value='" + value.model_name + "'>" + value.model_name + "</optin>";
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
                listAction: '../batch_history/get_model_time'
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
                                    'model_name': model_data.record.model,
                                    'member_name': model_data.record.member,
                                    'nickname': model_data.record.nickname,
                                    'run_type': model_data.record.run_type,
                                    'typhoon_mode': model_data.record.typhoon_mode,
                                    'cron_mode': model_data.record.cron_mode,
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
                                    'model_name': batch_data.record.model,
                                    'member_name': batch_data.record.member,
                                    'nickname': batch_data.record.nickname,
                                    'run_type': batch_data.record.run_type,
                                    'typhoon_mode': batch_data.record.typhoon_mode,
                                    'cron_mode': batch_data.record.cron_mode,
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
                listAction: '../batch_history/get_batch_detail'
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
                listAction: '../batch_history/get_shell_detail'
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


