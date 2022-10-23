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
    
    show_delay_dialog("/Home/GetShellDelayData", "/Home/DeleteShellDelayData");
    show_reject_log_dialog("/Home/LoadRejectLog", "/Home/DeleteRejectLog");
});


