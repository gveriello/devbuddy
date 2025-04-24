<?php

// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';
require_once JWT_PATH . '/JWT.php';
require_once JWT_PATH . '/Key.php';
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

class TokenController extends BaseApiController
{
    public $requiresAuth = true;
    protected $logService = null;

    public function __construct()
    {
        $this->logService = new LogService();
    }

    /**
     * Verifica validità token
     * GET /auth/verify
     */
    public function verifyAction($params, $data, $method)
    {
        $this->requireMethod('POST');

        if (!$this->user) {
            $this->Unauthorized("Il token fornito non è valido o è scaduto; perfavore rieffettua l'accesso.");
        }

        //Aggiungo tempo per non far scadere il token
        $this->user->exp = time() + 3600;

        // Generazione del JWT
        $jwt = JWT::encode($this->user, API_SECRET, 'HS256');
        $this->Ok($jwt);

        $this->Ok();
    }
}
