<?php
include("../cfg/SV_PATH.php");

$dtg=$_POST['p_dtg'];
$typh_num=$_POST['p_typhoon_num'];
$typh_data=(array)json_decode($_POST[p_typhoon_data]);

//定義颱風資料的單位、資料顯示格式
$data_unit=array("","N","E","N","E","MB","KM","M/S","KM");
$data_format=array("","%4.1f %s ","%5.1f %s ","%4.1f %s ","%5.1f %s ","%4.0f %s ","%3.0f %s ","%2.0f %s ","%3.0f %s");

$every_data_count=$_POST['p_every_data_count'];

for($i=1;$i<=$typh_num;$i++)
{
    for($j=1;$j<=$every_data_count;$j++)
    {
        $entry="entry" . (($i -1) * $every_data_count + $j);
        $value=$typh_data[$entry];
        if($j!==1 && $value==='')
        {
            $value = -1;
        }

        if($j === 1)
        {
            $typh_name = $value;
            $content_by_typh_name[$typh_name]['dat']=sprintf( "%' -8.8s ", $value);
            $content_by_typh_name[$typh_name]['txt']=sprintf( "%' -15.15s ", $value);
        }
        else
        {
            $content_by_typh_name[$typh_name]['dat'].=sprintf( $data_format[$j - 1], $value, $data_unit[$j - 1]);
            $content_by_typh_name[$typh_name]['txt'].=sprintf( $data_format[$j - 1], $value, $data_unit[$j - 1]);
        }
    }
}

foreach($content_by_typh_name as $typhoon_name => $typhoon_content){
    if(strpos($typhoon_name,'TD') !== FALSE){
        $content_by_dir['tdty']['dat'][]=$typhoon_content['dat'];
        $content_by_dir['tdty']['txt'][]=$typhoon_content['txt'];
    }else{
        $content_by_dir['ty']['dat'][]=$typhoon_content['dat'];
        $content_by_dir['ty']['txt'][]=$typhoon_content['txt'];
        $content_by_dir['tdty']['dat'][]=$typhoon_content['dat'];
        $content_by_dir['tdty']['txt'][]=$typhoon_content['txt'];
    }
}

$html="";
$current_time=date('YmdHis');
foreach($content_by_dir as $dir_name => $value){
    foreach($value as $file_type => $content){
        if($file_type==='dat'){
            $file_content=count($content) . "\n";
            }else{
            $file_content="20" . $dtg . "\n" . count($content) . "\n";
        }
        $file_content.=implode("\n",$content);

        $temp_file_path="${UI_PATH}temp/${dir_name}/typhoon.${file_type}";
        $real_dir="/ncs/${TYPHOON_ACCOUNT}/TYP/M00/dtg/${dir_name}/";
        $real_file_name="typhoon${dtg}.${file_type}";
        $real_file_path=$real_dir . $real_file_name;

        //=====寫入暫存檔=====
        fwrite(fopen($temp_file_path,"w"),$file_content);
        echo "Write ${temp_file_path}...<br>";

        //=====檢查同 DTG 檔案是否已存在，存在則變更檔名=====
        if(file_exists($real_file_path)){
            $command="rsh -l ${TYPHOON_ACCOUNT} ${LOGIN_IP} mv ${real_file_path} ${real_file_path}${current_time}";
            system($command);
            echo "Rename ${real_file_name} to ${real_file_name}${current_time}...<br>";
        }

        //=====將檔案傳回HPC主機
        $command="rsh -l ${TYPHOON_ACCOUNT} ${LOGIN_IP} cp ${temp_file_path} ${real_file_path}";
        system($command);
        echo "Copy to ${real_file_path}...<br><br>";
    }
}

//=====UI動作寫入UI_actions.log=====
$message=date('[Y/m/d] [G:i:s]')." Edit the typhoon{$_POST['p_dtg']}.dat and typhoon{$_POST['p_dtg']}.txt\r\n";
fwrite(fopen("../log/UI_actions.log","a+"),$message);

?>
