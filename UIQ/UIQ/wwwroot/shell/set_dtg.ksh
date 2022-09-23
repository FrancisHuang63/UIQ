#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$4/log"
### END Added by fujitsu 2013/03/29
TEST=`hostname`;
echo "hostname of shell: ${TEST}";
echo "in shell: ${ME}"
echo "login: ${LOGIN_IP}"
rsh ${LOGIN_IP} -l $1 $2 $3 2>&1 | tee -a ${LOGPATH}/${OPLOG}
#rsh ${LOGIN_IP} -l $1 ksh -x $2 $3 $4 $5 >/nwp/npcactl/set_dtg.ksh_result.txt 2>&1
