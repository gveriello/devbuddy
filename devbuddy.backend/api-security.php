<?
/**
 * Classe per la sicurezza delle richieste API
 */
class ApiSecurity
{
    /**
     * Verifica la presenza e validità del token JWT
     */
    public static function verifyJwtToken($requireAuth = true)
    {
        $headers = getallheaders();
        $authHeader = isset($headers['Authorization']) ? $headers['Authorization'] : '';
        
        if (empty($authHeader) || !preg_match('/Bearer\s(\S+)/', $authHeader, $matches)) {
            if ($requireAuth) {
                self::sendAuthError('Token di autorizzazione mancante o non valido');
            }
            return null;
        }
        
        $token = $matches[1];
        
        try {
            // $decodedToken = JWT::decode($token, API_SECRET_KEY, ['HS256']);
            // return $decodedToken;
            
            // Esempio semplificato per dimostrazione:
            if (strlen($token) < 10) {
                self::sendAuthError('Token non valido');
            }
            
            // Simula un token decodificato
            return (object)[
                'sub' => '123', // ID utente
                'name' => 'Example User',
                'role' => 'user',
                'exp' => time() + 3600
            ];
            
        } catch (Exception $e) {
            self::sendAuthError('Token non valido: ' . $e->getMessage());
        }
    }
    
    /**
     * Verifica l'API key
     */
    public static function verifyApiKey($requireKey = true)
    {
        $headers = getallheaders();
        $apiKey = isset($headers['X-API-Key']) ? $headers['X-API-Key'] : '';
        
        if (empty($apiKey)) {
            if ($requireKey) {
                self::sendAuthError('API Key mancante');
            }
            return false;
        }
        
        // In un'implementazione reale, verificheresti la chiave API nel database
        // Esempio semplificato:
        $validApiKeys = [
            'test-key-123' => ['role' => 'admin', 'rate_limit' => 100],
            'test-key-456' => ['role' => 'user', 'rate_limit' => 20]
        ];
        
        if (isset($validApiKeys[$apiKey])) {
            return $validApiKeys[$apiKey];
        } else if ($requireKey) {
            self::sendAuthError('API Key non valida');
        }
        
        return false;
    }
    
    /**
     * Previene attacchi CSRF
     */
    public static function validateCsrfToken()
    {
        // Per API stateless questo è meno rilevante, ma utile per API che usano sessioni
        if ($_SERVER['REQUEST_METHOD'] === 'POST' || 
            $_SERVER['REQUEST_METHOD'] === 'PUT' || 
            $_SERVER['REQUEST_METHOD'] === 'DELETE') {
            
            $headers = getallheaders();
            $csrfToken = isset($headers['X-CSRF-Token']) ? $headers['X-CSRF-Token'] : '';
            
            // Verifica del token CSRF, se viene utilizzato
            // In un'API stateless pura probabilmente utilizzerai JWT invece
        }
        
        return true;
    }
    
    /**
     * Limita il numero di richieste (rate limiting)
     */
    public static function checkRateLimit($userId = null, $limit = 60, $period = 60)
    {        
        $ip = $_SERVER['REMOTE_ADDR'];
        $identifier = $userId ? $userId : $ip;
        
        $cacheFile = 'rate_limit/rate_limit_' . md5($identifier) . '.json';
        
        $rateData = [];
        if (file_exists($cacheFile)) {
            $content = file_get_contents($cacheFile);
            $rateData = json_decode($content, true) ?: [];
        }
        
        $time = time();
        // Rimuovi richieste più vecchie del periodo
        $rateData = array_filter($rateData, function($timestamp) use ($time, $period) {
            return ($time - $timestamp) < $period;
        });
        
        // Aggiungi la richiesta corrente
        $rateData[] = $time;
        
        // Salva i dati aggiornati
        file_put_contents($cacheFile, json_encode($rateData));
        
        // Verifica limite
        if (count($rateData) > $limit) {
            http_response_code(429);
            echo json_encode([
                'status' => 'error',
                'message' => 'Abbiamo rilevato un tentativo di richieste troppo alto; riprova tra poco.',
                'limit' => $limit,
                'period' => $period,
                'retry_after' => $period - ($time - min($rateData))
            ]);
            exit;
        }
        
        return true;
    }
    
    /**
     * Verifica e sanifica i dati di input
     */
    public static function sanitizeInput($data)
    {
        if (is_array($data)) {
            foreach ($data as $key => $value) {
                // Rimuovi chiavi sospette che iniziano con $ (MongoDB injection)
                if (is_string($key) && substr($key, 0, 1) === '$') {
                    unset($data[$key]);
                    continue;
                }
                
                $data[$key] = self::sanitizeInput($value);
            }
        } else if (is_string($data)) {
            // Sanifica stringhe per prevenire XSS
            $data = htmlspecialchars($data, ENT_QUOTES, 'UTF-8');
        }
        
        return $data;
    }
    
    /**
     * Invia errore di autenticazione
     */
    private static function sendAuthError($message)
    {
        http_response_code(401);
        echo json_encode([
            'status' => 'error',
            'message' => $message
        ]);
        exit;
    }
    
    /**
     * Verifica il contenuto potenzialmente dannoso nei file uploadati
     */
    public static function validateFileUpload($file)
    {
        // Lista di estensioni consentite
        $allowedExtensions = ['jpg', 'jpeg', 'png', 'pdf', 'txt', 'csv'];
        
        // Lista di tipi MIME consentiti
        $allowedMimeTypes = [
            'image/jpeg', 'image/png', 'application/pdf', 
            'text/plain', 'text/csv'
        ];
        
        // Verifica estensione
        $extension = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
        if (!in_array($extension, $allowedExtensions)) {
            return false;
        }
        
        // Verifica tipo MIME
        $finfo = new finfo(FILEINFO_MIME_TYPE);
        $mimeType = $finfo->file($file['tmp_name']);
        
        if (!in_array($mimeType, $allowedMimeTypes)) {
            return false;
        }
        
        // Verifica contenuto per PHP code o altri script dannosi
        $content = file_get_contents($file['tmp_name']);
        $dangerousPatterns = [
            '/<\?php/i',
            '/<script/i',
            '/\beval\s*\(/i',
            '/\bexec\s*\(/i',
            '/\bsystem\s*\(/i'
        ];
        
        foreach ($dangerousPatterns as $pattern) {
            if (preg_match($pattern, $content)) {
                return false;
            }
        }
        
        return true;
    }
    
    /**
     * Genera un log di sicurezza
     */
    public static function logSecurityEvent($event, $severity = 'info', $details = [])
    {
        $logData = [
            'timestamp' => date('Y-m-d H:i:s'),
            'ip' => $_SERVER['REMOTE_ADDR'],
            'user_agent' => $_SERVER['HTTP_USER_AGENT'] ?? 'Unknown',
            'event' => $event,
            'severity' => $severity,
            'request_uri' => $_SERVER['REQUEST_URI'],
            'details' => $details
        ];
        
        // In produzione, usa un sistema di logging reale
        error_log(json_encode($logData));
    }
}