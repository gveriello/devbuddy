<?php
// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';
require_once 'services/ApplicationService.php';
require_once JWT_PATH.'/JWT.php';
require_once JWT_PATH.'/Key.php';
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

/**
 * UsersController
 * 
 * Controller per la gestione dell'utente
 */
class UsersController extends BaseApiController
{
    public $requiresAuth = true;
    protected $logService = null;
    protected $applicationService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
        $this->applicationService = new ApplicationService();
    }

    public function getUserDataAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        $this->validateRequired($data, ['AppId']);

        try
        {
            $appid = $data['AppId'];
            $appid = $this->applicationService->CheckAppId($appid);
            if ($appid > 0)
            {
                if ($this->user != null)
                {
                    $claimsObject = new stdClass();
                    if (isset($this->user->sub)) $claimsObject->sub = $this->user->sub;
                    if (isset($this->user->name)) $claimsObject->name = $this->user->name;
                    if (isset($this->user->surname)) $claimsObject->surname = $this->user->surname;
                    if (isset($this->user->email)) $claimsObject->email = $this->user->email;
                    if (isset($this->user->roles)) $claimsObject->roles = $this->user->roles;

                    $this->Ok($claimsObject);
                }
                $this->Unauthorized('La richiesta può essere effettuata solo se sei autenticato.');
            }
            $this->Unauthorized("Applicazione non autorizzata.");
        }
        catch (Exception $ex)
        {
            $this->logService->Log("GetUserDataAction, ".$ex->getMessage(), "exception", $data["AppId"]);
            $this->InternalServerError($ex->getMessage());
        }
    }
}