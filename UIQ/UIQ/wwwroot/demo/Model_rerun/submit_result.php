<?php
date_default_timezone_set("Asia/Taipei");//set the timezone
require_once("../sql_query_function.php");
require_once("../function.php");
include("../cfg/SV_PATH.php");

// 避免執行時間過長影響其他功能
session_write_close();

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

//由model member判斷account
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path=get_full_path($_POST['p_nickname'],$_POST['p_model'], $_POST['p_member']);
$shell = get_exe_shell($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member'], "submit_model");
$batch = $_POST['p_batch'];

if (isset($shell)){
	$command="rsh -l ${account} -n ${LOGIN_IP} /ncs/${HpcCTL}/web/shell/re_run.ksh $account $path$shell {$_POST['p_model']} {$_POST['p_member']} $batch ${path}";
	//輸出結果
	$message.=date('[Y/m/d] [G:i:s]')." Rerun the {$_POST['p_model']} {$_POST['p_member']} {$_POST['p_nickname']} with {$_POST['p_dtg']} in $batch run \r\n";
	echo $message;
} else {
	//輸出結果
	$message="submit model function is not avaliable for this member.\r\n";
	$command="echo $message";
}

$result=shell_exec($command);
$result = preg_replace("/\n/", "\n<br>", $result);
print "<br>$result";

fwrite(fopen("../log/UI_actions.log","a+"),$message);

?>
