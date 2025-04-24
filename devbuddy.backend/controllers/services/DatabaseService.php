<?php

if (class_exists('Database')) {
    return;
}

class Database {
    private $host = "localhost";
    private $db_name = "my_hubconnect";
    private $username = "hubconnect";
    private $password = "";
    protected $conn;

    // Connessione al database
    public function connect() {
        $this->conn = null;

        try {
            $this->conn = new PDO(
                "mysql:host=" . $this->host . ";dbname=" . $this->db_name,
                $this->username,
                $this->password
            );
            $this->conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
            $this->conn->exec("set names utf8");
        } catch(PDOException $e) {
            echo "Errore di connessione: " . $e->getMessage();
        }

        return $this->conn;
    }

    // Create
    public function create($table, $data) {
        try {
            $fields = array_keys($data);
            $values = array_fill(0, count($fields), '?');
            
            $sql = "INSERT INTO " . $table . " (" . implode(',', $fields) . ") 
                    VALUES (" . implode(',', $values) . ")";
            
            $stmt = $this->conn->prepare($sql);
            $stmt->execute(array_values($data));
            
            return $this->conn->lastInsertId();
        } catch(PDOException $e) {
            echo "Errore nell'inserimento: " . $e->getMessage();
            return false;
        }
    }

    // Read
    public function read($table, $conditions = [], $fields = "*") {
        try {
            $sql = "SELECT " . $fields . " FROM " . $table;
            
            if (!empty($conditions)) {
                $sql .= " WHERE ";
                $whereClauses = [];
                foreach($conditions as $key => $value) {
                    $whereClauses[] = "$key = ?";
                }
                $sql .= implode(' AND ', $whereClauses);
            }

            $stmt = $this->conn->prepare($sql);
            $stmt->execute(array_values($conditions));
            
            return $stmt->fetchAll(PDO::FETCH_ASSOC);
        } catch(PDOException $e) {
            echo "Errore nella lettura: " . $e->getMessage();
            return false;
        }
    }

    // Update
    public function update($table, $data, $conditions) {
        try {
            $setClauses = [];
            foreach($data as $key => $value) {
                $setClauses[] = "$key = ?";
            }
            
            $whereClauses = [];
            foreach($conditions as $key => $value) {
                $whereClauses[] = "$key = ?";
            }

            $sql = "UPDATE " . $table . 
                   " SET " . implode(',', $setClauses) . 
                   " WHERE " . implode(' AND ', $whereClauses);

            $stmt = $this->conn->prepare($sql);
            $stmt->execute(array_merge(array_values($data), array_values($conditions)));
            
            return $stmt->rowCount();
        } catch(PDOException $e) {
            echo "Errore nell'aggiornamento: " . $e->getMessage();
            return false;
        }
    }

    // Delete
    public function delete($table, $conditions) {
        try {
            $whereClauses = [];
            foreach($conditions as $key => $value) {
                $whereClauses[] = "$key = ?";
            }

            $sql = "DELETE FROM " . $table . " WHERE " . implode(' AND ', $whereClauses);
            
            $stmt = $this->conn->prepare($sql);
            $stmt->execute(array_values($conditions));
            
            return $stmt->rowCount();
        } catch(PDOException $e) {
            echo "Errore nella cancellazione: " . $e->getMessage();
            return false;
        }
    }

    // Query personalizzata
    public function query($sql, $params = []) {
        try {
            $stmt = $this->conn->prepare($sql);
            $stmt->execute($params);

            if (stripos(trim($sql), 'SELECT') === 0) {
                return $stmt->fetchAll(PDO::FETCH_ASSOC);
            }
            // Altrimenti restituisci true per indicare il successo
            return true;

        } catch(PDOException $e) {
            echo "Errore nella query: " . $e->getMessage();
            return false;
        }
    }
}