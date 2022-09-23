#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

export PATH="$PATH:/package/local/sms/bin"

cd ${SMSHOME}

`nohup sms > /dev/null 2>&1 &`

echo "SMS is Start at ${SMSHOST}!!"

exit 0
