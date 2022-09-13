<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<?php
$member_id = 0;
$model_id = 0;
$member_name = "";
$nickname = "";
$account = "";
$member_path = "";
$reset_model = "";
$dtg_adjust = "";
$fix_failed_model = "";
$submit_model = "";
$dtgvalue = "";
$model_group = "";
$member_position = "";
$maintainer_status = 0;
$normal_pre_time = "";
$typhoon_pre_time = "";
$typhoon_model= "";

if (isset($id)){
	// edit page
	for($i=0; $i < count($member); $i++){
		if ( $member[$i]['member_id'] == $id){
			$member_id = $id;
			$model_id = $member[$i]['model_id'];
			$member_name = $member[$i]['member_name'];
			$nickname = $member[$i]['nickname'];
			$account = $member[$i]['account'];
			$dtgvalue = $member[$i]['member_dtg_value'];
			$member_path = substr($member[$i]['member_path'],1);
			$reset_model = substr($member[$i]['reset_model'],1);
			$dtg_adjust = substr($member[$i]['dtg_adjust'],1);
			$fix_failed_model = substr($member[$i]['fix_failed_model'],1);
			$submit_model = substr($member[$i]['submit_model'],1);
			$maintainer_status = $member[$i]['maintainer_status'];
			$normal_pre_time = $member[$i]['normal_pre_time'];
			$typhoon_pre_time = $member[$i]['typhoon_pre_time'];
			$model_group = $member[$i]['model_group'];
			$member_position = $member[$i]['member_position'];
            		$typhoon_model = $member[$i]['typhoon_model'];
		}
	}
	$model_position = 0;
	$new_model_position = 0 ;
	$model_option = "";
	for($i=0; $i < count($model); $i++){
        if ( $model[$i]['model_position'] >= $new_model_position){
                $new_model_position = $model[$i]['model_position'] + 1;
            }

        if ( $model[$i]['model_id'] == $model_id) {
        $model_option .= "<option value='".$model[$i]['model_id']."' data-position='".$model[$i]['model_position']."' selected='selected'>".$model[$i]['model_name']."</option>";
                $model_position = $model[$i]['model_position'];
        } else {
                $model_option .= "<option value='".$model[$i]['model_id']."' data-position='".$model[$i]['model_position']."'>".$model[$i]['model_name']."</option>";
        }
	}

} else {
	// add page
	if (isset($model)){
        $model_position = '';
        $new_model_position = 0;
        $model_option = "";
        for($i=0; $i < count($model); $i++){
        if ( $model[$i]['model_position'] >= $new_model_position){ $new_model_position = $model[$i]['model_position'] + 1;}
        $model_option .= "<option value='".$model[$i]['model_id']."' data-position='".$model[$i]['model_position']."'>".$model[$i]['model_name'];
        }
	}
}
	//data 下拉選單
if (isset($data)){
	$data_option = "";
	for($i=0; $i < count($data); $i++){
		$data_option .= '<option value="'.$data[$i]['data_id'].'">'.$data[$i]['data_name'];
	}
}
	//work 下拉選單
if (isset($work)){
	$work_option = "";
	for($i=0; $i < count($work); $i++){
		$work_option .= '<option value="'.$work[$i]['work_id'].'">'.$work[$i]['work_name'];
	}
}
?>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../js/jquery-3.1.1.min.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../packages/jquery-ui-1.12.1/jquery-ui.min.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../js/apprise-1.5.full.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../js/tool.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../packages/jquery-flexselect/jquery.flexselect.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../packages/jquery-flexselect/liquidmetal.js"></script>
<script type="text/javascript" src="<?php if (isset($id)){ echo "../";}?>../js/setting.js"></script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<link href="<?php if (isset($id)){ echo "../";}?>../packages/jquery-ui-1.12.1/jquery-ui.min.css" rel="stylesheet" type="text/css" />
<link href="<?php if (isset($id)){ echo "../";}?>../css/style.css" rel="stylesheet" type="text/css" />
<link href="<?php if (isset($id)){ echo "../";}?>../css/jqueryslidemenu.css" rel="stylesheet" type="text/css" />
<link href="<?php if (isset($id)){ echo "../";}?>../css/apprise.css" rel="stylesheet" type="text/css" />
<link href="<?php if (isset($id)){ echo "../";}?>../packages/jquery-flexselect/flexselect.css" rel="stylesheet" type="text/css" />


<title>Model & Member Setting</title>
</head>
<body>
<div id="loading"><img src="<?php if (isset($id)){ echo "../";}?>../images/clock.gif"></div>
<div class=topic>Model & Member Setting</div>
<br>
<?= "<h1>".$infomation."</h1>" ?>
<div>

<?php
    if (isset($id)){
        echo "<input id='memberID' type='hidden' name='memberID' value='$id'>";
    }
?>

<form id='setting' name='setting' action='<?php if (isset($id)){ echo "../update";} else { echo "./save";}?>' method='post' autocomplete='off'>

    <div class="check_point_dialog" id="check_point_dialog" title="Check Points" style="display:none">
        <table width='100%' border='1'>
            <thead>
                <tr>
                    <th id="batch_name" width=65%></th>
                    <th id="add_check_btn" width=35%>
                        <a type="button" id="addCheckPoint" class="btn_default">
                        <img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" alt="新增"/>
                        Add Check Point
                        </a>
                    </th>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>
    </div>

<table width='85%' border='1'>
    <thead>
        <tr>
            <th>Name</th>
            <th width=80%>Setting</th>
        </tr>
    </thead>
    <tbody>
        <tr><td>Model Set</td>
            <td><table>
                <tr><td><input type="radio" name="model_rd" value="def" <?php if (isset($id)){ echo "checked";}?>/>Old Model Name<font color='red'>*</font>：</td>
                    <td><select id="model_select" name="model_select" /><?php	echo $model_option; ?></select><input type='button' value='DelModel' onclick='deleteModelCheck()'></td></tr>
                <tr><td><input type="radio" name="model_rd" value="input" />New Model Name<font color='red'>*</font>：</td>
                    <td><input type="text" name="model_input">(Ex: NFS)</td></tr>
                <tr><td>Model Position<font color='red'>*</font>：</td>
                    <td><input type="text" name="model_position" value="<?= $model_position?>">(Ex: 24)</td></tr>
                <input type="hidden" name="model_id" value="<?= $new_model_id?>"><!-- get the Max value -->
                <input type="hidden" name="new_model_position" value="<?= $new_model_position?>"><!-- get the Max value -->
            </table></td>
        </tr>
		<tr><td>Member Set</td>
            <td><table>
			<tr><td>Member Name<font color='red'>*</font>：</td>
				<td><input type="text" name="member_input" value="<?= $member_name ?>">(Ex: M03)
					<input type="hidden" name="member_id" value="<?php if (isset($id)){ echo $id;} else {echo $new_member_id;}?>"><!-- get the Max value --></td></tr>
			<tr><td>Member Nickname<font color='red'>*</font>：</td>
				<td><input type="text" name="member_nickname" value="<?= $nickname?>"></td></tr>
            <tr><td>Member Position<font color='red'>*</font>：</td>
				<td><input type="text" name="member_position" value="<?= $member_position ?>">(Ex: 24)</td></tr>
			<tr><td>Member Account<font color='red'>*</font>：</td>
				<td><input type="text" name="member_account" value="<?= $account ?>">(Ex: npcaxxx)</td></tr>
			<tr><td>Member Path：</td>
				<td><input type="text" name="member_path" value="<?= $member_path ?>">(Ex: /ncs/npcaxxx/<font color='red'>MemberPath</font>/NFS/M03)</td></tr>
			<tr><td>Dtg Value<font color='red'>*</font>：</td>
				<td><input type="text" name="member_dtg_value" value="<?= $dtgvalue ?>">(Ex: 6 or 12 or 24)</td></tr>
			<tr><td>Group Member Name：</td>
				<td><input type="text" name="model_group" value="<?= $model_group ?>">(Ex: M01 TWRF or M03 caa)</td></tr>
			<tr><td>Member Reset：</td>
				<td><input type="text" name="member_reset_model" value="<?= $reset_model ?>">(Ex: cgibin/KillJob.ksh)</td></tr>
			<tr><td>Member Dtg Adjust：</td>
				<td><input type="text" name="member_dtg_adjust" value="<?= $dtg_adjust ?>">(Ex: bin/setdtg.sh)</td></tr>
			<tr><td>Member Fix：</td>
				<td><input type="text" name="member_fix_fialed_model" value="<?= $fix_failed_model ?>">(Ex: cgibin/Rescue.ksh)</td></tr>
			<tr><td>Member Submit：</td>
				<td><input type="text" name="member_submit_model" value="<?= $submit_model ?>">(Ex: cgibin/ReRun.ksh)</td></tr>
			<tr><td>Normal Prediction Time：</td>
				<td><input type="text" name="member_normal_pre_time" value="<?= $normal_pre_time ?>">(Ex: 300)</td></tr>
			<tr><td>Typhoon Prediction Time：</td>
                <td><input type="text" name="member_typhoon_pre_time" value="<?= $typhoon_pre_time ?>">(Ex: 300)</td></tr>
            <tr><td>About Typhoon：</td>
                <td><input type="checkbox" id="typhoon_model" name="typhoon_model" <?php if ($typhoon_model != null && $typhoon_model == '1') echo 'checked="true"';?> value='1'></td></tr>
            </table></td>
        </tr>
        <tr><td>Cron Table<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingCronGroup()" alt="新增"/></td>
            <td><table id="crontabTable"><!-- "+" & "-" -->

			<?php	//Cron Table
			$crontabRowNum = 0;
			$active_gid = "Normal";
			if (isset($id) && isset($crontab)){
				for($i=0; $i < count($crontab); $i++){
					if ($crontab[$i]['member_id'] == $member_id){
						if(isset($cur_gid) && $cur_gid != $crontab[$i]['cron_group']){
							echo "</table>\n";
						}

						if(! isset($cur_gid) || $cur_gid != $crontab[$i]['cron_group']){
							$cur_gid = $crontab[$i]['cron_group'];
							if($crontab[$i]['group_validation'] == 1){
								echo "<td valign=top><table id=\"crongrpTable_$cur_gid\" border=1 bgcolor=pink><th bgcolor=lightcyan>$cur_gid\n";
								$active_gid = $cur_gid;
							}else{
								echo "<td valign=top><table id=\"crongrpTable_$cur_gid\" border=1><th bgcolor=lightcyan>$cur_gid\n";
							}
							echo "<img src=\"../../images/add_icons.png\" onclick=\"addSatnamingCrontab('$cur_gid')\" alt=\"新增\"/>\n";
						}
				?><tr><td id="crontab_<?= $crontabRowNum?>"><input id="crontabInput_<?= $crontabRowNum?>" name="crontabInput_<?= $crontabRowNum?>" type="text" value="<?= $crontab[$i]['start_time'] ?>"><img src="../../images/delete_icons.png" onclick="deleteSatnamingCrontab('crongrpTable_<?= $cur_gid?>', 'crontab_<?= $crontabRowNum?>')" alt="移除"/> <input id="crontab_gid_<?= $crontabRowNum?>" name="crontab_gid_<?= $crontabRowNum?>" type="hidden" value="<?=$crontab[$i]['cron_group'] ?>"
</td></tr>
			<?php   $crontabRowNum++;
					}
				}
				if($crontabRowNum == 0){
					?><tr><table id="crongrpTable_Normal" border=1><th bgcolor=lightcyan>Normal<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingCrontab('Normal')" alt="新增"/><tr><td id="crontab_0"><input id="crontabInput_0" name="crontabInput_0" type="text" value="">(Ex: 17:00:00)<img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingCrontab('crontab_0')" alt="移除"/></td> <input id="crontab_gid_<?= $crontabRowNum?>" name="crontab_gid_<?= $crontabRowNum?>" type="hidden" value="Normal">
			<?php
					$crontabRowNum++;

				}else{
					echo "</table>";
				}
			} else {?>
				<tr><table id="crongrpTable_Normal" border=1><th bgcolor=lightcyan>Normal<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingCrontab('Normal')" alt="新增"/><tr><td id="crontab_0"><input id="crontabInput_0" name="crontabInput_0" type="text" value="">(Ex: 17:00:00)<img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingCrontab('crontab_0')" alt="移除"/></td> <input id="crontab_gid_<?= $crontabRowNum?>" name="crontab_gid_<?= $crontabRowNum?>" type="hidden" value="Normal">
			<?php
					$crontabRowNum++;
			}?>

			</table>
			<input id="crontabRowNum" type="hidden" name="crontabRowNum" value="<?= $crontabRowNum?>">
			<input id="validation" type="hidden" name="validation" value="<?= $active_gid?>"></td>
        </tr>
		<tr><td>Batch<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingBatch()" alt="新增"/></td>
            <td><table id="batchTable" border='1' frame='below'><!-- "+" & "-" -->

			<?php	//Batch Table
			$batchRowNum = 0;
			if (isset($id) && isset($batch)){
				for($i=0; $i < count($batch); $i++){
					if ($batch[$i]['member_id'] == $member_id){
                        $batch_id = $batch[$i]['batch_id'];
			?>
                        <tr>
                            <td id="batch_<?= $batchRowNum?>">
                                <table width='100%'>
                                    <tr>
                                        <td>Batch Position<font color='red'>*</font></td>
                                        <td>
                                            <input id="batchInputPosition_<?= $batchRowNum?>" name="batchInputPosition_<?= $batchRowNum?>" type="text" value="<?= $batch[$i]['batch_position'] ?>">
                                            <img src="../../images/delete_icons.png" onclick="deleteSatnamingBatch('batch_<?= $batchRowNum?>',<?=$batchRowNum?>)" alt="移除" align="right"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Batch Name<font color='red'>*</font></td>
                                        <td>
                                            <input class="batchName" id="batchInputName_<?= $batchRowNum?>" name="batchInputName_<?= $batchRowNum?>" type="text" size="17" value="<?= $batch[$i]['batch_name'] ?>">
                                            <input id="batchInputRelay_<?= $batchRowNum?>" name="batchInputRelay_<?= $batchRowNum?>" type="checkbox" <?php if ($batch[$i]['batch_relay'] != null && $batch[$i]['batch_relay'] == '1') echo 'checked="true"';?> value="1">
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Batch Type</td>
                                        <td>
                                            <input id="batchInputType_<?= $batchRowNum?>" name="batchInputType_<?= $batchRowNum?>" type="text" value="<?= $batch[$i]['batch_type'] ?>">
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Batch Dtg</td>
                                        <td>
                                            <input id="batchInputDtg_<?= $batchRowNum?>" name="batchInputDtg_<?= $batchRowNum?>" type="text" value="<?= $batch[$i]['batch_dtg'] ?>">
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Batch Time<font color='red'>*</font></td>
                                        <td>
                                            <input id="batchInputTime_<?= $batchRowNum?>" name="batchInputTime_<?= $batchRowNum?>" type="text"  size="15" value="<?= $batch[$i]['batch_time'] ?>">分鐘
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Check Point
                                            <img id="showCheckPoint" src="../../images/add_icons.png" alt="新增"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <?php
                        if(array_key_exists($batch_id, $check_point)){?>
                            <script>
                                set_old_check_dialog($('#batchInputType_<?= $batchRowNum?>'),$('#batchInputDtg_<?= $batchRowNum?>'),$('#batchInputName_<?= $batchRowNum?>'),"<?= $batch[$i]['batch_name']?>", <?= $batchRowNum?>, <?= json_encode($check_point[$batch_id])?>);
                            </script>
            <?php }

                        $batchRowNum++;
					}
				}
			} else {?>
				<tr>
                    <td id="batch_0">
                        <table width='100%'>
                            <tr>
                                <td>Batch Position<font color='red'>*</font></td>
                                <td>
                                    <input id="batchInputPosition_0" name="batchInputPosition_0" type="text" value="">(Ex: 1, 2 ,3)
                                    <img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingBatch('batch_0')" alt="移除" align="right"/>
                                </td>
                            </tr>
                            <tr>
                                <td>Batch Name<font color='red'>*</font></td>
                                <td>
                                    <input class='batchName' id="batchInputName_0" name="batchInputName_0" type="text" size="17" value="">
                                    <input id="batchInputRelay_0" name="batchInputRelay_0" type="checkbox" value="1">(Ex: Rnwpnfs1&是否為中繼點)
                                </td>
                            </tr>
                            <tr>
                                <td>Batch Type</td>
                                <td>
                                    <input id="batchInputType_0" name="batchInputType_0" type="text" value="">(Ex: Major, Post)
                                </td>
                            </tr>
                            <tr>
                                <td>Batch Dtg</td>
                                <td>
                                    <input id="batchInputDtg_0" name="batchInputDtg_0" type="text" value="">(Ex: 00, 06)
                                </td>
                            </tr>
                            <tr>
                                <td>Batch Time<font color='red'>*</font></td>
                                <td>
                                    <input id="batchInputTime_0" name="batchInputTime_0" type="text" size="15" value="">分鐘&nbsp;(Ex: 44)
                                </td>
                            </tr>
                            <tr>
                                <td>Check Point<img id="showCheckPoint" src="../images/add_icons.png"" alt="新增"/></td>
                                <td></td>
                            </tr>
                        </table>
                    </td>
                </tr>
			<?php $batchRowNum++;
			}?>

				</table><input id="batchRowNum" type="hidden" name="batchRowNum" value="<?= $batchRowNum?>">
			</td>
		</tr>
		<tr><td>Archive<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingArchive()" alt="新增"/></td>
            <td><table id="archiveTable"><!-- "+" & "-" -->

			<?php	//Archive Table
			$archiveRowNum = 0;
			if (isset($id) && isset($archive)){
				for($i=0; $i < count($archive); $i++){
					if ($archive[$i]['member_id'] == $member_id){
						$data_option = "";
                        $target_directory_i = $archive[$i]['target_directory'];
						for($j=0; $j < count($data); $j++){
							if ($data[$j]['data_id'] == $archive[$i]['data_id']){
								$data_option .= '<option value="'.$data[$j]['data_id'].'" selected="selected" >'.$data[$j]['data_name'];}
							else {$data_option .= '<option value="'.$data[$j]['data_id'].'">'.$data[$j]['data_name'];}
						}
				?>
                <tr>
                    <td id='archive_<?= $archiveRowNum ?>'>Target Directory：
                        <input class='archive_directory' id='archive_directory_<?= $archiveRowNum ?>' type='text' name='archive_directory_<?= $archiveRowNum ?>' value='<?= $target_directory_i ?>'>
                    </td>
                <tr>
                <tr>
                    <td id="archive_<?= $archiveRowNum ?>">
                        <select name="archive_data_<?= $archiveRowNum ?>"><?= $data_option ?></select>
                        <input id="archiveInput_<?= $archiveRowNum ?>" name="archiveInput_<?= $archiveRowNum ?>" type="text" value="<?= substr($archive[$i]['archive_redo'],1) ?>">
                        <img src="../../images/delete_icons.png" onclick="deleteSatnamingArchive('archive_<?= $archiveRowNum ?>')" alt="移除"/>
                    </td>
                </tr>
			<?php		$archiveRowNum++;
					}
				}
			} else {?>
                <tr>
                    <td id='archive_0'>Target Directory：
                        <input class='archive_directory' id='archive_directory_0' type='text' name='archive_directory_0'>
                    </td>
                <tr>
				<tr><td id="archive_0">
                <select name="archive_data_0"><?= $data_option ?></select><input id="archiveInput_0" name="archiveInput_0" type="text" value="">(Ex: bin/Rarchive<font color='red'>xxx</font>.ksh)<img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingArchive('archive_0')" alt="移除"/></td></tr>
			<tr>

			<?php $archiveRowNum++;
			}?>

			</table>
			<input id="archiveRowNum" type="hidden" name="archiveRowNum" value="<?= $archiveRowNum ?>"></td>
        </tr>
		<tr><td>Output<img src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png" onclick="addSatnamingOutput()" alt="新增"/></td>
            <td><table id="outputTable"><!-- "+" & "-" -->

			<?php	//Output Table
			$outputRowNum = 0;
			if (isset($id) && isset($output)){
				for($i=0; $i < count($output); $i++){
					if ($output[$i]['member_id'] == $member_id){
						$work_option = "";
						for($j=0; $j < count($work); $j++){
							if ($work[$j]['work_id'] == $output[$i]['work_id']){
								$work_option .= '<option value="'.$work[$j]['work_id'].'" selected="selected" >'.$work[$j]['work_name'];}
							else {$work_option .= '<option value="'.$work[$j]['work_id'].'">'.$work[$j]['work_name'];}
						}
				?><tr><td id="output_<?= $outputRowNum?>"><select name="output_work_<?= $outputRowNum?>"><?= $work_option ?></select><input id="outputInput_<?= $outputRowNum?>" name="outputInput_<?= $outputRowNum?>" type="text" value="<?= substr($output[$i]['model_output'],1)?>"><img src="../../images/delete_icons.png" onclick="deleteSatnamingOutput('output_<?= $outputRowNum?>')" alt="移除"/></td></tr>
			<?php		$outputRowNum++;
					}
				}
			} else {?>
				<tr><td id="output_0"><select name="output_work_0"><?= $work_option ?></select><input id="outputInput_0" name="outputInput_0" type="text" value="">(Ex: bin/R<font color='red'>xxx</font>.ksh)<img src="../images/delete_icons.png" onclick="deleteSatnamingOutput('output_0')" alt="移除"/></td></tr>
			<?php $outputRowNum++;
			}?>

			</table>
			<input id="outputRowNum" type="hidden" name="outputRowNum" value="<?= $outputRowNum?>"></td>
        </tr>
        <tr>
            <td>maintainer Setting</td>
            <td>
                <input id="maintainer_status" type="radio" name="maintainer_status" value="0" <?php if($maintainer_status == 0) echo "checked"; ?> >on
                <input id="maintainer_status" type="radio" name="maintainer_status" value="2" <?php if($maintainer_status == 2) echo "checked"; ?> >maintainer
            </td>
        </tr>
    </tbody>
</table>
<input type='submit' value='<?php if (isset($id)){ echo "Update";} else { echo "Add";}?>'>
<?php if (isset($id)){?><input type='button' value='Delete' onclick='deleteCheck()'><?php }?>

</form>
</div>

<script>
function addSatnamingCrontab(gid){
	var table = 'crontab';
	var gtbl = 'crongrpTable_'+gid;
	var num = document.getElementById("crontabRowNum").value;
	var otr = document.getElementById(gtbl).insertRow(-1);
	var otd = document.createElement("td");
	otd.id = table+'_'+num ;
	otd.innerHTML='<input id="'+ table +'Input_'+ num +'" name="'+table+'Input_'+ num +'" type="text"><img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingCrontab(\''+ gtbl +'\', \''+table+'_'+ num +'\')" alt="移除"/>';
	otd.innerHTML+='<input id="crontab_gid_'+ num +'" name="crontab_gid_'+ num + '" type="hidden" value=' + gid + '>';
	otr.appendChild(otd);
	document.getElementById("crontabRowNum").value = parseInt(num) + 1;
}

function addSatnamingCronGroup(){
	var table = 'crontab';
	var num = document.getElementById("crontabRowNum").value;
	var otr = document.getElementById("crontabTable").insertRow(-1);
	var otd = document.createElement("td");
	otd.id = table+'_'+num ;
	otd.innerHTML='<table id="crongrpTable_'+ num + '" border=1><th bgcolor=lightcyan align=left>New Mode name:<br><input type="text" name="crontab_gid_' + num + '" id="crontab_gid_' + num +'"><tr><td id="crontab_'+ num + '">New Cron time:<br><input id="crontabInput_' + num + '" name="crontabInput_' + num + '" type="text" value=""></table>';
	otr.appendChild(otd);
	document.getElementById("crontabRowNum").value = parseInt(num) + 1;
}

function deleteSatnamingCrontab(tbl, str){
	var td = document.getElementById(str).parentNode.rowIndex;
	document.getElementById(tbl).deleteRow(td)
}

function addSatnamingBatch(){
	var table = 'batch';
	var num = document.getElementById("batchRowNum").value;
	var otr = document.getElementById("batchTable").insertRow(-1);
	var otd = document.createElement("td");
	otd.id = table+'_'+num ;
	otd.innerHTML='<table><tr><td>Batch Position<font color="red">*</font></td><td><input id="batchInputPosition_'+num+'" name="batchInputPosition_'+num+'" type="text" value=""><img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingBatch(\'batch_'+num+'\')" alt="移除" align="right"/></td></tr><tr><td>Batch Name<font color="red">*</font></td><td><input class="batchName"  id="batchInputName_'+num+'" name="batchInputName_'+num+'" type="text" size="17" value=""><input id="batchInputRelay_'+num+'" name="batchInputRelay_'+num+'" type="checkbox" value="1"></td></tr><tr><td>Batch Type</td><td><input id="batchInputType_'+num+'" name="batchInputType_'+num+'" type="text" value=""></td></tr><tr><td>Batch Dtg</td><td><input id="batchInputDtg_'+num+'" name="batchInputDtg_'+num+'" type="text" value=""></td></tr><tr><td>Batch Time<font color="red">*</font></td><td><input id="batchInputTime_'+num+'" name="batchInputTime_'+num+'" type="text" size="15" value="">分鐘</td></tr><tr><td>Check Point<img id="showCheckPoint" src="<?php if (isset($id)){ echo "../";}?>../images/add_icons.png"" alt="新增"/></td><td></td></tr></table>';
	otr.appendChild(otd);
	document.getElementById("batchRowNum").value = parseInt(num) + 1;
}

function deleteSatnamingBatch(str,row_num){
	var td = document.getElementById(str).parentNode.rowIndex;
	document.getElementById('batchTable').deleteRow(td);
        $('div[aria-describedby="check_point_dialog_'+row_num+'"]').remove();
}

function addSatnamingArchive(){
	var table = 'archive';
	var num = document.getElementById("archiveRowNum").value;
	var otr_directory = document.getElementById("archiveTable").insertRow(-1);
        var otr_option = document.getElementById("archiveTable").insertRow(-1);
        var otd_directory = document.createElement("td");
 	var otd_option = document.createElement("td");

        otd_directory.id = table+'_'+num ;
        otd_directory.innerHTML = 'Target Directory：<input class="archive_directory" id="' + table + '_directory_' + num + '" type="text" name="' + table + '_directory_' + num + '">';
        otr_directory.appendChild(otd_directory);

        otd_option.id = table+'_'+num ;
	otd_option.innerHTML='<select name="'+ table +'_data_'+ num +'"><?= $data_option ?></select><input id="'+ table +'Input_'+ num +'" name="'+table+'Input_'+ num +'" type="text"><img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingArchive(\''+table+'_'+ num +'\')" alt="移除"/>';
	otr_option.appendChild(otd_option);

	document.getElementById("archiveRowNum").value = parseInt(num) + 1;
}

function deleteSatnamingArchive(str){
	var td = document.getElementById(str).parentNode.rowIndex;
	document.getElementById('archiveTable').deleteRow(td);

        $('#'+str).remove();
}

function addSatnamingOutput(){
	var table = 'output';
	var num = document.getElementById("outputRowNum").value;
	var otr = document.getElementById("outputTable").insertRow(-1);
	var otd = document.createElement("td");
	otd.id = table+'_'+num ;
	otd.innerHTML='<select name="'+ table +'_work_'+ num +'"><?= $work_option ?></select><input id="'+ table +'Input_'+ num +'" name="'+table+'Input_'+ num +'" type="text"><img src="<?php if (isset($id)){ echo "../";}?>../images/delete_icons.png" onclick="deleteSatnamingOutput(\''+table+'_'+ num +'\')" alt="移除"/>';
	otr.appendChild(otd);
	document.getElementById("outputRowNum").value = parseInt(num) + 1;
}

function deleteSatnamingOutput(str){
	var td = document.getElementById(str).parentNode.rowIndex;
	document.getElementById('outputTable').deleteRow(td);
}

function deleteModelCheck(){
	if (confirm('Do You Want to Delete This Model ?\n\n(ALL the Members of this Model will be DELETE!)')){
		if (confirm('Are You Sure Delete This Model ?')){
			document.setting.action = '<?php if (isset($id)){ echo ".";}?>./deleteModel';
			document.setting.submit();
		}
	}
}

function deleteCheck(){
	if (confirm('Do You Want to Delete This Member ?')){
		document.setting.action = '../delete';
		document.setting.submit();
	}
}
</script>
</body>
