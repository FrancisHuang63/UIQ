#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
halt -y
exit
EOF

echo "SMS is HALTED!!"

exit 0
