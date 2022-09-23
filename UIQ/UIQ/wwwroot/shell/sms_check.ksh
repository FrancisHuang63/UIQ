#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg
. /nwp/npcactl/shell/SV_PATH.cfg

status=`rsh ${SMSHOST} -n "ps aux | grep npcasms | grep -v grep | grep 'npcactl.*sms'"`
PID=`echo ${status} | cut -d ' ' -f 2`

if [ "${PID}" = "" ]; then
	echo "["`date '+%H:%M:%S'`"]" "SMS was LOST!\nPlease check SMS at ${SMSHOST}." >> ${REJECTLOG}
	#rsh 172.16.111.1 -n rcp ${REJECTLOG} npcauis@172.16.111.4:/uis/npcauis/web/log/SMS/sms_start_reject.log
	cp ${REJECTLOG} /nwp/npcactl/web/UI/log/SMS/sms_start_reject.log
fi 
