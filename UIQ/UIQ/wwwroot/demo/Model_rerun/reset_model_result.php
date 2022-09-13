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
            
            $mem["{$mdl}_{$mem_name}_{$nickname}"]=new member();//?HModel_Member?򺰿?index
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->model=$mdl;
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->name=$mem_name;
            $mem["{$mdl}_{$mem_name}_{$nickname}"]->load($config[$mdl][$mem_name][$key]);//Member=nickname account archive rescue (Ū?J???????k?b?q)
        }
    }
}

//由model member判斷account
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path=get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$shell = get_exe_shell($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member'], "reset_model");
$jobid = $_POST['p_jobid'];
if (isset($shell)){
	if($account == "${Prefix}weps"){
		$command="rsh -l ${account} ${LOGIN_IP} ${path}${shell} ${path}";
	}else{
		#$command="rsh -l ${account} ${LOGIN_IP} pjdel ${jobid}";
		$command="rsh -l ${account} ${LOGIN_IP} /ncs/${HpcCTL}/web/shell/cancel_job.ksh ${account} ${path}${shell} ${jobid} ${path}";
	}

	//輸出結果
	$message=date('[Y/m/d] [G:i:s]')."Kill Model of {$_POST['p_model']} {$_POST['p_member']} {$_POST['p_nickname']} \r\n";
	echo $message;
} else {

	//輸出結果
	$message="Cancel running job function is not available for this member.\r\n";
	$command="echo $message";
}

$result=shell_exec($command);
$result = preg_replace("/\n/", "\n<br>", $result);
print "<br>$result";

fwrite(fopen("../log/UI_actions.log","a+"),$message);
?>
