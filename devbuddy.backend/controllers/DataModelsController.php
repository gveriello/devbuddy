<?
// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/CryptoService.php';
require_once 'services/DatabaseService.php';
require_once 'services/ApplicationService.php';
require_once 'services/ModuleService.php';
require_once 'services/LogService.php';

/**
 * DataModelsController
 * 
 * Controller per la gestione dei datamodel dei moduli
 */
class DataModelsController extends BaseApiController
{
    public $requiresAuth = true;
    protected $logService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
    }

    /**
     * Metodo per aggiungere o modificare un datamodel di un modulo
     * @method POST
     * @param string $body['AppId'] Rappresenta l'id applicativo che vuole consultare l'endpoint. 
     * @param string $body['DataModel'] Rappresenta il datamodel da salvare
     * * @param string $body['ApiKey'] Rappresenta l'id del modulo che sta salvando il datamodel
     * @return Ok visita registrata con successo
     * @return BadRequest se app id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function upsertAction($params, $body)
    {
        try
        {
            $this->requireMethod('POST');
            
            if ($this->user != null)
            {
                $this->validateRequired($body, ['AppId', 'DataModel', 'ApiKey']);
                $appid = $body['AppId'];
                $modApiKey = $body['ApiKey'];
                $applicationService = new ApplicationService();
                $appid = $applicationService->CheckAppId($appid);
                if ($appid > 0)
                {
                    $moduleService = new ModuleService();
                    $moduleId = $moduleService->CheckApiKey($modApiKey);
                    if ($moduleId > 0)
                    {
                        $db = new Database();
                        $db->connect();

                        $appmodule = $db->read('all_applications_modules', ['id_module' => $moduleId, 'id_application' => $appid]);
                        if ($appmodule != null)
                        {
                            $appmoduleId = $appmodule[0]["id"];
                            $idutente = $this->user->sub;
                            $crypto = new CryptoService();
                            $datamodel = $crypto->encrypt(base64_decode($body['DataModel']));
                            $dateNow = date('d-m-Y H:i:s');

                            $sql = "INSERT INTO devbuddy_datamodel (id_utente, id_application_module, datamodel, updated_at) 
                                    VALUES (?, ?, ?, '".$dateNow."') 
                                    ON DUPLICATE KEY UPDATE 
                                    datamodel = ?, 
                                    updated_at = '".$dateNow."';";

                            $parameters = [$idutente, $appmoduleId, $datamodel, $datamodel];
                            $result = $db->query($sql, $parameters);
                            $this->Ok();
                        }
                    }
                    $this->BadRequest('L\'apikey fornito non è valido.');
                }
                $this->BadRequest('L\'app id fornito non è valido.');
            }
            $this->Unauthorized('La richiesta può essere effettuata solo se sei autenticato.');
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", $body['AppId']);
            $this->InternalServerError($ex->getMessage());
        }
    }

    /**
     * Metodo per recuperare il datamodel di un modulo
     * @method POST
     * @param string $body['AppId'] Rappresenta l'id applicativo che vuole consultare l'endpoint. 
     * * @param string $body['ApiKey'] Rappresenta l'id del modulo che sta salvando il datamodel
     * @return Ok visita registrata con successo
     * @return BadRequest se app id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function getAction($params, $body)
    {
        try
        {
            $this->requireMethod('POST');
            
            if ($this->user != null)
            {
                $this->validateRequired($body, ['AppId', 'ApiKey']);
                $appid = $body['AppId'];
                $modApiKey = $body['ApiKey'];
                $applicationService = new ApplicationService();
                $appid = $applicationService->CheckAppId($appid);
                if ($appid > 0)
                {
                    $moduleService = new ModuleService();
                    $moduleId = $moduleService->CheckApiKey($modApiKey);
                    if ($moduleId > 0)
                    {
                        $db = new Database();
                        $db->connect();

                        $appmodule = $db->read('all_applications_modules', ['id_module' => $moduleId, 'id_application' => $appid]);
                        if ($appmodule != null)
                        {
                            $appmoduleId = $appmodule[0]["id"];
                            $idutente = $this->user->sub;
                            
                            
                            $datamodel = $db->read('devbuddy_datamodel', ['id_utente' => $idutente, 'id_application_module' => $appmoduleId]);
                            $crypto = new CryptoService();
                            if ($datamodel != null)
                                $this->Ok($crypto->decrypt($datamodel[0]["datamodel"]));

                            $this->NotFound("Non esiste nessun datamodel associato al modulo");
                        }
                    }
                    $this->BadRequest('L\'apikey fornito non è valido.');
                }
                $this->BadRequest('L\'app id fornito non è valido.');
            }
            $this->Unauthorized('La richiesta può essere effettuata solo se sei autenticato.');
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", $body['AppId']);
            $this->InternalServerError($ex->getMessage());
        }
    }
}