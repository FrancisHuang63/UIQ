<?php
require_once("../sql_query_function.php");
include("../cfg/SV_PATH.php");

$source=get_source_shell($_POST['p_node']);
$path=split("/",$source[0]);
$acc_path="/$path[1]/$path[2]/$path[3]/$path[4]/";
$account = $path[2];

$command="rsh -l ${account} ${DATAMV_IP} /ncs/${HpcCTL}/web/shell/run_Command.ksh ${DM_IP_AMDP} $path[2] $source[0] {$_POST['p_dtg']} ${acc_path}";

exec("$command 2>&1", $out);
foreach($out as $i){
        print("$i<br>");
}

//=====��X���G=====
$message=date('[Y/m/d] [G:i:s]')." Rerun Get data of \"{$_POST['p_node']}\" with {$_POST['p_dtg']}\r\n";
echo $message."Get data completely!";
fwrite(fopen("../log/UI_actions.log","a+"),$message);

?>
