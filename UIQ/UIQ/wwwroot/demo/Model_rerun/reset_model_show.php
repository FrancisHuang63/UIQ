<script src="../js/jquery-1.3.2.min.js"></script>
<script src="../js/tool.js"></script>
<link rel=stylesheet type="text/css" href="../../css/style.css">
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

$account=$mem["{$_POST['p_model']}_{$_POST['p_member']}_{$_POST['p_nickname']}"]->account;

echo "<div id='show' class='enquire'><pre>======================current Job status======================\n";

#$pjstat = `cat pjstat_res.txt | sed '1,4d'`;    ## 測試用
$pjstat = `rsh -l ${RSH_ACCOUNT} ${LOGIN_IP} /usr/bin/pjstat -A -s | sed '1,4d'`;
#$pjstat = `rsh -l ${HpcCTL} ${LOGIN_IP} pjstat -A -s | sed '1,4d'`;
#$pjstat = `rsh -l ${HpcCTL} ${LOGIN_IP} pjstat100 -A -s | sed '1,4d'`;
$pjstat = str_replace("#fx100# ","",$pjstat);
$pjstat = str_replace("#fx10 # ","",$pjstat);

#$pjstatarr = preg_split("/\n\n\n/", $pjstat);

$pjstatarr = preg_split("/\[Job\s+Statistical\s+Information\]/", $pjstat);
if(count($pjstatarr)>2)
print_r( $pjstatarr[1]);
else
print_r( $pjstatarr[0]);

$WEPS_flg = 0;
if($_POST['p_model'] == "WEPS" && $_POST['p_member'] == "CEN01"){
	$WEPS_flg = 1;
}

if($WEPS_flg == 1){
	$marr = preg_grep("%JOB NAME.*{$_POST['p_model']}.*E[012][0-9].*\n JOB TYPE%i", $pjstatarr);
}else{
	$marr = preg_grep("%JOB NAME.*{$_POST['p_model']}.*{$_POST['p_member']}.*\n JOB TYPE%i", $pjstatarr);
}
$marr = preg_grep("%USER.*${account}\n GROUP%", $marr);
$marr = array_values($marr);

print "<h3>{$_POST['p_model']}_{$_POST['p_member']}(User: ${account}, Nickname: {$_POST['p_nickname']})\n</h3>";

$jobidarr = array();
$jobnmarr = array();
if(! $marr){
	print "There is no job!\n";
	
}else{
	foreach($marr as $i){
		print "$i\n";
		print "-----------------------------------------------------------------\n";
		$marr_lines = preg_split("/\n/", $i);
		$jobid_line = preg_split("/: /", $marr_lines[1]);
		$jobnm_line = preg_split("/: /", $marr_lines[6]);
		array_push($jobidarr, $jobid_line[1]);
		array_push($jobnmarr, $jobnm_line[1]);
	}
}

print "\n";
print "</div>";

$len1 = count($jobidarr);
$len2 = count($jobnmarr);

if($len1 != $len2){
	print "Error occurred!";

}elseif($len1 != 0 && $WEPS_flg == 1){
	echo "<SELECT id=\"jobid\" class=\"form\">";
	echo "<option>-----";
	echo "<option value=\"ALLWEPS\">ALL jobs above";
	echo "</Select>";
}else{
	echo "<SELECT id=\"jobid\" class=\"form\">"; 
	echo "<option>-----";
	for($i = 0; $i < $len1; $i++){
		echo "<option value=\"$jobidarr[$i]\">$jobnmarr[$i]($jobidarr[$i])";
	}
	echo "</Select>";
}

echo <<<HTML
<input id="node" type="hidden" value="$job_id">
<input id="account" type="hidden" value="$account">
<input id="adjust" type="hidden" value="$flg">
<input type="button" class="form" value="kill" OnClick="if(confirm('Do you want to submit?'))  {sendAJAXRequest('post', 'reset_model_result.php', 'result');} ">

<div id="result" class="short">
result...
</div>
HTML;

?>
