#!/bin/sh
../../shell/SV_PATH.cfg
## Clear UI_errormsg.log
filename=/ncs/${HpcCTL}/web/UI/log/UI_errormsg.log

tail -30000 ${filename} > ${filename}_tmp

mv ${filename}_tmp ${filename}

chmod 775 ${filename}
chown apache:apache ${filename}


## Clear MySQL DB backup
#find /ncs/${HpcCTL}/web -name "db_ncs*.sql" -mtime +180 -exec rm {} \;
for AAA in `ls -1rt /ncs/${HpcCTL}/web/${HpcSQL}??????.sql | head -n -3`; do
	rm $AAA
done

