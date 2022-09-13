<?php
require_once("../sql_query_function.php");
include("../cfg/SV_PATH.php");
echo "<pre>";

$source=get_source_path_file($_POST['p_node']);
$path=str_replace("{DTG}",$_POST['p_dtg'],$source[0]);
$file=str_replace("{DTG}",$_POST['p_dtg'],$source[1]);
$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls -al $path/$file";
$data=shell_exec($command);
print "$command\n";

$data=str_replace($path."/","",$data);	//把檔案路徑去除掉

if ( $data == ''){
    $tok = 'No data!!';
} else {
    $tok = strtok($data, " \n\t");
}

//=====把顯示出來的資料切token 再排成table
echo "<table><tr>";
while ($tok !== false) 
{
    echo "<td class='summary'>$tok \n ";
    $tok = strtok(" \n\t");
	if($i%9==8) echo "</td><tr>";
	$i++;
}
?>
