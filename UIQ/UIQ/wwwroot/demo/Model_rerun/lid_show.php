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
//$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/etc/Lid";
$command="cat $path/etc/Lid";
$lid=shell_exec($command);
echo "[Model]=".$_POST['p_model'].", [Member]=".$_POST['p_member'].", [Nickname]=".$_POST['p_nickname']."<BR><BR>LID=$lid";

?>
