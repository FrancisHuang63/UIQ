$(document).ready(function () {
    show_delay_dialog("/Home/GetShellDelayData", "/Home/DeleteShellDelayData");
    show_reject_log_dialog("/Home/LoadRejectLog", "/Home/DeleteRejectLog");
});