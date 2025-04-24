<?php

/**
 * Classe base per i controller API
 */
abstract class BaseApiController
{
    protected $user = null;
    protected $apiKeyData = null;
    protected $requiredRole = null; // Ruolo minimo richiesto

    /**
     * Imposta l'utente corrente
     */
    public function setUser($user)
    {
        $this->user = $user;
    }
    
    /**
     * Imposta i dati dell'API key
     */
    public function setApiKeyData($apiKeyData)
    {
        $this->apiKeyData = $apiKeyData;
    }
    
    /**
     * Verifica i permessi per un'action specifica
     */
    public function checkPermission($action, $user, $apiKeyData)
    {        
        // Verifica ruolo se specificato
        if ($this->requiredRole) {
            if ($user && isset($user->role) && $user->role != $this->requiredRole) {
                $this->sendError(null, 'Permessi insufficienti', 403);
            } else if ($apiKeyData && isset($apiKeyData['role']) && $apiKeyData['role'] != $this->requiredRole) {
                $this->sendError(null, 'Permessi insufficienti per la API key', 403);
            }
        }
        
        return true;
    }


    
    protected function Ok($data = null)
    {
        $this->sendSuccess($data);
    }

    protected function BadRequest($message = 'La richiesta non è valida.')
    {
        $this->sendError(null, $message, 400);
    }
    
    protected function Unauthorized($message = 'Non sei autorizzato ad usare questo endpoint.')
    {
        $this->sendError(null, $message, 401);
    }

    protected function NotFound($message = "L'item da te cercato non esiste.")
    {
        $this->sendError(null, $message, 404);
    }

    protected function InternalServerError($error = null)
    {
        $this->sendError($error, 'Si è verificato un errore interno.', 500);
    }

    public function pingAction()
    {
        $this->requireMethod('GET');

        $toReturn = [];
        $toReturn['controller'] = get_class($this);

        if (class_exists('Database')) {
            $db = new Database();
            $db->connect();
            $result = $db->query("SELECT 1");
            $toReturn['databaseStatus'] = $result ? "OK" : "Failed";
        }

        $data = json_decode(file_get_contents("http://ip-api.com/json/"));
        $toReturn['localizationStatus'] = $data->status == "success" ? "OK" : "Failed";
        
        echo json_encode($toReturn);
    }

    public function getEndpointsAction($params)
    {
        $this->requireMethod('GET');
        
        @$type = $params[0];
        if ($type != 'json' && $type != 'html')
            $this->BadRequest('Fornire un type valido.');

        $docEndpointService = new DocEndpointService();
        $docs = $docEndpointService->GenerateDocs($this);
        if ($type == 'html')
        {
            header('Content-Type: text/html');
            echo "<!DOCTYPE html><html><head><title>API Documentation</title></head><body>";
            echo $docEndpointService->FormatAsHtml($docs);
            echo "</body></html>";
            return;
        }
        if ($type == 'json')
        {
            echo $docEndpointService->FormatAsJson($docs);
            return;
        }
    }

    /**
     * Invia una risposta di successo
     */
    protected function sendSuccess($data = null, $message = 'Operazione completata con successo.', $code = 200)
    {
        http_response_code($code);
        echo json_encode([
            'status' => 'success',
            'message' => $message,
            'data' => $data
        ]);
        exit;
    }
    
    /**
     * Invia una risposta di errore
     */
    protected function sendError($errors = null, $message = 'Si è verificato un errore.', $code = 400)
    {
        http_response_code($code);
        $response = [
            'status' => 'error',
            'message' => $message
        ];
        
        if ($errors !== null) {
            $response['errors'] = $errors;
        }
        
        echo json_encode($response);
        exit;
    }
    
    /**
     * Verifica se tutti i campi richiesti sono presenti
     */
    protected function validateRequired($data, $requiredFields)
    {
        $missingFields = [];
        
        foreach ($requiredFields as $field) {
            if (!isset($data[$field]) || empty($data[$field])) {
                $missingFields[] = $field;
            }
        }
        
        if (!empty($missingFields)) {
            $this->sendError(
                ['missing_fields' => $missingFields],
                'Campi obbligatori mancanti', 
                422                
            );
        }
        
        return true;
    }
    
    /**
     * Verifica il tipo di richiesta HTTP
     */
    protected function requireMethod($method)
    {
        $requestMethod = $_SERVER['REQUEST_METHOD'];
        
        if ($requestMethod !== $method) {
            $this->sendError(
                null,
                "Metodo non consentito. Richiesto $method, ricevuto $requestMethod", 
                405
            );
        }
        
        return true;
    }
    
    /**
     * Valida un indirizzo email
     */
    protected function validateEmail($email)
    {
        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
            $this->sendError(null, 'Indirizzo email non valido', 422);
        }
        return true;
    }
    
    /**
     * Valida i dati con regole personalizzate
     */
    protected function validate($data, $rules)
    {
        $errors = [];
        
        foreach ($rules as $field => $fieldRules) {
            foreach ($fieldRules as $rule => $ruleValue) {
                // Campo richiesto
                if ($rule === 'required' && $ruleValue && (!isset($data[$field]) || $data[$field] === '')) {
                    $errors[$field][] = "Il campo $field è obbligatorio";
                }
                
                // Salta altre validazioni se il campo è vuoto e non è richiesto
                if (!isset($data[$field]) || $data[$field] === '') {
                    continue;
                }
                
                // Lunghezza minima
                if ($rule === 'min' && strlen($data[$field]) < $ruleValue) {
                    $errors[$field][] = "Il campo $field deve essere lungo almeno $ruleValue caratteri";
                }
                
                // Lunghezza massima
                if ($rule === 'max' && strlen($data[$field]) > $ruleValue) {
                    $errors[$field][] = "Il campo $field non può superare $ruleValue caratteri";
                }
                
                // Email
                if ($rule === 'email' && $ruleValue && !filter_var($data[$field], FILTER_VALIDATE_EMAIL)) {
                    $errors[$field][] = "Il campo $field deve essere un indirizzo email valido";
                }
                
                // Numerico
                if ($rule === 'numeric' && $ruleValue && !is_numeric($data[$field])) {
                    $errors[$field][] = "Il campo $field deve essere numerico";
                }
                
                // Corrisponde a pattern regex
                if ($rule === 'pattern' && !preg_match($ruleValue, $data[$field])) {
                    $errors[$field][] = "Il campo $field non è nel formato corretto";
                }
                
                // In lista di valori
                if ($rule === 'in' && !in_array($data[$field], $ruleValue)) {
                    $errors[$field][] = "Il valore di $field non è valido";
                }
            }
        }
        
        if (!empty($errors)) {
            $this->sendError($errors, 'Errori di validazione', 422);
        }
        
        return true;
    }
}