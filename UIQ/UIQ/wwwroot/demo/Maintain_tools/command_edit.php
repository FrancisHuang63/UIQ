<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<?php
		include("./cfg/SV_PATH.php");
    if (isset($command_info)){
        $command_id = $command_info['command_id'];
        $command_name = $command_info['command_name'];
        $command_desc = $command_info['command_desc'];
        $command_content = $command_info['command_content'];
        $command_pwd = $command_info['command_pwd'];
        $command_example = $command_info['command_example'];
        $exec_time = $command_info['execution_time'];
    } else {
        $command_id = "";
        $command_name = "";
        $command_desc = "";
        $command_content = "";
        $command_pwd = "";
        $command_example = "";
        $exec_time = "";
    }

?>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<link href="<?php if ($command_id != ""){ print('../');}?>../packages/bootstrap/3.3.1/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
<link href="<?php if ($command_id != ""){ print('../');}?>../packages/bootstrap/3.3.1/css/bootstrap-theme.min.css" rel="stylesheet" type="text/css" />
<link href="<?php if ($command_id != ""){ print('../');}?>../css/style.css" rel="stylesheet" type="text/css" />
<link href="<?php if ($command_id != ""){ print('../');}?>../css/jqueryslidemenu.css" rel="stylesheet" type="text/css" />

<script type="text/javascript" src="<?php if ($command_id != ""){ print('../');}?>../js/jquery-3.1.1.min.js"></script>
<!-- <script type="text/javascript" src="<?php if ($command_id != ""){ print('../');}?>../js/jquery.min.js"></script> -->
<script type="text/javascript" src="<?php if ($command_id != ""){ print('../');}?>../packages/bootstrap/3.3.1/js/bootstrap.min.js"></script>
<script type="text/javascript" src="<?php if ($command_id != ""){ print('../');}?>../js/tool.js"></script>
<script type="text/javascript" src="<?php if ($command_id != ""){ print('../');}?>../js/command.js"></script>

<title>NWP Command List</title>
</head>

<body>
<div id="loading"><img src="<?php if ($command_id != ""){ print('../');}?>../images/clock.gif"></div>
<div class=topic>Maintain tools > Special Use command > Edit</div>

<div>
<form action='<?php if ($command_id != ""){ print('../Save');} else { print('./Save');}?>' method='post' class='form'>
<table width='100%' id="edit_command_table">
    <tbody>
        <tr>
            <td width=8%>Name</td>
            <td>
                <input type='text' name='name' value='<?= $command_name?>'>
                <input type='hidden' name='id' value='<?= $command_id?>'>
            </td>
        </tr>
        <tr>
            <td>Password</td>
            <td><input type='password' name='pwd' value='<?= $command_pwd?>'></td>
        </tr>
        <tr>
            <td>Execution Time(min)</td>
            <td><input id='exec_time' name='exec_time' value='<?= $exec_time?>'></td>
        </tr>
        <tr>
            <td>Describe</td>
            <td><textarea name='desc'><?= $command_desc?></textarea></td>
        </tr>
        <tr>
            <td>Content</td>
            <td><textarea id='content' name='content'><?= $command_content?></textarea></td>
        </tr>
        <tr>
            <td>Parameter Example</td>
            <td><textarea id='example' name='example'><?= $command_example?></textarea></td>
        </tr>
        <tr>
            <td>Result</td>
            <td><div id="show" class="short">show the Execute result</div></td>
        </tr>
    </tbody>
</table>
<div>
<input type='submit' value='Save'>
<input type='button' value='Execute' onclick='showParameterModal("<?= $command_id?>","ADM");'>
<?php
    if ($command_id != ""){
        print("<input type='button' value='Exit' onclick='if (confirm(\"Exit this edit page without saving?\")){location.href=\"../../command\";}'>");}
    else {  print("<input type='button' value='Exit' onclick='if (confirm(\"Exit this edit page without saving?\")){location.href=\"../command\";}'>");}
?>
</div>
</form>
</div>

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
