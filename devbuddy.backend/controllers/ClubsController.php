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
 * ClubsController
 * 
 * Controller per la gestione dei club
 */
class ClubsController extends BaseApiController
{
    public $requiresAuth = true;
    protected $logService = null;
    protected $applicationService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
        $this->applicationService = new ApplicationService();
    }

    public function getUserClubsAction($params, $data)
    {
        $this->requireMethod('POST');
        $this->validateRequired($data, ['AppId']);

        $appid = $data['AppId'];
        $appid = $this->applicationService->CheckAppId($appid);
        if ($appid > 0)
        {
            if ($this->user != null)
            {
                try
                {
                    $db = new Database();
                    $db->connect();
                    
                    $sql = "SELECT myclub.id, myclub.denominazione_sociale, myclub.cf, myclub.logo, myclub.address, myclub.city, myclub.telephone, myclub_founder.isAdmin, 
                        myclub_founder_roles.canWrite, myclub_founder_roles.canDelete, myclub_founder_roles.canExport, myclub_founder_roles.canView
                        FROM fitappness_clubs as myclub  
                        INNER JOIN fitappness_clubs_founders as myclub_founder ON myclub.id = myclub_founder.id
                        LEFT JOIN fitappness_clubs_founders_roles as myclub_founder_roles ON myclub_founder.id = myclub_founder_roles.id_founder_club
                        WHERE myclub.enabled = 1 AND myclub_founder.enabled = 1 AND myclub_founder.id_founder = ?;";

                    $parameters = [$this->user->sub];
                    $clubs = $db->query($sql, $parameters);
        
                    if ($clubs != null)
                    {
                        $this->Ok($clubs);
                    }
                    $this->NotFound('Non ci sono club a te associati.');
                }
                catch (Exception $ex)
                {
                    $this->logService->Log($ex->getMessage(), "Exception", $appid);
                    $this->InternalServerError($ex->getMessage());
                }
            }
            $this->Unauthorized('La richiesta può essere effettuata solo se sei autenticato.');
        }
        $this->logService->Log("Tentativo di autorizzazione applicativa con appId: ".$appid, "Exception", $appid);
        $this->Unauthorized("Applicazione non autorizzata.");
    }
}