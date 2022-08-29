#!/bin/sh
../../shell/SV_PATH.cfg
if [ $# != "2" ]; then
    echo "The Parameter must be {MODEL} and {MEMBER}." 
    exit -1
fi

a=`mysql -u${Hpcdb_login} -p${Hpcdb_password} << EOF
use ${HpcSQL};
select member.account from member left join model on member.model_id = model.model_id where model.model_name = "${1}" and member.member_name = "${2}";
EOF`
path=`echo $a | cut -d ' ' -f2-`
echo $path
