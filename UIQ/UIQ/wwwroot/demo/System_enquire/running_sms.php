<?php
require_once("../function.php");
echo "<pre>";
$command="../shell/sms_enquire.ksh {$_POST['p_model']} {$_POST['p_member']} | grep -v 'Goodbye \|Welcome \|logged \|logout'";
$result=shell_exec($command);
sleep(1.5);
echo $result;
?>
