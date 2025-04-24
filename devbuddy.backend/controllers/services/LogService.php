<?php

require_once 'DatabaseService.php';
require_once 'ApplicationService.php';

if (class_exists('LogService')) {
    return;
}

class LogService
{
    public function Log($message, $type, $appId)
    {
        try
        {
            $applicationService = new ApplicationService();
            $appId = $applicationService->CheckAppId($appId);
            if ($appId > 0)
            {
                $db = new Database();
                $db->connect();
                
                $sql = "INSERT INTO all_logs (idapplication, type, message, date) VALUES (?, ?, ?, ?)";
                $params = [$appId, $type, $message, date("Y-m-d H:i:s")];

                $db->query($sql, $params);
            }
        }
        catch(Exception $ex)
        {
            $this->LogFile($message, $type, $appId);
        }
    }

    public function LogFile($message, $type, $appId)
    {
        try
        {
            $logPath = "logs/log_".date("Y-m-d").".txt";
            $applicationService = new ApplicationService();
            $appId = $applicationService->CheckAppId($appId);
            if ($appId > 0)
            {
                $message = "[".date("Y-m-d H:i:s")."] | ".$type." | ".$message;
                file_put_contents($logPath, $message, FILE_APPEND);
            }
        }
        catch(Exception $ex)
        {

        }
    }
}