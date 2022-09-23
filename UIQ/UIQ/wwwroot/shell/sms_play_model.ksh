#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

cd ${DEFPATH}

for model in $@
do
	if [ -e ./${model}.def ]; then 
/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
play -r /${model} ${model}.def
exit
EOF
	echo "play -r /${model} ${model}.def"
	fi
done

exit 0
