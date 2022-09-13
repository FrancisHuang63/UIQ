<?php
include("../cfg/SV_PATH.php");

$dtg=$_POST['p_dtg'];
$typh_num=$_POST['p_typhoon_num'];
$every_data_count=$_POST['p_every_data_count'];
$typh_data=(array)json_decode($_POST[p_typhoon_data]);
$is_input_error=FALSE;

if($dtg===''){
    $is_input_error=TRUE;
}

//定義颱風資料的單位、資料顯示格式
$data_unit=array("","N","E","N","E","MB","KM","M/S","KM");
$data_format=array("","%4.1f %s ","%5.1f %s ","%4.1f %s ","%5.1f %s ","%4.0f %s ","%3.0f %s ","%2.0f %s ","%3.0f %s");

for($i=1;$i<=$typh_num;$i++)
{
    for($j=1;$j<=$every_data_count;$j++)
    {
        $entry="entry" . (($i -1) * $every_data_count + $j);
        $value=$typh_data[$entry];

        if($j<=3 && $value===''){
            $is_input_error=TRUE;
            echo "<font color='red'>*欄位必填</font><br>";
            break;
        }

        if($j!==1 && $value==='')
        {
            $value = -1;
        }

        if($j===1)
        {
            $typh_name=$value;
            $content_by_typh_name[$typh_name]['dat']=sprintf( "%' -8.8s ", $value);
            $content_by_typh_name[$typh_name]['txt']=sprintf( "%' -15.15s ", $value);
        }
        else
        {
            $content_by_typh_name[$typh_name]['dat'].=sprintf( $data_format[$j - 1], $value, $data_unit[$j - 1]);
            $content_by_typh_name[$typh_name]['txt'].=sprintf( $data_format[$j - 1], $value, $data_unit[$j - 1]);
        }
    }

    if($is_input_error){
        break;
    }
}

if($is_input_error){
    exit();
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

$html = "";
foreach($content_by_dir as $dir_name => $value){
    foreach($value as $file_type => $content){
        if($file_type==='dat'){
            $file_content=count($content) . '<br>' . implode("<br>",$content);
        }else{
            $full_dtg=$dtg;
            if($dtg){
                $full_dtg='20' . $dtg;
            }
            $file_content=$full_dtg . '<br>' . count($content) . '<br>' . implode("<br>",$content);
        }

        $html.="<h4 class='typhoon_file_name'>{$dir_name}/typhoon{$dtg}.{$file_type}</h4>";
        $html.="<div id='{$dir_name}_{$file_type}_content'>";
        $html.="<pre>{$file_content}</pre>";
        $html.="</div>";
    }
}

echo <<<TXT
{$html}
<br>
<input type="button" class="form" value="Submit" OnClick="sendTyphoonDataRequest('post', 'typhoon_result.php', 'file_created_result', '9')">
TXT;

?>
