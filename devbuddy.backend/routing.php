<?php

require_once JWT_PATH . 'JWTExceptionWithPayloadInterface.php';
require_once JWT_PATH . '/JWT.php';
require_once JWT_PATH . '/Key.php';
require_once JWT_PATH . '/BeforeValidException.php';
require_once JWT_PATH . '/ExpiredException.php';
require_once JWT_PATH . '/SignatureInvalidException.php';
use Firebase\JWT\JWT;
use Firebase\JWT\Key;


/**
 * Classe Router per gestire lo smistamento delle richieste API
 */
class Router
{
    private $controller;
    private $action;
    private $params = [];
    private $requestData = null;
    private $requestMethod;
    private $user = null;
    
    public function __construct()
    {
        $this->requestMethod = $_SERVER['REQUEST_METHOD'];
        $this->parseUrl();
        $this->parseRequestData();
    }

    /**
     * Verifica il token JWT dall'header Authorization
     * @return object|null Payload decodificato o null se token non valido
     */
    private function verifyToken()
    {
        try {
            // Ottieni l'header Authorization
            $headers = getallheaders();
            $token = null;
            
            if (isset($headers['Authorization'])) {
                $authHeader = $headers['Authorization'];
                if (preg_match('/Bearer\s(\S+)/', $authHeader, $matches)) {
                    $token = $matches[1];
                }
            }
            
            if ($token === null) {
                $this->handleError(401, 'Non autorizzato: bearer mancante.');
            }

            // Decodifica e verifica il token
            $payload = JWT::decode($token, new Key(API_SECRET, 'HS256'));

            // Verifica manuale della scadenza
            $now = time();
            if (isset($payload->exp) && $payload->exp < $now) {
                $this->handleError(401, 'Token scaduto.');
            }

            // Verifica che l'issuer sia tra quelli consentiti
            if (isset($payload->iss) && !in_array($payload->iss, Issuers)) {
                $this->handleError(401, 'Issuer '.$payload->iss.' non autorizzato.');
            }
            
            if (isset($payload->aud) && $payload->aud !== AUDIENCE) {
                $this->handleError(401, 'Audience '.$payload->aud.' non autorizzato.');
            }

            
            return $payload;
        } 
        catch (Exception $e) 
        {
            // Log dell'errore
            error_log('Errore verifica JWT: ' . $e->getMessage());
            return null;
        }
    }

    /**
     * Ottiene tutti gli header HTTP in modo compatibile con diversi server
     * @return array Headers della richiesta
     */
    private function getAllHeaders() 
    {
        // Se la funzione nativa esiste, usala
        if (function_exists('getallheaders')) {
            return getallheaders();
        }
        
        // Implementazione fallback per server non-Apache
        $headers = [];
        foreach ($_SERVER as $name => $value) {
            if (substr($name, 0, 5) === 'HTTP_') {
                $headerName = str_replace(' ', '-', ucwords(strtolower(str_replace('_', ' ', substr($name, 5)))));
                $headers[$headerName] = $value;
            } elseif ($name === 'CONTENT_TYPE') {
                $headers['Content-Type'] = $value;
            } elseif ($name === 'CONTENT_LENGTH') {
                $headers['Content-Length'] = $value;
            } elseif ($name === 'AUTHORIZATION') {
                $headers['Authorization'] = $value;
            }
        }
        
        return $headers;
    }


    /**
     * Analizza l'URL e determina controller, action e parametri
     */
    private function parseUrl()
    {
        // Ottieni il percorso dalla URL
        $url = isset($_SERVER['PATH_INFO']) ? $_SERVER['PATH_INFO'] : 
              (isset($_SERVER['REQUEST_URI']) ? parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH) : '/');
        
        // Rimuovi la directory base se necessario
        $basePath = dirname($_SERVER['SCRIPT_NAME']);
        if ($basePath !== '/' && strpos($url, $basePath) === 0) {
            $url = substr($url, strlen($basePath));
        }
        
        // Previeni attacchi path traversal
        $url = str_replace(['../', './'], '', $url);
        
        // Dividi il percorso in segmenti
        $segments = explode('/', trim($url, '/'));

        // Controlla se il primo segmento è "api" e rimuovilo se necessario
        if (!empty($segments) && strtolower($segments[0]) === 'api') {
            array_shift($segments); // Rimuove "api" dai segmenti
        }
        
        // Imposta controller, action e parametri
        $this->controller = !empty($segments[0]) ? ucfirst($segments[0]) : DEFAULT_CONTROLLER;
        $this->action = isset($segments[1]) ? $segments[1] : DEFAULT_ACTION;
        
        // Sanifica i parametri
        $this->params = array_map(function($param) {
            return ApiSecurity::sanitizeInput($param);
        }, array_slice($segments, 2));
    }
    
    /**
     * Analizza i dati della richiesta (POST, PUT, ecc.)
     */
    private function parseRequestData()
    {
        // Ottieni i dati inviati con la richiesta
        $contentType = isset($_SERVER['CONTENT_TYPE']) ? $_SERVER['CONTENT_TYPE'] : '';
        
        // Se è inviato come JSON, decodificalo
        if (strpos($contentType, 'application/json') !== false) {
            $input = file_get_contents('php://input');
            $data = json_decode($input, true);
            
            // Verifica errori JSON
            if (json_last_error() !== JSON_ERROR_NONE) {
                $this->handleError(400, 'JSON non valido: ' . json_last_error_msg());
            }
            
            $this->requestData = $data;
        } else {
            // Altrimenti usa i dati POST o PUT standard
            $this->requestData = $_POST;
            
            // Per richieste PUT, PATCH, ecc.
            if (empty($this->requestData) && in_array($this->requestMethod, ['PUT', 'PATCH', 'DELETE'])) {
                parse_str(file_get_contents('php://input'), $this->requestData);
            }
        }
        
        // Sanifica i dati di input per prevenire XSS e injection
        if ($this->requestData) {
            $this->requestData = ApiSecurity::sanitizeInput($this->requestData);
        }
    }
    
    /**
     * Esegue il controller appropriato
     */
    public function dispatch()
    {        
        // Controlla se il file del controller esiste
        $controllerFile = CONTROLLER_PATH . $this->controller . 'Controller.php';
        
        if (file_exists($controllerFile)) {
            require_once $controllerFile;
            
            // Nome completo della classe del controller
            $controllerClass = $this->controller . 'Controller';
            
            // Verifica se la classe esiste
            if (class_exists($controllerClass)) {
                $controller = new $controllerClass();

                $user = null;
                // Verifica se il controller richiede autenticazione
                if (property_exists($controller, 'requiresAuth') && $controller->requiresAuth === true && $this->action != 'getendpoints') {

                    // Verifica il token e imposta l'utente
                    $user = $this->verifyToken();

                    // Se l'autenticazione è richiesta ma il token non è valido, blocca
                    if ($user === null) {
                        $this->handleError(401, 'Autenticazione richiesta');
                    }

                    // Rate limiting - più severo per utenti non autenticati
                    if ($user) {
                        ApiSecurity::checkRateLimit($user->sub, 100, 60);
                    } else {
                        ApiSecurity::checkRateLimit(null, 30, 60);
                    }
                }
                
                // Passa i dati dell'utente al controller se disponibile
                if (method_exists($controller, 'setUser') && $user) {
                    $controller->setUser($user);
                }
                
                // Verifica se il metodo (action) esiste
                $action = $this->action . 'Action';
                if (method_exists($controller, $action)) {
                    // Verifica autorizzazione all'action
                    $permissionMethod = $this->action . 'Permission';
                    if (method_exists($controller, $permissionMethod)) {
                        $controller->$permissionMethod($user);
                    }
                    
                    // Passa i parametri URL e i dati della richiesta al controller
                    $result = call_user_func_array(
                        [$controller, $action], 
                        [$this->params, $this->requestData, $this->requestMethod]
                    );
                    
                    // Se il controller non ha già inviato una risposta, invia il risultato
                    if ($result !== null) {
                        echo json_encode($result);
                    }
                    return;
                }
            }
        }
        
        // Se arriviamo qui, controller o action non sono stati trovati
        $this->handleError(404, 'Endpoint API non trovato');
    }
    
    /**
     * Gestisce gli errori API
     */
    private function handleError($code, $message)
    {
        http_response_code($code);
        
        // Per errori 500, non rivelare dettagli in produzione
        if ($code >= 500) {
            ApiSecurity::logSecurityEvent("Errore Server: $message", 'error');
            $message = 'Si è verificato un errore interno del server';
        }
        
        echo json_encode([
            'status' => 'error',
            'code' => $code,
            'message' => $message
        ]);
        
        exit;
    }
}