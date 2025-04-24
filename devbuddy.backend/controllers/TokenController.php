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

        $payload = [
            "iss" => $data["AppId"],
            "aud" => AUDIENCE,
            "iat" => $this->user->iat,
            "exp" => time() + 3600,
            "sub" => $this->user->sub,
            "name" => $this->user->name,
            "surname" => $this->user->surname,
            "email" => $this->user->email
        ];

        // Generazione del JWT
        $jwt = JWT::encode($payload, API_SECRET, 'HS256');
        $this->Ok($jwt);
    }
}
