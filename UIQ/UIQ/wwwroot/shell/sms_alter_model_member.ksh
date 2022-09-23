#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg
. /nwp/npcactl/shell/SV_PATH.cfg

if [ $# != "3" ] && [ $# != "4" ]; then
	echo "The Parameter must be {MODEL}, {MEMBER} and {RUN Type} ({RUN Batch})." 
	exit -1
fi

account=`rsh 172.16.111.4 -l npcauis -n /uis/npcactl/web/shell/select_account.sh ${1} ${2}`
path=`/nwp/npcactl/web/UI/shell/select_path.sh ${1} ${2}`

lid=`cat ${path}/etc/Lid`
[ ${lid} != "0" ] && echo "The MEMBERs Lid must be 0." && exit -1

RUN=`ls ${path}/etc/?RUN`

rsh ${LOGIN_IP} -l ${account} -n "echo Y > ${path}/etc/re_run"
rsh ${LOGIN_IP} -l ${account} -n "echo 1 > ${path}/etc/Lid"
rsh ${LOGIN_IP} -l ${account} -n "echo ${3} > ${RUN}"


##alter -v /${1} CLASS 'priorop'
cd ${DEFPATH}
if [ -e ./${1}.def ]; then 

if [ $# = "3" ]; then
/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
alter -t /${1}/${2} +00:01
exit
EOF
else 
/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
alter -v /${1}/${2} RELAYBATCH "${4}"
alter -t /${1}/${2} +00:01
exit
EOF
fi

	echo "alter -t /${1}/${2} +00:01"
	echo "${1} ${2} will be Running in 1 min"
	sleep 30
	echo "sleep 30s"
	sleep 30
	echo "sleep 60s"
	sleep 20
	echo "sleep 80s"
	
/usr/bin/ksh ${SHPATH}/log_collector.ksh

info=`/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
status /${1}/${2}
exit
EOF`
	status=`echo $info | sed 's/^.*\///g' | sed 's/#.*$//g' | sed "s/ //g" | sed "s/${2}//g"`
	##echo $status
	member_status=`expr substr "$status" 2 3`
	initial_status=`echo $status | sed 's/^.*initial//g'`
	initial_status=`expr substr "$initial_status" 2 3`
	clean_status=`echo $status | sed 's/^.*clean//g'`
	clean_status=`expr substr "$clean_status" 2 3`
	echo "member_status=${member_status}, initial_status=${initial_status}, clean_status=${clean_status}"
	if [ $member_status != "act"  ] && [ $member_status != "sub" ] && [ $initial_status = "que" ] ; then
		sleep 40
		echo "sleep 120s"
/usr/bin/ksh ${SHPATH}/log_collector.ksh

info=`/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
status /${1}/${2}
exit
EOF`
		status=`echo $info | sed 's/^.*\///g' | sed 's/#.*$//g' | sed "s/ //g" | sed "s/${2}//g"`
		##echo $status
		member_status=`expr substr "$status" 2 3`
		initial_status=`echo $status | sed 's/^.*initial//g'`
		initial_status=`expr substr "$initial_status" 2 3`
		clean_status=`echo $status | sed 's/^.*clean//g'`
		clean_status=`expr substr "$clean_status" 2 3`
		echo "member_status=${member_status}, initial_status=${initial_status}, clean_status=${clean_status}"
	fi
	
	while [ $member_status = "act" ] || [ $member_status = "sub" ] || [ $initial_status = "com" ] && [ $clean_status != "com" ]
	do
		echo "${1} ${2} is RUNNING at ${member_status}."
		sleep 30
		info=`/package/local/sms/bin/cdp << EOF
		login ${SMSHOST} UID 1
		status /${1}/${2}
		exit
		EOF`
		status=`echo $info | sed 's/^.*\///g' | sed 's/#.*$//g' | sed "s/ //g" | sed "s/${2}//g"`
		##echo $status
		member_status=`expr substr "$status" 2 3`
		initial_status=`echo $status | sed 's/^.*initial//g'`
		initial_status=`expr substr "$initial_status" 2 3`
		clean_status=`echo $status | sed 's/^.*clean//g'`
		clean_status=`expr substr "$clean_status" 2 3`
		echo "member_status=${member_status}, initial_status=${initial_status}, clean_status=${clean_status}"
	done

/package/local/sms/bin/cdp << EOF
login ${SMSHOST} UID 1
play -r /${1}/${2} ${1}.def
exit
EOF

else 
	echo "${1}.def is not in SMS."
fi
exit 0
