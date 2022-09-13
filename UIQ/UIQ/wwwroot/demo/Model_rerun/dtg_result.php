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
//由model member判斷account
	$account = $mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
	$path = get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
	$shell = get_exe_shell($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member'], "dtg_adjust");

//調整DTG	
if (isset($shell)){
	$command="rsh -l ${account} ${LOGIN_IP} /ncs/${HpcCTL}/web/shell/set_dtg.ksh ${account} $path$shell {$_POST['p_dtg']} ${path}";
 	print "$command <br><br>";

	exec("$command 2>&1", $out);
	foreach($out as $i){
            print("$i<br>");
	}

	//輸出結果
	$message=date('[Y/m/d] [G:i:s]')." {$_POST['p_model']} {$_POST['p_member']} {$_POST['p_nickname']} adjust DTG value of {$_POST['p_dtg']} as follows.\r\n${result}\r\n";
	$message = preg_replace("/\n/", "\n<br>", $message);
	print "<br> $message";

}else{
		
	$message="DTG adjust function is not available for this member.\r\n";
	$command="echo $message";
	system($command);
}

fwrite(fopen("../log/UI_actions.log","a+"),$message);
?>
