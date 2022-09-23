#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$5/log"
### END Added by fujitsu 2013/03/29

set -x
#$@ 2>&1 | tee -a ${LOGPATH}/${OPLOG}
rsh $1 -l $2 $3 $4 2>&1 | tee -a ${LOGPATH}/${OPLOG}
