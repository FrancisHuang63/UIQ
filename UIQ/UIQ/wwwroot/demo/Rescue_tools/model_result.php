<?php
require_once("../sql_query_function.php");
require_once("../function.php");
include("../cfg/SV_PATH.php");

//=====由config設定 將各member物件化=====
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

//=====根據config讀出模式之帳號=====
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path=get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$shell=get_output_exe_shell($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member'], $_POST['p_method']);

$tau1=$_POST['p_tau1'];
$tau2=$_POST['p_tau2'];
if  ( is_numeric($tau1) && is_numeric($tau2) && $tau1 >= 0 && $tau2 >= 0)
{
if($tau1 > $tau2) echo "the tau range is illegal !";
//將tau1轉換成6倍數
$tau1=ceil($tau1/6)*6;
$tau2=ceil($tau2/6)*6;
call_shell($tau1,$tau2,$account,$path.$shell,$LOGIN_IP,$path);	

}
else
echo "the tau format is illegal !";
	

    
function call_shell($tau1,$tau2,$account,$shell_path,$LOGIN_IP,$path)
{
include("../cfg/SV_PATH.php");
	for($n=$tau1;$n<=$tau2;$n=$n+6)
	{
	$temp=4-strlen($n);//相減後的tau長度
	
	//=====將tau補齊4碼=====		
		for($i=1;$i<=$temp;$i++)
		{
			$n="0$n";
		}

        $command="rsh -l ${account} ${LOGIN_IP} /ncs/${HpcCTL}/web/shell/run_Output.ksh $account $shell_path {$_POST['p_dtg']} $n {$_POST['p_model']} {$_POST['p_member']} ${path}";

        print "COMMAND: $command<br>";
        $result=shell_exec($command);
	$result = preg_replace("/\n/", "\n<br>", $result);
	echo "$result";

	}
	//=====UI動作寫入UI_actions.log=====
	$message=date('[Y/m/d] [G:i:s]')." rescue the Model output in {$_POST['p_method']} with {$_POST['p_dtg']} from {$_POST['p_tau1']} to {$_POST['p_tau2']} on {$_POST['p_model']} {$_POST['p_member']} {$_POST['p_nickname']}\r\n";
	fwrite(fopen("../log/UI_actions.log","a+"),$message);
}
?>
