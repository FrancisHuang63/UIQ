<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<meta http-equiv="X-UA-Compatible" content="IE=9">

<script type="text/javascript" src="./js/jquery-3.1.1.min.js"></script>
<script type="text/javascript" src="./packages/bootstrap/3.3.1/js/bootstrap.min.js"></script>
<script type="text/javascript" src="./js/apprise-1.5.full.js"></script>
<script type="text/javascript" src="./js/tool.js"></script>
<script type="text/javascript" src="./js/command.js"></script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<link href="./packages/bootstrap/3.3.1/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
<link href="./packages/bootstrap/3.3.1/css/bootstrap-theme.min.css" rel="stylesheet" type="text/css" />
<link href="./css/style.css" rel="stylesheet" type="text/css" />
<link href="./css/jqueryslidemenu.css" rel="stylesheet" type="text/css" />
<link href="./css/apprise.css" rel="stylesheet" type="text/css" />

<script language="javascript"><!--
window.onload = function(){
//senfe("表格名称","奇数行背景","偶数行背景","鼠标经过背景","点击后背景");
    senfe("changecolor","#f8fbfc","#e5f1f4","#ecfbd4","#bce774");
};
--></script>

<title>Special Use Command</title>
</head>

<body>
<div id="loading"><img src="images/clock.gif"></div>
<div class=topic>Maintain tools > Special Use command<input style='float:right' type='button' value='Add' <?php    if ($group == 'OP' ){   print("disabled='disabled'");} ?> onclick='location.href="./command/Add"'></div>
<br>
<div>
<div id="authority" style='display:none'><?php echo $group?></div>
<table class="warp_table" id="changecolor">
    <thead>
        <tr>
            <th>Command Name</th>
            <th width=65%>Describe</th>
            <th width=12%>Exec Time(min)</th>
            <th width=6%>Edit</th>
            <th width=6%>Execute</th>
            <th width=6%>Delete</th>
        </tr>
    </thead>
    <tbody>
<?php
	include("./cfg/SV_PATH.php");
    $order = array("\r\n","\r","\n");
    $replace = "<br>";
    //$contents = str_replace($order,$replace,$contents);
    foreach ($command_info as $command){
            print("<tr>");
        //Command Name
            print("<td>".$command['command_name']."</td>");
        //Describe
            print("<td>".str_replace($order,$replace,$command['command_desc'])."</td>");
        //Execution Time
            print("<td>".$command['execution_time']."</td>");
        //Edit
            if ($group == 'ADM'){
                print("<td><input type='button' value='Edit' onclick='location.href=\"./command/Edit/".$command['command_id']."\"'></td>");
            } else {
                print("<td><input type='button' value='Edit' disabled='disabled'></td>");
            }
        //Execute
            if ($group == 'ADM'){
                print("<td><input id='exe_btn' type='button' id='Execute' value='Exe' onclick='showParameterModal(".$command['command_id'].",\"".$group."\");'></td>");
            } else {
                print("<td><input id='exe_botton'type='button' value='Exe' onclick='showParameterModal(".$command['command_id'].",\"".$group."\");'></td>");
            }
        //Delete
            if ($group == 'ADM'){
                print("<td><input type='button' class='tryit'value='Del' onclick='if (confirm(\"Delete this command?\")){location.href=\"./command/Del/".$command['command_id']."\"}'></td>");
            } else {
                print("<td><input type='button' value='Del' disabled='disabled'></td>");
            }
            print("</tr>");
    }
?>
    </tbody>
</table>
</div>
<br>Execute result:
<div id="show" class="enquire">show the Execute result</div>

<!-- Modal -->
<div aria-hidden="true" aria-labelledby="myModalLabel" role="dialog" tabindex="-1" id="parameter_modal" class="modal fade" style="display: none;">
    <div id="selected_cmd_id" style='display:none'></div>
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <button aria-hidden="true" data-dismiss="modal" class="close" type="button">×</button>
                <h4 class="modal-title">Please Input Command Information</h4>
            </div>
            <div class="modal-body">
                <form id="parameter_form" role="form" class="form-horizontal">
                    <div class="form-group">
                        <label class="col-md-11" for="parameters">Parameter</label>
                        <div class="col-md-11">
                            <input type="text" placeholder="Allow Empty" id="parameters" class="form-control">
                        </div>
                    </div>
                    <div id='pwd_form_group' class="form-group" style="display:none">
                        <label class="col-md-11" for="password">Password</label>
                        <div class="col-md-11">
                            <input type="password" placeholder="" id="password" class="form-control">
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button class="btn btn-info" type="button" id="modal_execute">Execute</button>
                <button type="button" class="btn btn-secondary" data-dismiss="modal" id="modal_close">Close</button>
            </div>
        </div><!-- /.modal-content -->
    </div><!-- /.modal-dialog -->
</div><!-- /.modal -->
</body>
