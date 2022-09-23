#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$6/log"
### END Added by fujitsu 2013/03/29

set -x
#echo "$1 $2 $3 $4 $5"
if [ ${1} == 'npcactl' ] ; then
  rsh ${LOGIN_IP} -l $1 $2 $3 $4 $5 2>&1 | tee -a ${LOGPATH}/${OPLOG}
else 
    rsh ${LOGIN_IP} -l $1 $2 $5 2>&1 | tee -a ${LOGPATH}/${OPLOG}
fi

/bin/ksh ${SHPATH}/log_collector.ksh
