<?

if (class_exists('DocEndpointService')) {
    return;
}

class DocEndpointService
{
    public function GenerateDocs($instance)
    {
        // Accetta sia una stringa che un'istanza
        if (is_object($instance)) {
            $reflectionClass = new ReflectionClass($instance);
            $instanceInstance = $instance;
        } elseif (is_string($instance) && class_exists($instance)) {
            $reflectionClass = new ReflectionClass($instance);
            $instanceInstance = new $instance();
        } else {
            throw new InvalidArgumentException("È richiesta una classe o un'istanza del instance.");
        }
        
        $docs = [];
        
        // Recupera tutti i metodi pubblici
        $methods = $reflectionClass->getMethods(ReflectionMethod::IS_PUBLIC);
        
        foreach ($methods as $method) {
            $methodName = $method->getName();
            
            if ($methodName == "pingAction" || $methodName == "getEndpointsAction")
                continue;

            // Considera solo i metodi che finiscono con "Action"
            if (substr($methodName, -6) !== 'Action') {
                continue;
            }
            
            // Ottiene il nome dell'endpoint rimuovendo "Action"
            $endpointName = substr($methodName, 0, -6);
            
            $docComment = $method->getDocComment();
            if (!$docComment) {
                $docs[$endpointName] = [
                    'name' => $endpointName,
                    'description' => 'Nessuna documentazione disponibile.',
                    'method' => 'Sconosciuto',
                    'parameters' => [],
                    'returns' => []
                ];
                continue;
            }
            
            // Estrae la descrizione generale
            $docComment = preg_replace('/^\s*\/\*\*\s*|\s*\*\/\s*$/', '', $docComment);
            $docComment = preg_replace('/^\s*\*\s*/m', '', $docComment);
            
            // Estrae la prima riga come descrizione
            $lines = explode("\n", $docComment);
            $description = trim($lines[0]);
            
            // Estrae il metodo HTTP (GET, POST, ecc.)
            $method = 'GET'; // Default
            if (preg_match('/@method\s+(\w+)/i', $docComment, $methodMatch)) {
                $method = strtoupper($methodMatch[1]);
            }
            
            // Estrae i parametri
            $parameters = [];
            preg_match_all('/@param\s+(\S+)\s+(\$\w+(?:\[\'([^\']+)\'\])?)\s*(.*)?/m', $docComment, $paramMatches, PREG_SET_ORDER);
            foreach ($paramMatches as $match) {
                $paramType = $match[1];
                $paramName = $match[2];
                $paramDesc = isset($match[4]) ? trim($match[4]) : '';
                
                // Gestisce parametri in formato $body['Name']
                if (isset($match[3])) {
                    $paramName = $match[3]; // Usa il nome dentro le parentesi quadre
                    $paramSource = 'body';
                } else {
                    // Rimuovi il $ dal nome del parametro
                    $paramName = ltrim($paramName, '$');
                    $paramSource = 'query';
                }
                
                $parameters[] = [
                    'name' => $paramName,
                    'type' => $paramType,
                    'source' => $paramSource,
                    'description' => $paramDesc
                ];
            }
            
            // Estrae i valori di ritorno
            $returns = [];
            preg_match_all('/@return\s+(\S+)(?:\s+(.*))?/m', $docComment, $returnMatches, PREG_SET_ORDER);
            foreach ($returnMatches as $match) {
                $returnType = $match[1];
                $returnDesc = isset($match[2]) ? trim($match[2]) : '';
                
                $returns[] = [
                    'type' => $returnType,
                    'description' => $returnDesc
                ];
            }
            
            // Compone la documentazione dell'endpoint
            $docs[$endpointName] = [
                'name' => $endpointName,
                'description' => $description,
                'method' => $method,
                'parameters' => $parameters,
                'returns' => $returns
            ];
        }
        
        return $docs;
    }
    
    /**
     * Formatta la documentazione come HTML
     * 
     * @param array $docs Array con la documentazione degli endpoint
     * @return string Documentazione HTML
     */
    public static function FormatAsHtml($docs)
    {
        $html = '<div class="api-docs">';
        
        foreach ($docs as $endpoint => $info) {
            $html .= '<div class="endpoint">';
            $html .= '<h2>Endpoint: <i>' . htmlspecialchars($endpoint) . '</i></h2>';
            $html .= '<p><strong>Description:</strong> <i>' . htmlspecialchars($info['description']) . '</i></p>';
            $html .= '<p><strong>HttpMethod:</strong> <i>' . htmlspecialchars($info['method']) . '</i></p>';
            
            if (!empty($info['parameters'])) {
                $html .= '<h3>Parameters:</h3>';
                $html .= '<table border="1">';
                $html .= '<tr><th>Name</th><th>Type</th><th>Origin</th><th>Description</th></tr>';
                
                foreach ($info['parameters'] as $param) {
                    $html .= '<tr>';
                    $html .= '<td>' . htmlspecialchars($param['name']) . '</td>';
                    $html .= '<td>' . htmlspecialchars($param['type']) . '</td>';
                    $html .= '<td>' . htmlspecialchars($param['source']) . '</td>';
                    $html .= '<td>' . htmlspecialchars($param['description']) . '</td>';
                    $html .= '</tr>';
                }
                
                $html .= '</table>';
            } 
            // else {
            //     $html .= '<p>Nessun parametro richiesto.</p>';
            // }
            
            if (!empty($info['returns'])) {
                $html .= '<h3>Response:</h3>';
                $html .= '<table border="1">';
                $html .= '<tr><th>Tipo</th><th>Descrizione</th></tr>';
                
                foreach ($info['returns'] as $return) {
                    $html .= '<tr>';
                    $html .= '<td>' . htmlspecialchars($return['type']) . '</td>';
                    $html .= '<td>' . htmlspecialchars($return['description']) . '</td>';
                    $html .= '</tr>';
                }
                
                $html .= '</table>';
            } 
            // else {
            //     $html .= '<p>Nessuna informazione sulla risposta disponibile.</p>';
            // }
            
            $html .= '</div>';
            $html .= '<br />';
            $html .= '<hr>';
        }
        
        $html .= '</div>';
        
        return $html;
    }
    
    /**
     * Formatta la documentazione come JSON
     * 
     * @param array $docs Array con la documentazione degli endpoint
     * @return string Documentazione in formato JSON
     */
    public static function FormatAsJson($docs)
    {
        return json_encode($docs, JSON_PRETTY_PRINT);
    }
    
    /**
     * Formatta la documentazione come testo semplice
     * 
     * @param array $docs Array con la documentazione degli endpoint
     * @return string Documentazione in formato testo
     */
    public static function FormatAsText($docs)
    {
        $text = '';
        
        foreach ($docs as $endpoint => $info) {
            $text .= "Endpoint: {$endpoint}\n";
            $text .= "Descrizione: {$info['description']}\n";
            $text .= "Metodo HTTP: {$info['method']}\n\n";
            
            if (!empty($info['parameters'])) {
                $text .= "Parametri:\n";
                foreach ($info['parameters'] as $param) {
                    $text .= "  - {$param['name']} ({$param['type']}, da {$param['source']}): {$param['description']}\n";
                }
                $text .= "\n";
            } else {
                $text .= "Nessun parametro richiesto.\n\n";
            }
            
            if (!empty($info['returns'])) {
                $text .= "Risposte:\n";
                foreach ($info['returns'] as $return) {
                    $text .= "  - {$return['type']}: {$return['description']}\n";
                }
                $text .= "\n";
            } else {
                $text .= "Nessuna informazione sulla risposta disponibile.\n\n";
            }
            
            $text .= "----------------------\n\n";
        }
        
        return $text;
    }
}