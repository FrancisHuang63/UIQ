#!/bin/ksh

/usr/local/bin/cdp << EOF
login 172.16.111.4 UID 1
echo '[current status on SMS]'
sms|grep 'Status\|SMSNODE'
echo '\n[SMS overview]'
overview
echo '\n[SMS job status]'
st -f
exit
EOF
