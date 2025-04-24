<?php

// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/ApplicationService.php';
require_once 'services/LogService.php';

/**
 * VisitorsController
 * 
 * Controller per la gestione degli utenti visitatori
 */
class VisitorsController extends BaseApiController
{
    protected $logService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
    }

    /**
     * Metodo per registrare un nuovo visitatore.
     * @method POST
     * @param string $body['AppId'] Rappresenta l'id applicativo che vuole consultare l'endpoint. 
     * @param string $body['Page'] Rappresenta la pagina che l'utente sta visitando. 
     * @return Ok visita registrata con successo
     * @return BadRequest se app id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function addAction($params, $body)
    {
        try
        {
            $this->requireMethod('POST');
            
            $this->validateRequired($body, ['AppId', 'Page']);
            $appid = $body['AppId'];
            $applicationService = new ApplicationService();
            $appid = $applicationService->CheckAppId($appid);
            if ($appid > 0)
            {
                $sql = "INSERT INTO all_visitors (ip, visitat, page, city, latC, longC, userAgent, idapplication) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                $city = "unknown";
                $lat = "unknown";
                $long = "unknown";
                
                // Ottenere l'indirizzo IP del client
                $ip = $_SERVER['REMOTE_ADDR'];
                $userAgent = $_SERVER['HTTP_USER_AGENT'];
                $page = $body["Page"];
                try
                {
                    $data = json_decode(file_get_contents("http://ip-api.com/json/".$ip));
                    if ($data->status == "success") 
                    {
                        $lat = $data->lat;
                        $long = $data->lon;
                        $city = $data->city;
                    }
                }
                catch(Exception $ex)
                {
                    $this->logService->log("Errore durante il download delle informazioni sull'ip: ".$ip."; stack trace: ".$ex->getMessage(), "Exception", $body['appid']);
                }

                // Array con i parametri nell'ordine corrispondente ai placeholder (?)
                $queryparams = [$ip, date("Y-m-d H:i:s"), $page, $city, $lat, $long, $userAgent, $appid];

                $db = new Database();
                $db->connect();

                // Chiamata al metodo
                $result = $db->query($sql, $queryparams);
                $this->Ok('welcome');
            }
            $this->BadRequest('L\'app id fornito non è valido.');
        }
        catch (Exception $ex)
        {
            $this->logService->Log($ex->getMessage(), "Exception", $body['AppId']);
            $this->InternalServerError($ex->getMessage());
        }
    }
}