<?php

require_once("../sql_query_function.php");
include("../cfg/SV_PATH.php");
$path = get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);

$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/log/{$_POST['p_node']}";
$content = shell_exec($command);
$logcontarr = preg_split("/\n/", $content);
echo "<pre>";
foreach ($logcontarr as $i) {
    if (preg_match("/^[A-Z]/", $i)) {
        print "<span class=c4>$i\n</span>";
    } elseif (preg_match("/Finish/i", $i)) {
        print "<span class=c4>$i\n</span>";
    } elseif (preg_match("/fail/i", $i)) {
        print "<span class=c3>$i\n</span>";
    } elseif (preg_match("/cancel/i", $i)) {
        print "<span class=c3>$i\n</span>";
    } else {
        print "$i\n";
    }
}
echo "</pre>";
?>
