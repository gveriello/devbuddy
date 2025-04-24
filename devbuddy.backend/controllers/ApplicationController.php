<?
// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';

class ApplicationController extends BaseApiController
{
    protected $logService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
    }

    /**
     * Metodo ottenere l'appid dell'applicativo chiamante.
     * @method GET
     * @param string $header['ORIGIN'] Ottiene l'url dell'applicativo chiamante
     * @return Ok ottiene l'appid attuale
     * @return BadRequest se l'applicativo non è pieno
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function getAppIdAction()
    {
        $this->requireMethod('GET');

        try
        {
            $origin = isset($_SERVER['HTTP_ORIGIN']) ? $_SERVER['HTTP_ORIGIN'] : $this->BadRequest('Applicativo non autorizzato.');

            $db = new Database();
            $db->connect();
            
            $sql = "SELECT appid
                    FROM all_applications 
                    WHERE domain = ?;";

            $parameters = [$origin];
            $app = $db->query($sql, $parameters);

            if ($app != null)
            {
                $this->Ok($app[0]);
            }

            $this->BadRequest('Applicativo non autorizzato.')
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", "9095976a-3063-4084-a657-951c842ef129");
            $this->InternalServerError($ex->getMessage());
        }
    }
}