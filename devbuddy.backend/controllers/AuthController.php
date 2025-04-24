<?php

// Inject 
require_once 'base/BaseApiController.php';
require_once 'services/CryptoService.php';
require_once 'services/DatabaseService.php';
require_once 'services/LogService.php';
require_once 'services/ApplicationService.php';
require_once JWT_PATH.'/JWT.php';
require_once JWT_PATH.'/Key.php';
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

/**
 * AuthController
 * 
 * Controller per la gestione dell'autenticazione e autorizzazione
 */
class AuthController extends BaseApiController
{
    protected $logService = null;
    protected $applicationService = null;

    public function  __construct()
    {
        $this->logService = new LogService();
        $this->applicationService = new ApplicationService();
    }

    /**
     * Metodo per loggare un utente
     * @method POST
     * @param string $body['AppId'] Rappresenta l'id applicativo che vuole consultare l'endpoint. 
     * @param string $body['Email'] Rappresenta l'email dell'utente. 
     * @param string $body['Password'] Rappresenta la password dell'utente. 
     * @return Ok login effettuato con successo. token jwt come stringa di ritorno
     * @return Unauthorized se email o password sono errate o se app id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function loginAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        // Valida i campi richiesti
        $this->validateRequired($data, ['AppId']);

        // Validazione email
        $this->validate($data, [
            'Email' => ['required' => true, 'email' => true],
            'Password' => ['required' => true, 'min' => 8, 'max' => 20]
        ]);
        
        try
        {
            $appid = $data['AppId'];
            $appid = $this->applicationService->CheckAppId($appid);
            if ($appid > 0)
            {
                $db = new Database();
                $db->connect();
    
                $email = $data['Email'];
                $psw = $data['Password'];
                $utente = $db->read('all_users', ['email' => $email]);
                if ($utente != null)
                {
                    $utente = $utente[0];
                    if ($utente["active"] == 0)
                        $this->Unauthorized("La tua utenza è stata disattivata.");

                    $crypto = new CryptoService();
                    $decrypted = $crypto->decrypt($utente["password"]);
                    if ($decrypted == $psw)
                    {
                        $payload = [
                            "iss" => $data["AppId"],      
                            "aud" => AUDIENCE,      
                            "iat" => time(),                    
                            "exp" => time() + 3600,             
                            "sub" => $utente["id"],              
                            "name" => $utente["name"],               
                            "surname" => $utente["surname"],       
                            "email" => $utente["email"]               
                        ];
                        
                        // Generazione del JWT
                        $jwt = JWT::encode($payload, API_SECRET, 'HS256');
                        $this->Ok($jwt);
                    }
                    else
                    {
                        $this->Unauthorized("La password fornita non è corretta.");
                    }
                }
                $this->Unauthorized("L'email fornita non è valida.");
            }
            $this->Unauthorized("Applicazione non autorizzata.");
        }
        catch (Exception $ex)
        {
            $this->logService->Log("Login action, ".$ex->getMessage(), "exception", $data["AppId"]);
            $this->InternalServerError($ex->getMessage());
        }
    }
    
    /**
     * POST /auth/register
     */
    /**
     * Registrazione nuovo utente
     * @method POST
     * @param string $body['AppId'] Rappresenta l'id applicativo che vuole consultare l'endpoint. 
     * @param string $body['Email'] Rappresenta l'email dell'utente. 
     * @param string $body['Password'] Rappresenta la password dell'utente. 
     * @param string $body['Name'] Rappresenta il nome dell'utente. 
     * @param string $body['Surname'] Rappresenta il cognome dell'utente. 
     * @return Ok registrazione effettuata con successo. token jwt come stringa di ritorno
     * @return Unauthorized email esistente o se app id non è valido
     * @return InternalServerError se c'è un errore interno nell'endpoint
     */
    public function registerAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        // Valida i campi richiesti
        $this->validateRequired($data, ['AppId']);
        
        // Altre validazioni
        $this->validate($data, [
            'Email' => ['required' => true, 'email' => true],
            'Password' => ['required' => true, 'min' => 8, 'max' => 20],
            'Name' => ['required' => true, 'min' => 2, 'max' => 50],
            'Surname' => ['required' => true, 'min' => 2, 'max' => 50]
        ]);
        
        try
        {
            $appid = $data['AppId'];
            $appid = $this->applicationService->CheckAppId($appid);
            if ($appid > 0)
            {
                $db = new Database();
                $db->connect();

                $email = $data['Email'];
                $utente = $db->read('all_users', ['email' => $email]);
                if ($utente != null)
                {
                    $this->logService->Log("Register action, tentativo di registrazione della mail ".$email." già esistente.", "warning", $data["AppId"]);
                    $this->Unauthorized("L'email che si sta tentando di registrare, esiste già.");
                }

                $psw = $data['Password'];
                $name = $data['Name'];
                $surname = $data['Surname'];
                $crypto = new CryptoService();
                $passwordHash = $crypto->encrypt($psw);

                // Prepara i dati utente per l'inserimento
                $userData = [
                    'name' => $name,
                    'surname' => $surname,
                    'email' => $email,
                    'password' => $passwordHash
                ];
                
                // Inserisce l'utente nel database
                $userId = $db->create('all_users', $userData);
                if ($userId > 0)
                {
                    $payload = [
                        "iss" => $data["AppId"],      
                        "aud" => AUDIENCE,      
                        "iat" => time(),                    
                        "exp" => time() + 3600,             
                        "sub" => (int)$userId,              
                        "name" => $name,               
                        "surname" => $surname,       
                        "email" => $email                      
                    ];
                    
                    // Generazione del JWT
                    $jwt = JWT::encode($payload, API_SECRET, 'HS256');
                    $this->Ok($jwt);
                }                
                else
                {
                    $this->logService->Log("Errore durante la registrazione per l'account ".$email.".", "error", $data["AppId"]);
                    $this->InternalServerError("Si è verificato un errore durante la registrazione dell'utenza.");
                }
            }
            $this->Unauthorized("Applicazione non autorizzata.");
        }
        catch (Exception $ex)
        {
            $this->logService->Log("Login action, ".$ex->getMessage(), "exception", $data["AppId"]);
            $this->InternalServerError($ex->getMessage());
        }
    }
    
    /**
     * Aggiorna token
     * POST /auth/refresh
     */
    public function refreshAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        if (!$this->user) {
            $this->sendError('Token mancante o non valido', 401);
        }
        
        // Genera un nuovo token con scadenza aggiornata
        $token = $this->generateToken([
            'sub' => $this->user->sub,
            'name' => $this->user->name,
            'email' => $this->user->email ?? '',
            'role' => $this->user->role,
            'iat' => time(),
            'exp' => time() + 3600 // 1 ora
        ]);
        
        return [
            'status' => 'success',
            'message' => 'Token aggiornato con successo',
            'data' => [
                'token' => $token,
                'expires_in' => 3600
            ]
        ];
    }
    
    /**
     * Logout
     * POST /auth/logout
     */
    public function logoutAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        // In un sistema stateless con JWT, il logout lato server è concettualmente diverso
        // Il client dovrebbe semplicemente eliminare il token
        
        // Tuttavia, potresti voler gestire una blacklist di token revocati
        // o eseguire altre operazioni lato server
        
        // Esempio: log del logout
        if ($this->user) {
            ApiSecurity::logSecurityEvent(
                'Utente disconnesso', 
                'info', 
                ['user_id' => $this->user->sub]
            );
        }
        
        return [
            'status' => 'success',
            'message' => 'Logout effettuato con successo'
        ];
    }
    
    /**
     * Recupero password dimenticata
     * POST /auth/forgot-password
     */
    public function forgotPasswordAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        // Valida i campi richiesti
        $this->validateRequired($data, ['email']);
        $this->validateEmail($data['email']);
        
        // In un'applicazione reale, qui genereresti un token di recupero
        // e invieresti un'email all'utente
        
        // Non confermare esplicitamente se l'email esiste o meno
        // (per motivi di sicurezza)
        
        return [
            'status' => 'success',
            'message' => 'Se l\'indirizzo email esiste nel nostro sistema, riceverai istruzioni per reimpostare la password'
        ];
    }
    
    /**
     * Reset password
     * POST /auth/reset-password
     */
    public function resetPasswordAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        // Valida i campi richiesti
        $this->validateRequired($data, ['token', 'password', 'password_confirmation']);
        
        // Verifica che le password corrispondano
        if ($data['password'] !== $data['password_confirmation']) {
            $this->sendError('Le password non corrispondono', 422);
        }
        
        // Valida la password
        $this->validate($data, [
            'password' => ['required' => true, 'min' => 8]
        ]);
        
        // In un'applicazione reale, qui verificheresti il token di reset
        // e aggiorneresti la password dell'utente nel database
        
        // Esempio simulato di verifica token
        if ($data['token'] !== 'valid_token_example') {
            $this->sendError('Token di reset non valido o scaduto', 400);
        }
        
        return [
            'status' => 'success',
            'message' => 'Password reimpostata con successo'
        ];
    }
    
    /**
     * Cambia password
     * POST /auth/change-password
     */
    public function changePasswordAction($params, $data, $method)
    {
        $this->requireMethod('POST');
        
        if (!$this->user) {
            $this->sendError('Autenticazione richiesta', 401);
        }
        
        // Valida i campi richiesti
        $this->validateRequired($data, ['current_password', 'new_password', 'new_password_confirmation']);
        
        // Verifica che le nuove password corrispondano
        if ($data['new_password'] !== $data['new_password_confirmation']) {
            $this->sendError('Le nuove password non corrispondono', 422);
        }
        
        // Valida la nuova password
        $this->validate($data, [
            'new_password' => ['required' => true, 'min' => 8]
        ]);
        
        // In un'applicazione reale, qui verificheresti la password corrente
        // e aggiorneresti la password nel database
        
        // Esempio simulato di verifica password corrente
        if ($data['current_password'] !== 'password') {
            $this->sendError('Password corrente non valida', 400);
        }
        
        return [
            'status' => 'success',
            'message' => 'Password modificata con successo'
        ];
    }
    
    /**
     * Genera un token JWT (implementazione semplificata)
     * In produzione, usa una libreria JWT dedicata
     */
    private function generateToken($payload)
    {
        // In un'implementazione reale, useresti una libreria come firebase/php-jwt
        // Questo è solo un esempio semplificato
        
        $header = json_encode(['typ' => 'JWT', 'alg' => 'HS256']);
        $payload = json_encode($payload);
        
        $base64UrlHeader = str_replace(['+', '/', '='], ['-', '_', ''], base64_encode($header));
        $base64UrlPayload = str_replace(['+', '/', '='], ['-', '_', ''], base64_encode($payload));
        
        $signature = hash_hmac('sha256', $base64UrlHeader . "." . $base64UrlPayload, API_SECRET_KEY, true);
        $base64UrlSignature = str_replace(['+', '/', '='], ['-', '_', ''], base64_encode($signature));
        
        return $base64UrlHeader . "." . $base64UrlPayload . "." . $base64UrlSignature;
    }
}