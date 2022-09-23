#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

#all_model=`rsh 172.16.111.4 -l npcauis /uis/npcauis/web/shell/select_model.sh`
all_model=`sh /nwp/npcactl/web/UI/shell/select_model.sh`
cd ${DEFPATH}

sh ${SHPATH}/sms_play_model.ksh ${all_model}
/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
restart -y
exit
EOF

echo "SMS is RUNNING!!"

exit 0
