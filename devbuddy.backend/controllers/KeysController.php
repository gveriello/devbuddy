<?php

// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';

/**
 * KeysController
 * 
 * Controller per la gestione delle chiavi applicative
 */
class KeysController extends BaseApiController
{
    protected $logService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
    }

    public function rotationKeysAction($params)
    {
        $this->requireMethod('GET');

        try
        {
            $db = new Database();
            $db->connect();
            
            $date24HoursAgo = date('Y-m-d H:i:s', strtotime('-23 hours'));
            $sql = "SELECT app.name as appName, app.appid, module.name as moduleName, appModule.id, appModule.apikey, appModule.lastRotation 
                    FROM all_applications as app inner join all_applications_modules as appModule ON (app.id = appModule.id_application) 
                                                inner join all_modules as module ON (module.id = appModule.id_module)
                    WHERE STR_TO_DATE(lastRotation, '%Y-%m-%d %H:%i:%s') < ? OR lastRotation is null";
            $parameters = [$date24HoursAgo];

            $modules = $db->query($sql, $parameters);
            
            if ($modules != null)
            {
                foreach ($modules as $module) 
                {
                    $result = $db->update("applications_modules", 
                    [
                        "lastRotation" => date('Y-m-d H:i:s'),
                        "apiKey" => $this->GUID()
                    ], 
                    [
                        "id" => $module['id'] 
                    ]);

                    if ($result)
                    {
                        $this->logService->Log("Updated key for module ".$module['moduleName'].' and application '.$module['appName'], "Information", $module['appid']);
                        continue;
                    }
                    $this->logService->Log("Failed to update key for module ".$module['moduleName'].' and application '.$module['appName'], "Error", $module['appid']);
                }
            }
        }
        catch (Exception $ex)
        {
            $this->logService->Log($ex->getMessage(), "Exception", $appid);
            $this->InternalServerError($ex->getMessage());
        }
    }

    private function GUID()
    {
        if (function_exists('com_create_guid') === true)
        {
            return trim(com_create_guid(), '{}');
        }

        return sprintf('%04X%04X-%04X-%04X-%04X-%04X%04X%04X', mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(16384, 20479), mt_rand(32768, 49151), mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(0, 65535));
    }
    
}