#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg


acc_path=${1}
	
Lid=`cat ${acc_path}/Lid`
dtg=`cat ${acc_path}/crdate | awk '{print $1}'`
RUN=`cat ${acc_path}/*RUN`
echo "Lid=${Lid}&dtg=${dtg}&RUN=${RUN}"
