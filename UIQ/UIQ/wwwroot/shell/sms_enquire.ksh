#!/bin/ksh

/usr/local/bin/cdp << EOF
login 172.16.111.4 UID 1
st -f $1/$2
exit
EOF
