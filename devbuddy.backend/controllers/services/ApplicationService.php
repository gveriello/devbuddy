<?php

require_once 'DatabaseService.php';

if (class_exists('ApplicationService')) {
    return;
}

class ApplicationService
{
    public function CheckAppId($appId)
    {
        try
        {
            $db = new Database();
            $db->connect();

            $application = $db->read('all_applications', ['appid' => $appId]);
            if ($application != null)
            {
                return $application[0]["id"];
            }
            return false;
        }
        catch (Exception $ex)
        {
            return false;
        }
    }
}