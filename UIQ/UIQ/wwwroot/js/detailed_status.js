$(document).ready(function () {
    var status_table_height = $('#top').height();
    $('#main').css('padding-top', status_table_height);

    var target_member = $('#target_member').val();
    if (target_member) {
        var target_tr = '#tablesorter-demo tr[id="' + target_member + '"]';

        $('html, body').animate({
            scrollTop: $(target_tr).offset().top
        }, 200);
    }

    show_delay_dialog("detailed_status/get_shell_delay_data", "detailed_status/delete_shell_delay_data");
    show_reject_log_dialog("detailed_status/load_reject_log", "detailed_status/delete_reject_log");
});


