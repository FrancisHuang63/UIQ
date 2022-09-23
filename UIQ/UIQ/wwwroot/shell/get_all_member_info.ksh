#!/bin/ksh

if [ $# -ge "1" ]; then
	all_acc_path=$@
else 
	#all_acc_path=`rsh 172.16.111.4 -l npcauis -n /uis/npcauis/web/shell/log_path.sh`
	all_acc_path=`sh /nwp/npcactl/web/UI/shell/log_path.sh`
fi 

for acc_path in $all_acc_path
do
	Lid=`cat ${acc_path}/etc/Lid`
	dtg=`cat ${acc_path}/etc/crdate | awk '{print $1}'`
	RUN=`cat ${acc_path}/etc/*RUN`
	echo "[${acc_path}]"
	echo "Lid=${Lid}"
	echo "dtg=${dtg}"
	echo "RUN=${RUN}"
done
	
