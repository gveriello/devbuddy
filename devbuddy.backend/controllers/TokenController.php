<?php

// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';

class TokenController extends BaseApiController
{
    public $requiresAuth = true;
    protected $logService = null;

    public function __construct()
    {
        $this->logService = new LogService();
    }

    /**
     * Verifica validit� token
     * GET /auth/verify
     */
    public function verifyAction($params, $data, $method)
    {
        $this->requireMethod('POST');

        if (!$this->user) {
            $this->Unauthorized("Il token fornito non � valido o � scaduto; perfavore rieffettua l'accesso.");
        }

        $this->Ok();
    }
}
