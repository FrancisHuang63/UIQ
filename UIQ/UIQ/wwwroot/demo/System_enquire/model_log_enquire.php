<?php

require_once("../sql_query_function.php");
include("../cfg/SV_PATH.php");
$path = get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$command = "rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls $path/log | grep job | grep -v job1 | grep -v OP_";

$list = shell_exec($command);

echo "Date:<select id='node' class='form'><option>-----";
$tok = strtok($list, " \n\t");

while ($tok !== false) {
    echo "<option>$tok";
    $tok = strtok(" \n\t");
}

echo <<<TAG
</select><input type='submit' value='enquire' class='form' OnClick="sendAJAXRequest('post', 'model_log_result.php', 'result')";>

TAG;
?>
