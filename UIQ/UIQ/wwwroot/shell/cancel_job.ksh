#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$4/log"
### END Added by fujitsu 2013/03/29

#rsh 172.30.0.14 -l $1 $2 $3 $4
rsh ${LOGIN_IP} -l $1 $2 $3 2>&1 | tee -a ${LOGPATH}/${OPLOG}

/bin/ksh ${SHPATH}/log_collector.ksh
