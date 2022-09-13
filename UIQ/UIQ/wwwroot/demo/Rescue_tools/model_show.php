<?php
require_once("../sql_query_function.php");
require_once("../function.php");
include("../cfg/SV_PATH.php");

//=====??config?]?w ?N?Umember??????=====
$config=get_output_array();

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

//?ھ?configŪ?X?Ҧ????b??
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path=get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$data_types=get_output_datatype($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
//Ū?X??DTG
$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/etc/crdate | awk '{print $1}'";
$dtg=shell_exec($command);
echo <<<TAG
Current DTG<input id="dtg" type="text" class="form" cols="70" value="$dtg"> (the DTG format is yymmddhh)
TAG;
echo "<br><br>Select work to do<select id=\"method\" class=\"form\">";
foreach($data_types as $data_type)
{
    echo "<option>$data_type";
}
echo "</select>";
?>
