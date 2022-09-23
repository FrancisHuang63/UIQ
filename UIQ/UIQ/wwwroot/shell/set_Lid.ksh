#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$2/log"
### END Added by fujitsu 2013/03/29

rsh ${LOGIN_IP} -l ${1} "echo ${3} > ${2}/etc/Lid" 2>&1 | tee -a ${LOGPATH}/${OPLOG}
if [ $? -eq 0 ]; then
	echo "`date +'[%Y/%m/%d] [%T]'` ${2}/etc/Lid adjust LID value to ${3}" 2>&1 | tee -a ${LOGPATH}/${OPLOG}
fi
##    rsh ${LOGIN_IP} -l ${1} cp /nwp/npcactl/shell/Lid_${3} ${2}/etc/Lid
