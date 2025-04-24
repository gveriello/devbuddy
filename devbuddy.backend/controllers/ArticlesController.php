<?
// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';

class ArticlesController extends BaseApiController
{
    protected $logService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
    }

    
    /**
     * Metodo per ottenere l'articolo in base al suo id.
     * @method GET
     * @param string $querystirng['id'] Ottiene l'id dell'articolo
     * @return Ok ottiene l'articolo
     * @return NotFound l'articolo non esiste
     * @return BadRequest se l'id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function byIdAction($params)
    {
        $this->requireMethod('GET');
        
        $id = $params[0];
        if ($id <= 0)
            $this->BadRequest('Fornire un id valido.');

        try
        {
            $db = new Database();
            $db->connect();
            
            $sql = "SELECT articles.*, CONCAT(users.name, ' ', users.surname) AS createdby 
                FROM website_articles   as articles
                INNER JOIN all_users as users ON articles.createdby = users.id
                WHERE articles.enabled = 1 AND articles.Id = ?;";
            $parameters = [$id];
            $article = $db->query($sql, $parameters);

            if ($article != null)
            {
                $this->Ok(json_encode($article[0]));
            }
            $this->NotFound('Non ci sono articoli censiti al momento.');
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", "9095976a-3063-4084-a657-951c842ef129");
            $this->InternalServerError($ex->getMessage());
        }
    }

    /**
     * Metodo per ottenere tutti gli articoli censiti
     * @method GET
     * @return Ok ottiene gli articoli censiti
     * @return NotFound non ci sono articoli
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function allAction()
    {
        $this->requireMethod('GET');

        try
        {
            $db = new Database();
            $db->connect();
            
            $sql = "SELECT articles.*, CONCAT(users.name, ' ', users.surname) AS createdby 
                FROM website_articles   as articles
                INNER JOIN all_users as users ON articles.createdby = users.id
                WHERE articles.enabled = 1
                ORDER BY articles.Id DESC;";
    
            $articles = $db->query($sql);
            if ($articles != null)
            {
                $this->Ok(json_encode($articles));
            }
            $this->NotFound('Non ci sono articoli censiti al momento.');
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", "9095976a-3063-4084-a657-951c842ef129");
            $this->InternalServerError($ex->getMessage());
        }
    }
    
    /**
     * Metodo per ottenere il numero di visitatori per articolo
     * @method GET
     * @param string $querystirng['id'] Ottiene l'id dell'articolo
     * @return Ok ottiene il numero di visitatori
     * @return BadRequest se l'id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function readedByAction($params)
    {
        $this->requireMethod('GET');
        
        $id = $params[0];
        if ($id <= 0)
            $this->BadRequest('Fornire un id valido.');

        try
        {
            $db = new Database();
            $db->connect();
            
            $sql = "SELECT COUNT(id) as Count
                FROM all_visitors as visitors
                WHERE visitors.page = ?;";
            $parameters = ["Blog/".$id];
            $article = $db->query($sql, $parameters);

            if ($article != null)
            {
                $this->Ok(json_encode($article[0]));
            }
            $this->Ok(json_encode(0));
        }
        catch (Exception $ex)
        {
            $logService->Log($ex->getMessage(), "Exception", "9095976a-3063-4084-a657-951c842ef129");
            $this->InternalServerError($ex->getMessage());
        }
    }
}