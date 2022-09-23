#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

if [ $# -ge "1" ]; then
	all_acc_path=$@
else 
	#all_acc_path=`rsh 172.16.111.4 -l npcauis -n /uis/npcauis/web/shell/log_path.sh`
	all_acc_path=`sh /nwp/npcactl/web/UI/shell/log_path.sh`
fi 

copy_path=${SHPATH}/log

for acc_path in $all_acc_path
do
    tau=`cat $acc_path/etc/crdate | cut -d ' ' -f1`
    str_len=${#tau}
    if [ $str_len == 8 ]
    then
        tau_date=`echo $tau | cut -c 3-6`
    else
        tau_date=`echo $tau | cut -c 5-8`
    fi
    model=`echo $acc_path | awk -F / '{print $(NF-1)}'`
    member=`echo $acc_path | awk -F / '{print $NF}'`
    test -d $copy_path/$model || mkdir $copy_path/$model
    
    if [ -e $acc_path/log/job.$tau_date ] 
    then
	#echo aaa $acc_path >>/tmp/tmplog.log
        cp $acc_path/log/job.$tau_date $copy_path/$model/$member.log >>/tmp/tmplog.log 2>&1
    fi 
done

#rsh ${LOGIN_IP} -n rcp -r ${copy_path} npcauis@172.16.111.4:/uis/npcauis/web/.
cp -r ${copy_path} /nwp/npcactl/web/UI/.

#/bin/ksh ${SHPATH}/sms_check.ksh
