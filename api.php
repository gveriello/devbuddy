<?

// Attiva la modalità strict per PHP
declare(strict_types=1);

// Inject dei parametri di configurazione e di startup
require_once 'configuration.php';

spl_autoload_register(function ($className) {
    $className = str_replace('\\', '/', $className);
    $filePath = BASE_PATH . '/' . $className . '.php';
    
    if (file_exists($filePath)) {
        require_once $filePath;
        return true;
    }
    return false;
});

// Inject del routing
require_once 'routing.php';

// Inject del sistema di sicurezza API
require_once 'api-security.php';


require_once CONTROLLER_PATH.'services/DocEndpointService.php';

require_once JWT_PATH.'/JWT.php';
require_once JWT_PATH.'/Key.php';
use Firebase\JWT\JWT;
use Firebase\JWT\Key;

// Gestione del routing
try {
    $router = new Router();
    $router->dispatch();
} catch (Exception $e) {
    // Log dell'errore
    ApiSecurity::logSecurityEvent('Exception: ' . $e->getMessage(), 'error', [
        'file' => $e->getFile(),
        'line' => $e->getLine()
    ]);
    
    http_response_code(500);
    echo json_encode([
        'status' => 'error',
        'message' => 'Si è verificato un errore interno al server.'
    ]);
}