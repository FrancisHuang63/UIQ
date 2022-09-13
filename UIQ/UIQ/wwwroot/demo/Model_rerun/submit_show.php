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

//由model member判斷account和LID
$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;
$path = get_full_path($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/etc/crdate | awk '{print $1}'";
$dtg=shell_exec($command);

$command="rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} cat $path/etc/Lid";
$lid=shell_exec($command);
$status=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->status;
if($_POST['p_member'] != "-----"){
    $mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->job_log_parse("leaf");//parse各log檔的status
    $mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->load_lid();
}
$lid = trim($lid);
$disable = ( $lid == '1') ? "disabled" : "";
$batch=get_member_relay($_POST['p_nickname'], $_POST['p_model'], $_POST['p_member']);
$select_batch='<SELECT id="batch" class="form"><option value="">---';
if (isset($batch) && count($batch) > 0){
	foreach($batch as $value=>$relay){
		$output = "";
		$DataList_R = "";
		exec("ls /ncs/$account/{$_POST['p_model']}/{$_POST['p_member']}/etc/$relay"."*", $output);
		if(count($output) > 0)
		{
			foreach ($output as &$tmpfile){
				$aList = explode('/',$tmpfile);
	   			$tmp = $aList[count($aList)-1];
	   			$IsStatus = false;
				if( substr($tmp, -2) != "_R")
				{
					foreach ($output as &$tmpfile1){
						$aList1 = explode('/',$tmpfile1);
	   					$tmp1 = $aList1[count($aList1)-1];
						if($tmp.'_R' == $tmp1)
						{
							
							$IsStatus = true;
							break;
						}
					}
					if($IsStatus)
					{
						if($DataList_R  == "")
							$DataList_R = $tmp1;
						else 
							$DataList_R = $DataList_R."|".$tmp1;
					}
					if( substr($tmp, -2) == "_M")
					{
						if($IsStatus)
							$select_batch.='<option value="'.$tmp1.'">'.$tmp.'ajor';
						else
							$select_batch.='<option value="'.$tmp.'">'.$tmp.'ajor';
					}
					elseif( substr($tmp, -2) == "_P")
					{
						if($IsStatus)
							$select_batch.='<option value="'.$tmp1.'">'.$tmp.'ost';
						else
							$select_batch.='<option value="'.$tmp.'">'.$tmp.'ost';
					}
					elseif( substr($tmp, -3) == "_P2")
					{
						if($IsStatus)
							$select_batch.='<option value="'.$tmp1.'">'.substr($tmp,0 ,count($tmp)-2).'ost2';
						else
							$select_batch.='<option value="'.$tmp.'">'.$tmp.'ost';
					}
					elseif( substr($tmp, -3) == "_MC")
					{
						$select_batch.='<option value="'.$tmp.'">'.substr($tmp,0 ,count($tmp)-2).'ajorC';
					}
					else 
					{
						if($IsStatus)
							$select_batch.='<option value="'.$tmp1.'">'.$tmp;		
						else 
							$select_batch.='<option value="'.$tmp.'">'.$tmp;
					}
				}
				$IsStatus = false;
				if( substr($tmp, -2) == "_R")
				{	
					$aList2 = explode('|',$DataList_R);
					if(count($aList2) > 0)
					{
						foreach ($aList2 as &$tmpfile1){
							if($tmp == $tmpfile1)
							{
								$IsStatus = true;
								break;
							}	
						}
						
					}
					
					if(!$IsStatus)
						$select_batch.='<option value="'.$tmp.'">'.$tmp;
				}
			}
		}
	}
}
$select_batch.='</SELECT>';
echo <<<HTML
<div class="short">
[Model]={$_POST['p_model']}, [Member]={$_POST['p_member']}, [Nickname]={$_POST['p_nickname']}<br><br> 
DTG=$dtg&nbsp;
Lid=$lid&nbsp;
batch:$select_batch&nbsp;
</div>
<input type="hidden" id="dtg" value="$dtg">
<input type="button" $disable class="form" value="Submit" OnClick="sendAJAXRequest('post', 'submit_result.php', 'result');">
HTML;
?>
