<?php
require_once("../sql_query_function.php");
require_once("../function.php");
include("../cfg/SV_PATH.php");

//由config設定 將各member物件化
$config=get_model_array();

foreach($config as $mdl => $val)
{
	foreach($val as $mem_name => $set)
    {
        foreach($set as $key => $mem_info){
            $mem_info_array=explode(' ',$mem_info);
            $nickname=$mem_info_array[0];
            
            $mem["{$mdl}_{$mem_name}_{$nickname}"]=new member();//以Model_Member_Nickname當物件index
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->model=$mdl;
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->name=$mem_name;
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->load($config[$mdl][$mem_name][$key]);//Member=nickname account archive rescue (讀入等號的右半段)
        }
    }
}

$index="{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}";//以Model_Member_Nickname當物件index
$mem[$index]->job_log_parse("leaf");

$path=get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
echo "<pre>";


## get file size
$filecommand="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls -l $path/log/{$_POST['p_node']} | awk '{print $5}'";
$filesize=shell_exec($filecommand);
print "File size: $filesize";

## get file time stamp
$filecommand="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} ls -l $path/log/{$_POST['p_node']} | awk '{print $6\" \"$7\" \"$8}'";
$filetime=shell_exec($filecommand);
print "File time: $filetime";


## get last 30 lines of the file
$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} tail -n 30 $path/log/{$_POST['p_node']}";
$log=shell_exec($command);
$logcontarr = preg_split("/\n/", $log);

foreach($logcontarr as $i){
		print "\n$i";
}
echo "</pre>";

?>
