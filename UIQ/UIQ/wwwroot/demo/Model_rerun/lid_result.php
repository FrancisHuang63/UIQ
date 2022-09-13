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

//由model member nickname判斷account
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path = get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);

//調整Lid
$command="rsh -l ${account} ${LOGIN_IP} /ncs/${HpcCTL}/web/shell/set_Lid.ksh $account $path {$_POST['p_lid']}";
//$command="/ncs/${HpcCTL}/web/shell/set_Lid.ksh $account $path {$_POST['p_lid']}";

echo $command;
system($command);
//輸出結果
$message=date('[Y/m/d] [G:i:s]')." {$_POST['p_model']} {$_POST['p_member']} {$_POST['p_nickname']} adjust LID value to {$_POST['p_lid']}\r\n";
#echo $message;
fwrite(fopen("../log/UI_actions.log","a+"),$message);
?>
