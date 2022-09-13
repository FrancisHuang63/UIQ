<?php

class Cron_setting extends Controller {

	function __construct()
	{
		parent::Controller();	
        $this->config->load('ncs_cfg');
        $this->load->model('settinginfo');
        $this->load->library('parse');
        $this->load->library('session');
        $this->load->helper('url');
        $this->load->helper('ncs_helper');
#        $this->load->helper('file');
	}
	
	function index()
	{
        $account = $this->session->userdata('account');
        $group = $this->session->userdata('group_name');

        //判斷是否登入
        if ( false == $account ){
            $login_msg = $this->config->item('login');
            $js_str = "
                alert('$login_msg');
                window.location = '../login';";
            
            ExeJS($js_str);
#        } else if ( $group != "ADM" ){
#		//判斷是否為ADM
#            $js_str = "
#                alert('Your Group Can Not Use This Page！');
#                window.close()";
#            
#            ExeJS($js_str);
        }
		
		$data['infomation'] = 'This page is used to CHANGE cron mode';
		$data['account'] = $account;
		$data['group'] = $group;
			
	        $this->load->view('cron_setting_view', $data);
	}
	
	function Edit()
	{
        $account = $this->session->userdata('account');
        $group = $this->session->userdata('group_name');

        //判斷是否登入
        if ( false == $account ){
            $login_msg = $this->config->item('login');
            $js_str = "
                alert('$login_msg');
                window.location = '../../login';";
            
            ExeJS($js_str);
#        } else if ( $group != "ADM" ){
#		//判斷是否為ADM
#            $js_str = "
#                alert('Your Group Can Not Use This Page！');
#                window.close()";
#            
#            ExeJS($js_str);
        }

		$data['infomation'] = 'This page is used to CHANGE cron mode';
		$data['account'] = $account;
		$data['group'] = $group;
		$cronmode = $this->input->post('cronmode',true);

		// change master_group value
		$this->settinginfo->update_master_group($cronmode);	

		// update members who have $cronmode setting 
		$data['ids'] = $this->settinginfo->show_member_ids($cronmode);
		foreach($data['ids'] as $i){
			$j = $i['member_id'];
			$this->settinginfo->update_group_validation($j, $cronmode);
		}

		// update members who don't have $cronmode setting 
		$data['ids2'] = $this->settinginfo->show_member_ids2($cronmode);
		foreach($data['ids2'] as $i){
			$j = $i['member_id'];
			$this->settinginfo->update_group_validation($j, "Normal");
		}

		//sync MySQL
		include("./cfg/SV_PATH.php");
		exec("rsh -l ${HpcCTL} ${DATAMV_IP} php /ncs/${HpcCTL}/web/UIQ/sqlsync.php");

		### For CWB #################################
		//Execute other scripts here.
		switch($cronmode){
			case "Normal":
				//Do anything.
				exec("rsh -l ${HpcCTL} ${CRON_IP} /ncs/${HpcCTL}/shfun/shbin/Change_mode.ksh Normal");
				break;

			case "Backup":
				//Do anything.
				exec("rsh -l ${HpcCTL} ${CRON_IP} /ncs/${HpcCTL}/shfun/shbin/Change_mode.ksh Backup");
				break;

			case "Typhoon":
				//Do anything.
				exec("rsh -l ${HpcCTL} ${CRON_IP} /ncs/${HpcCTL}/shfun/shbin/Change_mode.ksh Typhoon");
				break;

			default:
				//Do anything.
				break;
		}
		### For CWB #################################

            $js_str = "
                window.location = '/${HpcUI}/cron_setting';";
            
            ExeJS($js_str);
	    #$this->load->view('cron_setting_view', $data);
	}

}
