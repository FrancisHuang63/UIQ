<?php
require_once("../function.php");
include("../cfg/SV_PATH.php");

$typh_num=$_POST['p_adjust'];

//=====颱風欄位名稱=====
$typh_entry=array("Name","Lat.","Long.","6-hr ago Lat.","6-hr ago Long.","Center Pressure","15M/S Radius","Maximum Speed","25M/S Radius");
$format_description=array(
    "(颱風/TD 名稱：PHANFONE/TD201905)",
    "(緯度：13.9) (填寫範圍: 0.0~90.0)",
    "(經度：117.7) (填寫範圍: 0.0~180.0)",
    "(6hr 前緯度：13.4) (填寫範圍: 0.0~90.0)",
    "(6hr 前經度：118.0) (填寫範圍: 0.0~180.0)",
    "(中心氣壓：965)",
    "(七級風暴風半徑：180)",
    "(最大風速：35)",
    "(十級風暴風半徑：50)",
);

echo <<<TXT
<form>
<table>
<tr><td>DTG<font color='red'>*</font>: <td><input id="dtg" type="text" value="$dtg" name="dtg"><td class="input_description">(DTG:19122612)<br>
<tr style="display:none;"><td>Typhoon number:<td><input id="num" type="text" value="$typh_num" name="num" disabled><tr><td><hr></td><td><hr></td></tr>
TXT;

//=====產生輸入欄位=====
$n=1;
$input_number = count($typh_entry);
$html='';
for($i=1;$i<=$typh_num*14;$i++)
{

if(($i%14==1)||($i%14==2)||($i%14==4))
{
    $html.="<tr><td>{$typh_entry[($n-1)%$input_number]}<font color='red'>*</font>:
    <td><input type='text' value='{$typh_array[$i]}' name='entry$n'>
    <td class='input_description'>{$format_description[($n-1)%$input_number]}";
    $n++;
}
else if(($i%14==13)||($i%2==0))
{
    $html.="<tr><td>{$typh_entry[($n-1)%$input_number]}:
    <td><input type='text' value='{$typh_array[$i]}' name='entry$n'>
    <td class='input_description'>{$format_description[($n-1)%$input_number]}";
    $n++;
}

if($i%14==0) $html.="<tr><td><hr></td><td><hr></td></tr>";

}

echo <<<TXT
{$html}<tr><td></td><td><input type="button" class="form" value="View" OnClick="sendTyphoonDataRequest('post', 'typhoon_preview.php', 'file_content', '9')"></td></tr>
</form>
</table>
TXT;

?>
