<?php
require_once("../sql_query_function.php");
include("../cfg/SV_PATH.php");
$path=get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$keyw=$_POST['p_keyw'];

#$command="rsh -l ${HpcCTL} ${LOGIN_IP} ls $path/log | grep job | grep -v job1 | grep -v OP_";
if($keyw){
	$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls $path/log | grep -i ${keyw}";
}else{
	$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls $path/log";
}

$list=shell_exec($command);

echo "LogFile:<select id='node' class='form' ><option>-----";
$tok = strtok($list, " \n\t");

while ($tok !== false) 
{
    echo "<option>$tok";
    $tok = strtok(" \n\t");
}
echo "</select>";

echo "<input type='submit' value='enquire' class='form' OnClick='sendAJAXRequest(\"post\", \"running_member.php\", \"show\")'>";

?>
