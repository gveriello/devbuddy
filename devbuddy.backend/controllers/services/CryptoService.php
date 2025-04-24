<?php
class CryptoService {
    // Costanti per l'algoritmo di crittografia
    private const CIPHER_ALGO = 'aes-256-gcm';  // AES-256 in modalità GCM
    private const KEY_BYTES = 32;               // 256 bit per la chiave
    private const TAG_BYTES = 16;               // 128 bit per il tag di autenticazione
    private const SALT_BYTES = 16;              // 128 bit per il salt
    private const IV_BYTES = 12;                // 96 bit per l'IV (come raccomandato per GCM)
    
    private $masterKey = "MLewh3TXmBEPqA5dr8MbgdBik+0IOSXvFapi8DAPZJc=";
    
    public function __construct() {
        if (strlen($this->masterKey) < 32) {
            throw new \InvalidArgumentException('Master key troppo corta. Richiesti almeno 32 bytes.');
        }
    }
    
    /**
     * Cripta un messaggio
     * @param string $plaintext Il messaggio da criptare
     * @return string Dati criptati in formato sicuro (base64)
     */
    public function encrypt(string $plaintext): string {
        // Genera salt e IV casuali
        $salt = random_bytes(self::SALT_BYTES);
        $iv = random_bytes(self::IV_BYTES);
        
        // Deriva una chiave unica per questa operazione
        $key = $this->deriveKey($salt);
        
        // Cripta i dati
        $tag = '';
        $ciphertext = openssl_encrypt(
            $plaintext,
            self::CIPHER_ALGO,
            $key,
            OPENSSL_RAW_DATA,
            $iv,
            $tag
        );
        
        if ($ciphertext === false) {
            throw new \RuntimeException('Errore durante la crittografia');
        }
        
        // Combina tutti i componenti
        $encrypted = $salt . $iv . $tag . $ciphertext;
        
        // Codifica in base64 per un trasporto sicuro
        return base64_encode($encrypted);
    }
    
    /**
     * Decripta un messaggio
     * @param string $encrypted Il messaggio criptato (in base64)
     * @return string Il messaggio originale
     */
    public function decrypt(string $encrypted): string {
        // Decodifica da base64
        $data = base64_decode($encrypted, true);
        if ($data === false) {
            throw new \InvalidArgumentException('Dati non validi');
        }
        
        // Estrae i componenti
        $salt = substr($data, 0, self::SALT_BYTES);
        $iv = substr($data, self::SALT_BYTES, self::IV_BYTES);
        $tag = substr($data, self::SALT_BYTES + self::IV_BYTES, self::TAG_BYTES);
        $ciphertext = substr($data, self::SALT_BYTES + self::IV_BYTES + self::TAG_BYTES);
        
        // Deriva la chiave
        $key = $this->deriveKey($salt);
        
        // Decripta
        $plaintext = openssl_decrypt(
            $ciphertext,
            self::CIPHER_ALGO,
            $key,
            OPENSSL_RAW_DATA,
            $iv,
            $tag
        );
        
        if ($plaintext === false) {
            throw new \RuntimeException('Errore durante la decrittografia. Dati corrotti o chiave errata.');
        }
        
        return $plaintext;
    }
    
    /**
     * Deriva una chiave usando HKDF
     */
    private function deriveKey(string $salt): string {
        return hash_hkdf(
            'sha256',
            $this->masterKey,
            self::KEY_BYTES,
            'encryption',
            $salt
        );
    }
    
    /**
     * Genera una chiave master sicura
     */
    public static function generateMasterKey(): string {
        return base64_encode(random_bytes(self::KEY_BYTES));
    }
    
    /**
     * Cripta dati per memorizzazione nel database
     */
    public function encryptForStorage(string $data): array {
        $encrypted = $this->encrypt($data);
        return [
            'data' => $encrypted,
            'algo' => self::CIPHER_ALGO,
            'created_at' => time()
        ];
    }
}

