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

$model=$_POST['p_model']; 
$member=$_POST['p_member'];
$nickname=$_POST['p_nickname'];
$account=$mem["{$model}_{$member}_{$nickname}"]->account;
$dtg_value=$mem["{$model}_{$member}_{$nickname}"]->dtg_value;
$path = get_full_path($nickname, $model, $member);
//$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/etc/crdate | awk '{print $1}'";
$command="cat $path/etc/crdate | awk '{print $1}'";
$dtg=shell_exec($command);
echo <<<HTML
<div class="short">
[Model]=$model, [Member]=$member, [Nickname]=$nickname<BR><BR>DTG=$dtg;
</div>
<br>
<form class="form">Adjust value
<select id="dtg" class="form">
<option value="$dtg_value">+
<option value="-$dtg_value">-
</select> 
$dtg_value
<input type="button" class="form" value="Submit" OnClick="sendAJAXRequest('post', 'dtg_result.php', 'result');">
</form>

HTML;
?>

