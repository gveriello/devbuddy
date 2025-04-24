<?php

// Configurazione
define('BASE_PATH', __DIR__);
define('CONTROLLER_PATH', BASE_PATH . '/controllers/');
define('JWT_PATH', BASE_PATH . '/jwt/');
define('DEFAULT_CONTROLLER', 'Api');
define('DEFAULT_ACTION', 'index');
define('MAX_TIMESTAMP_DIFF', 300000); 

// Impostazioni di sicurezza PHP
ini_set('display_errors', '1');
error_reporting(E_ALL);
ini_set('session.cookie_httponly', '1');
ini_set('session.use_only_cookies', '1');
ini_set('session.cookie_secure', '1');
ini_set('session.cookie_samesite', 'Strict');

// Request header
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *'); 
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');

// Response header
header('Content-Type: application/json');
header('X-Content-Type-Options: nosniff');
header('X-Frame-Options: DENY');
header('X-XSS-Protection: 1; mode=block');
header('Strict-Transport-Security: max-age=31536000; includeSubDomains; preload');

require 'secret/secrets.php';

// Gestione delle richieste OPTIONS (CORS preflight)
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    exit(0);
}

// CORS - modificare in produzione con domini specifici
// $allowedOrigins = ['https://tuodominio.com', 'https://app.tuodominio.com'];

// $origin = isset($_SERVER['HTTP_ORIGIN']) ? $_SERVER['HTTP_ORIGIN'] : '';

// if (in_array($origin, $allowedOrigins)) {
//     header('Access-Control-Allow-Origin: ' . $origin);
//     header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
//     header('Access-Control-Allow-Headers: Content-Type, Authorization, X-API-Key');
//     header('Access-Control-Allow-Credentials: true');
//     header('Access-Control-Max-Age: 86400'); // 24 ore
// } 

// if (!isset($_SERVER['HTTPS']) || $_SERVER['HTTPS'] !== 'on') {
//     http_response_code(403);
//     echo json_encode([
//         'status' => 'error',
//         'message' => 'Prevista connessione https.'
//     ]);
// }