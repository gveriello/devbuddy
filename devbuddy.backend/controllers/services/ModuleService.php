<?
require_once 'DatabaseService.php';

if (class_exists('ModuleService')) {
    return;
}

class ModuleService
{
    public function CheckApiKey($apiKey)
    {
        try
        {
            $db = new Database();
            $db->connect();

            $module = $db->read('all_modules', ['apikey' => $apiKey]);
            if ($module != null)
            {
                return $module[0]["id"];
            }
            return false;
        }
        catch (Exception $ex)
        {
            return false;
        }
    }
}