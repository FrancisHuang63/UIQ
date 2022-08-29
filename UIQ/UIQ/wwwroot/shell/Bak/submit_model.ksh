#!/bin/ksh


/usr/local/bin/cdp << EOF
login 172.16.111.4 UID 1
requeue /$1/$2/RUN
exit
EOF
