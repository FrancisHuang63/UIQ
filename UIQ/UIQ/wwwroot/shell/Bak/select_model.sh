#!/bin/sh
../../shell/SV_PATH.cfg
a=`mysql -u${Hpcdb_login} -p${Hpcdb_password} << EOF
use ${HpcSQL};
select model.model_name from model group by model.model_name;
EOF`

echo `echo $a | cut -d ' ' -f2-`
